using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assistant;
using ClassicAssist.Data.Macros;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using Newtonsoft.Json;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Targeting
{
    public class TargetManager
    {
        public delegate void dTargetChanged( int newSerial, int oldSerial );

        private const int MAX_DISTANCE = 18;
        private static TargetManager _instance;
        private static readonly object _lock = new object();
        private static readonly List<Mobile> _ignoreList = new List<Mobile>();

        private TargetManager()
        {
            string dataPath = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Data" );

            BodyData = JsonConvert
                .DeserializeObject<TargetBodyData[]>( File.ReadAllText( Path.Combine( dataPath, "Bodies.json" ) ) )
                .ToList();
        }

        public List<TargetBodyData> BodyData { get; set; }

        public static event dTargetChanged EnemyChangedEvent;
        public static event dTargetChanged FriendChangedEvent;
        public static event dTargetChanged LastTargetChangedEvent;

        public void SetEnemy( Entity m )
        {
            if ( !UOMath.IsMobile( m.Serial ) )
            {
                return;
            }

            if ( !MacroManager.QuietMode )
            {
                MsgCommands.HeadMsg( Options.CurrentOptions.EnemyTargetMessage, m.Serial );
            }

            MsgCommands.HeadMsg( $"Target: {m.Name?.Trim() ?? "Unknown"}" );

            if ( m.Serial != Engine.Player.EnemyTargetSerial )
            {
                EnemyChangedEvent?.Invoke( m.Serial, Engine.Player.EnemyTargetSerial );
                LastTargetChangedEvent?.Invoke( m.Serial, Engine.Player.LastTargetSerial );
            }

            Engine.Player.LastTargetSerial = m.Serial;
            Engine.Player.EnemyTargetSerial = m.Serial;
            Engine.SendPacketToClient( new ChangeCombatant( m.Serial ) );
        }

        public void SetFriend( Entity m )
        {
            if ( !UOMath.IsMobile( m.Serial ) )
            {
                return;
            }

            if ( !MacroManager.QuietMode )
            {
                MsgCommands.HeadMsg( Options.CurrentOptions.FriendTargetMessage, m.Serial );
            }

            MsgCommands.HeadMsg( $"Target: {m.Name?.Trim() ?? "Unknown"}" );

            if ( m.Serial != Engine.Player.FriendTargetSerial )
            {
                FriendChangedEvent?.Invoke( m.Serial, Engine.Player.FriendTargetSerial );
                LastTargetChangedEvent?.Invoke( m.Serial, Engine.Player.LastTargetSerial );
            }

            Engine.Player.LastTargetSerial = m.Serial;
            Engine.Player.FriendTargetSerial = m.Serial;
            Engine.SendPacketToClient( new ChangeCombatant( m.Serial ) );
        }

        public void SetLastTarget( Entity m )
        {
            if ( m.Serial != Engine.Player.LastTargetSerial )
            {
                LastTargetChangedEvent?.Invoke( m.Serial, Engine.Player.LastTargetSerial );
            }

            Engine.Player.LastTargetSerial = m.Serial;

            if ( !UOMath.IsMobile( m.Serial ) )
            {
                return;
            }

            if ( !MacroManager.QuietMode )
            {
                MsgCommands.HeadMsg( Options.CurrentOptions.LastTargetMessage, m.Serial );
            }

            MsgCommands.HeadMsg( $"Target: {m.Name?.Trim() ?? "Unknown"}" );
            Engine.SendPacketToClient( new ChangeCombatant( m.Serial ) );
        }

        public Mobile GetClosestMobile( IEnumerable<Notoriety> notoriety, TargetBodyType bodyType = TargetBodyType.Any,
            TargetFriendType friendType = TargetFriendType.Include,
            TargetInfliction inflictionType = TargetInfliction.Any, int maxDistance = -1 )
        {
            Mobile mobile;

            Func<int, bool> bodyTypePredicate;

            switch ( bodyType )
            {
                case TargetBodyType.Any:
                    bodyTypePredicate = i => true;
                    break;
                case TargetBodyType.Humanoid:
                    bodyTypePredicate = i =>
                        BodyData.Where( bd => bd.BodyType == TargetBodyType.Humanoid ).Select( bd => bd.Graphic )
                            .Contains( i );
                    break;
                case TargetBodyType.Transformation:
                    bodyTypePredicate = i =>
                        BodyData.Where( bd => bd.BodyType == TargetBodyType.Transformation ).Select( bd => bd.Graphic )
                            .Contains( i );
                    break;
                case TargetBodyType.Both:
                    bodyTypePredicate = i =>
                        BodyData.Where( bd =>
                                bd.BodyType == TargetBodyType.Humanoid || bd.BodyType == TargetBodyType.Transformation )
                            .Select( bd => bd.Graphic ).Contains( i );
                    break;
                default:
                    throw new ArgumentOutOfRangeException( nameof( bodyType ), bodyType, null );
            }

            if ( friendType == TargetFriendType.Only )
            {
                mobile = Engine.Mobiles.SelectEntities( m =>
                        MobileCommands.InFriendList( m.Serial ) && bodyTypePredicate( m.ID ) &&
                        ( !Options.CurrentOptions.GetFriendEnemyUsesIgnoreList ||
                          !ObjectCommands.IgnoreList.Contains( m.Serial ) ) && m.Serial != Engine.Player?.Serial &&
                        ( maxDistance == -1 || m.Distance <= maxDistance ) ).OrderBy( m => m.Distance )
                    .ByInflication( inflictionType ).FirstOrDefault();
            }
            else
            {
                mobile = Engine.Mobiles.SelectEntities( m =>
                        notoriety.Contains( m.Notoriety ) && m.Distance < MAX_DISTANCE && bodyTypePredicate( m.ID ) &&
                        ( friendType == TargetFriendType.Include || !MobileCommands.InFriendList( m.Serial ) ) &&
                        ( !Options.CurrentOptions.GetFriendEnemyUsesIgnoreList ||
                          !ObjectCommands.IgnoreList.Contains( m.Serial ) ) && m.Serial != Engine.Player?.Serial &&
                        ( maxDistance == -1 || m.Distance <= maxDistance ) ).OrderBy( m => m.Distance )
                    .ByInflication( inflictionType )?.FirstOrDefault();
            }

            return mobile;
        }

        public Mobile GetNextMobile( IEnumerable<Notoriety> notoriety, TargetBodyType bodyType = TargetBodyType.Any,
            int distance = MAX_DISTANCE, TargetFriendType friendType = TargetFriendType.Include,
            TargetInfliction inflictionType = TargetInfliction.Any, bool reverse = false, Mobile previousMobile = null )
        {
            bool looped = false;

            while ( true )
            {
                Mobile[] mobiles;

                Func<int, bool> bodyTypePredicate;

                switch ( bodyType )
                {
                    case TargetBodyType.Any:
                        bodyTypePredicate = i => true;
                        break;
                    case TargetBodyType.Humanoid:
                        bodyTypePredicate = i =>
                            BodyData.Where( bd => bd.BodyType == TargetBodyType.Humanoid ).Select( bd => bd.Graphic )
                                .Contains( i );
                        break;
                    case TargetBodyType.Transformation:
                        bodyTypePredicate = i =>
                            BodyData.Where( bd => bd.BodyType == TargetBodyType.Transformation )
                                .Select( bd => bd.Graphic ).Contains( i );
                        break;
                    case TargetBodyType.Both:
                        bodyTypePredicate = i =>
                            BodyData.Where( bd =>
                                    bd.BodyType == TargetBodyType.Humanoid ||
                                    bd.BodyType == TargetBodyType.Transformation )
                                .Select( bd => bd.Graphic ).Contains( i );
                        break;
                    default:
                        throw new ArgumentOutOfRangeException( nameof( bodyType ), bodyType, null );
                }

                if ( previousMobile != null )
                {
                    _ignoreList.Clear();
                }

                if ( friendType == TargetFriendType.Only )
                {
                    //Notoriety, bodyType ignored
                    mobiles = Engine.Mobiles.SelectEntities( m =>
                        m.Distance <= distance && MobileCommands.InFriendList( m.Serial ) &&
                        bodyTypePredicate( m.ID ) && !_ignoreList.Contains( m ) &&
                        ( !Options.CurrentOptions.GetFriendEnemyUsesIgnoreList ||
                          !ObjectCommands.IgnoreList.Contains( m.Serial ) ) && m.Serial != Engine.Player?.Serial &&
                        m.Distance <= distance );
                }
                else
                {
                    mobiles = Engine.Mobiles.SelectEntities( m =>
                        notoriety.Contains( m.Notoriety ) && m.Distance <= distance && bodyTypePredicate( m.ID ) &&
                        !_ignoreList.Contains( m ) &&
                        ( friendType == TargetFriendType.Include || !MobileCommands.InFriendList( m.Serial ) ) &&
                        ( !Options.CurrentOptions.GetFriendEnemyUsesIgnoreList ||
                          !ObjectCommands.IgnoreList.Contains( m.Serial ) ) && m.Serial != Engine.Player?.Serial );
                }

                if ( previousMobile != null )
                {
                    mobiles = mobiles.Where( m => m.Serial != previousMobile.Serial ).OrderBy( m =>
                        Math.Max( Math.Abs( m.X - previousMobile.X ), Math.Abs( m.Y - previousMobile.Y ) ) ).ToArray();

                    if ( mobiles.Length == 0 )
                    {
                        mobiles = new[] { previousMobile };
                    }
                }

                if ( reverse )
                {
                    mobiles = mobiles.Reverse().ToArray();
                }

                mobiles = mobiles.ByInflication( inflictionType );

                if ( mobiles == null || mobiles.Length == 0 )
                {
                    _ignoreList.Clear();

                    if ( looped )
                    {
                        return null;
                    }

                    looped = true;
                    continue;
                }

                Mobile mobile = mobiles.FirstOrDefault();
                _ignoreList.Add( mobile );
                return mobile;
            }
        }

        public Entity PromptTarget()
        {
            int serial = UOC.GetTargetSerialAsync( Strings.Target_object___ ).Result;

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id, true );
                return null;
            }

            Entity entity = UOMath.IsMobile( serial )
                ? (Entity) Engine.Mobiles.GetMobile( serial )
                : Engine.Items.GetItem( serial );

            if ( entity != null )
            {
                return entity;
            }

            UOC.SystemMessage( UOMath.IsMobile( serial ) ? Strings.Mobile_not_found___ : Strings.Cannot_find_item___,
                true );

            return null;
        }

        public static TargetManager GetInstance()
        {
            // ReSharper disable once InvertIf
            if ( _instance == null )
            {
                lock ( _lock )
                {
                    if ( _instance != null )
                    {
                        return _instance;
                    }

                    _instance = new TargetManager();
                    return _instance;
                }
            }

            return _instance;
        }

        public bool GetEnemy( TargetNotoriety notoFlags, TargetBodyType bodyType, TargetDistance targetDistance,
            TargetFriendType friendType = TargetFriendType.None, TargetInfliction inflictionType = TargetInfliction.Any,
            int maxDistance = -1 )
        {
            Mobile m = GetMobile( notoFlags, bodyType, targetDistance, friendType, inflictionType,
                targetDistance == TargetDistance.Nearest ? AliasCommands.GetAlias( "enemy" ) : 0, maxDistance );

            if ( m == null )
            {
                return false;
            }

            SetEnemy( m );
            return true;
        }

        public bool GetFriend( TargetNotoriety notoFlags, TargetBodyType bodyType, TargetDistance targetDistance,
            TargetFriendType friendType = TargetFriendType.Include,
            TargetInfliction inflictionType = TargetInfliction.Any, int maxDistance = -1 )
        {
            Mobile m = GetMobile( notoFlags, bodyType, targetDistance, friendType, inflictionType,
                targetDistance == TargetDistance.Nearest ? AliasCommands.GetAlias( "friend" ) : 0, maxDistance );

            if ( m == null )
            {
                return false;
            }

            SetFriend( m );
            return true;
        }

        public Mobile GetMobile( TargetNotoriety notoFlags, TargetBodyType bodyType, TargetDistance targetDistance,
            TargetFriendType friendType, TargetInfliction inflictionType, int previousSerial = 0, int maxDistance = -1 )
        {
            Notoriety[] noto = NotoFlagsToArray( notoFlags );

            Mobile m;

            if ( maxDistance == -1 )
            {
                maxDistance = MAX_DISTANCE;
            }

            switch ( targetDistance )
            {
                case TargetDistance.Next:

                    m = GetNextMobile( noto, bodyType, maxDistance, friendType, inflictionType );

                    break;
                case TargetDistance.Nearest:

                    Mobile previousMobile = Engine.Mobiles.GetMobile( previousSerial );

                    if ( previousMobile == null || previousMobile.Distance > MAX_DISTANCE )
                    {
                        m = GetClosestMobile( noto, bodyType, friendType, inflictionType, maxDistance );
                    }
                    else
                    {
                        m = GetNextMobile( noto, bodyType, maxDistance, friendType, inflictionType, false,
                            previousMobile );
                    }

                    break;
                case TargetDistance.Closest:

                    m = GetClosestMobile( noto, bodyType, friendType, inflictionType, maxDistance );

                    break;
                case TargetDistance.Previous:

                    m = GetNextMobile( noto, bodyType, maxDistance, friendType, inflictionType, true );

                    break;
                default:
                    throw new ArgumentOutOfRangeException( nameof( targetDistance ), targetDistance, null );
            }

            return m;
        }

        private static Notoriety[] NotoFlagsToArray( TargetNotoriety notoFlags )
        {
            List<Notoriety> notos = new List<Notoriety>();

            if ( notoFlags.HasFlag( TargetNotoriety.Criminal ) || notoFlags.HasFlag( TargetNotoriety.Any ) )
            {
                notos.Add( Notoriety.Criminal );
            }

            if ( notoFlags.HasFlag( TargetNotoriety.Enemy ) || notoFlags.HasFlag( TargetNotoriety.Any ) )
            {
                notos.Add( Notoriety.Enemy );
            }

            if ( notoFlags.HasFlag( TargetNotoriety.Gray ) || notoFlags.HasFlag( TargetNotoriety.Any ) )
            {
                notos.Add( Notoriety.Attackable );
            }

            if ( notoFlags.HasFlag( TargetNotoriety.Innocent ) || notoFlags.HasFlag( TargetNotoriety.Any ) )
            {
                notos.Add( Notoriety.Innocent );
            }

            if ( notoFlags.HasFlag( TargetNotoriety.Murderer ) || notoFlags.HasFlag( TargetNotoriety.Any ) )
            {
                notos.Add( Notoriety.Murderer );
            }

            if ( notoFlags.HasFlag( TargetNotoriety.Friend ) || notoFlags.HasFlag( TargetNotoriety.Any ) )
            {
                notos.Add( Notoriety.Ally );
            }

            return notos.ToArray();
        }
    }

    public static class MobileEnumerableExtensionMethods
    {
        public static Mobile[] ByInflication( this IEnumerable<Mobile> mobiles, TargetInfliction inflictionType )
        {
            switch ( inflictionType )
            {
                case TargetInfliction.Any:
                    return mobiles.ToArray();
                case TargetInfliction.Lowest:
                    return mobiles.Where( m => m.Hits < m.HitsMax && !m.IsDead ).OrderBy( m => m.Hits ).ToArray();
                case TargetInfliction.Poisoned:
                    return mobiles.Where( m => m.IsPoisoned ).ToArray();
                case TargetInfliction.Mortaled:
                    return mobiles.Where( m => m.IsYellowHits ).ToArray();
                case TargetInfliction.Paralyzed:
                    return mobiles.Where( m => m.IsFrozen ).ToArray();
                case TargetInfliction.Dead:
                    return mobiles.Where( m => m.IsDead ).ToArray();
                case TargetInfliction.Unmounted:
                    return mobiles.Where( m => m.GetLayer( Layer.Mount ) == 0 ).ToArray();
                default:
                    throw new ArgumentOutOfRangeException( nameof( inflictionType ), inflictionType, null );
            }
        }
    }
}
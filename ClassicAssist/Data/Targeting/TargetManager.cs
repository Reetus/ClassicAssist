using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assistant;
using ClassicAssist.Data.Macros;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Resources;
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
        private const int MAX_DISTANCE = 18;
        private static TargetManager _instance;
        private static readonly object _lock = new object();
        private static readonly List<Mobile> _ignoreList = new List<Mobile>();
        private readonly List<TargetBodyData> _bodyData;

        private TargetManager()
        {
            string dataPath = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Data" );

            _bodyData = JsonConvert
                .DeserializeObject<TargetBodyData[]>( File.ReadAllText( Path.Combine( dataPath, "Bodies.json" ) ) )
                .ToList();
        }

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
            Engine.Player.LastTargetSerial = m.Serial;
            Engine.Player.FriendTargetSerial = m.Serial;
            Engine.SendPacketToClient( new ChangeCombatant( m.Serial ) );
        }

        public void SetLastTarget( Entity m )
        {
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
            TargetInfliction inflictionType = TargetInfliction.Any )
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
                        _bodyData.Where( bd => bd.BodyType == TargetBodyType.Humanoid )
                            .Select( bd => bd.Graphic ).Contains( i );
                    break;
                case TargetBodyType.Transformation:
                    bodyTypePredicate = i =>
                        _bodyData.Where( bd => bd.BodyType == TargetBodyType.Transformation )
                            .Select( bd => bd.Graphic ).Contains( i );
                    break;
                case TargetBodyType.Both:
                    bodyTypePredicate = i =>
                        _bodyData.Where( bd =>
                                bd.BodyType == TargetBodyType.Humanoid ||
                                bd.BodyType == TargetBodyType.Transformation )
                            .Select( bd => bd.Graphic ).Contains( i );
                    break;
                default:
                    throw new ArgumentOutOfRangeException( nameof( bodyType ), bodyType, null );
            }

            if ( friendType == TargetFriendType.Only )
            {
                mobile = Engine.Mobiles
                    .SelectEntities( m => MobileCommands.InFriendList( m.Serial ) && bodyTypePredicate( m.ID ) &&
                                          ( !Options.CurrentOptions.GetFriendEnemyUsesIgnoreList ||
                                            !ObjectCommands.IgnoreList.Contains( m.Serial ) ) )
                    .OrderBy( m => m.Distance )
                    .ByInflication( inflictionType ).FirstOrDefault();
            }
            else
            {
                mobile = Engine.Mobiles.SelectEntities( m =>
                        notoriety.Contains( m.Notoriety ) && m.Distance < MAX_DISTANCE &&
                        bodyTypePredicate( m.ID ) &&
                        ( friendType == TargetFriendType.Include || !MobileCommands.InFriendList( m.Serial ) ) &&
                        ( !Options.CurrentOptions.GetFriendEnemyUsesIgnoreList ||
                          !ObjectCommands.IgnoreList.Contains( m.Serial ) ) )
                    .OrderBy( m => m.Distance ).ByInflication( inflictionType )?
                    .FirstOrDefault();
            }

            return mobile;
        }

        public Mobile GetNextMobile( IEnumerable<Notoriety> notoriety, TargetBodyType bodyType = TargetBodyType.Any,
            int distance = MAX_DISTANCE, TargetFriendType friendType = TargetFriendType.Include,
            TargetInfliction inflictionType = TargetInfliction.Any )
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
                            _bodyData.Where( bd => bd.BodyType == TargetBodyType.Humanoid )
                                .Select( bd => bd.Graphic ).Contains( i );
                        break;
                    case TargetBodyType.Transformation:
                        bodyTypePredicate = i =>
                            _bodyData.Where( bd => bd.BodyType == TargetBodyType.Transformation )
                                .Select( bd => bd.Graphic ).Contains( i );
                        break;
                    case TargetBodyType.Both:
                        bodyTypePredicate = i =>
                            _bodyData.Where( bd =>
                                    bd.BodyType == TargetBodyType.Humanoid ||
                                    bd.BodyType == TargetBodyType.Transformation )
                                .Select( bd => bd.Graphic ).Contains( i );
                        break;
                    default:
                        throw new ArgumentOutOfRangeException( nameof( bodyType ), bodyType, null );
                }

                if ( friendType == TargetFriendType.Only )
                {
                    //Notoriety, bodyType ignored
                    mobiles = Engine.Mobiles.SelectEntities( m =>
                        m.Distance < distance && MobileCommands.InFriendList( m.Serial ) && bodyTypePredicate( m.ID ) &&
                        !_ignoreList.Contains( m ) && ( !Options.CurrentOptions.GetFriendEnemyUsesIgnoreList ||
                                                        !ObjectCommands.IgnoreList.Contains( m.Serial ) ) );
                }
                else
                {
                    mobiles = Engine.Mobiles.SelectEntities( m =>
                        notoriety.Contains( m.Notoriety ) && m.Distance < distance &&
                        bodyTypePredicate( m.ID ) && !_ignoreList.Contains( m ) &&
                        ( friendType == TargetFriendType.Include || !MobileCommands.InFriendList( m.Serial ) ) &&
                        ( !Options.CurrentOptions.GetFriendEnemyUsesIgnoreList ||
                          !ObjectCommands.IgnoreList.Contains( m.Serial ) ) );
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
            int serial = UOC.GetTargeSerialAsync( Strings.Target_object___ ).Result;

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return null;
            }

            Mobile mobile = Engine.Mobiles.GetMobile( serial );

            if ( mobile != null )
            {
                return mobile;
            }

            UOC.SystemMessage( Strings.Mobile_not_found___ );
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
            TargetFriendType friendType = TargetFriendType.None,
            TargetInfliction inflictionType = TargetInfliction.Any )
        {
            Mobile m = GetMobile( notoFlags, bodyType, targetDistance, friendType, inflictionType );

            if ( m == null )
            {
                return false;
            }

            SetEnemy( m );
            return true;
        }

        public bool GetFriend( TargetNotoriety notoFlags, TargetBodyType bodyType, TargetDistance targetDistance,
            TargetFriendType friendType = TargetFriendType.Include,
            TargetInfliction inflictionType = TargetInfliction.Any )
        {
            Mobile m = GetMobile( notoFlags, bodyType, targetDistance, friendType, inflictionType );

            if ( m == null )
            {
                return false;
            }

            SetFriend( m );
            return true;
        }

        public Mobile GetMobile( TargetNotoriety notoFlags, TargetBodyType bodyType, TargetDistance targetDistance,
            TargetFriendType friendType, TargetInfliction inflictionType )
        {
            Notoriety[] noto = NotoFlagsToArray( notoFlags );

            Mobile m;

            switch ( targetDistance )
            {
                case TargetDistance.Next:

                    m = GetNextMobile( noto, bodyType, MAX_DISTANCE, friendType, inflictionType );

                    break;
                case TargetDistance.Nearest:

                    m = GetNextMobile( noto, bodyType, 3, friendType, inflictionType );

                    break;
                case TargetDistance.Closest:

                    m = GetClosestMobile( noto, bodyType, friendType, inflictionType );

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
        public static Mobile[] ByInflication( this IEnumerable<Mobile> mobiles,
            TargetInfliction inflictionType )
        {
            switch ( inflictionType )
            {
                case TargetInfliction.Any:
                    return mobiles.ToArray();
                case TargetInfliction.Lowest:
                    return mobiles.Where( m => m.Hits < m.HitsMax && !m.IsDead).OrderBy( m => m.Hits ).ToArray();
                case TargetInfliction.Poisoned:
                    return mobiles.Where( m => m.IsPoisoned ).ToArray();
                case TargetInfliction.Mortaled:
                    return mobiles.Where( m => m.IsYellowHits ).ToArray();
                case TargetInfliction.Paralyzed:
                    return mobiles.Where( m => m.IsFrozen ).ToArray();
                case TargetInfliction.Dead:
                    return mobiles.Where( m => m.IsDead ).ToArray();
                default:
                    throw new ArgumentOutOfRangeException( nameof( inflictionType ), inflictionType, null );
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assistant;
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

            MsgCommands.HeadMsg( "[Target]", m.Serial );
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

            MsgCommands.HeadMsg( "[Target]", m.Serial );
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

            MsgCommands.HeadMsg( "[Target]", m.Serial );
            MsgCommands.HeadMsg( $"Target: {m.Name?.Trim() ?? "Unknown"}" );
            Engine.SendPacketToClient( new ChangeCombatant( m.Serial ) );
        }

        public Mobile GetClosestMobile( IEnumerable<Notoriety> notoriety, TargetBodyType bodyType = TargetBodyType.Any )
        {
            Mobile mobile = Engine.Mobiles.SelectEntities( m =>
                    notoriety.Contains( m.Notoriety ) && m.Distance < MAX_DISTANCE &&
                    ( bodyType == TargetBodyType.Any || _bodyData.Where( bd => bd.BodyType == bodyType )
                          .Select( bd => bd.Graphic ).Contains( m.ID ) ) && !_ignoreList.Contains( m ) )
                .OrderBy( m => m.Distance )
                .FirstOrDefault();

            return mobile;
        }

        public Mobile GetNextMobile( IEnumerable<Notoriety> notoriety, TargetBodyType bodyType = TargetBodyType.Any,
            int distance = MAX_DISTANCE )
        {
            bool looped = false;

            while ( true )
            {
                Mobile[] mobiles = Engine.Mobiles.SelectEntities( m =>
                    notoriety.Contains( m.Notoriety ) && m.Distance < distance &&
                    ( bodyType == TargetBodyType.Any || _bodyData.Where( bd => bd.BodyType == bodyType )
                          .Select( bd => bd.Graphic ).Contains( m.ID ) ) && !_ignoreList.Contains( m ) );

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

        public bool GetEnemy( TargetNotoriety notoFlags, TargetBodyType bodyType, TargetDistance targetDistance )
        {
            Mobile m = GetMobile( notoFlags, bodyType, targetDistance );

            if ( m == null )
            {
                return false;
            }

            SetEnemy( m );
            return true;
        }

        public bool GetFriend( TargetNotoriety notoFlags, TargetBodyType bodyType, TargetDistance targetDistance )
        {
            Mobile m = GetMobile( notoFlags, bodyType, targetDistance );

            if ( m == null )
            {
                return false;
            }

            SetFriend( m );
            return true;
        }

        public Mobile GetMobile( TargetNotoriety notoFlags, TargetBodyType bodyType, TargetDistance targetDistance )
        {
            Notoriety[] noto = NotoFlagsToArray( notoFlags );

            Mobile m;

            switch ( targetDistance )
            {
                case TargetDistance.Next:

                    m = GetNextMobile( noto, bodyType );

                    break;
                case TargetDistance.Nearest:

                    m = GetNextMobile( noto, bodyType, 3 );

                    break;
                case TargetDistance.Closest:

                    m = GetClosestMobile( noto, bodyType );

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
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assistant;
using ClassicAssist.Resources;
using ClassicAssist.UO.Network;
using Newtonsoft.Json;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.BuffIcons
{
    public class BuffIconManager
    {
        private static BuffIconManager _instance;
        private static readonly object _lock = new object();
        private readonly List<BuffIconData> _buffIconData;
        private bool[] _enabledIds;

        private BuffIconManager()
        {
            string dataPath = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Data" );

            _buffIconData = JsonConvert
                .DeserializeObject<BuffIconData[]>( File.ReadAllText( Path.Combine( dataPath, "BuffIcons.json" ) ) )
                .ToList();

            _enabledIds = new bool[short.MaxValue];

            IncomingPacketHandlers.BufficonEnabledDisabledEvent += SetID;
            Engine.PlayerInitializedEvent += p => Clear();
        }

        private void Clear()
        {
            _enabledIds = new bool[short.MaxValue];
        }

        private void SetID( int type, bool enabled )
        {
            _enabledIds[type] = enabled;
        }

        public bool BuffExists( string name )
        {
            name = name.ToLower();

            BuffIconData data = _buffIconData.FirstOrDefault( bd => bd.Name.ToLower().Equals( name ) );

            if ( data != null )
            {
                return _enabledIds[data.ID];
            }

            UOC.SystemMessage( Strings.Unknown_buff_name___ );
            return false;
        }

        public string[] GetEnabledNames()
        {
            List<int> enabled = new List<int>();

            for ( int i = 0; i < _enabledIds.Length; i++ )
            {
                if ( _enabledIds[i] )
                {
                    enabled.Add( i );
                }
            }

            return enabled.Select( GetDataByID ).Select( data => data?.Name ).ToArray();
        }

        public BuffIconData GetDataByName( string name )
        {
            return _buffIconData.FirstOrDefault( i => i.Name == name );
        }

        public BuffIconData GetDataByID( int id )
        {
            return _buffIconData.FirstOrDefault( i => i.ID == id );
        }

        public static BuffIconManager GetInstance()
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

                    _instance = new BuffIconManager();
                    return _instance;
                }
            }

            return _instance;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assistant;
using ClassicAssist.UO.Network.Packets;
using Newtonsoft.Json;

namespace ClassicAssist.Data.Spells
{
    public class SpellManager
    {
        private static readonly object _lock = new object();
        private static SpellManager _instance;
        private readonly List<SpellData> _spellData;

        private SpellManager()
        {
            string dataPath = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Data" );

            _spellData = JsonConvert
                .DeserializeObject<SpellData[]>( File.ReadAllText( Path.Combine( dataPath, "Spells.json" ) ) )
                .ToList();
        }

        public SpellData GetSpellData( int id )
        {
            SpellData sd = _spellData.FirstOrDefault( s => s.ID == id );

            return sd;
        }

        public SpellData GetSpellData( string name )
        {
            SpellData sd = _spellData.FirstOrDefault( s => s.Name.ToLower().Equals( name.ToLower() ) );

            return sd;
        }

        public SpellData[] GetSpellData()
        {
            return _spellData.ToArray();
        }

        public void CastSpell( string name )
        {
            SpellData sd = GetSpellData( name );

            if ( sd == null )
            {
                return;
            }

            if ( sd.Target )
            {
                Engine.WaitingForTarget = true;
            }

            CastSpell( sd.ID );
        }

        public void CastSpell( int id )
        {
            Engine.SendPacketToServer( new CastSpell( id ) );
        }

        public static SpellManager GetInstance()
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

                    _instance = new SpellManager();
                    return _instance;
                }
            }

            return _instance;
        }
    }
}
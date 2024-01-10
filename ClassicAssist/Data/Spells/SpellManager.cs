using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Assistant;
using ClassicAssist.Misc;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Network.Packets;
using Newtonsoft.Json;

namespace ClassicAssist.Data.Spells
{
    public class SpellManager
    {
        private static readonly object _lock = new object();
        private static SpellManager _instance;
        private readonly List<SpellData> _masteryData;
        private readonly List<SpellData> _spellData;

        private SpellManager()
        {
            string dataPath = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Data" );

            _spellData = JsonConvert
                .DeserializeObject<SpellData[]>( File.ReadAllText( Path.Combine( dataPath, "Spells.json" ) ) ).ToList();

            _masteryData = JsonConvert
                .DeserializeObject<SpellData[]>( File.ReadAllText( Path.Combine( dataPath, "Masteries.json" ) ) )
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

        public SpellData GetMasteryData( int id )
        {
            SpellData sd = _masteryData.FirstOrDefault( s => s.ID == id );

            return sd;
        }

        public SpellData GetMasteryData( string name )
        {
            SpellData sd = _masteryData.FirstOrDefault( s => s.Name.ToLower().Equals( name.ToLower() ) );

            return sd;
        }

        public SpellData[] GetSpellData()
        {
            return _spellData.ToArray();
        }

        public SpellData[] GetMasteryData()
        {
            return _masteryData.ToArray();
        }

        public void CastSpell( string name )
        {
            SpellData sd = GetSpellData( name );

            if ( sd != null )
            {
                CastSpell( sd );
                return;
            }

            SpellData md = GetMasteryData( name );

            if ( md == null )
            {
                return;
            }

            if ( md.Target )
            {
                Engine.WaitingForTarget = true;
            }

            CastSpell( md );
        }

        public void CastSpell( SpellData sd )
        {
            if ( sd.Target )
            {
                Engine.WaitingForTarget = true;
            }

            CastSpell( sd.ID );

            if ( sd.Timeout <= 0 || !sd.Target)
            {
                return;
            }

            Task.Run( async () =>
            {
                if ( !Options.CurrentOptions.UseExperimentalFizzleDetection )
                {
                    await Task.Delay( sd.Timeout + 100 );
                }
                else
                {
                    Task<bool> timeoutTask = Task.Run( async () =>
                    {
                        await Task.Delay( sd.Timeout + 100 );

                        return true;
                    } );

                    PacketWaitEntry[] entries = UO.Commands.GetFizzleWaitEntries();

                    IEnumerable<Task<bool>> tasks = entries.Select( e => e.Lock.ToTask() ).Append( timeoutTask );

                    try
                    {
                        await Task.WhenAny( tasks );
                    }
                    finally
                    {
                        Engine.PacketWaitEntries.RemoveRange( entries );
                    }
                }

                if ( Engine.WaitingForTarget )
                {
                    Engine.WaitingForTarget = false;
                }
            } );
        }

        public void CastSpell( int id )
        {
            Engine.LastSpellID = id;
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
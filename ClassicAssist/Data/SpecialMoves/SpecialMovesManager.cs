using System.Collections.Generic;
using System.Linq;
using Assistant;
using ClassicAssist.Data.Spells;
using ClassicAssist.Resources;
using ClassicAssist.UO.Network;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.SpecialMoves
{
    public class SpecialMovesManager
    {
        public delegate void dSpecialMovesChanged( string name, bool enabled );

        private static SpecialMovesManager _instance;
        private static readonly object _lock = new object();
        private bool[] _enabledIds = new bool[ushort.MaxValue];

        private SpecialMovesManager()
        {
            IncomingPacketHandlers.ToggleSpecialMoveEvent += SetID;
            Engine.PlayerInitializedEvent += p => Clear();
        }

        public event dSpecialMovesChanged SpecialMovesChanged;

        private void SetID( int spellid, bool enabled )
        {
            _enabledIds[spellid] = enabled;

            OnSpecialMovesChanged( spellid, enabled );
        }

        private void OnSpecialMovesChanged( int spellid, bool enabled )
        {
            SpellManager manager = SpellManager.GetInstance();

            SpellData spellData = manager.GetSpellData( spellid );

            SpecialMovesChanged?.Invoke( spellData?.Name ?? string.Empty, enabled );
        }

        private void Clear()
        {
            _enabledIds = new bool[ushort.MaxValue];
        }

        public bool SpecialMoveExists( string name )
        {
            name = name.ToLower();

            SpellData data = SpellManager.GetInstance().GetSpellData( name );

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

            SpellManager manager = SpellManager.GetInstance();

            return enabled.Select( manager.GetSpellData ).Select( data => data?.Name ).ToArray();
        }

        public static SpecialMovesManager GetInstance()
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

                    _instance = new SpecialMovesManager();

                    return _instance;
                }
            }

            return _instance;
        }
    }
}
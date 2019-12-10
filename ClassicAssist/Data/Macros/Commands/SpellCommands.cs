using System.Diagnostics;
using ClassicAssist.Data.Spells;
using ClassicAssist.Resources;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class SpellCommands
    {
        private static readonly SpellManager _manager = SpellManager.GetInstance();

        [CommandsDisplay( Category = "Spells", Description = "Cast the given named spell.",
            InsertText = "Cast(\"Recall\")" )]
        public static void Cast( string name )
        {
            _manager?.CastSpell( name );
        }

        [CommandsDisplay( Category = "Spells",
            Description = "Cast the given named spell and automatically target given object.",
            InsertText = "Cast(\"Recall\", \"runebook\")" )]
        public static bool Cast( string name, object obj )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return false;
            }

            SpellData sd = _manager.GetSpellData( name );

            if ( sd == null )
            {
                UOC.SystemMessage( Strings.Unknown_spell___ );
                return false;
            }

            // Debugging only
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //

            _manager.CastSpell( sd.ID );

            bool result = TargetCommands.WaitForTarget( sd.Timeout + 500 );

            if ( !result )
            {
                UOC.SystemMessage( Strings.Timeout___ );
                return false;
            }

            // Debugging only
            sw.Stop();
            UOC.SystemMessage( $"Target received in {sw.ElapsedMilliseconds}ms, Spell data timeout: {sd.Timeout}ms" );
            //

            TargetCommands.Target( serial );

            return true;
        }
    }
}
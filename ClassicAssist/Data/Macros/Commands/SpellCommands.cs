using System.Diagnostics;
using ClassicAssist.Data.Spells;
using ClassicAssist.Resources;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class SpellCommands
    {
        private static readonly SpellManager _manager = SpellManager.GetInstance();

        [CommandsDisplay( Category = nameof( Strings.Spells ) )]
        public static void Cast( string name )
        {
            _manager?.CastSpell( name );
        }

        [CommandsDisplay( Category = nameof( Strings.Spells ) )]
        public static bool Cast( string name, object obj )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return false;
            }

            SpellData sd = _manager.GetSpellData( name ) ?? _manager.GetMasteryData( name );

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

            int index = 0;
            bool result;

            if ( Options.CurrentOptions.UseExperimentalFizzleDetection )
            {
                ( index, result ) = UOC.WaitForTargetOrFizzle( sd.Timeout + 1000 );
            }
            else
            {
                result = TargetCommands.WaitForTarget( sd.Timeout + 500 );
            }

            if ( index == 0 && !result )
            {
                UOC.SystemMessage( Strings.Timeout___ );
                return false;
            }

            if ( !result )
            {
                return false;
            }

            // Debugging only
            sw.Stop();

            if ( Options.CurrentOptions.Debug )
            {
                UOC.SystemMessage(
                    $"Target received in {sw.ElapsedMilliseconds}ms, Spell data timeout: {sd.Timeout}ms" );
            }
            //

            TargetCommands.Target( serial );

            return true;
        }
    }
}
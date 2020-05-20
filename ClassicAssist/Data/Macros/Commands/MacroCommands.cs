using System.Linq;
using System.Threading.Tasks;
using ClassicAssist.Resources;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class MacroCommands
    {
        [CommandsDisplay( Category = nameof( Strings.Macros ) )]
        public static void PlayMacro( string name )
        {
            MacroManager manager = MacroManager.GetInstance();

            MacroEntry current = manager.GetCurrentMacro();

            if ( current != null && current.IsBackground )
            {
                UOC.SystemMessage( Strings.Cannot_PlayMacro_from_background_macro___ );
                return;
            }

            MacroEntry macro = manager.Items.FirstOrDefault( m => m.Name == name );

            if ( macro == null )
            {
                UOC.SystemMessage( Strings.Unknown_macro___ );
                return;
            }

            Task.Run( () => macro.Action( macro ) );
        }

        [CommandsDisplay( Category = nameof( Strings.Macros ) )]
        public static void Stop()
        {
            MacroManager.GetInstance().StopAll();
        }

        [CommandsDisplay( Category = nameof( Strings.Macros ) )]
        public static void Replay()
        {
            MacroManager manager = MacroManager.GetInstance();

            MacroEntry current = manager.GetCurrentMacro();

            manager.Replay = true;

            Task.Run( () => current.Action( current ) );
        }
    }
}
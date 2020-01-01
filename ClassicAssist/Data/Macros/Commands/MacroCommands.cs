using System.Linq;
using System.Threading.Tasks;
using ClassicAssist.Resources;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class MacroCommands
    {
        [CommandsDisplay( Category = "Macros", Description = "Plays the given macro name.",
            InsertText = "PlayMacro(\"beep\")" )]
        public static void PlayMacro( string name )
        {
            MacroManager manager = MacroManager.GetInstance();

            MacroEntry macro = manager.Items.FirstOrDefault( m => m.Name == name );

            if ( macro == null )
            {
                UOC.SystemMessage( Strings.Unknown_macro___ );
                return;
            }

            Task.Run( () => { macro.Action( macro ); } );
        }

        [CommandsDisplay( Category = "Macros", Description = "Stops the current macro.", InsertText = "Stop()" )]
        public static void Stop()
        {
            MacroManager.GetInstance().Stop();
        }
    }
}
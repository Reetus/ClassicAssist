using System.Linq;
using System.Threading;
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

            macro.ActionSync( macro );

            Stop();
        }

        [CommandsDisplay( Category = "Macros", Description = "Stops the current macro.", InsertText = "Stop()" )]
        public static void Stop()
        {
            Thread.CurrentThread.Abort();
        }
    }
}
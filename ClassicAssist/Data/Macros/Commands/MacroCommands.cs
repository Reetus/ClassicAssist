using System.Linq;
using System.Threading.Tasks;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UO.Data;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class MacroCommands
    {
        [CommandsDisplay( Category = nameof( Strings.Macros ),
            Parameters = new[] { nameof( ParameterType.MacroName ) } )]
        public static void PlayMacro( string name )
        {
            MacroManager manager = MacroManager.GetInstance();

            MacroEntry macro = manager.Items.FirstOrDefault( m => m.Name == name );

            if ( macro == null )
            {
                UOC.SystemMessage( Strings.Unknown_macro___ );
                return;
            }

            Task.Run( () => macro.Action( macro ) );
        }

        [CommandsDisplay( Category = nameof( Strings.Macros ),
            Parameters = new[] { nameof( ParameterType.MacroName ) } )]
        public static void Stop( string name = null )
        {
            MacroManager.GetInstance().Stop( name );
        }

        [CommandsDisplay( Category = nameof( Strings.Macros ) )]
        public static void StopAll()
        {
            MacroManager.GetInstance().StopAll();
        }

        [CommandsDisplay( Category = nameof( Strings.Macros ) )]
        public static void Replay()
        {
            MacroManager manager = MacroManager.GetInstance();

            MacroEntry current = manager?.GetCurrentMacro();

            if ( current?.Action == null )
            {
                return;
            }

            manager.Replay = true;

            Task.Run( () => current.Action( current ) );
        }

        [CommandsDisplay( Category = nameof( Strings.Macros ),
            Parameters = new[] { nameof( ParameterType.MacroName ) } )]
        public static bool IsRunning( string name )
        {
            MacroManager manager = MacroManager.GetInstance();

            MacroEntry macro = manager.Items.FirstOrDefault( m => m.Name == name );

            if ( macro != null )
            {
                return macro.IsRunning;
            }

            UOC.SystemMessage( Strings.Unknown_macro___, (int) SystemMessageHues.Normal, true );
            return false;
        }

        [CommandsDisplay( Category = nameof( Strings.Macros ),
            Parameters = new[] { nameof( ParameterType.MacroName ) } )]
        public static void PlayCUOMacro( string name )
        {
            ClassicUO.Macros.PlayCUOMacro( name );
        }
    }
}
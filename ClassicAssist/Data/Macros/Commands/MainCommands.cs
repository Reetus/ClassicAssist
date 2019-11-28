using System.Threading;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class MainCommands
    {
        [CommandsDisplay(Category = "Main", Description = "Sends Resync request to server.")]
        public static void Resync()
        {
            UOC.Resync();
        }

        [CommandsDisplay(Category = "Main", Description = "Pauses execution for the given amount in milliseconds.")]
        public static void Pause( int milliseconds )
        {
            Thread.Sleep( milliseconds );
        }
    }
}
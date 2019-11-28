using System.Linq;
using ClassicAssist.Data.Organizer;
using ClassicAssist.Resources;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class OrganizerCommands
    {
        [CommandsDisplay(Category = "Agents", Description = "Returns true if currently running an organizer agent, or false if not.")]
        public static bool Organizing()
        {
            OrganizerManager manager = OrganizerManager.GetInstance();

            bool organizing = manager.Items.Any( ( o ) => o.IsRunning() );

            return organizing;
        }

        [CommandsDisplay(Category = "Agents", Description = "Executes the named Organizer agent.")]
        public static void Organizer( string name )
        {
            OrganizerManager manager = OrganizerManager.GetInstance();

            OrganizerEntry agent = manager.Items.FirstOrDefault( ( oa ) => oa.Name.ToLower().Equals( name.ToLower() ) );

            if ( agent == null )
            {
                UOC.SystemMessage( string.Format( Strings.Organizer___0___not_found___, name ) );
                return;
            }

            agent.Action.Invoke( agent );
        }
    }
}
using System.Linq;
using ClassicAssist.Data.Counters;
using ClassicAssist.Data.Dress;
using ClassicAssist.Resources;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class AgentCommands
    {
        [CommandsDisplay( Category = "Agents", Description = "Dress all items in the specified dress agent.",
            InsertText = "Dress(\"Dress-1\")" )]
        public static void Dress( string name )
        {
            DressManager manager = DressManager.GetInstance();

            DressAgentEntry dressAgentEntry = manager.Items.FirstOrDefault( dae => dae.Name == name );

            if ( dressAgentEntry == null )
            {
                UOC.SystemMessage( string.Format( Strings.Unknown_dress_agent___0___, name ) );
                return;
            }

            dressAgentEntry.Action( dressAgentEntry );
        }

        [CommandsDisplay( Category = "Agents", Description = "Undress all items in the specified dress agent.",
            InsertText = "Undress(\"Dress-1\")" )]
        public static void Undress( string name )
        {
            DressManager manager = DressManager.GetInstance();

            DressAgentEntry dressAgentEntry = manager.Items.FirstOrDefault( dae => dae.Name == name );

            if ( dressAgentEntry == null )
            {
                UOC.SystemMessage( string.Format( Strings.Unknown_dress_agent___0___, name ) );
                return;
            }

            dressAgentEntry.Undress();
        }

        [CommandsDisplay( Category = "Agents",
            Description = "Returns true if the Dress agent is currently dressing or undressing.",
            InsertText = "if Dressing():" )]
        public static bool Dressing()
        {
            DressManager manager = DressManager.GetInstance();

            return manager.IsDressing;
        }

        [CommandsDisplay( Category = "Agents", Description = "Returns the count of the given counter agent.",
            InsertText = "Counter(\"bm\")" )]
        public static int Counter( string name )
        {
            CountersManager manager = CountersManager.GetInstance();

            CountersAgentEntry entry = manager.Items.FirstOrDefault( cae => cae.Name.ToLower() == name.ToLower() );

            if ( entry != null )
            {
                return entry.Count;
            }

            UOC.SystemMessage( Strings.Invalid_counter_agent_name___ );
            return 0;
        }
    }
}
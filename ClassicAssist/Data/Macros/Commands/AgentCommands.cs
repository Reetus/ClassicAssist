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
        public static void Dress( string name = null )
        {
            DressManager manager = DressManager.GetInstance();

            DressAgentEntry dressAgentEntry;

            if ( string.IsNullOrEmpty( name ) )
            {
                if ( manager.TemporaryDress == null )
                {
                    UOC.SystemMessage( Strings.No_temporary_dress_layout_configured___ );
                    return;
                }

                dressAgentEntry = manager.TemporaryDress;
            }
            else
            {
                dressAgentEntry = manager.Items.FirstOrDefault( dae => dae.Name == name );
            }

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

        [CommandsDisplay( Category = "Agents",
            Description = "Adds all equipped items to a temporary list that isn't persisted on client close.",
            InsertText = "DressConfig()" )]
        public static void DressConfig()
        {
            DressManager manager = DressManager.GetInstance();
            manager.TemporaryDress = new DressAgentEntry();
            manager.TemporaryDress.Action = async hks => await manager.DressAllItems( manager.TemporaryDress, false );
            manager.ImportItems( manager.TemporaryDress );
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
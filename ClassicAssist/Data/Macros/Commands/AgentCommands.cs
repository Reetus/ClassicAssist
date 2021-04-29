using System.Linq;
using ClassicAssist.Data.Autoloot;
using ClassicAssist.Data.Counters;
using ClassicAssist.Data.Dress;
using ClassicAssist.Data.Organizer;
using ClassicAssist.Data.Vendors;
using ClassicAssist.Shared.Resources;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class AgentCommands
    {
        [CommandsDisplay( Category = nameof( Strings.Agents ),
            Parameters = new[] { nameof( ParameterType.AgentEntryName ) } )]
        public static void Dress( string name = null )
        {
            DressManager manager = DressManager.GetInstance();

            if ( manager.IsDressing )
            {
                UOC.SystemMessage( Strings.Dress_already_in_progress___, (int) UOC.SystemMessageHues.Red );
                return;
            }

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

        [CommandsDisplay( Category = nameof( Strings.Agents ),
            Parameters = new[] { nameof( ParameterType.AgentEntryName ) } )]
        public static void Undress( string name )
        {
            DressManager manager = DressManager.GetInstance();

            DressAgentEntry dressAgentEntry = manager.Items.FirstOrDefault( dae => dae.Name == name );

            if ( dressAgentEntry == null )
            {
                UOC.SystemMessage( string.Format( Strings.Unknown_dress_agent___0___, name ) );
                return;
            }

            dressAgentEntry.Undress().Wait();
        }

        [CommandsDisplay( Category = nameof( Strings.Agents ) )]
        public static bool Dressing()
        {
            DressManager manager = DressManager.GetInstance();

            return manager.IsDressing;
        }

        [CommandsDisplay( Category = nameof( Strings.Agents ) )]
        public static void DressConfig()
        {
            DressManager manager = DressManager.GetInstance();
            manager.TemporaryDress = new DressAgentEntry();
            manager.TemporaryDress.Action = async hks => await manager.DressAllItems( manager.TemporaryDress, false );
            manager.ImportItems( manager.TemporaryDress );
        }

        [CommandsDisplay( Category = nameof( Strings.Agents ),
            Parameters = new[] { nameof( ParameterType.AgentEntryName ) } )]
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

        [CommandsDisplay( Category = nameof( Strings.Agents ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static void SetAutolootContainer( object obj )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            AutolootHelpers.SetAutolootContainer?.Invoke( serial );
        }

        [CommandsDisplay( Category = nameof( Strings.Agents ),
            Parameters = new[]
            {
                nameof( ParameterType.AgentEntryName ), nameof( ParameterType.SerialOrAlias ),
                nameof( ParameterType.SerialOrAlias )
            } )]
        public static void SetOrganizerContainers( string entryName, object sourceContainer = null,
            object destinationContainer = null )
        {
            int sourceSerial = AliasCommands.ResolveSerial( sourceContainer, false );
            int destinationSerial = AliasCommands.ResolveSerial( destinationContainer, false );

            OrganizerManager manager = OrganizerManager.GetInstance();

            OrganizerEntry entry = manager.Items.FirstOrDefault( e => e.Name.ToLower().Equals( entryName.ToLower() ) );

            if ( entry == null )
            {
                UOC.SystemMessage( Strings.Invalid_organizer_agent_name___, true );
                return;
            }

            entry.SourceContainer = sourceSerial;
            entry.DestinationContainer = destinationSerial;

            UOC.SystemMessage( Strings.Organizer_containers_set___, true );
        }

        [CommandsDisplay( Category = nameof( Strings.Agents ),
            Parameters = new[] { nameof( ParameterType.ListName ), nameof( ParameterType.OnOff ) } )]
        public static void SetVendorBuyAutoBuy( string listName, string onOff = "toggle" )
        {
            VendorBuyManager manager = VendorBuyManager.GetInstance();

            VendorBuyAgentEntry entry =
                manager.Items.FirstOrDefault( e => e.Name.Trim().ToLower().Equals( listName.Trim().ToLower() ) );

            if ( entry == null )
            {
                UOC.SystemMessage( Strings.Invalid_VendorBuy_list_name___, (int) UOC.SystemMessageHues.Red );
                return;
            }

            switch ( onOff.Trim().ToLower() )
            {
                case "on":
                    entry.Enabled = true;
                    break;
                case "off":
                    entry.Enabled = false;
                    break;
                case "toggle":
                    entry.Enabled = !entry.Enabled;
                    break;
                default:
                    UOC.SystemMessage( Strings.Invalid_state_name___on____off___or__toggle____ );
                    break;
            }
        }
    }
}
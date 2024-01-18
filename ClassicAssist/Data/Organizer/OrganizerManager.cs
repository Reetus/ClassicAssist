using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Assistant;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Organizer
{
    public class OrganizerManager : SetPropertyNotifyChanged
    {
        private static readonly object _lock = new object();
        private static OrganizerManager _instance;

        private OrganizerManager()
        {
        }

        public Action<string> InvokeByName { get; set; }

        public bool IsOrganizing { get; set; }

        public ObservableCollectionEx<OrganizerEntry> Items { get; set; }

        private CancellationTokenSource _cancellationTokenSource { get; set; } = new CancellationTokenSource();

        public void Stop()
        {
            if ( IsOrganizing && !( _cancellationTokenSource?.IsCancellationRequested ?? false ) )
            {
                _cancellationTokenSource?.Cancel();
            }
        }

        internal async Task Organize( OrganizerEntry entry, ItemCollection sourceContainer, int sourceContainerSerial = 0, int destinationContainer = 0,
            CancellationToken token = default )
        {
            if ( IsOrganizing )
            {
                _cancellationTokenSource.Cancel();
                return;
            }

            Entity destinationContainerItem = (Entity) Engine.Items.GetItem( destinationContainer ) ?? Engine.Mobiles.GetMobile( destinationContainer );

            if ( destinationContainerItem == null )
            {
                //TODO
                UOC.SystemMessage( Strings.Cannot_find_container___ );
                return;
            }

            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                IsOrganizing = true;

                _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( token, _cancellationTokenSource.Token );

                UOC.SystemMessage( string.Format( Strings.Organizer__0__running___, entry.Name ) );

                foreach ( OrganizerItem entryItem in entry.Items )
                {
                    Item[] moveItems = sourceContainer?.SelectEntities( i =>
                        entryItem.ID == i.ID && ( entryItem.Hue == -1 || i.Hue == entryItem.Hue ) && !i.IsDescendantOf( destinationContainer, -1, sourceContainerSerial ) );

                    if ( moveItems == null )
                    {
                        continue;
                    }

                    bool limitAmount = entryItem.Amount != -1;
                    int moveAmount = entryItem.Amount;
                    int moved = 0;

                    if ( entry.Complete )
                    {
                        ItemCollection container = null;

                        switch ( destinationContainerItem )
                        {
                            case Item item:
                                container = item.Container;
                                break;
                            case Mobile mobile:
                                container = mobile.Backpack.Container;
                                break;
                        }

                        int existingCount = container?.SelectEntities( i => entryItem.ID == i.ID && ( entryItem.Hue == -1 || i.Hue == entryItem.Hue ) )?.Select( i => i.Count )
                            .Sum() ?? 0;

                        moved += existingCount;
                    }

                    foreach ( Item moveItem in moveItems )
                    {
                        if ( limitAmount && moved >= moveAmount )
                        {
                            break;
                        }

                        int amount = moveItem.Count;

                        if ( limitAmount )
                        {
                            if ( moveItem.Count > moveAmount )
                            {
                                amount = moveAmount - moved;
                            }

                            moved += amount;
                        }

                        if ( entry.Stack )
                        {
                            await ActionPacketQueue.EnqueueDragDrop( moveItem.Serial, amount, destinationContainerItem.Serial );
                        }
                        else
                        {
                            await ActionPacketQueue.EnqueueDragDrop( moveItem.Serial, amount, destinationContainerItem.Serial, x: 0, y: 0 );
                        }

                        if ( _cancellationTokenSource.IsCancellationRequested )
                        {
                            break;
                        }
                    }
                }
            }
            finally
            {
                UOC.SystemMessage( string.Format( Strings.Organizer__0__finished___, entry.Name ) );
                IsOrganizing = false;
            }
        }

        internal async Task Organize( OrganizerEntry entry, int sourceContainer = 0, int destinationContainer = 0 )
        {
            if ( IsOrganizing )
            {
                _cancellationTokenSource.Cancel();
                return;
            }

            if ( sourceContainer == 0 )
            {
                sourceContainer = entry.SourceContainer;
            }

            if ( destinationContainer == 0 )
            {
                destinationContainer = entry.DestinationContainer;
            }

            if ( sourceContainer == 0 || destinationContainer == 0 )
            {
                await SetContainers( entry );
                return;
            }

            Item sourceContainerItem = Engine.Items.GetItem( sourceContainer );

            if ( sourceContainerItem == null )
            {
                //TODO
                UOC.SystemMessage( Strings.Cannot_find_container___ );
                return;
            }

            PacketFilterInfo pfi = new PacketFilterInfo( 0x3C, new[] { PacketFilterConditions.IntAtPositionCondition( sourceContainerItem.Serial, 19 ) } );

            if ( UOC.WaitForIncomingPacket( pfi, 1000, () => Engine.SendPacketToServer( new UseObject( sourceContainerItem.Serial ) ) ) )
            {
                await Task.Delay( Options.CurrentOptions.ActionDelayMS );
            }

            if ( sourceContainerItem.Container == null )
            {
                //TODO
                UOC.SystemMessage( Strings.Cannot_find_container___ );
                return;
            }

            await Organize( entry, sourceContainerItem.Container, sourceContainer, destinationContainer );
        }

        public async Task SetContainers( object obj )
        {
            if ( !( obj is OrganizerEntry entry ) )
            {
                return;
            }

            int sourceContainer = await UOC.GetTargetSerialAsync( Strings.Select_source_container___ );

            if ( sourceContainer <= 0 )
            {
                UOC.SystemMessage( Strings.Invalid_source_container___ );
                return;
            }

            int desintationContainer = await UOC.GetTargetSerialAsync( Strings.Select_destination_container___ );

            if ( desintationContainer <= 0 )
            {
                UOC.SystemMessage( Strings.Invalid_destination_container___ );
                return;
            }

            entry.SourceContainer = sourceContainer;
            entry.DestinationContainer = desintationContainer;

            UOC.SystemMessage( Strings.Organizer_containers_set___ );
        }

        public static OrganizerManager GetInstance()
        {
            // ReSharper disable once InvertIf
            if ( _instance == null )
            {
                lock ( _lock )
                {
                    if ( _instance != null )
                    {
                        return _instance;
                    }

                    _instance = new OrganizerManager();
                    return _instance;
                }
            }

            return _instance;
        }
    }
}
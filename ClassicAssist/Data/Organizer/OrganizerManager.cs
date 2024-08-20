using System;
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
                    int itemDestinationContainer = destinationContainer;
                    Entity itemDestinationContainerItem = destinationContainerItem;

                    if ( entryItem.DestinationContainer.HasValue )
                    {
                        itemDestinationContainer = entryItem.DestinationContainer.Value;

                        itemDestinationContainerItem = (Entity) Engine.Items.GetItem( itemDestinationContainer ) ?? Engine.Mobiles.GetMobile( itemDestinationContainer );

                        if ( itemDestinationContainerItem == null )
                        {
                            UOC.SystemMessage( Strings.Invalid_container___ );

                            continue;
                        }
                    }

                    int itemSourceContainerSerial = sourceContainerSerial;
                    ItemCollection itemSourceContainer = sourceContainer;

                    if ( entryItem.SourceContainer.HasValue )
                    {
                        itemSourceContainer = Engine.Items.GetItem( entryItem.SourceContainer.Value )?.Container;

                        if ( itemSourceContainer == null )
                        {
                            UOC.SystemMessage( Strings.Invalid_container___ );

                            continue;
                        }
                    }

                    Item[] moveItems = itemSourceContainer?.SelectEntities( i =>
                        entryItem.ID == i.ID && ( entryItem.Hue == -1 || i.Hue == entryItem.Hue ) && !i.IsDescendantOf( itemDestinationContainer, -1, itemSourceContainerSerial ) );

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

                        switch ( itemDestinationContainerItem )
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
                            if ( moveItem.Count > moveAmount - moved )
                            {
                                amount = moveAmount - moved;
                            }

                            moved += amount;
                        }

                        if ( entry.Stack )
                        {
                            await ActionPacketQueue.EnqueueDragDrop( moveItem.Serial, amount, itemDestinationContainerItem.Serial, cancellationToken: token );
                        }
                        else
                        {
                            await ActionPacketQueue.EnqueueDragDrop( moveItem.Serial, amount, itemDestinationContainerItem.Serial, x: 0, y: 0, cancellationToken: token );
                        }

                        if ( _cancellationTokenSource.IsCancellationRequested )
                        {
                            break;
                        }
                    }
                }

                if ( entry.ReturnExcess )
                {
                    foreach ( OrganizerItem entryItem in entry.Items )
                    {
                        ItemCollection container = null;

                        int itemDestinationContainerSerial = destinationContainer;
                        Entity itemDestinationContainerItem = destinationContainerItem;

                        if ( entryItem.DestinationContainer.HasValue )
                        {
                            itemDestinationContainerSerial = entryItem.DestinationContainer.Value;

                            itemDestinationContainerItem = (Entity) Engine.Items.GetItem( entryItem.DestinationContainer.Value ) ??
                                                           Engine.Mobiles.GetMobile( entryItem.DestinationContainer.Value );

                            if ( itemDestinationContainerItem == null )
                            {
                                UOC.SystemMessage( Strings.Invalid_container___ );

                                continue;
                            }
                        }

                        int itemSourceContainerSerial = sourceContainerSerial;

                        if ( entryItem.SourceContainer.HasValue )
                        {
                            itemSourceContainerSerial = entryItem.SourceContainer.Value;
                        }

                        switch ( itemDestinationContainerItem )
                        {
                            case Item item:
                                container = item.Container;
                                break;
                            case Mobile mobile:
                                container = mobile.Backpack.Container;
                                break;
                        }

                        if ( container == null )
                        {
                            UOC.WaitForContainerContentsUse( container.Serial, 5000 );
                        }

                        int currentCount =
                            container?.SelectEntities( i => entryItem.ID == i.ID && ( entryItem.Hue == -1 || i.Hue == entryItem.Hue ) )?.Select( i => i.Count ).Sum() ?? 0;

                        if ( currentCount <= entryItem.Amount )
                        {
                            continue;
                        }

                        int diffAmount = currentCount - entryItem.Amount;

                        Item[] moveItems = container?.SelectEntities( i =>
                            entryItem.ID == i.ID && ( entryItem.Hue == -1 || i.Hue == entryItem.Hue ) &&
                            !i.IsDescendantOf( itemSourceContainerSerial, -1, itemDestinationContainerSerial ) );

                        if ( moveItems == null )
                        {
                            continue;
                        }

                        foreach ( Item moveItem in moveItems )
                        {
                            int amount = moveItem.Count;

                            if ( moveItem.Count > diffAmount )
                            {
                                amount = diffAmount;
                            }

                            await ActionPacketQueue.EnqueueDragDrop( moveItem.Serial, amount, itemSourceContainerSerial, cancellationToken: token );

                            diffAmount -= amount;

                            if ( diffAmount <= 0 )
                            {
                                break;
                            }
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
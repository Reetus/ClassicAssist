using System.Threading;
using System.Windows.Threading;
using Assistant;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UI.ViewModels;
using ClassicAssist.UI.Views;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    [HotkeyCommand( Name = "Grid Container Viewer" )]
    public class EntityCollectionViewerHotkey : HotkeyCommand
    {
        public override void Execute()
        {
            int serial = UOC.GetTargetSerialAsync( Strings.Target_container___ ).Result;

            if ( serial <= 0 )
            {
                return;
            }

            Entity entity = Engine.Items.GetItem( serial ) ?? (Entity) Engine.Mobiles.GetMobile( serial );

            if ( entity == null )
            {
                UOC.SystemMessage( Strings.Cannot_find_item___ );
                return;
            }

            ItemCollection collection = new ItemCollection( entity.Serial );

            switch ( entity )
            {
                case Item item:
                    if ( item.Container == null )
                    {
                        UOC.WaitForContainerContentsUse( item.Serial, 1000 );
                    }

                    collection = item.Container ?? new ItemCollection( item.Serial );
                    break;
                case Mobile mobile:
                    collection = new ItemCollection( entity.Serial ) { mobile.GetEquippedItems() };
                    break;
            }

            Engine.Dispatcher.Invoke( () =>
            {
                EntityCollectionViewer window = new EntityCollectionViewer
                {
                    DataContext = new EntityCollectionViewerViewModel( collection )
                };

                window.Show();
                window.Activate();
            } );
        }
    }
}
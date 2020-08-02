using System.Threading;
using System.Windows.Threading;
using Assistant;
using ClassicAssist.Resources;
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
            int serial = UOC.GetTargeSerialAsync( Strings.Target_container___ ).Result;

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

                    collection = item.Container;
                    break;
                case Mobile mobile:
                    collection = new ItemCollection( entity.Serial ) { mobile.GetEquippedItems() };
                    break;
            }

            Thread t = new Thread( () =>
            {
                EntityCollectionViewer window = new EntityCollectionViewer
                {
                    DataContext = new EntityCollectionViewerViewModel( collection )
                };

                window.Show();
                Dispatcher.Run();
            } ) { IsBackground = true };

            t.SetApartmentState( ApartmentState.STA );
            t.Start();
        }
    }
}
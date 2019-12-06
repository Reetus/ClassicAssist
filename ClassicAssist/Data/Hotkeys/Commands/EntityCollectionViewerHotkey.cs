using Assistant;
using ClassicAssist.Resources;
using ClassicAssist.UI.ViewModels;
using ClassicAssist.UI.Views;
using ClassicAssist.UO.Network.Packets;
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

            Item item = Engine.Items.GetItem( serial );

            if ( item.Container == null )
            {
                Engine.SendPacketToServer( new UseObject( item.Serial ) );

                UOC.WaitForContainerContents( item.Serial, 1000 );
            }

            Engine.Dispatcher.Invoke( () =>
            {
                EntityCollectionViewer window = new EntityCollectionViewer
                {
                    DataContext =
                        new EntityCollectionViewerViewModel( item.Container ?? new ItemCollection( item.Serial ) )
                };

                window.Show();
            } );
        }
    }
}
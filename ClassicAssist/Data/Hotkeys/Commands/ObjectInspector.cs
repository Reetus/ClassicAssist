using System.Linq;
using System.Threading;
using Assistant;
using ClassicAssist.Resources;
using ClassicAssist.UI.ViewModels;
using ClassicAssist.UI.Views;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    [HotkeyCommand( Name = "Object Inspector" )]
    public class ObjectInspector : HotkeyCommand
    {
        public override void Execute()
        {
            ( TargetType targetType, TargetFlags targetFlags, int serial, int x, int y, int z, int itemID ) =
                UO.Commands.GetTargeInfoAsync( Strings.Target_object___ ).Result;

            if ( targetType == TargetType.Object && serial != 0 )
            {
                Entity entity = UOMath.IsMobile( serial )
                    ? (Entity) Engine.Mobiles.GetMobile( serial )
                    : Engine.Items.GetItem( serial );

                if ( entity == null )
                {
                    return;
                }

                Thread t = new Thread( () =>
                {
                    ObjectInspectorWindow window =
                        new ObjectInspectorWindow { DataContext = new ObjectInspectorViewModel( entity ) };

                    window.ShowDialog();
                } ) { IsBackground = true };

                t.SetApartmentState( ApartmentState.STA );
                t.Start();
            }
            else
            {
                if ( itemID == 0 )
                {
                    return;
                }

                StaticTile[] statics = Statics.GetStatics( (int) Engine.Player.Map, x, y );

                if ( statics == null )
                {
                    return;
                }

                StaticTile selectedStatic = statics.FirstOrDefault( i => i.ID == itemID );

                if ( selectedStatic.ID == 0 )
                {
                    return;
                }

                Thread t = new Thread( () =>
                {
                    ObjectInspectorWindow window = new ObjectInspectorWindow
                    {
                        DataContext = new ObjectInspectorViewModel( selectedStatic )
                    };

                    window.ShowDialog();
                } ) { IsBackground = true };

                t.SetApartmentState( ApartmentState.STA );
                t.Start();
            }
        }
    }
}
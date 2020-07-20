using ClassicAssist.Shared;
using ClassicAssist.Misc;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    [HotkeyCommand( Name = "Interrupt Spell" )]
    public class InterruptSpellCommand : HotkeyCommand
    {
        public override void Execute()
        {
            PlayerMobile player = Engine.Player;

            if ( player == null )
            {
                return;
            }

            Layer[] layerPriority =
            {
                Layer.Shirt, Layer.Shoes, Layer.Pants, Layer.Helm, Layer.Gloves, Layer.Ring, Layer.Neck,
                Layer.Waist, Layer.InnerTorso, Layer.Bracelet, Layer.MiddleTorso, Layer.Earrings, Layer.Arms,
                Layer.Cloak, Layer.OuterTorso, Layer.OuterLegs, Layer.InnerLegs, Layer.TwoHanded, Layer.OneHanded
            };

            Layer selectedLayer = Layer.Invalid;
            int serial = 0;

            foreach ( Layer layer in layerPriority )
            {
                serial = player.GetLayer( layer );

                if ( serial == 0 )
                {
                    continue;
                }

                selectedLayer = layer;
                break;
            }

            if ( selectedLayer == Layer.Invalid )
            {
                return;
            }

            ActionPacketQueue.EnqueueActionPackets(
                new BasePacket[]
                {
                    new DragItem( serial, 1 ), new EquipRequest( serial, selectedLayer, player.Serial )
                }, QueuePriority.High );
        }
    }
}
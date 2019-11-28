using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Assistant;
using ClassicAssist.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class ActionCommands
    {
        [CommandsDisplay(Category = "Actions", Description = "Attack mobile (parameter can be serial or alias).")]
        public static void Attack( object obj )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return;
            }

            Engine.SendPacketToServer( new AttackRequest( serial ) );
        }

        [CommandsDisplay(Category = "Actions", Description = "Clear hands, \"left\", \"right\", or \"both\"")]
        public static void ClearHands( string hand )
        {
            hand = hand.ToLower();
            List<Layer> unequipLayers = new List<Layer>();

            switch ( hand )
            {
                case "left":
                    unequipLayers.Add( Layer.OneHanded );
                    break;
                case "right":
                    unequipLayers.Add( Layer.TwoHanded );
                    break;
                case "both":
                    unequipLayers.Add( Layer.OneHanded );
                    unequipLayers.Add( Layer.TwoHanded );
                    break;
                default:
                    throw new ArgumentOutOfRangeException( nameof( hand ) );
            }

            PlayerMobile player = Engine.Player;

            List<int> serials = unequipLayers.Select( unequipLayer => Engine.Player.GetLayer( unequipLayer ) ).Where( serial => serial != 0 ).ToList();

            foreach ( int serial in serials )
            {
                UOC.DragDropAsync( serial, 1, player.Backpack?.Serial ?? 0 ).Wait();
                Thread.Sleep( Options.CurrentOptions.ActionDelayMS );
            }
        }

        [CommandsDisplay(Category = "Actions", Description = "Single click object (parameter can be serial or alias).")]
        public static void ClickObject( object obj )
        {
            int serial = AliasCommands.ResolveSerial(obj);

            if (serial == 0)
            {
                UOC.SystemMessage(Strings.Invalid_or_unknown_object_id);
                return;
            }

            Engine.SendPacketToServer( new LookRequest( serial ) );
        }

        [CommandsDisplay(Category = "Actions", Description = "Move item to container (parameters can be serials or aliases).")]
        public static void MoveItem( object item, object destination, int amount = -1 )
        {
            int itemSerial = AliasCommands.ResolveSerial( item );

            if ( itemSerial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return;
            }

            Item itemObj = Engine.Items.GetItem( itemSerial );

            if (itemObj == null)
            {
                UOC.SystemMessage(Strings.Invalid_or_unknown_object_id);
                return;
            }

            if ( amount == -1 )
                amount = itemObj.Count;

            int containerSerial = AliasCommands.ResolveSerial( destination );

            if (containerSerial == 0)
            {
                //TODO
                UOC.SystemMessage(Strings.Invalid_or_unknown_object_id);
                return;
            }

            UOC.DragDropAsync( itemSerial, amount, containerSerial ).Wait();
        }

        [CommandsDisplay(Category = "Actions", Description = "Unmounts if mounted, or mounts if unmounted, will prompt for mount if no \"mount\" alias.")]
        public static void ToggleMounted()
        {
            PlayerMobile player = Engine.Player;

            if ( player == null )
                return;

            if ( player.IsMounted )
            {
                Engine.SendPacketToServer( new UseObject( player.Serial ) );
                return;
            }

            if ( !AliasCommands.FindAlias( "mount" ) )
            {
                int serial = UOC.GetTargeSerialAsync( Strings.Target_new_mount___, 10000 ).Result;

                if ( serial == -1 )
                {
                    UOC.SystemMessage( Strings.Invalid_mount___ );
                    return;
                }

                AliasCommands.SetAlias( "mount", serial );
            }

            int mountSerial = AliasCommands.GetAlias( "mount" );

            Engine.SendPacketToServer( new UseObject( mountSerial ) );
        }
    }
}
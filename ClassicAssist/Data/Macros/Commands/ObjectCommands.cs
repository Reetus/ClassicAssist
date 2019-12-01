using System.Collections.Generic;
using System.Linq;
using Assistant;
using ClassicAssist.Resources;
using ClassicAssist.UO;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class ObjectCommands
    {
        [CommandsDisplay( Category = "Actions",
            Description = "Sends use (doubleclick) request for given object (parameter can be serial or alias).",
            InsertText = "UseObject(\"mount\")" )]
        public static void UseObject( object obj )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial <= 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return;
            }

            Engine.SendPacketToServer( new UseObject( serial ) );
        }

        [CommandsDisplay( Category = "Actions",
            Description =
                "Sends use (doubleclick) request for given type, optional parameters of hue and container object (defaults to player backpack) (parameters can be serial or alias).",
            InsertText = "UseType(0xff)" )]
        public static void UseType( object type, int hue = -1, object container = null )
        {
            int serial = AliasCommands.ResolveSerial( type );

            if ( serial <= 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return;
            }

            if ( container == null )
            {
                container = Engine.Player?.Backpack?.Serial;
            }

            int containerSerial = AliasCommands.ResolveSerial( container );

            if ( !Engine.Items.GetItem( containerSerial, out Item containerItem ) )
            {
                UOC.SystemMessage( Strings.Cannot_find_container___ );
                return;
            }

            Item useItem = hue == -1
                ? containerItem.Container.SelectEntity( i => i.ID == serial )
                : containerItem.Container.SelectEntity( i => i.ID == serial && i.Hue == hue );

            if ( useItem == null )
            {
                UOC.SystemMessage( Strings.Cannot_find_item___ );
                return;
            }

            Engine.SendPacketToServer( new UseObject( useItem.Serial ) );
        }

        [CommandsDisplay( Category = "Entity", Description = "Amount comparison of item type inside a container.",
            InsertText = "CountType(0xff, \"backpack\")" )]
        public static int CountType( int graphic, int hue, int source )
        {
            Item countainerItem = Engine.Items.GetItem( source );

            if ( countainerItem?.Container == null )
            {
                UOC.SystemMessage( Strings.Invalid_container___ );
                return 0;
            }

            Item[] matches =
                countainerItem.Container.SelectEntities( i => i.ID == graphic && ( hue == -1 || i.Hue == hue ) );

            return matches?.Sum( i => i.Count ) ?? 0;
        }

        [CommandsDisplay( Category = "Entity",
            Description = "Amount comparison of item or mobile type on the ground.",
            InsertText = "if CountGround(0xff, 0, 10) < 1:" )]
        public static int CountTypeGround( int graphic, int hue, int range )
        {
            PlayerMobile player = Engine.Player;

            IEnumerable<Item> matches = Engine.Items.Where( i =>
                i.ID == graphic && ( hue == -1 || hue == i.ID ) &&
                UOMath.Distance( player.X, player.Y, i.X, i.Y ) <= range );

            int count = matches.Sum( match => match.Count );

            if ( count > 0 )
            {
                return count;
            }

            IEnumerable<Mobile> mobileMatches = Engine.Mobiles.Where( i =>
                i.ID == graphic && ( hue == -1 || hue == i.ID ) &&
                UOMath.Distance( player.X, player.Y, i.X, i.Y ) <= range );

            count += mobileMatches.Count();

            return count;
        }
    }
}
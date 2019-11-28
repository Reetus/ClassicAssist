using Assistant;
using ClassicAssist.Resources;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class ObjectCommands
    {
        [CommandsDisplay(Category = "Actions", Description = "Sends use (doubleclick) request for given object (parameter can be serial or alias).")]
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

        [CommandsDisplay(Category = "Actions", Description = "Sends use (doubleclick) request for given type, optional parameters of hue and container object (defaults to player backpack) (parameters can be serial or alias).")]
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
    }
}
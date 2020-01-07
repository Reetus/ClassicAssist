using System;
using System.Collections.Generic;
using System.Linq;
using Assistant;
using ClassicAssist.Resources;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class ObjectCommands
    {
        private static readonly List<int> _ignoreList = new List<int>();

        [CommandsDisplay( Category = "Entity", Description = "Ignores the given object from find commands",
            InsertText = "IgnoreObject(\"self\")" )]
        public static void IgnoreObject( object obj )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );

                return;
            }

            if ( !_ignoreList.Contains( serial ) )
            {
                _ignoreList.Add( serial );
            }
        }

        [CommandsDisplay( Category = "Entity", Description = "Clears the ignore list.",
            InsertText = "ClearIgnoreList()" )]
        public static void ClearIgnoreList()
        {
            _ignoreList.Clear();
        }

        [CommandsDisplay( Category = "Actions",
            Description = "Sends use (doubleclick) request for given object (parameter can be serial or alias).",
            InsertText = "UseObject(\"mount\")" )]
        public static void UseObject( object obj, bool skipQueue = false )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );

                return;
            }

            if ( !Options.CurrentOptions.UseObjectQueue || skipQueue )
            {
                Engine.SendPacketToServer( new UseObject( serial ) );
            }
            else if ( Engine.UseObjectQueue.Count < Options.CurrentOptions.UseObjectQueueAmount )
            {
                Engine.UseObjectQueue.Enqueue( serial );
            }
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
        public static int CountType( int graphic, object source = null, int hue = -1 )
        {
            if ( source == null )
            {
                source = "backpack";
            }

            int sourceSerial = AliasCommands.ResolveSerial( source );

            Item countainerItem = Engine.Items.GetItem( sourceSerial );

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
        public static int CountTypeGround( int graphic, int hue = -1, int range = -1 )
        {
            PlayerMobile player = Engine.Player;

            IEnumerable<Item> matches = Engine.Items.Where( i =>
                i.ID == graphic && ( hue == -1 || hue == i.ID ) &&
                ( range == -1 || i.Distance <= range ) );

            int count = matches.Sum( match => match.Count );

            if ( count > 0 )
            {
                return count;
            }

            IEnumerable<Mobile> mobileMatches = Engine.Mobiles.Where( i =>
                i.ID == graphic && ( hue == -1 || hue == i.ID ) &&
                ( range == -1 || i.Distance <= range ) );

            count += mobileMatches.Count();

            return count;
        }

        [CommandsDisplay( Category = "Entity",
            Description =
                "Searches for entity by graphic ID and sets found alias, defaults to ground if no source given.",
            InsertText = "FindType(0xff)\r\nUseObject(\"found\")" )]
        public static bool FindType( int graphic, int range = -1, object findLocation = null, int hue = -1 )
        {
            int owner = 0;

            if ( findLocation != null )
            {
                owner = AliasCommands.ResolveSerial( findLocation );
            }

            Entity entity;

            bool Predicate( Entity i )
            {
                return i.ID == graphic && ( hue == -1 || i.Hue == hue ) &&
                       ( range == -1 || i.Distance < range ) &&
                       !_ignoreList.Contains( i.Serial );
            }

            if ( owner != 0 )
            {
                entity = Engine.Items.SelectEntities( i => Predicate( i ) && i.IsDescendantOf( owner ) )
                    ?.FirstOrDefault();
            }
            else
            {
                entity =
                    (Entity) Engine.Mobiles.SelectEntities( Predicate )?.FirstOrDefault() ??
                    Engine.Items.SelectEntities( i => Predicate( i ) && i.Owner == 0 )?.FirstOrDefault();
            }

            if ( entity == null )
            {
                return false;
            }

            AliasCommands.SetAlias( "found", entity.Serial );
            UOC.SystemMessage( string.Format( Strings.Object___0___updated___, "found" ) );

            return true;
        }

        [CommandsDisplay( Category = "Entity",
            Description =
                "Searches for entity by serial and sets found alias, defaults to ground if no source given.",
            InsertText = "FindObject(\"mount\")\r\nUseObject(\"found\")" )]
        public static bool FindObject( object obj, int range = -1, object findLocation = null )
        {
            int owner = 0;

            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );

                return false;
            }

            if ( findLocation != null )
            {
                owner = AliasCommands.ResolveSerial( findLocation );
            }

            Entity entity;

            bool Predicate( Entity i )
            {
                return i.Serial == serial &&
                       ( range == -1 || i.Distance < range );
            }

            if ( owner != 0 )
            {
                entity = Engine.Items.SelectEntities( i => Predicate( i ) && i.IsDescendantOf( owner ) )
                    ?.FirstOrDefault();
            }
            else
            {
                entity =
                    (Entity) Engine.Mobiles.SelectEntities( Predicate )?.FirstOrDefault() ??
                    Engine.Items.SelectEntities( i => Predicate( i ) && i.Owner == 0 )?.FirstOrDefault();
            }

            if ( entity == null )
            {
                return false;
            }

            AliasCommands.SetAlias( "found", entity.Serial );
            UOC.SystemMessage( string.Format( Strings.Object___0___updated___, "found" ) );

            return true;
        }

        [CommandsDisplay( Category = "Entity",
            Description =
                "Move the given serial/alias to the specified x,y,z offset of the player, no amount specified or -1 will move the full stack.",
            InsertText = "MoveItemOffset(\"trashitem\", 0, 1, 0, -1\")" )]
        public static void MoveItemOffset( object obj, int xOffset, int yOffset, int zOffset, int amount = -1 )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );

                return;
            }

            PlayerMobile player = Engine.Player;

            if ( player == null )
            {
                return;
            }

            int x = player.X + xOffset;
            int y = player.Y + yOffset;
            int z = player.Z + zOffset;

            UOC.DragDropAsync( serial, amount, -1, x, y, z ).Wait();
        }

        [CommandsDisplay( Category = "Entity",
            Description =
                "Move the given type from the specified source container to the specified x,y,z offset of the player, no amount specified or -1 will move the full stack.",
            InsertText = "MoveTypeOffset(0xf0e, \"backpack\", 0, 1, 0, -1\")" )]
        public static bool MoveTypeOffset( int id, object findLocation, int xOffset, int yOffset, int zOffset,
            int amount = -1 )
        {
            int owner = 0;

            if ( findLocation == null ||
                 ( (string) findLocation ).Equals( "ground", StringComparison.InvariantCultureIgnoreCase ) )
            {
                UOC.SystemMessage( Strings.Invalid_container___ );
                return false;
            }

            owner = AliasCommands.ResolveSerial( findLocation );

            bool Predicate( Item i )
            {
                return i.ID == id && i.IsDescendantOf( owner );
            }

            Item entity = Engine.Items.SelectEntities( Predicate ).FirstOrDefault();

            if ( entity == null )
            {
                return false;
            }

            PlayerMobile player = Engine.Player;

            if ( player == null )
            {
                return false;
            }

            int x = player.X + xOffset;
            int y = player.Y + yOffset;
            int z = player.Z + zOffset;

            UOC.DragDropAsync( entity.Serial, amount, -1, x, y, z ).Wait();

            return true;
        }
    }
}
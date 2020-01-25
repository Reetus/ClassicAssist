using Assistant;
using ClassicAssist.Data.BuffIcons;
using ClassicAssist.Data.SpecialMoves;
using ClassicAssist.Resources;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class EntityCommands
    {
        [CommandsDisplay( Category = "Entity", Description = "Returns the distance to the given entity.",
            InsertText = "if Distance(\"mount\") < 4:" )]
        public static int Distance( object obj = null )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return 0;
            }

            Entity entity = Engine.Items.GetItem( serial ) ?? (Entity) Engine.Mobiles.GetMobile( serial );

            return entity?.Distance ?? int.MaxValue;
        }

        [CommandsDisplay( Category = "Entity",
            Description = "Check for range between your character and another mobile or an item",
            InsertText = "if InRange(\"enemy\", 10):" )]
        public static bool InRange( object obj, int distance )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return false;
            }

            Entity entity = Engine.Items.GetItem( serial ) ?? (Entity) Engine.Mobiles.GetMobile( serial );

            if ( entity != null )
            {
                return entity.Distance < distance;
            }

            UOC.SystemMessage( Strings.Cannot_find_item___ );
            return false;
        }

        [CommandsDisplay( Category = "Entity",
            Description = "Returns Hue of given object (parameter can be serial or alias).",
            InsertText = "if Hue(\"mount\") == 0:" )]
        public static int Hue( object obj = null )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return 0;
            }

            Entity entity = UOMath.IsMobile( serial )
                ? Engine.Mobiles.GetMobile( serial )
                : Engine.Items.GetItem( serial ) as Entity;

            if ( entity != null )
            {
                return entity.Hue;
            }

            if ( !MacroManager.QuietMode )
            {
                UOC.SystemMessage( Strings.Entity_not_found___ );
            }

            return 0;
        }

        [CommandsDisplay( Category = "Entity",
            Description = "Returns Item ID of given object (parameter can be serial or alias).",
            InsertText = "Graphic(\"mount\")" )]
        public static int Graphic( object obj = null )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return 0;
            }

            Entity entity = UOMath.IsMobile( serial )
                ? Engine.Mobiles.GetMobile( serial )
                : Engine.Items.GetItem( serial ) as Entity;

            if ( entity != null )
            {
                return entity.ID;
            }

            if ( !MacroManager.QuietMode )
            {
                UOC.SystemMessage( Strings.Entity_not_found___ );
            }

            return 0;
        }

        [CommandsDisplay( Category = "Entity",
            Description = "Returns X coordinate of given object (parameter can be serial or alias).",
            InsertText = "x = X(\"self\")" )]
        public static int X( object obj = null )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return 0;
            }

            Entity entity = UOMath.IsMobile( serial )
                ? Engine.Mobiles.GetMobile( serial )
                : Engine.Items.GetItem( serial ) as Entity;

            if ( entity != null )
            {
                return entity.X;
            }

            if ( !MacroManager.QuietMode )
            {
                UOC.SystemMessage( Strings.Entity_not_found___ );
            }

            return 0;
        }

        [CommandsDisplay( Category = "Entity",
            Description = "Returns Y coordinate of given object (parameter can be serial or alias).",
            InsertText = "y = Y(\"self\")" )]
        public static int Y( object obj = null )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return 0;
            }

            Entity entity = UOMath.IsMobile( serial )
                ? Engine.Mobiles.GetMobile( serial )
                : Engine.Items.GetItem( serial ) as Entity;

            if ( entity != null )
            {
                return entity.Y;
            }

            if ( !MacroManager.QuietMode )
            {
                UOC.SystemMessage( Strings.Entity_not_found___ );
            }

            return 0;
        }

        [CommandsDisplay( Category = "Entity",
            Description = "Returns Z coordinate of given object (parameter can be serial or alias).",
            InsertText = "y = Y(\"self\")" )]
        public static int Z( object obj = null )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return 0;
            }

            Entity entity = UOMath.IsMobile( serial )
                ? Engine.Mobiles.GetMobile( serial )
                : Engine.Items.GetItem( serial ) as Entity;

            if ( entity != null )
            {
                return entity.Z;
            }

            if ( !MacroManager.QuietMode )
            {
                UOC.SystemMessage( Strings.Entity_not_found___ );
            }

            return 0;
        }

        [CommandsDisplay( Category = "Entity", Description = "Check for a specific buff",
            InsertText = "if BuffExists(\"Blood Oath\"):" )]
        public static bool BuffExists( string name )
        {
            return BuffIconManager.GetInstance().BuffExists( name );
        }

        [CommandsDisplay( Category = "Entity", Description = "Check for a specific special move",
            InsertText = "if SpecialMoveExists(\"Death Strike\"):" )]
        public static bool SpecialMoveExists( string name )
        {
            return SpecialMovesManager.GetInstance().SpecialMoveExists( name );
        }

        [CommandsDisplay( Category = "Entity",
            Description = "Returns the Direction the entity is in relative to the player.",
            InsertText = "Run(DirectionTo(\"enemy\"))" )]
        public static string DirectionTo( object obj )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                return Direction.Invalid.ToString();
            }

            Entity entity = UOMath.IsMobile( serial )
                ? Engine.Mobiles.GetMobile( serial )
                : Engine.Items.GetItem( serial ) as Entity;

            if ( entity == null )
            {
                return Direction.Invalid.ToString();
            }

            return UOMath.MapDirection( Engine.Player.X, Engine.Player.Y, entity.X, entity.Y ).ToString();
        }
    }
}
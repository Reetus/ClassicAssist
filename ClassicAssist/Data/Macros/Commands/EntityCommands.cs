using Assistant;
using ClassicAssist.Resources;
using ClassicAssist.UO;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class EntityCommands
    {
        [CommandsDisplay( Category = "Entity",
            Description = "Returns Hue of given object (parameter can be serial or alias).",
            InsertText = "if Hue(\"mount\") == 0:" )]
        public static int Hue( object obj = null )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial <= 0 )
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

            UOC.SystemMessage( Strings.Entity_not_found___ );
            return 0;
        }

        [CommandsDisplay( Category = "Entity",
            Description = "Returns Item ID of given object (parameter can be serial or alias).",
            InsertText = "Graphic(\"mount\")" )]
        public static int Graphic( object obj = null )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial <= 0 )
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

            UOC.SystemMessage( Strings.Entity_not_found___ );
            return 0;
        }

        [CommandsDisplay( Category = "Entity",
            Description = "Returns X coordinate of given object (parameter can be serial or alias).",
            InsertText = "x = X(\"self\")" )]
        public static int X( object obj = null )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial <= 0 )
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

            UOC.SystemMessage( Strings.Entity_not_found___ );
            return 0;
        }

        [CommandsDisplay( Category = "Entity",
            Description = "Returns Y coordinate of given object (parameter can be serial or alias).",
            InsertText = "y = Y(\"self\")" )]
        public static int Y( object obj = null )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial <= 0 )
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

            UOC.SystemMessage( Strings.Entity_not_found___ );
            return 0;
        }

        [CommandsDisplay( Category = "Entity",
            Description = "Returns Z coordinate of given object (parameter can be serial or alias).",
            InsertText = "y = Y(\"self\")" )]
        public static int Z( object obj = null )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial <= 0 )
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

            UOC.SystemMessage( Strings.Entity_not_found___ );
            return 0;
        }
    }
}
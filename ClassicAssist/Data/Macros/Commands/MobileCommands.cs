using Assistant;
using ClassicAssist.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class MobileCommands
    {
        [CommandsDisplay(Category = "Entity", Description = "Returns true if given mobile is dead, false if not, if parameter is null, then returns the value from the player (parameter can be serial or alias).")]
        public static bool Dead(object obj = null)
        {
            int serial = AliasCommands.ResolveSerial(obj);

            if (serial <= 0)
            {
                UOC.SystemMessage(Strings.Invalid_or_unknown_object_id);
                return false;
            }

            Mobile mobile = Engine.Mobiles.GetMobile(serial);

            if (mobile != null)
            {
                return mobile.IsDead;
            }

            UOC.SystemMessage(Strings.Mobile_not_found___);
            return false;
        }

        [CommandsDisplay(Category = "Entity", Description = "Returns true if given mobile is hidden, false if not, if parameter is null, then returns the value from the player (parameter can be serial or alias).")]
        public static bool Hidden( object obj = null )
        {
            int serial = AliasCommands.ResolveSerial(obj);

            if (serial <= 0)
            {
                UOC.SystemMessage(Strings.Invalid_or_unknown_object_id);
                return false;
            }

            Mobile mobile = Engine.Mobiles.GetMobile(serial);

            if (mobile != null)
            {
                return mobile.Status.HasFlag( MobileStatus.Hidden );
            }

            UOC.SystemMessage(Strings.Mobile_not_found___);
            return false;
        }

        [CommandsDisplay(Category = "Entity", Description = "Returns the given mobiles hitpoints, if parameter is null, then returns the value from the player (parameter can be serial or alias).")]
        public static int Hits( object obj = null )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial <= 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return 0;
            }

            Mobile mobile = Engine.Mobiles.GetMobile( serial );

            if ( mobile != null )
            {
                return mobile.Hits;
            }

            UOC.SystemMessage( Strings.Mobile_not_found___ );
            return 0;
        }

        [CommandsDisplay(Category = "Entity", Description = "Returns the given mobiles stamina, if parameter is null, then returns the value from the player (parameter can be serial or alias).")]
        public static int Stam( object obj = null )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial <= 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return 0;
            }

            Mobile mobile = Engine.Mobiles.GetMobile( serial );

            if ( mobile != null )
            {
                return mobile.Stamina;
            }

            UOC.SystemMessage( Strings.Mobile_not_found___ );
            return 0;
        }

        [CommandsDisplay(Category = "Entity", Description = "Returns the given mobiles mana, if parameter is null, then returns the value from the player (parameter can be serial or alias).")]
        public static int Mana( object obj = null )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial <= 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return 0;
            }

            Mobile mobile = Engine.Mobiles.GetMobile( serial );

            if ( mobile != null )
            {
                return mobile.Mana;
            }

            UOC.SystemMessage( Strings.Mobile_not_found___ );
            return 0;
        }
    }
}
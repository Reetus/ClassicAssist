using Assistant;
using ClassicAssist.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class NotorietyCommands
    {
        [CommandsDisplay( Category = "Entity", Description = "Returns true if the mobile's notoriety is Criminal",
            InsertText = "if Criminal(\"mount\"):" )]
        public static bool Criminal( object obj )
        {
            return CheckNotoriety( obj, Notoriety.Criminal );
        }

        [CommandsDisplay( Category = "Entity", Description = "Returns true if the mobile's notoriety is Attackable",
            InsertText = "if Criminal(\"mount\"):" )]
        public static bool Gray( object obj )
        {
            return CheckNotoriety( obj, Notoriety.Attackable );
        }

        [CommandsDisplay( Category = "Entity", Description = "Returns true if the mobile's notoriety is Ally",
            InsertText = "if Criminal(\"mount\"):" )]
        public static bool Ally( object obj )
        {
            return CheckNotoriety( obj, Notoriety.Ally );
        }

        [CommandsDisplay( Category = "Entity", Description = "Returns true if the mobile's notoriety is Enemy",
            InsertText = "if Criminal(\"mount\"):" )]
        public static bool Enemy( object obj )
        {
            return CheckNotoriety( obj, Notoriety.Enemy );
        }

        [CommandsDisplay( Category = "Entity", Description = "Returns true if the mobile's notoriety is Invulnerable",
            InsertText = "if Criminal(\"mount\"):" )]
        public static bool Invulnerable( object obj )
        {
            return CheckNotoriety( obj, Notoriety.Invulnerable );
        }

        [CommandsDisplay( Category = "Entity", Description = "Returns true if the mobile's notoriety is Innocent",
            InsertText = "if Criminal(\"mount\"):" )]
        public static bool Innocent( object obj )
        {
            return CheckNotoriety( obj, Notoriety.Innocent );
        }

        [CommandsDisplay( Category = "Entity", Description = "Returns true if the mobile's notoriety is Murderer",
            InsertText = "if Criminal(\"mount\"):" )]
        public static bool Murderer( object obj )
        {
            return CheckNotoriety( obj, Notoriety.Murderer );
        }

        private static bool CheckNotoriety( object obj, Notoriety notoriety )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return false;
            }

            Mobile m = Engine.Mobiles.GetMobile( serial );

            if ( m == null )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return false;
            }

            return m.Notoriety == notoriety;
        }
    }
}
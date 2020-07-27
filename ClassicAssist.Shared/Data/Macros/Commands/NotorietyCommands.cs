using ClassicAssist.Shared;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.Shared.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class NotorietyCommands
    {
        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static bool Criminal( object obj )
        {
            return CheckNotoriety( obj, Notoriety.Criminal );
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static bool Gray( object obj )
        {
            return CheckNotoriety( obj, Notoriety.Attackable );
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static bool Ally( object obj )
        {
            return CheckNotoriety( obj, Notoriety.Ally );
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static bool Enemy( object obj )
        {
            return CheckNotoriety( obj, Notoriety.Enemy );
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static bool Invulnerable( object obj )
        {
            return CheckNotoriety( obj, Notoriety.Invulnerable );
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static bool Innocent( object obj )
        {
            return CheckNotoriety( obj, Notoriety.Innocent );
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
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
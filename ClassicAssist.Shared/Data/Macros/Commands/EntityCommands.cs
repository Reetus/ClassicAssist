using System.Reflection;
using ClassicAssist.Shared;
using ClassicAssist.Data.BuffIcons;
using ClassicAssist.Data.SpecialMoves;
using ClassicAssist.Resources;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.Shared.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class EntityCommands
    {
        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
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

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ), nameof( ParameterType.Distance ) } )]
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

            return false;
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
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

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
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

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
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

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
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

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
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

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.BuffName ) } )]
        public static bool BuffExists( string name )
        {
            return BuffIconManager.GetInstance().BuffExists( name );
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.BuffName ) } )]
        public static double BuffTime( string name )
        {
            return BuffIconManager.GetInstance().BuffTime( name );
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SpecialMoveName ) } )]
        public static bool SpecialMoveExists( string name )
        {
            return SpecialMovesManager.GetInstance().SpecialMoveExists( name );
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
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

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static string Name( object obj = null )
        {
            return GetEntityProperty<string>( obj, nameof( Entity.Name ) )?.Trim() ?? string.Empty;
        }

        private static T GetEntityProperty<T>( object obj, string propertyName )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial <= 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return default;
            }

            Entity entity = UOMath.IsMobile( serial )
                ? Engine.Mobiles.GetMobile( serial )
                : (Entity) Engine.Items.GetItem( serial );

            if ( entity == null )
            {
                UOC.SystemMessage( Strings.Entity_not_found___ );
                return default;
            }

            PropertyInfo property = entity.GetType().GetProperty( propertyName );

            if ( property == null )
            {
                return default;
            }

            T val = (T) property.GetValue( entity );

            return val;
        }

        [CommandsDisplay( Category = nameof( Strings.Main ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static void HideEntity( object obj )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Cannot_find_item___ );
                return;
            }

            UOC.RemoveObject( serial );
        }
    }
}
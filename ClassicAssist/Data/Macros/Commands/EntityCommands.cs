using System;
using System.Reflection;
using Assistant;
using ClassicAssist.Data.BuffIcons;
using ClassicAssist.Data.SpecialMoves;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UO;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

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
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id, true );
                return 0;
            }

            Entity entity = Engine.Items.GetItem( serial ) ?? (Entity) Engine.Mobiles.GetMobile( serial );

            return entity?.Distance ?? int.MaxValue;
        }

        // ReSharper disable once ExplicitCallerInfoArgument
        [CommandsDisplay( "DistanceCoordinates", Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.XCoordinate ), nameof( ParameterType.YCoordinate ) } )]
        public static int Distance( int x, int y )
        {
            return Math.Max( Math.Abs( x - Engine.Player?.X ?? x ), Math.Abs( y - Engine.Player?.Y ?? y ) );
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ), nameof( ParameterType.Distance ) } )]
        public static bool InRange( object obj, int distance )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id, true );
                return false;
            }

            Entity entity = Engine.Items.GetItem( serial ) ?? (Entity) Engine.Mobiles.GetMobile( serial );

            if ( entity != null )
            {
                return entity.Distance <= distance;
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
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id, true );
                return 0;
            }

            Entity entity = UOMath.IsMobile( serial )
                ? Engine.Mobiles.GetMobile( serial )
                : Engine.Items.GetItem( serial ) as Entity;

            if ( entity != null )
            {
                return entity.Hue;
            }

            UOC.SystemMessage( Strings.Entity_not_found___, true );

            return 0;
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static int Graphic( object obj = null )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id, true );
                return 0;
            }

            Entity entity = UOMath.IsMobile( serial )
                ? Engine.Mobiles.GetMobile( serial )
                : Engine.Items.GetItem( serial ) as Entity;

            if ( entity != null )
            {
                return entity.ID;
            }

            UOC.SystemMessage( Strings.Entity_not_found___, true );

            return 0;
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static int X( object obj = null )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id, true );
                return 0;
            }

            Entity entity = UOMath.IsMobile( serial )
                ? Engine.Mobiles.GetMobile( serial )
                : Engine.Items.GetItem( serial ) as Entity;

            if ( entity != null )
            {
                return entity.X;
            }

            UOC.SystemMessage( Strings.Entity_not_found___, true );

            return 0;
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static int Y( object obj = null )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id, true );
                return 0;
            }

            Entity entity = UOMath.IsMobile( serial )
                ? Engine.Mobiles.GetMobile( serial )
                : Engine.Items.GetItem( serial ) as Entity;

            if ( entity != null )
            {
                return entity.Y;
            }

            UOC.SystemMessage( Strings.Entity_not_found___, true );

            return 0;
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static int Z( object obj = null )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id, true );
                return 0;
            }

            Entity entity = UOMath.IsMobile( serial )
                ? Engine.Mobiles.GetMobile( serial )
                : Engine.Items.GetItem( serial ) as Entity;

            if ( entity != null )
            {
                return entity.Z;
            }

            UOC.SystemMessage( Strings.Entity_not_found___, true );

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
                return UO.Data.Direction.Invalid.ToString();
            }

            Entity entity = UOMath.IsMobile( serial )
                ? Engine.Mobiles.GetMobile( serial )
                : Engine.Items.GetItem( serial ) as Entity;

            if ( entity == null )
            {
                return UO.Data.Direction.Invalid.ToString();
            }

            return UOMath.MapDirection( Engine.Player.X, Engine.Player.Y, entity.X, entity.Y ).ToString();
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static string Direction( object obj = null )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            Entity entity = UOMath.IsMobile( serial )
                ? Engine.Mobiles.GetMobile( serial )
                : Engine.Items.GetItem( serial ) as Entity;

            if ( serial == 0 || entity == null )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id, true );
                return UO.Data.Direction.Invalid.ToString();
            }

            return entity.Direction.ToString();
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
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id, true );
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
﻿using System;
using System.Linq;
using ClassicAssist.Shared;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.Shared.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class PropertiesCommands
    {
        [CommandsDisplay( Category = nameof( Strings.Properties ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ), nameof( ParameterType.Timeout ) } )]
        public static bool WaitForProperties( object obj, int timeout )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return false;
            }

            PacketFilterInfo pfi = new PacketFilterInfo( 0xD6,
                new[] { PacketFilterConditions.IntAtPositionCondition( serial, 5 ) } );

            PacketWaitEntry we = Engine.PacketWaitEntries.Add( pfi, PacketDirection.Incoming, true );

            Engine.SendPacketToServer( new BatchQueryProperties( serial ) );

            try
            {
                bool result = we.Lock.WaitOne( timeout );

                return result;
            }
            finally
            {
                Engine.PacketWaitEntries.Remove( we );
            }
        }

        [CommandsDisplay( Category = nameof( Strings.Properties ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ), nameof( ParameterType.String ) } )]
        public static bool Property( object obj, string value )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return false;
            }

            Entity entity = (Entity) Engine.Items.GetItem( serial ) ?? Engine.Mobiles.GetMobile( serial );

            if ( entity?.Properties != null )
            {
                return entity.Properties.Any( pe => pe.Text.ToLower().Contains( value.ToLower() ) );
            }

            UOC.SystemMessage( Strings.Item_properties_null_or_not_loaded___ );
            return false;
        }

        [CommandsDisplay( Category = nameof( Strings.Properties ),
            Parameters = new[]
            {
                nameof( ParameterType.SerialOrAlias ), nameof( ParameterType.String ),
                nameof( ParameterType.IntegerValue )
            } )]
        public static T PropertyValue<T>( object obj, string property, int argument = 0 )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return default;
            }

            Entity entity = (Entity) Engine.Items.GetItem( serial ) ?? Engine.Mobiles.GetMobile( serial );

            if ( entity?.Properties != null )
            {
                Property p = entity.Properties.FirstOrDefault( pe => pe.Text.ToLower().Contains( property.ToLower() ) );

                if ( p == null )
                {
                    return default;
                }

                if ( p.Arguments[0].Trim().Equals( string.Empty ) )
                {
                    return default;
                }

                return (T) Convert.ChangeType( p?.Arguments?[argument], typeof( T ) );
            }

            UOC.SystemMessage( Strings.Item_properties_null_or_not_loaded___ );
            return default;
        }
    }
}
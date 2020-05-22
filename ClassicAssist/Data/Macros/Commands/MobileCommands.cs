using System.Linq;
using System.Reflection;
using Assistant;
using ClassicAssist.Data.Friends;
using ClassicAssist.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class MobileCommands
    {
        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static bool InFriendList( object obj )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial != 0 )
            {
                bool result = Options.CurrentOptions.Friends.Any( fe => fe.Serial == serial );

                if ( result )
                {
                    return true;
                }

                if ( Options.CurrentOptions.IncludePartyMembersInFriends )
                {
                    result = Engine.Player?.Party?.Contains( serial ) ?? false;
                }

                return result;
            }

            UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
            return false;
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static void AddFriend( object obj = null )
        {
            int serial = obj != null
                ? AliasCommands.ResolveSerial( obj )
                : UOC.GetTargeSerialAsync( Strings.Target_new_friend___ ).Result;

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return;
            }

            Mobile m = Engine.Mobiles.GetMobile( serial );

            if ( m == null )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return;
            }

            FriendEntry fe = new FriendEntry { Name = m.Name.Trim(), Serial = m.Serial };

            if ( !Options.CurrentOptions.Friends.Contains( fe ) )
            {
                Engine.Dispatcher?.Invoke( () => Options.CurrentOptions.Friends.Add( fe ) );
            }
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static void RemoveFriend( object obj = null )
        {
            int serial = obj != null
                ? AliasCommands.ResolveSerial( obj )
                : UOC.GetTargeSerialAsync( Strings.Target_friend_to_remove___ ).Result;

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return;
            }

            FriendEntry entry = Options.CurrentOptions.Friends.FirstOrDefault( i => i.Serial == serial );

            if ( entry == null )
            {
                return;
            }

            Engine.Dispatcher?.Invoke( () => Options.CurrentOptions.Friends.Remove( entry ) );
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static bool Dead( object obj = null )
        {
            return GetMobileProperty<bool>( obj, nameof( Mobile.IsDead ) );
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static bool Hidden( object obj = null )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial <= 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return false;
            }

            Mobile mobile = Engine.Mobiles.GetMobile( serial );

            if ( mobile != null )
            {
                return mobile.Status.HasFlag( MobileStatus.Hidden );
            }

            UOC.SystemMessage( Strings.Mobile_not_found___ );
            return false;
        }

        private static T GetMobileProperty<T>( object obj, string propertyName )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial <= 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return default;
            }

            Mobile mobile = Engine.Mobiles.GetMobile( serial );

            if ( mobile == null )
            {
                UOC.SystemMessage( Strings.Mobile_not_found___ );
                return default;
            }

            PropertyInfo property = mobile.GetType().GetProperty( propertyName );

            if ( property == null )
            {
                return default;
            }

            T val = (T) property.GetValue( mobile );

            return val;
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static int MaxMana( object obj = null )
        {
            return GetMobileProperty<int>( obj, nameof( Mobile.ManaMax ) );
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static int MaxStam( object obj = null )
        {
            return GetMobileProperty<int>( obj, nameof( Mobile.StaminaMax ) );
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static int Hits( object obj = null )
        {
            return GetMobileProperty<int>( obj, nameof( Mobile.Hits ) );
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ) )]
        public static int Str()
        {
            return GetMobileProperty<int>( "self", nameof( PlayerMobile.Strength ) );
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ) )]
        public static int Dex()
        {
            return GetMobileProperty<int>( "self", nameof( PlayerMobile.Dex ) );
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ) )]
        public static int Int()
        {
            return GetMobileProperty<int>( "self", nameof( PlayerMobile.Int ) );
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static int MaxHits( object obj = null )
        {
            return GetMobileProperty<int>( obj, nameof( Mobile.HitsMax ) );
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static int DiffHits( object obj = null )
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
                return mobile.HitsMax - mobile.Hits;
            }

            UOC.SystemMessage( Strings.Mobile_not_found___ );
            return 0;
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static int Stam( object obj = null )
        {
            return GetMobileProperty<int>( obj, nameof( Mobile.Stamina ) );
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static int Mana( object obj = null )
        {
            return GetMobileProperty<int>( obj, nameof( Mobile.Mana ) );
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static bool War( object obj )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial != 0 )
            {
                return Engine.Mobiles.GetMobile( serial )?.Status.HasFlag( MobileStatus.WarMode ) ?? false;
            }

            UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
            return false;
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ) )]
        public static int Followers()
        {
            return Engine.Player?.Followers ?? 0;
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ) )]
        public static int MaxFollowers()
        {
            return Engine.Player?.FollowersMax ?? 0;
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ) )]
        public static int Weight()
        {
            return Engine.Player?.Weight ?? 0;
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ) )]
        public static int MaxWeight()
        {
            return Engine.Player?.WeightMax ?? 0;
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ) )]
        public static int DiffWeight()
        {
            PlayerMobile player = Engine.Player;

            if ( player == null )
            {
                return 0;
            }

            return player.WeightMax - player.Weight;
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ) )]
        public static int Gold()
        {
            return Engine.Player?.Gold ?? 0;
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ) )]
        public static int Luck()
        {
            return Engine.Player?.Luck ?? 0;
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ) )]
        public static int TithingPoints()
        {
            return Engine.Player?.TithingPoints ?? 0;
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static bool Poisoned( object obj )
        {
            return GetMobileProperty<bool>( obj, nameof( Mobile.IsPoisoned ) );
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static bool YellowHits( object obj )
        {
            return GetMobileProperty<bool>( obj, nameof( Mobile.IsYellowHits ) );
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static bool Paralyzed( object obj )
        {
            return GetMobileProperty<bool>( obj, nameof( Mobile.IsFrozen ) );
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static bool Mounted( object obj )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return false;
            }

            Mobile mobile = Engine.Mobiles.GetMobile( serial );

            if ( mobile != null )
            {
                return mobile.IsMounted;
            }

            UOC.SystemMessage( Strings.Mobile_not_found___ );
            return false;
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static bool InParty( object obj )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial != 0 )
            {
                return Engine.Player?.Party != null && Engine.Player.Party.Contains( serial );
            }

            UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
            return false;
        }
    }
}
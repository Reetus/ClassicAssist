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
        [CommandsDisplay( Category = "Entity",
            Description = "Returns true if supplied mobile exists in the friends list.",
            InsertText = "if InFriendsList(\"last\"):" )]
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

        [CommandsDisplay( Category = "Entity",
            Description = "Adds a mobile to friends list, will display target cursor if no serial/alias supplied.",
            InsertText = "AddFriend()" )]
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

        [CommandsDisplay( Category = "Entity",
            Description =
                "Removes a mobile from the friends list, will display target cursor if no serial/alias supplied.",
            InsertText = "RemoveFriend()" )]
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

        [CommandsDisplay( Category = "Entity",
            Description =
                "Returns true if given mobile is dead, false if not, if parameter is null, then returns the value from the player (parameter can be serial or alias).",
            InsertText = "if Dead(\"self\"):" )]
        public static bool Dead( object obj = null )
        {
            return GetMobileProperty<bool>( obj, nameof( Mobile.IsDead ) );
        }

        [CommandsDisplay( Category = "Entity",
            Description =
                "Returns true if given mobile is hidden, false if not, if parameter is null, then returns the value from the player (parameter can be serial or alias).",
            InsertText = "if Hidden(\"self\"):" )]
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

        [CommandsDisplay( Category = "Entity",
            Description =
                "Returns the given mobiles max mana, if parameter is null, then returns the value from the player (parameter can be serial or alias).",
            InsertText = "mana = MaxMana(\"self\")" )]
        public static int MaxMana( object obj = null )
        {
            return GetMobileProperty<int>( obj, nameof( Mobile.ManaMax ) );
        }

        [CommandsDisplay( Category = "Entity",
            Description =
                "Returns the given mobiles max stamina, if parameter is null, then returns the value from the player (parameter can be serial or alias).",
            InsertText = "stam = MaxStam(\"self\")" )]
        public static int MaxStam( object obj = null )
        {
            return GetMobileProperty<int>( obj, nameof( Mobile.StaminaMax ) );
        }

        [CommandsDisplay( Category = "Entity",
            Description =
                "Returns the given mobiles hitpoints, if parameter is null, then returns the value from the player (parameter can be serial or alias).",
            InsertText = "hits = Hits(\"self\")" )]
        public static int Hits( object obj = null )
        {
            return GetMobileProperty<int>( obj, nameof( Mobile.Hits ) );
        }

        [CommandsDisplay( Category = "Entity", Description = "Returns the strength of the player",
            InsertText = "if Str() < 100:" )]
        public static int Str()
        {
            return GetMobileProperty<int>( "self", nameof( PlayerMobile.Strength ) );
        }

        [CommandsDisplay( Category = "Entity", Description = "Returns the dexterity of the player",
            InsertText = "if Str() < 100:" )]
        public static int Dex()
        {
            return GetMobileProperty<int>( "self", nameof( PlayerMobile.Dex ) );
        }

        [CommandsDisplay( Category = "Entity", Description = "Returns the intelligence of the player",
            InsertText = "if Str() < 100:" )]
        public static int Int()
        {
            return GetMobileProperty<int>( "self", nameof( PlayerMobile.Int ) );
        }

        [CommandsDisplay( Category = "Entity",
            Description =
                "Returns the given mobiles max hitpoints, if parameter is null, then returns the value from the player (parameter can be serial or alias).",
            InsertText = "hits = MaxHits(\"self\")" )]
        public static int MaxHits( object obj = null )
        {
            return GetMobileProperty<int>( obj, nameof( Mobile.HitsMax ) );
        }

        [CommandsDisplay( Category = "Entity",
            Description =
                "Returns the given mobiles difference between max and current hits, if parameter is null, then returns the value from the player (parameter can be serial or alias).",
            InsertText = "if DiffHits(\"self\") > 50:" )]
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

        [CommandsDisplay( Category = "Entity",
            Description =
                "Returns the given mobiles stamina, if parameter is null, then returns the value from the player (parameter can be serial or alias).",
            InsertText = "if Stam(\"self\") < 25:" )]
        public static int Stam( object obj = null )
        {
            return GetMobileProperty<int>( obj, nameof( Mobile.Stamina ) );
        }

        [CommandsDisplay( Category = "Entity",
            Description =
                "Returns the given mobiles mana, if parameter is null, then returns the value from the player (parameter can be serial or alias).",
            InsertText = "if Mana(\"self\") < 25:" )]
        public static int Mana( object obj = null )
        {
            return GetMobileProperty<int>( obj, nameof( Mobile.Mana ) );
        }

        [CommandsDisplay( Category = "Entity", Description = "Checks whether a mobile is in war mode.",
            InsertText = "if War(\"self\"):" )]
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

        [CommandsDisplay( Category = "Entity",
            Description = "Returns the number of current followers as per status bar data.",
            InsertText = "if Followers() < 1:" )]
        public static int Followers()
        {
            return Engine.Player?.Followers ?? 0;
        }

        [CommandsDisplay( Category = "Entity",
            Description = "Returns the number of max followers as per status bar data.",
            InsertText = "if Followers() == MaxFollowers():" )]
        public static int MaxFollowers()
        {
            return Engine.Player?.FollowersMax ?? 0;
        }

        [CommandsDisplay( Category = "Entity", Description = "Returns the current weight as as per status bar data.",
            InsertText = "if Weight() > 300:" )]
        public static int Weight()
        {
            return Engine.Player?.Weight ?? 0;
        }

        [CommandsDisplay( Category = "Entity", Description = "Returns the max weight as per status bar data.",
            InsertText = "if MaxWeight() < 300:" )]
        public static int MaxWeight()
        {
            return Engine.Player?.WeightMax ?? 0;
        }

        [CommandsDisplay( Category = "Entity", Description = "Returns the difference between max weight and weight.",
            InsertText = "if DiffWeight() > 50:" )]
        public static int DiffWeight()
        {
            PlayerMobile player = Engine.Player;

            if ( player == null )
            {
                return 0;
            }

            return player.WeightMax - player.Weight;
        }

        [CommandsDisplay( Category = "Entity", Description = "Returns the gold value as per status bar data.",
            InsertText = "if Gold() < 2000:" )]
        public static int Gold()
        {
            return Engine.Player?.Gold ?? 0;
        }

        [CommandsDisplay( Category = "Entity", Description = "Returns the luck value as per status bar data.",
            InsertText = "if Luck() < 800:" )]
        public static int Luck()
        {
            return Engine.Player?.Luck ?? 0;
        }

        [CommandsDisplay( Category = "Entity", Description = "Returns the current players' tithing points.",
            InsertText = "if TithingPoints() < 1000:" )]
        public static int TithingPoints()
        {
            return Engine.Player?.TithingPoints ?? 0;
        }

        [CommandsDisplay( Category = "Entity", Description = "Returns true if the specified mobile is poisoned.",
            InsertText = "if Poisoned(\"self\"):" )]
        public static bool Poisoned( object obj )
        {
            return GetMobileProperty<bool>( obj, nameof( Mobile.IsPoisoned ) );
        }

        [CommandsDisplay( Category = "Entity", Description = "Returns true if the specified mobile is yellowhits.",
            InsertText = "if YellowHits(\"self\"):" )]
        public static bool YellowHits( object obj )
        {
            return GetMobileProperty<bool>( obj, nameof( Mobile.IsYellowHits ) );
        }

        [CommandsDisplay( Category = "Entity", Description = "Returns true if the specified mobile is frozen.",
            InsertText = "if Paralyzed(\"self\"):" )]
        public static bool Paralyzed( object obj )
        {
            return GetMobileProperty<bool>( obj, nameof( Mobile.IsFrozen ) );
        }

        [CommandsDisplay( Category = "Entity", Description = "Returns true if the specified mobile is mounted.",
            InsertText = "if Mounted(\"self\"):" )]
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

        [CommandsDisplay( Category = "Entity",
            Description = "Return the true if the given serial/alias is in party with you.",
            InsertText = "if InParty(\"friend\"):" )]
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
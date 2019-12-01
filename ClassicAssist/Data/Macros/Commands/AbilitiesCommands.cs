using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assistant;
using ClassicAssist.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using Newtonsoft.Json;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class AbilitiesCommands
    {
        private static List<WeaponData> _weaponData;

        [CommandsDisplay( Category = "Abilities", Description = "Clear weapon ability.",
            InsertText = "ClearAbility()" )]
        public static void ClearAbility()
        {
            UOC.ClearWeaponAbility();
        }

        [CommandsDisplay( Category = "Abilities",
            Description = "Set weapon ability, parameter \"primary\" / \"secondary\".",
            InsertText = "SetAbility(\"primary\")" )]
        public static void SetAbility( string ability )
        {
            // TODO stun/disarm old
            bool primary;

            switch ( ability.ToLower() )
            {
                case "primary":
                    primary = true;
                    break;
                default:
                    primary = false;
                    break;
            }

            if ( _weaponData == null )
            {
                LoadWeaponData( Engine.StartupPath );
            }

            if ( Engine.Player == null )
            {
                return;
            }

            int twoHandSerial = Engine.Player.GetLayer( Layer.TwoHanded );

            Item twoHandItem = Engine.Items.GetItem( twoHandSerial );

            if ( twoHandItem != null )
            {
                WeaponData wd =
                    ( _weaponData ?? throw new InvalidOperationException() ).FirstOrDefault( d =>
                        d.Graphic == twoHandItem.ID && d.Twohanded );

                if ( wd != null )
                {
                    UOC.SetWeaponAbility( primary ? wd.Primary : wd.Secondary );
                    return;
                }
            }

            int oneHandSerial = Engine.Player.GetLayer( Layer.OneHanded );

            Item oneHandItem = Engine.Items.GetItem( oneHandSerial );

            if ( oneHandItem != null )
            {
                WeaponData wd =
                    ( _weaponData ?? throw new InvalidOperationException() ).FirstOrDefault( d =>
                        d.Graphic == oneHandItem.ID && !d.Twohanded );

                if ( wd != null )
                {
                    UOC.SetWeaponAbility( primary ? wd.Primary : wd.Secondary );
                    return;
                }
            }

            // Fists etc
            UOC.SetWeaponAbility( primary ? 11 : 5 );
        }

        [CommandsDisplay( Category = "Abilities", Description = "(Garoyle) Start flying if not already flying.",
            InsertText = "Fly()" )]
        public static void Fly()
        {
            PlayerMobile player = Engine.Player;

            if ( player == null )
            {
                return;
            }

            if ( !player.Status.HasFlag( MobileStatus.Flying ) )
            {
                UOC.ToggleGargoyleFlying();
            }
        }

        [CommandsDisplay( Category = "Abilities", Description = "Returns true if mobile is currently flying.",
            InsertText = "if Flying(\"self\"):" )]
        public static bool Flying( object obj )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id );
                return false;
            }

            Mobile mobile = Engine.Mobiles.GetMobile( serial );

            if ( mobile == null )
            {
                // TODO better message
                UOC.SystemMessage( Strings.Cannot_find_item___ );
                return false;
            }

            return mobile.Status.HasFlag( MobileStatus.Flying );
        }

        [CommandsDisplay( Category = "Abilities", Description = "(Garoyle) Stop flying if currently flying.",
            InsertText = "Land()" )]
        public static void Land()
        {
            PlayerMobile player = Engine.Player;

            if ( player == null )
            {
                return;
            }

            if ( player.Status.HasFlag( MobileStatus.Flying ) )
            {
                UOC.ToggleGargoyleFlying();
            }
        }

        private static void LoadWeaponData( string basePath )
        {
            string dataPath = Path.Combine( basePath, "Data" );

            _weaponData = JsonConvert
                .DeserializeObject<WeaponData[]>( File.ReadAllText( Path.Combine( dataPath, "Weapons.json" ) ) )
                .ToList();
        }

        internal class WeaponData
        {
            public int Graphic { get; set; }
            public string Name { get; set; }
            public int Primary { get; set; }
            public int Secondary { get; set; }
            public bool Twohanded { get; set; }
        }
    }
}
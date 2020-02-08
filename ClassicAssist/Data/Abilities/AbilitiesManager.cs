using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assistant;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Objects;
using Newtonsoft.Json;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Abilities
{
    public enum AbilityType
    {
        None,
        Primary,
        Secondary
    }

    public class AbilitiesManager
    {
        private static AbilitiesManager _instance;
        private static readonly object _lock = new object();
        private static List<WeaponData> _weaponData;

        private AbilitiesManager()
        {
            if ( _weaponData == null )
            {
                LoadWeaponData( Engine.StartupPath ?? Environment.CurrentDirectory );
            }
        }

        public AbilityType Enabled { get; set; }
        public bool IsPrimaryEnabled => Enabled == AbilityType.Primary;
        public bool IsSecondaryEnabled => Enabled == AbilityType.Secondary;

        public void SetAbility( AbilityType abilityType )
        {
            if ( Engine.Player == null )
            {
                return;
            }

            switch ( abilityType )
            {
                case AbilityType.Primary when !IsPrimaryEnabled:
                    Enabled = AbilityType.Primary;
                    break;
                case AbilityType.Secondary when !IsSecondaryEnabled:
                    Enabled = AbilityType.Secondary;
                    break;
                default:
                    Enabled = AbilityType.None;
                    break;
            }

            int twoHandSerial = Engine.Player.GetLayer( Layer.TwoHanded );

            Item twoHandItem = Engine.Items.GetItem( twoHandSerial );

            if ( twoHandItem != null )
            {
                WeaponData wd =
                    ( _weaponData ?? throw new InvalidOperationException() ).FirstOrDefault( d =>
                        d.Graphic == twoHandItem.ID );

                if ( wd != null )
                {
                    UOC.SetWeaponAbility( abilityType == AbilityType.Primary ? wd.Primary : wd.Secondary );
                    ResendGump( wd.Primary, wd.Secondary, abilityType );
                    return;
                }
            }

            int oneHandSerial = Engine.Player.GetLayer( Layer.OneHanded );

            Item oneHandItem = Engine.Items.GetItem( oneHandSerial );

            if ( oneHandItem != null )
            {
                WeaponData wd =
                    ( _weaponData ?? throw new InvalidOperationException() ).FirstOrDefault( d =>
                        d.Graphic == oneHandItem.ID );

                if ( wd != null )
                {
                    UOC.SetWeaponAbility( abilityType == AbilityType.Primary ? wd.Primary : wd.Secondary );
                    ResendGump( wd.Primary, wd.Secondary, abilityType );
                    return;
                }
            }

            // Fists etc
            UOC.SetWeaponAbility( abilityType == AbilityType.Primary ? 11 : 5 );
        }

        public void ResendGump( AbilityType abilityType )
        {
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
                        d.Graphic == twoHandItem.ID );

                if ( wd != null )
                {
                    ResendGump( wd.Primary, wd.Secondary, abilityType );
                    return;
                }
            }

            int oneHandSerial = Engine.Player.GetLayer( Layer.OneHanded );

            Item oneHandItem = Engine.Items.GetItem( oneHandSerial );

            if ( oneHandItem != null )
            {
                WeaponData wd =
                    ( _weaponData ?? throw new InvalidOperationException() ).FirstOrDefault( d =>
                        d.Graphic == oneHandItem.ID );

                if ( wd != null )
                {
                    ResendGump( wd.Primary, wd.Secondary, abilityType );
                    return;
                }
            }

            ResendGump( 5, 11, abilityType );
        }

        private static void ResendGump( int primaryId, int secondaryId, AbilityType abilityType )
        {
            if ( !Options.CurrentOptions.AbilitiesGump )
            {
                return;
            }

            WeaponAbilitiesGump gump = new WeaponAbilitiesGump( primaryId, abilityType == AbilityType.Primary,
                secondaryId, abilityType == AbilityType.Secondary );

            gump.SendGump();
        }

        public static AbilitiesManager GetInstance()
        {
            // ReSharper disable once InvertIf
            if ( _instance == null )
            {
                lock ( _lock )
                {
                    if ( _instance != null )
                    {
                        return _instance;
                    }

                    _instance = new AbilitiesManager();
                    return _instance;
                }
            }

            return _instance;
        }

        private static void LoadWeaponData( string basePath )
        {
            string dataPath = Path.Combine( basePath, "Data" );

            _weaponData = JsonConvert
                .DeserializeObject<WeaponData[]>( File.ReadAllText( Path.Combine( dataPath, "Weapons.json" ) ) )
                .ToList();
        }

        public void CheckAbility( int abilityIndex )
        {
            int twoHandSerial = Engine.Player.GetLayer( Layer.TwoHanded );

            Item twoHandItem = Engine.Items.GetItem( twoHandSerial );

            AbilityType abilityType = AbilityType.None;

            if ( twoHandItem != null )
            {
                WeaponData wd =
                    ( _weaponData ?? throw new InvalidOperationException() ).FirstOrDefault( d =>
                        d.Graphic == twoHandItem.ID );

                if ( wd != null )
                {
                    if ( wd.Primary == abilityIndex )
                    {
                        abilityType = AbilityType.Primary;
                    }
                    else if ( wd.Secondary == abilityIndex )
                    {
                        abilityType = AbilityType.Secondary;
                    }

                    ResendGump( wd.Primary, wd.Secondary, abilityType );
                    return;
                }
            }

            int oneHandSerial = Engine.Player.GetLayer( Layer.OneHanded );

            Item oneHandItem = Engine.Items.GetItem( oneHandSerial );

            if ( oneHandItem != null )
            {
                WeaponData wd =
                    ( _weaponData ?? throw new InvalidOperationException() ).FirstOrDefault( d =>
                        d.Graphic == oneHandItem.ID );

                if ( wd != null )
                {
                    if ( wd.Primary == abilityIndex )
                    {
                        abilityType = AbilityType.Primary;
                    }
                    else if ( wd.Secondary == abilityIndex )
                    {
                        abilityType = AbilityType.Secondary;
                    }

                    ResendGump( wd.Primary, wd.Secondary, abilityType );
                    return;
                }
            }

            if ( abilityIndex == 5 )
            {
                abilityType = AbilityType.Primary;
            }
            else if ( abilityIndex == 11 )
            {
                abilityType = AbilityType.Secondary;
            }

            ResendGump( 5, 11, abilityType );
        }
    }
}
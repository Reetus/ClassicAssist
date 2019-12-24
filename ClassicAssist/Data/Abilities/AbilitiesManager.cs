using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assistant;
using ClassicAssist.UO.Data;
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
                        d.Graphic == twoHandItem.ID && d.Twohanded );

                if ( wd != null )
                {
                    UOC.SetWeaponAbility( abilityType == AbilityType.Primary ? wd.Primary : wd.Secondary );
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
                    UOC.SetWeaponAbility( abilityType == AbilityType.Primary ? wd.Primary : wd.Secondary );
                    return;
                }
            }

            // Fists etc
            UOC.SetWeaponAbility( abilityType == AbilityType.Primary ? 11 : 5 );
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
    }
}
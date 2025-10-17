using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assistant;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Network.Packets;
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
        private static readonly int[] _potionTypes = { 0xf06, 0xf07, 0xf08, 0xf09, 0xf0b, 0xf0c, 0xf0d };
        private static AbilitiesManager _instance;
        private static readonly object _lock = new object();
        private static List<WeaponData> _weaponData;
        private static bool _checkHandsInProgress;

        private AbilitiesManager()
        {
            if ( _weaponData == null )
            {
                LoadWeaponData( Engine.StartupPath ?? Environment.CurrentDirectory );
            }

            PlayerMobile.LayerChangedEvent += OnLayerChangedEvent;
        }

        public AbilityType Enabled { get; set; }
        public bool IsPrimaryEnabled => Enabled == AbilityType.Primary;
        public bool IsSecondaryEnabled => Enabled == AbilityType.Secondary;

        private void OnLayerChangedEvent( Layer layer, int serial )
        {
            if ( layer != Layer.OneHanded && layer != Layer.TwoHanded )
            {
                return;
            }

            AbilitiesManager manager = GetInstance();
            manager.ResendGump( manager.Enabled );
        }

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
                    UOC.SetWeaponAbility( abilityType == AbilityType.Primary ? wd.Primary : wd.Secondary, abilityType );
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
                    UOC.SetWeaponAbility( abilityType == AbilityType.Primary ? wd.Primary : wd.Secondary, abilityType );
                    ResendGump( wd.Primary, wd.Secondary, abilityType );
                    return;
                }
            }

            // Fists etc
            ResendGump( 5, 11, abilityType );
            UOC.SetWeaponAbility( abilityType == AbilityType.Primary ? 5 : 11, abilityType );
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

            UOC.CloseClientGump( typeof( WeaponAbilitiesGump ) );

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

        internal WeaponData GetWeaponData( int id )
        {
            return _weaponData?.FirstOrDefault( w => w.Graphic == id );
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

        public bool CheckHands( int serial )
        {
            if ( !Options.CurrentOptions.CheckHandsPotions )
            {
                return false;
            }

            Item item = Engine.Items?.GetItem( serial );

            if ( item == null || !_potionTypes.Contains( item.ID ) )
            {
                return false;
            }

            // Explosion / Conflagaration pot doesn't need hand free
            if ( item.ID == 0xf0d && item.Hue == 0 || item.ID == 0xf06 && item.Hue == 1161 )
            {
                return false;
            }

            if ( _checkHandsInProgress )
            {
                UOC.SystemMessage( Strings.Arm___Disarm_already_in_progress___, (int) SystemMessageHues.Red );
                return false;
            }

            _checkHandsInProgress = true;

            Item leftHand = Engine.Player?.Equipment.FirstOrDefault( i => i.Layer == Layer.TwoHanded );
            Item rightHand = Engine.Player?.Equipment.FirstOrDefault( i => i.Layer == Layer.OneHanded );

            if ( leftHand == null && rightHand != null )
            {
                //OSI crossbow in OneHanded layer??
                leftHand = rightHand;
                rightHand = null;
            }

            WeaponData leftHandWD = null;

            if ( leftHand != null )
            {
                leftHandWD = GetWeaponData( leftHand.ID );
            }

            if ( ( !( leftHandWD?.Twohanded ?? false ) && leftHand?.Properties != null &&
                   !( leftHand.Properties?.Any( p => p.Cliloc == 1061171 /* Two-Handed Weapon */ ) ?? false ) ||
                   leftHand?.Properties != null &&
                   !( leftHand.Properties?.All( p => p.Cliloc != 1072792 /* Balanced */ ) ?? false ) ) &&
                 ( rightHand == null || leftHand == null ) )
            {
                _checkHandsInProgress = false;
                return false;
            }

            if ( leftHand == null )
            {
                _checkHandsInProgress = false;
                return false;
            }

            ActionPacketQueue.EnqueueDragDrop( leftHand.Serial, 1, Engine.Player.GetLayer( Layer.Backpack ),
                QueuePriority.High );
            ActionPacketQueue.EnqueuePackets(
                new BasePacket[]
                {
                    new UseObject( serial ), new DragItem( leftHand.Serial, 1, Options.CurrentOptions.DragDelay ? Options.CurrentOptions.DragDelayMS : 0 ),
                    new EquipRequest( leftHand.Serial, leftHand.Layer, (int) Engine.Player?.Serial )
                }, QueuePriority.High ).ContinueWith( t => _checkHandsInProgress = false );

            return true;
        }
    }
}
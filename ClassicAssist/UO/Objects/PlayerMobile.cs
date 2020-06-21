using System.Text;
using ClassicAssist.Data.Abilities;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Misc;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Objects
{
    public class PlayerMobile : Mobile
    {
        public delegate void dLastTargetChanged( int serial );

        private int _enemyTargetSerial;
        private int _friendTargetSerial;
        private int _id;
        private int _lastTargetSerial;

        public PlayerMobile( int serial ) : base( serial )
        {
        }

        public int ColdResistance { get; set; }
        public int ColdResistanceMax { get; set; }
        public int Damage { get; set; }
        public int DamageIncrease { get; set; }
        public int DamageMax { get; set; }
        public int DefenseChanceIncrease { get; set; }
        public int DefenseChanceIncreaseMax { get; set; }

        public int Dex { get; set; }

        [DisplayFormat( typeof( HexFormatProvider ) )]
        public int EnemyTargetSerial
        {
            get => _enemyTargetSerial;
            set
            {
                _enemyTargetSerial = value;
                AliasCommands.SetAlias( "enemy", value );
            }
        }

        public int EnergyResistance { get; set; }
        public int EnergyResistanceMax { get; set; }
        public int FasterCasting { get; set; }
        public int FasterCastRecovery { get; set; }
        public int FireResistance { get; set; }
        public int FireResistanceMax { get; set; }
        public int Followers { get; set; }
        public int FollowersMax { get; set; }

        [DisplayFormat( typeof( HexFormatProvider ) )]
        public int FriendTargetSerial
        {
            get => _friendTargetSerial;
            set
            {
                _friendTargetSerial = value;
                AliasCommands.SetAlias( "friend", value );
            }
        }

        public int Gold { get; set; }
        public int HitChanceIncrease { get; set; }
        public int Holding { get; set; }
        public int HoldingAmount { get; set; }
        public int Int { get; set; }

        [DisplayFormat( typeof( HexFormatProvider ) )]
        public int LastObjectSerial { get; set; }

        [DisplayFormat( typeof( HexFormatProvider ) )]
        public int LastTargetSerial
        {
            get => _lastTargetSerial;
            set
            {
                if ( value == Serial )
                {
                    return;
                }

                _lastTargetSerial = value;
                AliasCommands.SetAlias( "last", value );
                LastTargetChangedEvent?.Invoke( value );
            }
        }

        public TargetType LastTargetType { get; set; }
        public int LowerManaCost { get; set; }
        public int LowerReagentCost { get; set; }
        public int Luck { get; set; }
        public Map Map { get; set; }
        public int[] Party { get; set; }
        public int PhysicalResistance { get; set; }
        public int PhysicalResistanceMax { get; set; }
        public int PoisonResistance { get; set; }
        public int PoisonResistanceMax { get; set; }
        public MobileRace Race { get; set; }
        public int SpellDamageIncrease { get; set; }
        public int StatCap { get; set; }
        public int Strength { get; set; }
        public int SwingSpeedIncrease { get; set; }
        public int TithingPoints { get; set; }
        public int Weight { get; set; }
        public int WeightMax { get; set; }

        public event dLastTargetChanged LastTargetChangedEvent;

        internal override void SetLayer( Layer layer, int serial )
        {
            base.SetLayer( layer, serial );

            if ( layer != Layer.OneHanded && layer != Layer.TwoHanded )
            {
                return;
            }

            AbilitiesManager manager = AbilitiesManager.GetInstance();
            manager.ResendGump( manager.Enabled );
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine( $"Name: {Name}" );
            sb.AppendLine( $"Serial: 0x{Serial:x}" );

            return sb.ToString();
        }
    }
}
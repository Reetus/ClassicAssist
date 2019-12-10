using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Objects
{
    public class PlayerMobile : Mobile
    {
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
        public int EnergyResistance { get; set; }
        public int EnergyResistanceMax { get; set; }
        public int FasterCasting { get; set; }
        public int FasterCastRecovery { get; set; }
        public int FireResistance { get; set; }
        public int FireResistanceMax { get; set; }
        public int Followers { get; set; }
        public int FollowersMax { get; set; }
        public int Gold { get; set; }
        public int HitChanceIncrease { get; set; }
        public int Int { get; set; }
        public int LastTargetSerial { get; set; }
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
    }
}
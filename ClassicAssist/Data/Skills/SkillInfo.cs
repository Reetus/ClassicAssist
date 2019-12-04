using ClassicAssist.UO.Data;

namespace ClassicAssist.Data.Skills
{
    public sealed class SkillInfo
    {
        public float BaseValue { get; internal set; }
        public int ID { get; internal set; }
        public LockStatus LockStatus { get; internal set; }
        public float SkillCap { get; internal set; }
        public float Value { get; internal set; }
    }
}
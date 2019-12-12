using ClassicAssist.UO.Data;

namespace ClassicAssist.Data.Spells
{
    public class SpellData
    {
        public int Circle { get; set; }
        public TargetFlags Flag { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public int[] Reagents { get; set; }
        public int Timeout { get; set; }
        public string Words { get; set; }
    }
}
namespace ClassicAssist.Data.Skills
{
    public struct Skill
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool Invokable { get; set; }

        public override bool Equals( object obj )
        {
            Skill? skill = obj as Skill?;
            return ID == skill?.ID;
        }

        public override int GetHashCode()
        {
            int hashCode = 1458105184;
            hashCode = hashCode * -1521134295 + ID.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return Name;
        }

        public static bool operator ==( Skill left, Skill right )
        {
            return left.Equals( right );
        }

        public static bool operator !=( Skill left, Skill right )
        {
            return !( left == right );
        }
    }
}
using System;
using ClassicAssist.Resources;
using ClassicAssist.UO.Data;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class SkillCommands
    {
        [CommandsDisplay( Category = "Skills", Description = "Invokes the given skill name.",
            InsertText = "UseSkill(\"Hiding\")" )]
        public static void UseSkill( string skill )
        {
            try
            {
                Skill sk = ResolveSkillName( skill );

                UOC.UseSkill( sk );
            }
            catch ( ArgumentOutOfRangeException )
            {
                UOC.SystemMessage( string.Format( Strings.Invalid_skill_name___0__, skill ) );
            }
        }

        private static Skill ResolveSkillName( string skill )
        {
            skill = skill.ToLower();

            string[] enumNames = typeof( Skill ).GetEnumNames();

            foreach ( string t in enumNames )
            {
                string normalizedName = t.Replace( '_', ' ' ).ToLower();

                if ( skill.Equals( normalizedName ) )
                {
                    Skill s = (Skill) Enum.Parse( typeof( Skill ), t );

                    return s;
                }
            }

            throw new ArgumentOutOfRangeException();
        }
    }
}
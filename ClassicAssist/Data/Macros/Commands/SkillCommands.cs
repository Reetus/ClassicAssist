using System;
using System.Linq;
using ClassicAssist.Data.Skills;
using ClassicAssist.Misc;
using ClassicAssist.Resources;
using ClassicAssist.UO.Data;
using Skill = ClassicAssist.UO.Data.Skill;
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

                if ( !skill.Equals( normalizedName ) )
                {
                    continue;
                }

                Skill s = (Skill) Enum.Parse( typeof( Skill ), t );

                return s;
            }

            throw new ArgumentOutOfRangeException();
        }

        [CommandsDisplay( Category = "Skills", Description = "Returns the base value of the given skill name.",
            InsertText = "if Skill(\"hiding\") < 100:" )]
        public static double Skill( string name )
        {
            SkillManager manager = SkillManager.GetInstance();

            SkillEntry s = manager.Items.FirstOrDefault( se => se.Skill.Name.ToLower().Contains( name.ToLower() ) );

            return s?.Value ?? 0;
        }

        [CommandsDisplay( Category = "Skills",
            Description = "Returns the lock status of the given skill, up, down, or locked.",
            InsertText = "if SkillState(\"hiding\') == \"locked\":" )]
        public static string SkillState( string name )
        {
            SkillManager manager = SkillManager.GetInstance();

            SkillEntry s = manager.Items.FirstOrDefault( se => se.Skill.Name.ToLower().Contains( name.ToLower() ) );

            return s?.LockStatus.ToString().ToLower() ?? "up";
        }

        [CommandsDisplay( Category = "Skills",
            Description = "Sets the lock state of the given skill, up, down or locked.",
            InsertText = "SetSkill(\"hiding\", \"locked\")" )]
        public static void SetSkill( string skill, string status )
        {
            SkillManager manager = SkillManager.GetInstance();

            SkillEntry s = manager.Items.FirstOrDefault( se => se.Skill.Name.ToLower().Contains( skill.ToLower() ) );

            if ( s == null )
            {
                UOC.SystemMessage( string.Format( Strings.Invalid_skill_name___0__, skill ) );
                return;
            }

            LockStatus ls = Utility.GetEnumValueByName<LockStatus>( status );

            UOC.ChangeSkillLock( s, ls );
        }
    }
}
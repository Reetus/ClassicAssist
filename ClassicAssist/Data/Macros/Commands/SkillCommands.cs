using System.Collections.Generic;
using System.Linq;
using Assistant;
using ClassicAssist.Data.Skills;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.Packets;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class SkillCommands
    {
        [CommandsDisplay( Category = nameof( Strings.Skills ),
            Parameters = new[] { nameof( ParameterType.SkillName ) } )]
        public static void UseSkill( string skill )
        {
            int id = GetSkillID( skill );

            if ( id == -1 )
            {
                UOC.SystemMessage( string.Format( Strings.Invalid_skill_name___0__, skill ) );
                return;
            }

            Engine.SendPacketToServer( new UseSkill( id ) );
        }

        private static int GetSkillID( string skill )
        {
            skill = skill.ToLower().Replace( '_', ' ' );

            KeyValuePair<int, SkillData> sk = UO.Data.Skills.GetSkills()
                .FirstOrDefault( s => s.Value.Name.ToLower().Equals( skill ) );

            if ( sk.Value == null )
            {
                return -1;
            }

            return sk.Key;
        }

        [CommandsDisplay( Category = nameof( Strings.Skills ),
            Parameters = new[] { nameof( ParameterType.SkillName ) } )]
        public static double Skill( string name, bool baseSkill = false )
        {
            SkillManager manager = SkillManager.GetInstance();

            SkillEntry s = manager.Items.FirstOrDefault( se => se.Skill.Name.ToLower().Contains( name.ToLower() ) );

            return baseSkill ? s?.Base ?? 0 : s?.Value ?? 0;
        }

        [CommandsDisplay( Category = nameof( Strings.Skills ),
            Parameters = new[] { nameof( ParameterType.SkillName ) } )]
        public static double SkillDelta( string name )
        {
            SkillManager manager = SkillManager.GetInstance();

            SkillEntry s = manager.Items.FirstOrDefault( se => se.Skill.Name.ToLower().Contains( name.ToLower() ) );

            return s?.Delta ?? 0;
        }

        [CommandsDisplay( Category = nameof( Strings.Skills ),
            Parameters = new[] { nameof( ParameterType.SkillName ) } )]
        public static double SkillCap( string name )
        {
            SkillManager manager = SkillManager.GetInstance();

            SkillEntry s = manager.Items.FirstOrDefault( se => se.Skill.Name.ToLower().Contains( name.ToLower() ) );

            return s?.Cap ?? 0;
        }

        [CommandsDisplay( Category = nameof( Strings.Skills ),
            Parameters = new[] { nameof( ParameterType.SkillName ) } )]
        public static string SkillState( string name )
        {
            SkillManager manager = SkillManager.GetInstance();

            SkillEntry s = manager.Items.FirstOrDefault( se => se.Skill.Name.ToLower().Contains( name.ToLower() ) );

            return s?.LockStatus.ToString().ToLower() ?? "up";
        }

        [CommandsDisplay( Category = nameof( Strings.Skills ),
            Parameters = new[] { nameof( ParameterType.SkillName ), nameof( ParameterType.UpDownLocked ) } )]
        [CommandsDisplayStringSeeAlso( new[] { null, nameof( LockStatus ) } )]
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

        [CommandsDisplay( Category = nameof( Strings.Skills ),
            Parameters = new[] { nameof( ParameterType.String ), nameof( ParameterType.UpDownLocked ) } )]
        [CommandsDisplayStringSeeAlso( new[] { nameof( StatType ), nameof( LockStatus ) } )]
        public static void SetStatus( string stat, string lockStatus )
        {
            StatType st = Utility.GetEnumValueByName<StatType>( stat );
            LockStatus ls = Utility.GetEnumValueByName<LockStatus>( lockStatus );

            UOC.ChangeStatLock( st, ls );
        }

        [CommandsDisplay( Category = nameof( Strings.Skills ) )]
        public static void UseLastSkill()
        {
            Engine.SendPacketToServer( new UseSkill( Engine.LastSkillID ) );
        }
    }
}
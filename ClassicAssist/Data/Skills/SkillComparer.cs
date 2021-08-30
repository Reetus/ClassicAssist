﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using ClassicAssist.UI.Misc;

namespace ClassicAssist.Data.Skills
{
    public class SkillComparer : IComparer<SkillEntry>, IComparer
    {
        private readonly ListSortDirection _direction;
        private readonly SkillsGridViewColumn.Enums _sortField;

        public SkillComparer( ListSortDirection direction, SkillsGridViewColumn.Enums sortField )
        {
            _direction = direction;
            _sortField = sortField;
        }

        public int Compare( object x, object y )
        {
            if ( !( x is SkillEntry first ) || !( y is SkillEntry second ) )
            {
                return 0;
            }

            return Compare( first, second );
        }

        public int Compare( SkillEntry first, SkillEntry second )
        {
            if ( first == null || second == null )
            {
                return -1;
            }

            int result;

            switch ( _sortField )
            {
                case SkillsGridViewColumn.Enums.Name:
                    result = string.Compare( first.Skill.Name, second.Skill.Name, StringComparison.Ordinal );
                    break;
                case SkillsGridViewColumn.Enums.Value:
                    result = first.Value.CompareTo( second.Value );
                    break;
                case SkillsGridViewColumn.Enums.Base:
                    result = first.Base.CompareTo( second.Base );
                    break;
                case SkillsGridViewColumn.Enums.Delta:
                    result = first.Delta.CompareTo( second.Delta );

                    if ( result == 0 )
                    {
                        result = first.Base.CompareTo( second.Base );
                    }

                    break;
                case SkillsGridViewColumn.Enums.Cap:
                    result = first.Cap.CompareTo( second.Cap );
                    break;
                case SkillsGridViewColumn.Enums.LockStatus:
                    result = first.LockStatus.CompareTo( second.LockStatus );
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if ( _direction == ListSortDirection.Descending )
            {
                result = -result;
            }

            if ( result == 0 )
            {
                result = string.Compare( first.Skill.Name, second.Skill.Name, StringComparison.Ordinal );
            }

            return result;
        }
    }
}
// Copyright (C) $CURRENT_YEAR$ Reetus
//  
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ClassicAssist.Data.Enemies
{
    public class EnemyEntry
    {
        public string Name { get; set; }
        public int Serial { get; set; }

        public override string ToString()
        {
            return $"{Name}: 0x{Serial:x}";
        }

        public override bool Equals( object obj )
        {
            if ( !( obj is EnemyEntry fe ) )
            {
                return false;
            }

            return Equals( fe );
        }

        protected bool Equals( EnemyEntry other )
        {
            return Serial == other.Serial;
        }

        public override int GetHashCode()
        {
            return Serial.GetHashCode();
        }
    }
}
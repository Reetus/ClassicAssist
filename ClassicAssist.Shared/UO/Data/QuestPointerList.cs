#region License

// Copyright (C) 2020 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ClassicAssist.Shared;
using ClassicAssist.UO.Network.Packets;

namespace ClassicAssist.UO.Data
{
    public enum QuestPointerType
    {
        Resurrection,
        Corpse,
        Other
    }

    public class QuestPointerList : IEnumerable<QuestPointer>
    {
        private readonly object _lock = new object();
        private readonly List<QuestPointer> _questPointers = new List<QuestPointer>();

        public IEnumerator<QuestPointer> GetEnumerator()
        {
            return _questPointers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add( QuestPointer questPointer )
        {
            lock ( _lock )
            {
                _questPointers.Add( questPointer );
            }

            Engine.SendPacketToClient( new DisplayQuestPointer( true, questPointer.X, questPointer.Y,
                questPointer.Serial ) );
        }

        public void Remove( QuestPointer questPointer )
        {
            lock ( _lock )
            {
                _questPointers.Remove( questPointer );
            }

            Engine.SendPacketToClient( new DisplayQuestPointer( false, questPointer.X, questPointer.Y,
                questPointer.Serial ) );
        }

        public void Clear()
        {
            Remove( _questPointers );
        }

        public void Remove( IEnumerable<QuestPointer> questPointers )
        {
            foreach ( QuestPointer questPointer in questPointers.ToArray() )
            {
                Remove( questPointer );
            }
        }

        public void RemoveByType( QuestPointerType type )
        {
            IEnumerable<QuestPointer> pointers = _questPointers.Where( p => p.Type == type );

            Remove( pointers );
        }
    }

    public class QuestPointer
    {
        public int Serial { get; set; }
        public QuestPointerType Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
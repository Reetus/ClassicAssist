#region License

// Copyright (C) 2021 Reetus
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

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ClassicAssist.Plugin.Shared.Reflection.ClassicUO.Objects
{
    public class MacroManager : LinkedObject<Macro, Macro>
    {
        public MacroManager( object obj ) : base( obj )
        {
        }

        public override Macro Items => WrapField<Macro>();
        public override Macro Next => null;

        public override Macro Previous => null;
        public bool WaitingBandageTarget => WrapField<bool>();

        public IEnumerable<Macro> GetAllMacros()
        {
            List<Macro> list = new List<Macro>();

            Macro items = Items;

            do
            {
                if ( items == null )
                {
                    break;
                }

                list.Add( items );

                items = items.Next;
            }
            while ( items != null );

            return list;
        }

        public void PushToBack( Macro macro )
        {
            ( (dynamic) this ).PushToBack( macro.AssociatedObject );
        }

        public void PlayMacro( Macro macro )
        {
            FieldInfo fieldInfo = macro.AssociatedObject.GetType().GetFields().FirstOrDefault( e => e.Name == "Items" );

            object items = fieldInfo?.GetValue( macro.AssociatedObject );

            if ( items != null )
            {
                PropertyInfo codeProperty = items.GetType().GetProperties().FirstOrDefault( e => e.Name == "Code" );
                PropertyInfo subCodeProperty =
                    items.GetType().GetProperties().FirstOrDefault( e => e.Name == "SubCode" );

                if ( codeProperty != null && subCodeProperty != null )
                {
                    int code = (int) codeProperty.GetValue( items );

                    int subCode = (int) subCodeProperty.GetValue( items );

                    if ( code == 5 /*Walk*/ )
                    {
                        ReflectionImpl.Move( subCode, false );
                        return;
                    }
                }
            }

            ReflectionImpl.TickWorkQueue.Enqueue( () =>
            {
                ( (dynamic) this ).SetMacroToExecute( items );
                ( (dynamic) this ).WaitingBandageTarget = false;
                ( (dynamic) this ).WaitForTargetTimer = 0;
                ( (dynamic) this ).Update();
            } );
        }
    }
}
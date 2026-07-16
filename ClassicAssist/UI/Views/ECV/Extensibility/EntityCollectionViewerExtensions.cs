#region License

// Copyright (C) 2026 Reetus
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

using System;
using System.Collections.Generic;

namespace ClassicAssist.UI.Views.ECV.Extensibility
{
    /// <summary>
    ///     Registry of custom toolbar actions contributed by additional assemblies to the
    ///     Entity Collection Viewer. Register actions from your assembly's static
    ///     <c>Initialize()</c> method; each Entity Collection Viewer instance reads the
    ///     registered actions when it is opened.
    /// </summary>
    public static class EntityCollectionViewerExtensions
    {
        private static readonly object _lock = new object();
        private static readonly List<IEntityCollectionViewerAction> _toolbarActions = new List<IEntityCollectionViewerAction>();

        /// <summary>
        ///     A snapshot of the currently registered toolbar actions.
        /// </summary>
        public static IReadOnlyList<IEntityCollectionViewerAction> ToolbarActions
        {
            get
            {
                lock ( _lock )
                {
                    return _toolbarActions.ToArray();
                }
            }
        }

        /// <summary>
        ///     Register a toolbar action. Has no effect if <paramref name="action" /> is already registered.
        /// </summary>
        public static void RegisterToolbarAction( IEntityCollectionViewerAction action )
        {
            if ( action == null )
            {
                throw new ArgumentNullException( nameof( action ) );
            }

            lock ( _lock )
            {
                if ( !_toolbarActions.Contains( action ) )
                {
                    _toolbarActions.Add( action );
                }
            }
        }

        /// <summary>
        ///     Remove a previously registered toolbar action.
        /// </summary>
        public static bool UnregisterToolbarAction( IEntityCollectionViewerAction action )
        {
            lock ( _lock )
            {
                return _toolbarActions.Remove( action );
            }
        }
    }
}

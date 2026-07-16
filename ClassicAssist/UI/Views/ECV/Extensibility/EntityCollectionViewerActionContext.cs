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
using System.Threading;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UI.Views.ECV.Extensibility
{
    /// <summary>
    ///     Default <see cref="IEntityCollectionViewerContext" /> implementation built by the
    ///     Entity Collection Viewer view model when invoking a custom action. Captures a
    ///     snapshot of the current collection/selection and forwards Refresh/EnqueueAction
    ///     back to the view model.
    /// </summary>
    internal class EntityCollectionViewerActionContext : IEntityCollectionViewerContext
    {
        private readonly Action<Func<CancellationToken, bool>, string> _enqueue;
        private readonly Action _refresh;

        public EntityCollectionViewerActionContext( ItemCollection collection, Item[] selectedItems, bool showProperties,
            Action refresh, Action<Func<CancellationToken, bool>, string> enqueue )
        {
            Collection = collection;
            SelectedItems = selectedItems ?? Array.Empty<Item>();
            ShowProperties = showProperties;
            _refresh = refresh;
            _enqueue = enqueue;
        }

        public ItemCollection Collection { get; }

        public Item[] SelectedItems { get; }

        public bool ShowProperties { get; }

        public void Refresh()
        {
            _refresh?.Invoke();
        }

        public void EnqueueAction( Func<CancellationToken, bool> action, string status )
        {
            _enqueue?.Invoke( action, status );
        }
    }
}

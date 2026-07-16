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
    ///     Snapshot of the Entity Collection Viewer state passed to an
    ///     <see cref="IEntityCollectionViewerAction" /> when it is invoked.
    /// </summary>
    public interface IEntityCollectionViewerContext
    {
        /// <summary>
        ///     The full set of items currently shown in the viewer.
        /// </summary>
        ItemCollection Collection { get; }

        /// <summary>
        ///     The items currently selected in the viewer. Empty array if nothing is selected.
        /// </summary>
        Item[] SelectedItems { get; }

        /// <summary>
        ///     Whether the viewer is showing full item properties (<c>true</c>) or just item
        ///     names (<c>false</c>) — the state of the viewer's "toggle item properties" button.
        /// </summary>
        bool ShowProperties { get; }

        /// <summary>
        ///     Refresh the viewer's contents.
        /// </summary>
        void Refresh();

        /// <summary>
        ///     Queue a long-running action on the viewer's action queue, surfacing
        ///     <paramref name="status" /> in the viewer's status list with a cancel button.
        ///     The <see cref="CancellationToken" /> passed to <paramref name="action" /> is
        ///     signalled when the user presses cancel; observe it in long-running work.
        ///     Return <c>false</c> from <paramref name="action" /> to indicate cancellation.
        /// </summary>
        void EnqueueAction( Func<CancellationToken, bool> action, string status );
    }
}

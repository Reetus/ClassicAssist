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

using System.Windows.Media;

namespace ClassicAssist.UI.Views.ECV.Extensibility
{
    /// <summary>
    ///     A custom toolbar action that an additional assembly can register against the
    ///     Entity Collection Viewer via
    ///     <see cref="EntityCollectionViewerExtensions.RegisterToolbarAction" />, typically
    ///     from its static <c>Initialize()</c> method.
    /// </summary>
    public interface IEntityCollectionViewerAction
    {
        /// <summary>
        ///     Displayed as the toolbar button's tooltip.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Icon shown on the toolbar button. Should be a frozen <see cref="ImageSource" />
        ///     (e.g. a <see cref="DrawingImage" />) so it is safe to render regardless of which
        ///     thread it was created on. May be <c>null</c>.
        /// </summary>
        ImageSource Icon { get; }

        /// <summary>
        ///     Whether the action can run for the given context. Controls the button's enabled
        ///     state; re-queried by WPF as the selection changes.
        /// </summary>
        bool CanExecute( IEntityCollectionViewerContext context );

        /// <summary>
        ///     Invoked when the toolbar button is clicked. Runs on the UI thread; use
        ///     <see cref="IEntityCollectionViewerContext.EnqueueAction" /> for long-running work.
        /// </summary>
        void Execute( IEntityCollectionViewerContext context );
    }
}

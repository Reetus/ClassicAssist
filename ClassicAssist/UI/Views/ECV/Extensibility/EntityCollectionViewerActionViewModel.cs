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
using System.Windows.Input;
using System.Windows.Media;
using ClassicAssist.Shared.UI;

namespace ClassicAssist.UI.Views.ECV.Extensibility
{
    /// <summary>
    ///     Toolbar-button wrapper around a registered <see cref="IEntityCollectionViewerAction" />.
    ///     Binds Name/Icon for display and invokes the action against a freshly built context.
    /// </summary>
    public class EntityCollectionViewerActionViewModel
    {
        private readonly IEntityCollectionViewerAction _action;
        private readonly Func<IEntityCollectionViewerContext> _contextFactory;
        private ICommand _executeCommand;

        public EntityCollectionViewerActionViewModel( IEntityCollectionViewerAction action,
            Func<IEntityCollectionViewerContext> contextFactory )
        {
            _action = action;
            _contextFactory = contextFactory;
        }

        public string Name => _action.Name;

        public ImageSource Icon => _action.Icon;

        public ICommand ExecuteCommand =>
            _executeCommand ?? ( _executeCommand = new RelayCommand( OnExecute, OnCanExecute ) );

        private void OnExecute( object o )
        {
            _action.Execute( _contextFactory() );
        }

        private bool OnCanExecute( object o )
        {
            try
            {
                return _action.CanExecute( _contextFactory() );
            }
            catch ( Exception )
            {
                return false;
            }
        }
    }
}

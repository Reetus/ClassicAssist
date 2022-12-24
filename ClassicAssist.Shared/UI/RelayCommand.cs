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

using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClassicAssist.Shared.UI
{
    public class RelayCommand : ICommand
    {
        private readonly Func<object, bool> _canExecute;
        private readonly Action<object> _execute;

        public RelayCommand( Action<object> execute, Func<object, bool> canExecute = null )
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
                CanExecuteChangedInternal += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
                CanExecuteChangedInternal -= value;
            }
        }

        public bool CanExecute( object parameter )
        {
            return _canExecute == null || _canExecute( parameter );
        }

        public void Execute( object parameter )
        {
            _execute( parameter );
        }

        private event EventHandler CanExecuteChangedInternal;

        // ReSharper disable once UnusedMember.Global
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChangedInternal?.Invoke( this, EventArgs.Empty );
        }
    }

    public class RelayCommandAsync : ICommand
    {
        private readonly Func<object, bool> canExecuteMethod;
        private readonly Func<object, Task> executedMethod;

        public RelayCommandAsync( Func<object, Task> execute, Func<object, bool> canExecute )
        {
            executedMethod = execute ?? throw new ArgumentNullException( nameof( execute ) );
            canExecuteMethod = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute( object parameter )
        {
            return canExecuteMethod == null || canExecuteMethod( parameter );
        }

        public async void Execute( object parameter )
        {
            await executedMethod( parameter );
        }
    }
}
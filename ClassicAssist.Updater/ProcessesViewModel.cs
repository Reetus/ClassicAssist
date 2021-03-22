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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Input;
using ClassicAssist.Updater.Annotations;
using ClassicAssist.Updater.Misc;

namespace ClassicAssist.Updater
{
    public class ProcessesViewModel : INotifyPropertyChanged
    {
        private ICommand _okCommand;
        private ObservableCollection<Process> _processes = new ObservableCollection<Process>();

        public ProcessesViewModel()
        {
        }

        public ProcessesViewModel( IEnumerable<Process> processes )
        {
            Processes = new ObservableCollection<Process>( processes );
        }

        public DialogResult DialogResult { get; set; } = DialogResult.Cancel;

        public ICommand OKCommand => _okCommand ?? ( _okCommand = new RelayCommand( OK, o => true ) );

        public ObservableCollection<Process> Processes
        {
            get => _processes;
            set => SetProperty( ref _processes, value );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OK( object obj )
        {
            DialogResult = DialogResult.OK;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        // ReSharper disable once RedundantAssignment
        private void SetProperty<T>( ref T obj, T value, [CallerMemberName] string propertyName = null )
        {
            obj = value;
            OnPropertyChanged( propertyName );
        }
    }
}
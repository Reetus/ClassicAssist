#region License

// Copyright (C) 2025 Reetus
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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ClassicAssist.Data.Macros;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.ViewModels;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UI.Views.Macros
{
    /// <summary>
    ///     Interaction logic for MacrosDebuggerControl.xaml
    /// </summary>
    public partial class MacrosDebuggerControl : INotifyPropertyChanged
    {
        public static readonly DependencyProperty MacroEntryProperty = DependencyProperty.Register( nameof( MacroEntry ), typeof( MacroEntry ), typeof( MacrosDebuggerControl ),
            new PropertyMetadata( default( MacroEntry ) ) );

        public static readonly DependencyProperty OverlayWidthProperty =
            DependencyProperty.Register( nameof( OverlayWidth ), typeof( double ), typeof( MacrosDebuggerControl ), new PropertyMetadata( 250.0 ) );

        private ICommand _clipboardCopyCommand;

        private ICommand _resumeCommand;
        private KeyValuePair<string, object> _selectedItem;
        private double _startWidth;
        private ICommand _stepCommand;
        private ICommand _stopCommand;
        private ICommand _itemDoubleClickCommand;

        public MacrosDebuggerControl()
        {
            InitializeComponent();

            ResizeThumb.DragStarted += ResizeThumb_DragStarted;
            ResizeThumb.DragDelta += ResizeThumb_DragDelta;
        }

        public ICommand ClipboardCopyCommand => _clipboardCopyCommand ?? ( _clipboardCopyCommand = new RelayCommand( ClipboardCopy ) );

        public MacroEntry MacroEntry
        {
            get => (MacroEntry) GetValue( MacroEntryProperty );
            set => SetValue( MacroEntryProperty, value );
        }

        public double OverlayWidth
        {
            get => (double) GetValue( OverlayWidthProperty );
            set => SetValue( OverlayWidthProperty, value );
        }

        public ICommand ResumeCommand => _resumeCommand ?? ( _resumeCommand = new RelayCommand( Resume ) );

        public KeyValuePair<string, object> SelectedItem
        {
            get => _selectedItem;
            set => SetField( ref _selectedItem, value );
        }

        public ICommand StepCommand => _stepCommand ?? ( _stepCommand = new RelayCommand( Step ) );

        public ICommand StopCommand => _stopCommand ?? ( _stopCommand = new RelayCommand( Stop ) );
        public ICommand ItemDoubleClickCommand => _itemDoubleClickCommand ?? ( _itemDoubleClickCommand = new RelayCommandAsync( ItemDoubleClick, o => o != null ) );

        private static Task ItemDoubleClick( object obj )
        {
            if ( !( obj is KeyValuePair<string, object> kvp ) || !( kvp.Value is Entity entity ) )
            {
                return Task.CompletedTask;
            }

            Thread t = new Thread( () =>
                {
                    ObjectInspectorWindow window =
                        new ObjectInspectorWindow { DataContext = new ObjectInspectorViewModel( entity ) };

                    window.ShowDialog();
                } )
                { IsBackground = true };

            t.SetApartmentState( ApartmentState.STA );
            t.Start();

            return Task.CompletedTask;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ClipboardCopy( object obj )
        {
            Clipboard.SetText( MacroInvoker.GetDisplayValue( SelectedItem.Value, true ) );
        }

        private void Resume( object obj )
        {
            MacroEntry?.Resume();
        }

        private void Stop( object obj )
        {
            MacroEntry?.Stop();
        }

        private void Step( object obj )
        {
            MacroEntry?.Step();
        }

        private void ResizeThumb_DragStarted( object sender, DragStartedEventArgs e )
        {
            _startWidth = OverlayWidth;
        }

        private void ResizeThumb_DragDelta( object sender, DragDeltaEventArgs e )
        {
            // Move left increases width, move right decreases
            double delta = -e.HorizontalChange;
            double newWidth = _startWidth + delta;

            OverlayWidth = newWidth;
        }

        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        protected bool SetField<T>( ref T field, T value, [CallerMemberName] string propertyName = null )
        {
            if ( EqualityComparer<T>.Default.Equals( field, value ) )
            {
                return false;
            }

            field = value;
            OnPropertyChanged( propertyName );
            return true;
        }
    }
}
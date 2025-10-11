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

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using ClassicAssist.Shared.UI;

namespace ClassicAssist.Controls
{
    /// <summary>
    ///     Interaction logic for MultiValueSelector.xaml
    /// </summary>
    public partial class MultiValueSelector : INotifyPropertyChanged
    {
        public static readonly DependencyProperty ValuesProperty = DependencyProperty.Register( nameof( Values ), typeof( ObservableCollection<int> ), typeof( MultiValueSelector ),
            new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValuesChanged ) );

        public static readonly DependencyProperty ButtonsProperty = DependencyProperty.Register(nameof(Buttons), typeof(object), typeof(MultiValueSelector));

        public object Buttons
        {
            get => GetValue(ButtonsProperty);
            set => SetValue(ButtonsProperty, value);
        }

        private RelayCommand _removeItemCommand;

        public MultiValueSelector()
        {
            InitializeComponent();
        }

        public ICommand RemoveItemCommand =>
            _removeItemCommand ?? ( _removeItemCommand = new RelayCommand( v =>
            {
                if ( !( v is int value ) )
                {
                    return;
                }

                Values.Remove( value );
            } ) );

        public ObservableCollection<int> Values
        {
            get => (ObservableCollection<int>) GetValue( ValuesProperty );
            set => SetValue( ValuesProperty, value );
        }

        public string ValuesDisplay => string.Join( ", ", Values?.Select( v => $"0x{v:x}" ) ?? Array.Empty<string>() );

        public event PropertyChangedEventHandler PropertyChanged;

        private static void OnValuesChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            if ( e.NewValue == null )
            {
                return;
            }

            if ( !( d is MultiValueSelector selector ) )
            {
                return;
            }

            selector.OnPropertyChanged( nameof( ValuesDisplay ) );

            if ( e.NewValue is ObservableCollection<int> newCollection )
            {
                newCollection.CollectionChanged += ( s, args ) => selector.OnPropertyChanged( nameof( ValuesDisplay ) );
            }
        }

        private void Button_Click( object sender, RoutedEventArgs e )
        {
            Popup.IsOpen = !Popup.IsOpen;
        }

        private void TextBox_PreviewKeyDown( object sender, KeyEventArgs e )
        {
            if ( e.Key != Key.Enter )
            {
                return;
            }

            if ( Values == null )
            {
                Values = new ObservableCollection<int>();
            }

            string text = ( (TextBox) sender ).Text;

            if ( text.StartsWith( "0x" ) )
            {
                int value = Convert.ToInt32( text, 16 );

                if ( !Values.Contains( value ) )
                {
                    Values.Add( value );
                }
            }
            else
            {
                if ( int.TryParse( text, out int value ) && !Values.Contains( value ) )
                {
                    Values.Add( value );
                }
            }

            ( (TextBox) sender ).Clear();

            e.Handled = true;
        }

        private void Popup_Opened( object sender, EventArgs e )
        {
            Dispatcher.BeginInvoke( new Action( () =>
            {
                TextBox.Focus();
                Keyboard.Focus( TextBox );
            } ), DispatcherPriority.Input );
        }

        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }
}
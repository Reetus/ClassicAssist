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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace ClassicAssist.UI.Misc.Behaviours
{
    public class ComboBoxBehavior : Behavior<ComboBox>
    {
        public static readonly DependencyProperty CommandBindingProperty = DependencyProperty.Register( nameof( CommandBinding ), typeof( ICommand ), typeof( ComboBoxBehavior ),
            new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None, PropertyChangedCallback ) );

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register( nameof( CommandParameter ), typeof( object ), typeof( ComboBoxBehavior ),
            new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None, PropertyChangedCallback ) );

        public static readonly DependencyProperty OnlyUserTriggeredProperty = DependencyProperty.Register( nameof( OnlyUserTriggered ), typeof( bool ), typeof( ComboBoxBehavior ),
            new FrameworkPropertyMetadata( false, FrameworkPropertyMetadataOptions.None, PropertyChangedCallback ) );

        private bool _userTriggered;

        public ICommand CommandBinding
        {
            get => (ICommand) GetValue( CommandBindingProperty );
            set => SetValue( CommandBindingProperty, value );
        }

        public object CommandParameter
        {
            get => GetValue( CommandParameterProperty );
            set => SetValue( CommandParameterProperty, value );
        }

        public bool OnlyUserTriggered
        {
            get => (bool) GetValue( OnlyUserTriggeredProperty );
            set => SetValue( OnlyUserTriggeredProperty, value );
        }

        private static void PropertyChangedCallback( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectionChanged += OnSelectionChanged;
            AssociatedObject.PreviewMouseDown += OnPreviewMouseDown;
        }

        private void OnPreviewMouseDown( object sender, MouseButtonEventArgs e )
        {
            _userTriggered = true;
        }

        private void OnSelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            if ( OnlyUserTriggered && !_userTriggered )
            {
                return;
            }

            CommandBinding?.Execute( AssociatedObject.SelectedItem );
            _userTriggered = false;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SelectionChanged -= OnSelectionChanged;
            AssociatedObject.PreviewMouseDown -= OnPreviewMouseDown;
        }
    }
}
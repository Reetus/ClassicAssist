#region License

// Copyright (C) 2020 Reetus
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

namespace ClassicAssist.Browser.Controls
{
    public class ClearableComboBox : ComboBox
    {
        public static readonly DependencyProperty ClearCommandProperty =
            DependencyProperty.Register( "ClearCommand", typeof( ICommand ), typeof( ClearableComboBox ) );

        public static readonly DependencyProperty ClearCommandParameterProperty =
            DependencyProperty.Register( "ClearCommandParameter", typeof( object ), typeof( ClearableComboBox ),
                new UIPropertyMetadata( null ) );

        public ICommand ClearCommand
        {
            get => (ICommand) GetValue( ClearCommandProperty );
            set => SetValue( ClearCommandProperty, value );
        }

        public object ClearCommandParameter
        {
            get => GetValue( ClearCommandParameterProperty );
            set => SetValue( ClearCommandParameterProperty, value );
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Button button = (Button) GetTemplateChild( "ClearButton" );

            if ( button != null )
            {
                button.Click += ClearButtonOnClick;
            }
        }

        private void ClearButtonOnClick( object sender, RoutedEventArgs e )
        {
            SelectedItem = null;
            ClearCommand?.Execute( ClearCommandParameter );
        }
    }
}
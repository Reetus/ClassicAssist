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

using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace ClassicAssist.Shared.UI.Behaviours
{
    public class WindowClosingBehaviour : Behavior<Window>
    {
        public static DependencyProperty CommandProperty = DependencyProperty.Register( "Command", typeof( ICommand ),
            typeof( WindowClosingBehaviour ),
            new FrameworkPropertyMetadata( default, FrameworkPropertyMetadataOptions.None ) );

        public ICommand Command
        {
            get => (ICommand) GetValue( CommandProperty );
            set => SetValue( CommandProperty, value );
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Closing += OnClosing;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.Closing -= OnClosing;
        }

        private void OnClosing( object sender, CancelEventArgs e )
        {
            Command?.Execute( sender );
        }
    }
}
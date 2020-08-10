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

using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit.Utils;

namespace ClassicAssist.Avalonia.Misc
{
    public class CloseOnClickBehaviour : Behavior<Button>
    {
        public static readonly DirectProperty<CloseOnClickBehaviour, ICommand> CommandProperty =
            AvaloniaProperty.RegisterDirect<CloseOnClickBehaviour, ICommand>( nameof( Command ), o => o.Command,
                ( o, v ) => o.Command = v, default, BindingMode.TwoWay );

        public static readonly DirectProperty<CloseOnClickBehaviour, object> CommandParameterProperty =
            AvaloniaProperty.RegisterDirect<CloseOnClickBehaviour, object>( nameof( CommandParameter ),
                o => o.CommandParameter, ( o, v ) => o.CommandParameter = v, default, BindingMode.TwoWay );

        private ICommand _command;
        private object _commandParameter;

        public ICommand Command
        {
            get => _command;
            set => SetAndRaise( CommandProperty, ref _command, value );
        }

        public object CommandParameter
        {
            get => _commandParameter;
            set => SetAndRaise( CommandParameterProperty, ref _commandParameter, value );
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            if ( AssociatedObject != null )
            {
                AssociatedObject.Click += OnPointerPressed;
            }
        }

        private void OnPointerPressed( object sender, RoutedEventArgs e )
        {
            if ( !( sender is Button button ) )
            {
                return;
            }

            Window window = button.VisualAncestorsAndSelf().OfType<Window>().FirstOrDefault();

            Command?.Execute( CommandParameter );

            window?.Close();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if ( AssociatedObject != null )
            {
                AssociatedObject.Click -= OnPointerPressed;
            }
        }
    }
}
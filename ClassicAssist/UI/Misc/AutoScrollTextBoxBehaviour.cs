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

using System.Windows.Controls;
using System.Windows.Interactivity;

namespace ClassicAssist.UI.Misc
{
    public class AutoScrollTextBoxBehaviour : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.TextChanged += OnTextChanged;
        }

        private void OnTextChanged( object sender, TextChangedEventArgs e )
        {
            AssociatedObject.ScrollToEnd();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if ( AssociatedObject != null )
            {
                AssociatedObject.TextChanged -= OnTextChanged;
            }
        }
    }
}
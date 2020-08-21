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

using System;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using ClassicAssist.Data;
using ReactiveUI;

namespace ClassicAssist.Avalonia.Misc
{
    public class WindowResizeBehaviour : Behavior<Window>
    {
        private IDisposable _subscription;

        protected override void OnAttached()
        {
            base.OnAttached();

#if !DEVELOP
            if ( AssociatedObject != null )
            {
                AssociatedObject.Width = AssistantOptions.WindowWidth;
                AssociatedObject.Height = AssistantOptions.WindowHeight;
            }
#endif

            _subscription = AssociatedObject.WhenAnyValue( e => e.ClientSize, e => SetSize( e ) ).Subscribe();

            AssistantOptions.OptionsLoaded += OnOptionsLoaded;
        }

        private static Unit SetSize( in Size size )
        {
#if !DEVELOP
            AssistantOptions.WindowHeight = size.Height;
            AssistantOptions.WindowWidth = size.Width;
#endif
            return Unit.Default;
        }

        private void OnOptionsLoaded( object sender, EventArgs e )
        {
            if ( AssociatedObject == null )
            {
                return;
            }

#if !DEVELOP
            AssociatedObject.Width = AssistantOptions.WindowWidth;
            AssociatedObject.Height = AssistantOptions.WindowHeight;
#endif
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            _subscription.Dispose();
        }
    }
}
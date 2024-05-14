#region License

// Copyright (C) 2024 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using System.Windows;
using System.Windows.Controls;
using ClassicAssist.Misc;
using Microsoft.Xaml.Behaviors;

namespace ClassicAssist.UI.Views.ECV.Settings.Misc
{
    public class SetBindingProxyBehaviour : Behavior<UserControl>
    {
        public static readonly DependencyProperty ProxyProperty =
            DependencyProperty.Register( nameof( Proxy ), typeof( BindingProxy ), typeof( SetBindingProxyBehaviour ), new PropertyMetadata( null ) );

        public BindingProxy Proxy
        {
            get => (BindingProxy) GetValue( ProxyProperty );
            set => SetValue( ProxyProperty, value );
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            if ( Proxy != null )
            {
                Proxy.Data = AssociatedObject;
            }
        }
    }
}
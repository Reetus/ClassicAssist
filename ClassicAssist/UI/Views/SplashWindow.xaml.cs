// Copyright (C) $CURRENT_YEAR$ Reetus
//  
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using ClassicAssist.Misc;

namespace ClassicAssist.UI.Views
{
    /// <summary>
    ///     Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : INotifyPropertyChanged
    {
        public ImageSource Image { get; set; }

        public SplashWindow()
        {
            InitializeComponent();

            Image = Properties.Resources.splash_logo.ToImageSource();
        }

        public event PropertyChangedEventHandler PropertyChanged;

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
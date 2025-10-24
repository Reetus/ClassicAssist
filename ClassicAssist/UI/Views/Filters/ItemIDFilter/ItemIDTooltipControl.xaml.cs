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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClassicAssist.Misc;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UI.Views.Filters.ItemIDFilter
{
    /// <summary>
    ///     Interaction logic for ItemIDTooltipControl.xaml
    /// </summary>
    public partial class ItemIDTooltipControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ItemIDProperty = DependencyProperty.Register( nameof( ItemID ), typeof( int ), typeof( ItemIDTooltipControl ),
            new FrameworkPropertyMetadata( 0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnItemIDChanged ) );

        public static readonly DependencyProperty HueProperty = DependencyProperty.Register( nameof( Hue ), typeof( int ), typeof( ItemIDTooltipControl ),
            new FrameworkPropertyMetadata( 0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnHueChanged ) );

        private ImageSource _image;
        private string _itemName;

        public ItemIDTooltipControl()
        {
            InitializeComponent();
        }

        public int Hue
        {
            get => (int) GetValue( HueProperty );
            set => SetValue( HueProperty, value );
        }

        public ImageSource Image
        {
            get => _image;
            set => SetField( ref _image, value );
        }

        public int ItemID
        {
            get => (int) GetValue( ItemIDProperty );
            set => SetValue( ItemIDProperty, value );
        }

        public string ItemName
        {
            get => _itemName;
            set => SetField( ref _itemName, value );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private static void OnHueChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            if ( !( d is ItemIDTooltipControl control ) )
            {
                return;
            }

            if ( e.NewValue != e.OldValue )
            {
                control.UpdateImage();
            }
        }

        private static void OnItemIDChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            if ( !( d is ItemIDTooltipControl control ) )
            {
                return;
            }

            if ( e.NewValue != e.OldValue )
            {
                control.UpdateImage();
            }
        }

        private void UpdateImage()
        {
            Image = Art.GetStatic( ItemID, Hue > 0 ? Hue : 0 ).ToImageSource();
            ItemName = TileData.GetStaticTile( ItemID ).Name;
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
// Copyright (C) 2024 Reetus
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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Shared.UI;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UI.Views.ECV.Settings.Controls.EditTextBlocks
{
    /// <summary>
    ///     Interaction logic for GraphicEditTextBlock.xaml
    /// </summary>
    public partial class GraphicEditTextBlock : INotifyPropertyChanged
    {
        public static readonly DependencyProperty IDProperty = DependencyProperty.Register( nameof( ID ), typeof( int ), typeof( GraphicEditTextBlock ),
            new FrameworkPropertyMetadata( -1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, IDChangedCallback ) );

        public static readonly DependencyProperty ClilocEditTextBlockProperty = DependencyProperty.Register( nameof( ClilocEditTextBlock ), typeof( ClilocEditTextBlock ),
            typeof( GraphicEditTextBlock ), new PropertyMetadata( null ) );

        public static readonly DependencyProperty HueEditTextBlockProperty = DependencyProperty.Register( nameof( HueEditTextBlock ), typeof( HueEditTextBlock ),
            typeof( GraphicEditTextBlock ), new PropertyMetadata( null ) );

        private string _label;
        private ICommand _targetCommand;

        public GraphicEditTextBlock()
        {
            InitializeComponent();

            UpdateLabel( this, ID );
        }

        public ClilocEditTextBlock ClilocEditTextBlock
        {
            get => (ClilocEditTextBlock) GetValue( ClilocEditTextBlockProperty );
            set => SetValue( ClilocEditTextBlockProperty, value );
        }

        public HueEditTextBlock HueEditTextBlock
        {
            get => (HueEditTextBlock) GetValue( HueEditTextBlockProperty );
            set => SetValue( HueEditTextBlockProperty, value );
        }

        public int ID
        {
            get => (int) GetValue( IDProperty );
            set => SetValue( IDProperty, value );
        }

        public string Label
        {
            get => _label;
            set => SetField( ref _label, value );
        }

        public ICommand TargetCommand => _targetCommand ?? ( _targetCommand = new RelayCommandAsync( Target, o => Engine.Connected ) );

        public event PropertyChangedEventHandler PropertyChanged;

        private static void IDChangedCallback( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            if ( !( e.NewValue is int id ) )
            {
                return;
            }

            if ( e.OldValue is int oldValue && oldValue == id )
            {
                return;
            }

            if ( !( d is GraphicEditTextBlock block ) )
            {
                return;
            }

            block.ID = id;
            UpdateLabel( block, id );
        }

        private static void UpdateLabel( GraphicEditTextBlock block, int id )
        {
            block.Label = id == -1 ? "Any" : $"0x{id:x8}";
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

        private async Task Target( object arg )
        {
            ( TargetType _, TargetFlags _, int serial, int _, int _, int _, int itemId ) = await Commands.GetTargetInfoAsync();

            if ( serial <= 0 )
            {
                return;
            }

            ID = itemId;

            Item item = Engine.Items.GetItem( serial );

            if ( item != null && itemId == 0 )
            {
                ID = item.ID;
            }

            if ( item != null && ClilocEditTextBlock != null && item.Properties != null )
            {
                ClilocEditTextBlock.Cliloc = item.Properties.FirstOrDefault()?.Cliloc ?? -1;
            }

            if ( item != null && HueEditTextBlock != null )
            {
                HueEditTextBlock.Hue = item.Hue;
            }
        }
    }
}
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
using System.Windows.Forms;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.ViewModels.Autoloot;
using ClassicAssist.UI.Views.Autoloot;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using UOCliloc = ClassicAssist.UO.Data.Cliloc;

namespace ClassicAssist.UI.Views.ECV.Settings.Controls.EditTextBlocks
{
    /// <summary>
    ///     Interaction logic for ClilocEditTextBlock.xaml
    /// </summary>
    public partial class ClilocEditTextBlock : INotifyPropertyChanged
    {
        public static readonly DependencyProperty ClilocProperty = DependencyProperty.Register( nameof( Cliloc ), typeof( int ), typeof( ClilocEditTextBlock ),
            new FrameworkPropertyMetadata( -1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ClilocChangedCallback ) );

        private ICommand _chooseClilocCommand;

        private string _label;
        private ICommand _targetCommand;

        public ClilocEditTextBlock()
        {
            InitializeComponent();

            UpdateLabel( this, Cliloc );
        }

        public ICommand ChooseClilocCommand => _chooseClilocCommand ?? ( _chooseClilocCommand = new RelayCommand( ChooseCliloc, o => true ) );

        public int Cliloc
        {
            get => (int) GetValue( ClilocProperty );
            set => SetValue( ClilocProperty, value );
        }

        public string Label
        {
            get => _label;
            set => SetField( ref _label, value );
        }

        public ICommand TargetCommand => _targetCommand ?? ( _targetCommand = new RelayCommandAsync( Target, o => Engine.Connected ) );

        public event PropertyChangedEventHandler PropertyChanged;

        private static void ClilocChangedCallback( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            if ( !( e.NewValue is int cliloc ) )
            {
                return;
            }

            if ( e.OldValue is int oldValue && oldValue == cliloc )
            {
                return;
            }

            if ( !( d is ClilocEditTextBlock block ) )
            {
                return;
            }

            block.Cliloc = cliloc;
            UpdateLabel( block, cliloc );
        }

        private async Task Target( object arg )
        {
            ( TargetType _, TargetFlags _, int serial, int _, int _, int _, int _ ) = await Commands.GetTargetInfoAsync();

            if ( serial <= 0 )
            {
                return;
            }

            Item item = Engine.Items.GetItem( serial );

            Cliloc = item.Properties?.Select( e => e.Cliloc ).FirstOrDefault() ?? -1;
        }

        private static void UpdateLabel( ClilocEditTextBlock block, int cliloc )
        {
            block.Label = cliloc == -1 ? "Any" : UOCliloc.GetProperty( cliloc );
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

        private void ChooseCliloc( object obj )
        {
            if ( !( obj is int ) )
            {
                return;
            }

            ClilocSelectionViewModel vm = new ClilocSelectionViewModel();
            ClilocSelectionWindow window = new ClilocSelectionWindow { DataContext = vm };

            window.ShowDialog();

            if ( vm.DialogResult != DialogResult.OK )
            {
                return;
            }

            Cliloc = vm.SelectedCliloc.Key;
        }
    }
}
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.Views.ECV.Settings.Models;
using ClassicAssist.UO;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UI.Views.ECV.Settings
{
    /// <summary>
    ///     Interaction logic for ContainerSetsSettingControl.xaml
    /// </summary>
    public partial class ContainerSetsSettingsControl : INotifyPropertyChanged
    {
        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register( nameof( Items ), typeof( ObservableCollection<ContainerSet> ),
            typeof( ContainerSetsSettingsControl ), new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

        private ICommand _addItemCommand;

        private ICommand _addSetCommand;
        private ICommand _identifyContainerCommand;
        private ICommand _removeItemCommand;
        private ICommand _removeSetCommand;
        private int _selectedItem;
        private ContainerSet _selectedSet;

        public ContainerSetsSettingsControl()
        {
            InitializeComponent();
        }

        public ICommand AddItemCommand => _addItemCommand ?? ( _addItemCommand = new RelayCommandAsync( AddItem, o => SelectedSet != null && Engine.Connected ) );

        public ICommand AddSetCommand => _addSetCommand ?? ( _addSetCommand = new RelayCommand( AddSet ) );

        public ICommand IdentifyContainersCommand =>
            _identifyContainerCommand ?? ( _identifyContainerCommand =
                new RelayCommandAsync( IdentifyContainers, o => ( o is int || o is ObservableCollection<int> ) && Engine.Connected ) );

        public ObservableCollection<ContainerSet> Items
        {
            get => (ObservableCollection<ContainerSet>) GetValue( ItemsProperty );
            set => SetValue( ItemsProperty, value );
        }

        public ICommand RemoveItemCommand => _removeItemCommand ?? ( _removeItemCommand = new RelayCommand( RemoveItem, o => SelectedSet != null ) );

        public ICommand RemoveSetCommand => _removeSetCommand ?? ( _removeSetCommand = new RelayCommand( RemoveSet, o => SelectedSet != null ) );

        public int SelectedItem
        {
            get => _selectedItem;
            set => SetField( ref _selectedItem, value );
        }

        public ContainerSet SelectedSet
        {
            get => _selectedSet;
            set => SetField( ref _selectedSet, value );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private static async Task IdentifyContainers( object arg )
        {
            List<int> serials = new List<int>();

            switch ( arg )
            {
                case int serial when serial != 0:
                    serials.Add( serial );
                    break;
                case ObservableCollection<int> set:
                    serials.AddRange( set );
                    break;
            }

            if ( serials.Count == 0 )
            {
                return;
            }

            Item[] items = Engine.Items.SelectEntities( e => serials.Contains( e.Serial ) );

            if ( items == null )
            {
                return;
            }

            foreach ( Item item in items )
            {
                Engine.SendPacketToClient( new SAWorldItem( item, 1919 ) );
            }

            await Task.Delay( 1000 );

            foreach ( Item item in items )
            {
                Engine.SendPacketToClient( new SAWorldItem( item ) );
            }
        }

        private void RemoveItem( object obj )
        {
            SelectedSet.Items.Remove( SelectedItem );
        }

        private async Task AddItem( object obj )
        {
            int serial = await Commands.GetTargetSerialAsync();

            if ( serial != 0 && serial != -1 && SelectedSet != null )
            {
                SelectedSet.Items.Add( serial );
            }
        }

        private void RemoveSet( object obj )
        {
            Items.Remove( SelectedSet );
        }

        private void AddSet( object obj )
        {
            Items.Add( new ContainerSet { Name = "Set" } );
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
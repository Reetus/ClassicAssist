using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Assistant;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;
using Microsoft.Win32;

namespace ClassicAssist.UI.Views.ECV.Settings.Controls
{
    /// <summary>
    ///     Interaction logic for AdditionalAssembliesControl.xaml
    /// </summary>
    public partial class AdditionalAssembliesControl : INotifyPropertyChanged
    {
        public static readonly DependencyProperty ClilocProperty = DependencyProperty.Register( nameof( Items ), typeof( ObservableCollection<Assembly> ),
            typeof( AdditionalAssembliesControl ), new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

        private readonly Dispatcher _dispatcher;
        private ICommand _loadCommand;
        private ICommand _removeCommand;
        private Assembly _selectedItem;

        public AdditionalAssembliesControl()
        {
            InitializeComponent();

            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public ObservableCollection<Assembly> Items
        {
            get => (ObservableCollection<Assembly>) GetValue( ClilocProperty );
            set => SetValue( ClilocProperty, value );
        }

        public ICommand LoadCommand => _loadCommand ?? ( _loadCommand = new RelayCommandAsync( Load, o => true ) );

        public ICommand RemoveCommand => _removeCommand ?? ( _removeCommand = new RelayCommand( Remove, o => o != null ) );

        public Assembly SelectedItem
        {
            get => _selectedItem;
            set => SetField( ref _selectedItem, value );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async Task Load( object arg )
        {
            OpenFileDialog ofd = new OpenFileDialog { InitialDirectory = Engine.StartupPath, Filter = "DLL Files|*.dll", CheckFileExists = true };

            bool? result = ofd.ShowDialog();

            if ( result.HasValue && result.Value )
            {
                try
                {
                    Assembly assembly = LoadAssembly( ofd.FileName );

                    if ( assembly != null )
                    {
                        await _dispatcher.InvokeAsync( () => Items.Add( assembly ) );
                    }
                }
                catch ( Exception e )
                {
                    MessageBox.Show( string.Format( Strings.Error_loading_assembly___0_, e.Message ) );
                }
            }
        }

        private static Assembly LoadAssembly( string fileName )
        {
            if ( !File.Exists( fileName ) )
            {
                return null;
            }

            return Assembly.LoadFile( fileName );
        }

        private void Remove( object obj )
        {
            if ( !( obj is Assembly assembly ) )
            {
                return;
            }

            Items.Remove( assembly );
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
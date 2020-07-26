using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassicAssist.Shared;
using ClassicAssist.Annotations;
using ClassicAssist.Data.Autoloot;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UI.Models;
using ClassicAssist.UI.ViewModels;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace ClassicAssist.UI.Views
{
    /// <summary>
    ///     Interaction logic for EntityCollectionFilterControl.xaml
    /// </summary>
    public partial class EntityCollectionFilterControl : INotifyPropertyChanged
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register( nameof( Command ),
            typeof( ICommand ), typeof( EntityCollectionFilterControl ),
            new FrameworkPropertyMetadata( default( ICommand ) ) );

        private readonly string _propertiesFileCustom =
            Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Data", "Properties.Custom.json" );

        private ICommand _addCommand;

        private ICommand _applyCommand;

        private ObservableCollection<PropertyEntry> _constraints = new ObservableCollection<PropertyEntry>();

        private ObservableCollection<EntityCollectionFilter>
            _items = new ObservableCollection<EntityCollectionFilter>();

        private ICommand _loadFilterCommand;
        private ICommand _removeCommand;
        private ICommand _resetCommand;
        private ICommand _saveFilterCommand;

        public EntityCollectionFilterControl()
        {
            InitializeComponent();

            string constraintsFile = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Data",
                "Properties.json" );

            if ( !File.Exists( constraintsFile ) )
            {
                return;
            }

            JsonSerializer serializer = new JsonSerializer();

            using ( StreamReader sr = new StreamReader( constraintsFile ) )
            {
                using ( JsonTextReader reader = new JsonTextReader( sr ) )
                {
                    PropertyEntry[] constraints = serializer.Deserialize<PropertyEntry[]>( reader );

                    foreach ( PropertyEntry constraint in constraints )
                    {
                        Constraints.AddSorted( constraint );
                    }
                }
            }

            if ( File.Exists( _propertiesFileCustom ) )
            {
                LoadCustomProperties();
            }

            Items.Add( new EntityCollectionFilter { Constraint = Constraints.FirstOrDefault() } );
        }

        public ICommand AddCommand => _addCommand ?? ( _addCommand = new RelayCommand( AddItem, o => true ) );
        public ICommand ApplyCommand => _applyCommand ?? ( _applyCommand = new RelayCommand( Apply, o => true ) );

        public ICommand Command
        {
            get => (ICommand) GetValue( CommandProperty );
            set => SetValue( CommandProperty, value );
        }

        public ObservableCollection<PropertyEntry> Constraints
        {
            get => _constraints;
            set => SetProperty( ref _constraints, value );
        }

        public ObservableCollection<EntityCollectionFilter> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public ICommand LoadFilterCommand =>
            _loadFilterCommand ?? ( _loadFilterCommand = new RelayCommand( LoadFilter, o => true ) );

        public ICommand RemoveCommand =>
            _removeCommand ?? ( _removeCommand = new RelayCommand( Remove, o => Items.Count > 1 ) );

        public ICommand ResetCommand => _resetCommand ?? ( _resetCommand = new RelayCommand( Reset, o => true ) );

        public ICommand SaveFilterCommand =>
            _saveFilterCommand ?? ( _saveFilterCommand = new RelayCommand( SaveFilter, o => Items.Count > 0 ) );

        public EntityCollectionFilter SelectedItem { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void LoadCustomProperties()
        {
            JsonSerializer serializer = new JsonSerializer();

            using ( StreamReader sr = new StreamReader( _propertiesFileCustom ) )
            {
                using ( JsonTextReader reader = new JsonTextReader( sr ) )
                {
                    PropertyEntry[] constraints = serializer.Deserialize<PropertyEntry[]>( reader );

                    foreach ( PropertyEntry constraint in constraints )
                    {
                        Constraints.AddSorted( constraint );
                    }
                }
            }
        }

        private void LoadFilter( object obj )
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                InitialDirectory = Engine.StartupPath ?? Environment.CurrentDirectory,
                Filter = "JSON Filter Files|*.filter.json"
            };

            bool? result = ofd.ShowDialog();

            if ( !result.HasValue || !result.Value )
            {
                return;
            }

            string file = ofd.FileName;

            try
            {
                JsonSerializer serializer = new JsonSerializer();

                using ( JsonTextReader jtr = new JsonTextReader( new StreamReader( file ) ) )
                {
                    EntityCollectionFilter[] entries = serializer.Deserialize<EntityCollectionFilter[]>( jtr );

                    if ( entries != null && entries.Length > 0 )
                    {
                        ResetCommand?.Execute( null );
                        Items.Clear();

                        foreach ( EntityCollectionFilter entry in entries )
                        {
                            PropertyEntry constraint =
                                Constraints.FirstOrDefault( c => c.Name == entry.Constraint.Name );

                            if ( constraint != null )
                            {
                                Items.Add( new EntityCollectionFilter
                                {
                                    Constraint = constraint, Operator = entry.Operator, Value = entry.Value
                                } );
                            }
                        }
                    }
                }
            }
            catch ( Exception e )
            {
                MessageBox.Show( e.Message, Strings.Error );
            }
        }

        private void SaveFilter( object obj )
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                InitialDirectory = Engine.StartupPath ?? Environment.CurrentDirectory,
                Filter = "JSON Filter Files|*.filter.json"
            };

            bool? result = sfd.ShowDialog();

            if ( !result.HasValue || !result.Value )
            {
                return;
            }

            string file = sfd.FileName;

            JsonSerializer serializer = new JsonSerializer { Formatting = Formatting.Indented };

            using ( JsonTextWriter jtw = new JsonTextWriter( new StreamWriter( file ) ) )
            {
                serializer.Serialize( jtw, Items );
            }
        }

        private void Apply( object obj )
        {
            Command?.Execute( Items.ToList() );
        }

        private void Reset( object obj )
        {
            Command?.Execute( new List<EntityCollectionFilter>() );
        }

        private void Remove( object obj )
        {
            if ( !( obj is ListViewItem lvi ) )
            {
                return;
            }

            if ( !( lvi.DataContext is EntityCollectionFilter filter ) )
            {
                return;
            }

            Items.Remove( filter );
        }

        private void AddItem( object obj )
        {
            Items.Add( new EntityCollectionFilter { Constraint = Constraints.FirstOrDefault() } );
        }

        // ReSharper disable once RedundantAssignment
        public virtual void SetProperty<T>( ref T obj, T value, [CallerMemberName] string propertyName = "" )
        {
            obj = value;
            OnPropertyChanged( propertyName );
            CommandManager.InvalidateRequerySuggested();
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }
}
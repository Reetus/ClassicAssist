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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data.Autoloot;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;
using CsvHelper;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace ClassicAssist.UI.ViewModels.Agents.Autoloot.Folder
{
    public class CSVImportViewModel : SetPropertyNotifyChanged
    {
        private readonly string[] _operators = { "==", "!=", ">=", "<=", "X" };
        private readonly string _propertiesFile = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Data", "Properties.json" );

        private ObservableCollection<PropertyEntry> _constraints = new ObservableCollection<PropertyEntry>();
        private ObservableCollection<AutolootEntry> _entries = new ObservableCollection<AutolootEntry>();
        private bool _ignoreDuplicateEntries;
        private AutolootEntry _selectedEntry;
        private ICommand _selectFileCommand;
        private ICommand _setImportCommand;

        public CSVImportViewModel()
        {
            LoadProperties();
        }

        public ObservableCollection<PropertyEntry> Constraints
        {
            get => _constraints;
            set => SetProperty( ref _constraints, value );
        }

        public ObservableCollection<AutolootEntry> Entries
        {
            get => _entries;
            set => SetProperty( ref _entries, value );
        }

        public bool IgnoreDuplicateEntries
        {
            get => _ignoreDuplicateEntries;
            set => SetProperty( ref _ignoreDuplicateEntries, value );
        }

        public bool Import { get; set; }

        public AutolootEntry SelectedEntry
        {
            get => _selectedEntry;
            set => SetProperty( ref _selectedEntry, value );
        }

        public ICommand SelectFileCommand => _selectFileCommand ?? ( _selectFileCommand = new RelayCommand( SelectFile ) );

        public ICommand SetImportCommand => _setImportCommand ?? ( _setImportCommand = new RelayCommand( SetImport ) );

        private void SelectFile( object obj )
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "CSV files (*.csv)|*.csv" };

            if ( ofd.ShowDialog() == true )
            {
                LoadFile( ofd.FileName );
            }
        }

        private void SetImport( object obj )
        {
            Import = true;
        }

        private void LoadFile( string fileName )
        {
            try
            {
                using ( StreamReader reader = new StreamReader( fileName ) )
                {
                    using ( CsvReader csv = new CsvReader( reader, CultureInfo.InvariantCulture ) )
                    {
                        csv.Read();
                        csv.ReadHeader();

                        while ( csv.Read() )
                        {
                            if ( !csv.TryGetField( "ID", out string idString ) )
                            {
                                continue;
                            }

                            try
                            {
                                int id = idString.StartsWith( "0x", StringComparison.CurrentCultureIgnoreCase ) ? Convert.ToInt32( idString, 16 ) : Convert.ToInt32( idString );

                                string name = $"0x{id:x}";

                                if ( csv.TryGetField( "Name", out string nameString ) )
                                {
                                    name = nameString;
                                }

                                AutolootEntry autolootEntry = new AutolootEntry
                                {
                                    ID = id,
                                    Autoloot = true,
                                    Enabled = true,
                                    Priority = AutolootPriority.Normal,
                                    Name = name,
                                    Children = new ObservableCollection<AutolootBaseModel>()
                                    {
                                        new AutolootPropertyEntry()
                                        {
                                            Constraints = new ObservableCollection<AutolootConstraintEntry>()
                                        }
                                    },
                                    Rehue = false
                                };

                                List<string> columns = csv.HeaderRecord.Where( ( value, index ) => value.StartsWith( "Property" ) ).ToList();

                                if ( columns.Any() )
                                {
                                    foreach ( string column in columns )
                                    {
                                        if ( !csv.TryGetField( column, out string fieldValue ) )
                                        {
                                            continue;
                                        }

                                        if ( string.IsNullOrEmpty( fieldValue ) )
                                        {
                                            continue;
                                        }

                                        PropertyEntry entry = Constraints.FirstOrDefault( e => fieldValue.Contains( e.ShortName ) );

                                        if ( entry == null )
                                        {
                                            continue;
                                        }

                                        AutolootOperator operation = AutolootOperator.Equal;

                                        string remaining = fieldValue.Substring( entry.ShortName.Length );

                                        foreach ( string @operator in _operators )
                                        {
                                            if ( !remaining.StartsWith( @operator ) )
                                            {
                                                continue;
                                            }

                                            operation = GetOperator( @operator );
                                            remaining = remaining.Substring( @operator.Length );
                                            break;
                                        }

                                        int value = remaining.Length > 0 ? Convert.ToInt32( remaining ) : 0;

                                        ( autolootEntry.Children.First() as AutolootPropertyEntry ).Constraints.Add(
                                            new AutolootConstraintEntry { Property = entry, Operator = operation, Value = value } );
                                    }
                                }

                                Entries.Add( autolootEntry );
                            }
                            catch ( Exception )
                            {
                                // We tried
                            }
                        }
                    }
                }
            }
            catch ( Exception )
            {
                MessageBox.Show( Strings.Error_loading_file__ensure_it_isn_t_currently_in_use, Strings.Error );
            }
        }

        private void LoadProperties()
        {
            JsonSerializer serializer = new JsonSerializer();
            List<PropertyEntry> list = new List<PropertyEntry>();

            using ( StreamReader sr = new StreamReader( _propertiesFile ) )
            {
                using ( JsonTextReader reader = new JsonTextReader( sr ) )
                {
                    PropertyEntry[] constraints = serializer.Deserialize<PropertyEntry[]>( reader );

                    if ( constraints == null )
                    {
                        return;
                    }

                    list.AddRange( constraints );
                }
            }

            foreach ( PropertyEntry entry in list.Where( e => !string.IsNullOrEmpty( e.ShortName ) ).OrderByDescending( e => e.ShortName.Length ) )
            {
                Constraints.Add( entry );
            }
        }

        private static AutolootOperator GetOperator( string @operator )
        {
            switch ( @operator )
            {
                case "==":
                    return AutolootOperator.Equal;
                case "!=":
                    return AutolootOperator.NotEqual;
                case ">=":
                    return AutolootOperator.GreaterThan;
                case "<=":
                    return AutolootOperator.LessThan;
                case "X":
                    return AutolootOperator.NotPresent;
            }

            return AutolootOperator.Equal;
        }
    }
}
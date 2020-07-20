using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ClassicAssist.Data.Macros;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Misc;
using ClassicAssist.Resources;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UO;

namespace ClassicAssist.UI.ViewModels
{
    public class TimerData
    {
        public string Name { get; set; }
        public OffsetStopwatch Value { get; set; }
    }

    public class ActiveObjectsViewModel : BaseViewModel
    {
        private ICommand _clearAllAliasesCommand;
        private ICommand _clearAllListsCommand;
        private ICommand _clearIgnoreListCommand;
        private ObservableCollection<int> _ignoreEntries = new ObservableCollection<int>();
        private ICommand _refreshAliasesCommand;
        private ICommand _refreshIgnoreListCommand;
        private ICommand _refreshListsCommand;
        private ICommand _refreshTimersCommand;
        private ICommand _removeAliasCommand;
        private ICommand _removeIgnoreEntryCommand;
        private ICommand _removeInstanceAliasCommand;
        private ICommand _removeListCommand;
        private AliasEntry _selectedAlias;
        private int _selectedIgnoreEntry;
        private InstanceAliasEntry _selectedInstanceAlias;
        private ListEntry _selectedList;
        private TimerData _selectedTimer;
        private ICommand _setAliasCommand;
        private ObservableCollection<TimerData> _timers = new ObservableCollection<TimerData>();

        public ActiveObjectsViewModel()
        {
            RefreshAliases();
            RefreshInstanceAliases();
            RefreshLists();
            RefreshTimers();
            RefreshIgnoreList();
        }

        public ObservableCollection<AliasEntry> Aliases { get; set; } = new ObservableCollection<AliasEntry>();

        public ICommand ClearAllAliasesCommand =>
            _clearAllAliasesCommand ?? ( _clearAllAliasesCommand = new RelayCommand( ClearAllAliases, o => true ) );

        public ICommand ClearAllListsCommand =>
            _clearAllListsCommand ?? ( _clearAllListsCommand = new RelayCommand( ClearAllLists, o => true ) );

        public ICommand ClearIgnoreListCommand =>
            _clearIgnoreListCommand ?? ( _clearIgnoreListCommand = new RelayCommand( ClearIgnoreList, o => true ) );

        public ObservableCollection<int> IgnoreEntries
        {
            get => _ignoreEntries;
            set => SetProperty( ref _ignoreEntries, value );
        }

        public ObservableCollection<InstanceAliasEntry> InstanceAliases { get; set; } =
            new ObservableCollection<InstanceAliasEntry>();

        public ObservableCollection<ListEntry> Lists { get; set; } = new ObservableCollection<ListEntry>();

        public ICommand RefreshAliasesCommand =>
            _refreshAliasesCommand ?? ( _refreshAliasesCommand = new RelayCommand( o => RefreshAliases(), o => true ) );

        public ICommand RefreshIgnoreListCommand =>
            _refreshIgnoreListCommand ??
            ( _refreshIgnoreListCommand = new RelayCommand( o => RefreshIgnoreList(), o => true ) );

        public ICommand RefreshListsCommand =>
            _refreshListsCommand ?? ( _refreshListsCommand = new RelayCommand( o => RefreshLists(), o => true ) );

        public ICommand RefreshTimersCommand =>
            _refreshTimersCommand ?? ( _refreshTimersCommand = new RelayCommand( o => RefreshTimers(), o => true ) );

        public ICommand RemoveAliasCommand =>
            _removeAliasCommand ??
            ( _removeAliasCommand = new RelayCommand( RemoveAlias, o => SelectedAlias != null ) );

        public ICommand RemoveIgnoreEntryCommand =>
            _removeIgnoreEntryCommand ??
            ( _removeIgnoreEntryCommand = new RelayCommand( RemoveIgnoreEntry, o => true ) );

        public ICommand RemoveInstanceAliasCommand =>
            _removeInstanceAliasCommand ?? ( _removeInstanceAliasCommand =
                new RelayCommand( RemoveInstanceAlias, o => SelectedInstanceAlias != null ) );

        public ICommand RemoveListCommand =>
            _removeListCommand ?? ( _removeListCommand = new RelayCommand( RemoveList, o => SelectedList != null ) );

        public AliasEntry SelectedAlias
        {
            get => _selectedAlias;
            set => SetProperty( ref _selectedAlias, value );
        }

        public int SelectedIgnoreEntry
        {
            get => _selectedIgnoreEntry;
            set => SetProperty( ref _selectedIgnoreEntry, value );
        }

        public InstanceAliasEntry SelectedInstanceAlias
        {
            get => _selectedInstanceAlias;
            set => SetProperty( ref _selectedInstanceAlias, value );
        }

        public ListEntry SelectedList
        {
            get => _selectedList;
            set => SetProperty( ref _selectedList, value );
        }

        public TimerData SelectedTimer
        {
            get => _selectedTimer;
            set => SetProperty( ref _selectedTimer, value );
        }

        public ICommand SetAliasCommand =>
            _setAliasCommand ?? ( _setAliasCommand = new RelayCommandAsync( SetAlias, o => o != null ) );

        public ObservableCollection<TimerData> Timers
        {
            get => _timers;
            set => SetProperty( ref _timers, value );
        }

        private void RefreshInstanceAliases()
        {
            InstanceAliases.Clear();

            MacroManager manager = MacroManager.GetInstance();

            foreach ( MacroEntry entry in manager.Items )
            {
                foreach ( KeyValuePair<string, int> alias in entry.Aliases )
                {
                    InstanceAliases.Add( new InstanceAliasEntry
                    {
                        Macro = entry, Name = alias.Key, Serial = alias.Value
                    } );
                }
            }
        }

        private void RemoveInstanceAlias( object obj )
        {
            if ( !( obj is InstanceAliasEntry entry ) )
            {
                return;
            }

            entry.Macro.Aliases.Remove( entry.Name );

            RefreshInstanceAliases();
        }

        private void ClearIgnoreList( object obj )
        {
            ObjectCommands.ClearIgnoreList();

            RefreshIgnoreList();
        }

        private void RemoveIgnoreEntry( object obj )
        {
            if ( !( obj is int serial ) )
            {
                return;
            }

            ObjectCommands.IgnoreList.Remove( serial );

            RefreshIgnoreList();
        }

        private void RefreshIgnoreList()
        {
            IgnoreEntries.Clear();

            foreach ( int serial in ObjectCommands.IgnoreList )
            {
                IgnoreEntries.Add( serial );
            }
        }

        private void RefreshTimers()
        {
            Dictionary<string, OffsetStopwatch> timers = TimerCommands.GetAllTimers();

            if ( timers == null )
            {
                return;
            }

            Timers.Clear();

            foreach ( KeyValuePair<string, OffsetStopwatch> timer in timers )
            {
                Timers.Add( new TimerData { Name = timer.Key, Value = timer.Value } );
            }
        }

        private void RemoveList( object obj )
        {
            if ( !( obj is ListEntry entry ) )
            {
                return;
            }

            ListCommands.RemoveList( entry.Name );
            Lists.Remove( entry );
        }

        private void ClearAllLists( object obj )
        {
            string[] lists = ListCommands.GetAllLists().Select( l => l.Key ).ToArray();

            for ( int i = 0; i < lists.Count(); i++ )
            {
                ListCommands.RemoveList( lists[i] );
            }

            RefreshLists();
        }

        private void RefreshLists()
        {
            Lists.Clear();

            foreach ( KeyValuePair<string, List<int>> list in ListCommands.GetAllLists() )
            {
                Lists.Add( new ListEntry { Name = list.Key, Serials = list.Value.ToArray() } );
            }
        }

        private void RemoveAlias( object obj )
        {
            if ( !( obj is AliasEntry entry ) )
            {
                return;
            }

            AliasCommands.UnsetAlias( entry.Name );
            Aliases.Remove( entry );
        }

        public void RefreshAliases()
        {
            Aliases.Clear();

            foreach ( KeyValuePair<string, int> alias in AliasCommands.GetAllAliases() )
            {
                Aliases.AddSorted( new AliasEntry { Name = alias.Key, Serial = alias.Value } );
            }
        }

        private void ClearAllAliases( object obj )
        {
            string[] aliases = AliasCommands.GetAllAliases().Select( a => a.Key ).ToArray();

            // ReSharper disable once ForCanBeConvertedToForeach
            for ( int i = 0; i < aliases.Length; i++ )
            {
                AliasCommands.UnsetAlias( aliases[i] );
            }

            RefreshAliases();
        }

        private static async Task SetAlias( object arg )
        {
            if ( !( arg is AliasEntry entry ) )
            {
                return;
            }

            int serial =
                await Shared.UO.Commands.GetTargeSerialAsync( string.Format( Strings.Target_object___0_____, entry.Name ) );

            if ( serial == 0 )
            {
                return;
            }

            AliasCommands.SetAlias( entry.Name, serial );
        }

        public class InstanceAliasEntry
        {
            public MacroEntry Macro { get; set; }
            public string Name { get; set; }
            public int Serial { get; set; }
        }

        public class AliasEntry : IComparable<AliasEntry>
        {
            public string Name { get; set; }
            public int Serial { get; set; }

            public int CompareTo( AliasEntry other )
            {
                if ( ReferenceEquals( this, other ) )
                {
                    return 0;
                }

                if ( ReferenceEquals( null, other ) )
                {
                    return 1;
                }

                return string.Compare( Name, other.Name, StringComparison.InvariantCultureIgnoreCase );
            }
        }

        public class ListEntry
        {
            public string Name { get; set; }
            public int[] Serials { get; set; }
        }
    }
}
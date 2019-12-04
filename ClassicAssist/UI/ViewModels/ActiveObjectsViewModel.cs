using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ClassicAssist.Data.Macros.Commands;

namespace ClassicAssist.UI.ViewModels
{
    public class ActiveObjectsViewModel : BaseViewModel
    {
        private ICommand _clearAllAliasesCommand;
        private ICommand _clearAllListsCommand;
        private ICommand _refreshAliasesCommand;
        private ICommand _refreshListsCommand;
        private ICommand _removeAliasCommand;
        private ICommand _removeListCommand;
        private AliasEntry _selectedAlias;
        private ListEntry _selectedList;

        public ActiveObjectsViewModel()
        {
            RefreshAliases();
            RefreshLists();
        }

        public ObservableCollection<AliasEntry> Aliases { get; set; } = new ObservableCollection<AliasEntry>();

        public ICommand ClearAllAliasesCommand =>
            _clearAllAliasesCommand ?? ( _clearAllAliasesCommand = new RelayCommand( ClearAllAliases, o => true ) );

        public ICommand ClearAllListsCommand =>
            _clearAllListsCommand ?? ( _clearAllListsCommand = new RelayCommand( ClearAllLists, o => true ) );

        public ObservableCollection<ListEntry> Lists { get; set; } = new ObservableCollection<ListEntry>();

        public ICommand RefreshAliasesCommand =>
            _refreshAliasesCommand ?? ( _refreshAliasesCommand = new RelayCommand( o => RefreshAliases(), o => true ) );

        public ICommand RefreshListsCommand =>
            _refreshListsCommand ?? ( _refreshListsCommand = new RelayCommand( o => RefreshLists(), o => true ) );

        public ICommand RemoveAliasCommand =>
            _removeAliasCommand ??
            ( _removeAliasCommand = new RelayCommand( RemoveAlias, o => SelectedAlias != null ) );

        public ICommand RemoveListCommand =>
            _removeListCommand ??
            ( _removeListCommand = new RelayCommand( RemoveList, o => SelectedList != null ) );

        public AliasEntry SelectedAlias
        {
            get => _selectedAlias;
            set => SetProperty( ref _selectedAlias, value );
        }

        public ListEntry SelectedList
        {
            get => _selectedList;
            set => SetProperty( ref _selectedList, value );
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
            foreach ( KeyValuePair<string, List<int>> list in ListCommands.GetAllLists() )
            {
                ListCommands.RemoveList( list.Key );
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
                Aliases.Add( new AliasEntry { Name = alias.Key, Serial = alias.Value } );
            }
        }

        private void ClearAllAliases( object obj )
        {
            foreach ( KeyValuePair<string, int> alias in AliasCommands.GetAllAliases() )
            {
                AliasCommands.UnsetAlias( alias.Key );
            }

            RefreshAliases();
        }

        public class AliasEntry
        {
            public string Name { get; set; }
            public int Serial { get; set; }
        }

        public class ListEntry
        {
            public string Name { get; set; }
            public int[] Serials { get; set; }
        }
    }
}
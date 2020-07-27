using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using ClassicAssist.Data.Macros;

namespace ClassicAssist.UI.ViewModels.Macros
{
    public class CommandsData
    {
        public CommandsDisplayAttribute Attribute { get; set; }
        public string Category { get; set; }
        public bool IsExpanded { get; set; }
        public string Name { get; set; }
        public string Tooltip { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class MacrosCommandViewModel : BaseViewModel
    {
        private readonly MacrosTabViewModel _macrosViewModel;
        private ICommand _insertCommand;
        private ObservableCollection<CommandsData> _items = new ObservableCollection<CommandsData>();
        private CommandsData _selectedItem;

        public MacrosCommandViewModel( MacrosTabViewModel macros ) : this()
        {
            _macrosViewModel = macros;
        }

        public MacrosCommandViewModel()
        {
            IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetTypes().Where( t =>
                t.Namespace != null && t.Namespace.StartsWith( "ClassicAssist.Data.Macros.Commands" ) );

            foreach ( Type type in types )
            {
                MemberInfo[] members = type.GetMembers( BindingFlags.Public | BindingFlags.Static );

                foreach ( MemberInfo memberInfo in members )
                {
                    CommandsDisplayAttribute attr = memberInfo.GetCustomAttribute<CommandsDisplayAttribute>();

                    if ( attr == null )
                    {
                        continue;
                    }

                    CommandsData entry = new CommandsData
                    {
                        Category = attr.Category,
                        IsExpanded = false,
                        Name = memberInfo.ToString(),
                        Tooltip = attr.Description,
                        Attribute = attr
                    };

                    Items.Add( entry );
                }
            }
        }

        public ICommand InsertCommand =>
            _insertCommand ?? ( _insertCommand = new RelayCommand( Insert, o => SelectedItem != null ) );

        public ObservableCollection<CommandsData> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public CommandsData SelectedItem
        {
            get => _selectedItem;
            set => SetProperty( ref _selectedItem, value );
        }

        private void Insert( object obj )
        {
            if ( !( obj is CommandsData cd ) )
            {
                return;
            }

            if ( cd.Attribute != null )
            {
                //TODO UI
                //_macrosViewModel?.Document.Insert( _macrosViewModel.CaretPosition, cd.Attribute.InsertText );
            }
        }
    }
}
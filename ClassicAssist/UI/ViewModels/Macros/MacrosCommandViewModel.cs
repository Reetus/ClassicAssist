using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using ClassicAssist.Data.Macros;

namespace ClassicAssist.UI.ViewModels.Macros
{
    public class CommandsData
    {
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
        private ObservableCollection<CommandsData> _items = new ObservableCollection<CommandsData>();

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

                    if ( attr != null )
                    {
                        CommandsData entry = new CommandsData
                        {
                            Category = attr.Category,
                            IsExpanded = false,
                            Name = memberInfo.ToString(),
                            Tooltip = attr.Description
                        };

                        Items.Add( entry );
                    }
                }
            }
        }

        public ObservableCollection<CommandsData> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }
    }
}
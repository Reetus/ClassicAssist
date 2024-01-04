#region License

// Copyright (C) 2020 Reetus
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using Microsoft.Scripting.Utils;

namespace ClassicAssist.UI.ViewModels.Debug
{
    public class DebugPropertiesViewModel : DebugBaseViewModel
    {
        private ObservableCollection<Property> _items = new ObservableCollection<Property>();
        private ICommand _targetCommand;

        public DebugPropertiesViewModel()
        {
            PropertyChanged += ( sender, args ) =>
            {
                if ( args.PropertyName != nameof( Object ) || !( Object is IEnumerable<Property> properties ) )
                {
                    return;
                }

                Items.Clear();
                Items.AddRange( properties.Select( entityProperty => new Property
                {
                    Cliloc = entityProperty.Cliloc,
                    Text = Cliloc.GetProperty( entityProperty.Cliloc ),
                    Arguments = entityProperty.Arguments
                } ).ToList() );
            };
        }

        public ObservableCollection<Property> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public ICommand TargetCommand =>
            _targetCommand ?? ( _targetCommand = new RelayCommandAsync( Target, o => Engine.Connected ) );

        private async Task Target( object arg )
        {
            int serial = await Commands.GetTargetSerialAsync( Strings.Target_object___ );

            if ( serial == 0 )
            {
                return;
            }

            Entity entity = UOMath.IsMobile( serial )
                ? Engine.Mobiles.GetMobile( serial )
                : (Entity) Engine.Items.GetItem( serial );

            if ( entity == null )
            {
                return;
            }

            if ( entity.Properties == null )
            {
                if ( Engine.TooltipsEnabled )
                {
                    PropertiesCommands.WaitForProperties( entity.Serial, 5000 );
                }

                if ( entity.Properties == null )
                {
                    Commands.SystemMessage( Strings.Item_properties_null_or_not_loaded___ );
                    return;
                }
            }

            List<Property> properties = entity.Properties.Select( entityProperty => new Property
            {
                Cliloc = entityProperty.Cliloc,
                Text = Cliloc.GetProperty( entityProperty.Cliloc ),
                Arguments = entityProperty.Arguments
            } ).ToList();

            Items.Clear();
            Items.AddRange( properties );
        }
    }

    public class JoinArrayValueConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            if ( !( value is string[] array ) )
            {
                return null;
            }

            return string.Join( (string) parameter ?? ", ", array );
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }
}
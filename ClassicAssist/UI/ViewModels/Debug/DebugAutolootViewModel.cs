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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Assistant;
using ClassicAssist.Data.Autoloot;
using ClassicAssist.Resources;
using ClassicAssist.UI.ViewModels.Agents;
using ClassicAssist.UO;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UI.ViewModels.Debug
{
    public class DebugAutolootViewModel : BaseViewModel
    {
        private ICommand _clearResultsCommand;
        private int _containerSerial;
        private ICommand _retestContainerCommand;
        private ICommand _testContainerCommand;
        private string _testResults;

        public ICommand ClearResultsCommand =>
            _clearResultsCommand ?? ( _clearResultsCommand = new RelayCommand( ClearResults, o => true ) );

        public int ContainerSerial
        {
            get => _containerSerial;
            set => SetProperty( ref _containerSerial, value );
        }

        public ICommand RetestContainerCommand =>
            _retestContainerCommand ??
            ( _retestContainerCommand = new RelayCommand( RetestContainer, o => ContainerSerial != 0 ) );

        public ICommand TestContainerCommand =>
            _testContainerCommand ?? ( _testContainerCommand = new RelayCommandAsync( TestContainerAsync, o => true ) );

        public string TestResults
        {
            get => _testResults;
            set => SetProperty( ref _testResults, value );
        }

        private void ClearResults( object obj )
        {
            TestResults = string.Empty;
        }

        private void RetestContainer( object obj )
        {
            TestContainer( ContainerSerial );
        }

        private async Task TestContainerAsync( object arg )
        {
            int serial = await Commands.GetTargeSerialAsync( "Choose container...", 60000 );

            if ( serial == 0 )
            {
                Commands.SystemMessage( Strings.Invalid_container___ );
            }

            ContainerSerial = serial;
            TestContainer( serial );
        }

        private void TestContainer( int serial )
        {
            Item[] items = Engine.Items.GetItem( serial )?.Container?.GetItems();

            if ( items == null )
            {
                TestResults += $"{Strings.Cannot_find_container___}\n";
                Commands.SystemMessage( Strings.Cannot_find_container___ );
                return;
            }

            Engine.SendPacketToServer( new BatchQueryProperties( items.Select( i => i.Serial ).ToArray() ) );

            foreach ( AutolootEntry entry in AutolootManager.GetInstance().GetEntries() )
            {
                if ( !entry.Enabled )
                {
                    TestResults += $"Entry {entry.Name} disabled...\n";
                    continue;
                }

                TestResults += $"Entry {entry.Name}...\n\n";

                IEnumerable<Item> matchItems = AutolootHelpers.AutolootFilter( items, entry );

                if ( matchItems == null )
                {
                    TestResults += "No items matched...\n";
                    continue;
                }

                int count = 0;

                foreach ( Item matchItem in matchItems )
                {
                    count++;

                    TestResults += matchItem.ToString();

                    if ( matchItem.Properties != null )
                    {
                        TestResults += "\nProperties...\n\n";

                        foreach ( Property property in matchItem.Properties )
                        {
                            TestResults += $"{property.Text}\n";
                        }
                    }

                    TestResults += "\n";
                }

                TestResults += $"{count} matches...\n\n";
            }
        }
    }
}
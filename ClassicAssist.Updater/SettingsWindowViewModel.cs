#region License

// Copyright (C) 2021 Reetus
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

using ClassicAssist.Shared;
using ClassicAssist.Shared.UI;

namespace ClassicAssist.Updater
{
    public class SettingsWindowViewModel : SetPropertyNotifyChanged
    {
        private UpdaterSettings _updaterSettings;

        public SettingsWindowViewModel()
        {
            UpdaterSettings = App.UpdaterSettings;
        }

        public UpdaterSettings UpdaterSettings
        {
            get => _updaterSettings;
            set => SetProperty( ref _updaterSettings, value );
        }
    }
}
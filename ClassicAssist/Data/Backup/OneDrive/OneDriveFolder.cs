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

using System.Collections.ObjectModel;
using ClassicAssist.UI.ViewModels;

namespace ClassicAssist.Data.Backup.OneDrive
{
    public class OneDriveFolder : SetPropertyNotifyChanged
    {
        private ObservableCollection<OneDriveFolder> _children = new ObservableCollection<OneDriveFolder>();
        private string _id;
        private string _name;

        public ObservableCollection<OneDriveFolder> Children
        {
            get => _children;
            set => SetProperty( ref _children, value );
        }

        public string Id
        {
            get => _id;
            set => SetProperty( ref _id, value );
        }

        public string Name
        {
            get => _name;
            set => SetProperty( ref _name, value );
        }
    }
}
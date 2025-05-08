#region License

// Copyright (C) 2024 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
#endregion

using System.Collections.ObjectModel;

namespace ClassicAssist.Data.Autoloot
{
    public class AutolootPropertyEntry : AutolootBaseModel
    {
        private ObservableCollection<AutolootConstraintEntry> _constraints = new ObservableCollection<AutolootConstraintEntry>();

        public ObservableCollection<AutolootConstraintEntry> Constraints
        {
            get => _constraints;
            set => SetProperty( ref _constraints, value );
        }
    }
}
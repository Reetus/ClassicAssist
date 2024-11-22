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
using ClassicAssist.UI.Views.ECV.Filter.Models;

namespace ClassicAssist.Data.Autoloot
{
    public class AutolootPropertyGroup : AutolootBaseModel
    {
        private ObservableCollection<AutolootBaseModel> _children = new ObservableCollection<AutolootBaseModel>();
        private BooleanOperation _operation;

        public ObservableCollection<AutolootBaseModel> Children
        {
            get => _children;
            set => SetProperty( ref _children, value );
        }

        public BooleanOperation Operation
        {
            get => _operation;
            set => SetProperty( ref _operation, value );
        }
    }
}
#region License

// Copyright (C) 2024 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using ClassicAssist.Shared.UI;

namespace ClassicAssist.UI.Views.ECV.Settings.Models
{
    public class CombineStacksOpenContainersIgnoreEntry : SetPropertyNotifyChanged
    {
        private int _cliloc = -1;
        private int _hue;
        private int _id;

        public int Cliloc
        {
            get => _cliloc;
            set => SetProperty( ref _cliloc, value );
        }

        public int Hue
        {
            get => _hue;
            set => SetProperty( ref _hue, value );
        }

        public int ID
        {
            get => _id;
            set => SetProperty( ref _id, value );
        }
    }
}
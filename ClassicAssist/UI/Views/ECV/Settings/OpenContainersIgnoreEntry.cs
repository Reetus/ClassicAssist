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
using ClassicAssist.UO.Data;

namespace ClassicAssist.UI.Views.ECV.Settings
{
    public class OpenContainersIgnoreEntry : SetPropertyNotifyChanged
    {
        private int _id;

        public int ID
        {
            get => _id;
            set
            {
                SetProperty( ref _id, value );
                OnPropertyChanged( nameof( Name ) );
            }
        }

        public string Name => TileData.GetStaticTile( ID ).Name;
    }
}
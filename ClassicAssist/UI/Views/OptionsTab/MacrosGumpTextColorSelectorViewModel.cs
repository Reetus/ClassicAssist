#region License

// Copyright (C) 2023 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using System.Windows.Input;
using System.Windows.Media;
using ClassicAssist.Shared.UI;

namespace ClassicAssist.UI.Views.OptionsTab
{
    public class MacrosGumpTextColorSelectorViewModel : SetPropertyNotifyChanged
    {
        private ICommand _okCommand;
        private Color _selectedColor = Colors.White;

        public ICommand OKCommand => _okCommand ?? ( _okCommand = new RelayCommand( OK ) );
        public bool Result { get; set; }

        public Color SelectedColor
        {
            get => _selectedColor;
            set => SetProperty( ref _selectedColor, value );
        }

        private void OK( object obj )
        {
            Result = true;
        }
    }
}
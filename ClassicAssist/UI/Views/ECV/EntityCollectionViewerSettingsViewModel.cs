#region License

// Copyright (C) $CURRENT_YEAR$ Reetus
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
using ClassicAssist.Data.Misc;
using ClassicAssist.Shared.UI;

namespace ClassicAssist.UI.Views.ECV
{
    public class EntityCollectionViewerSettingsViewModel : SetPropertyNotifyChanged
    {
        private EntityCollectionViewerOptions _options;
        private ICommand _okCommand;

        public EntityCollectionViewerOptions Options
        {
            get => _options;
            set => SetProperty( ref _options, value );
        }

        public ICommand OKCommand => _okCommand ?? ( _okCommand = new RelayCommand( OK ) );

        private static void OK( object obj )
        {
        }
    }
}
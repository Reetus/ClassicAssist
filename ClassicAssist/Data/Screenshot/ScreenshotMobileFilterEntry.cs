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

using ClassicAssist.Shared.UI;

namespace ClassicAssist.Data.Screenshot
{
    public class ScreenshotMobileFilterEntry : SetPropertyNotifyChanged
    {
        private bool _enabled;
        private int _id;
        private string _note;

        public bool Enabled
        {
            get => _enabled;
            set => SetProperty( ref _enabled, value );
        }

        public int ID
        {
            get => _id;
            set => SetProperty( ref _id, value );
        }

        public string Note
        {
            get => _note;
            set => SetProperty( ref _note, value );
        }
    }
}
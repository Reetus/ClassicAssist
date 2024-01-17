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

namespace ClassicAssist.Data.TrapPouch
{
    public class TrapPouchEntry : SetPropertyNotifyChanged
    {
        private string _name;
        private int _originalHue;
        private int _serial;

        public string Name
        {
            get => _name;
            set => SetProperty( ref _name, value );
        }

        public int OriginalHue
        {
            get => _originalHue;
            set => SetProperty( ref _originalHue, value );
        }

        public int Serial
        {
            get => _serial;
            set => SetProperty( ref _serial, value );
        }
    }
}
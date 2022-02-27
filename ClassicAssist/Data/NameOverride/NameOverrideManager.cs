#region License

// Copyright (C) 2022 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using System;
using System.Collections.ObjectModel;
using System.Linq;
using ClassicAssist.Shared.UI;

namespace ClassicAssist.Data.NameOverride
{
    public class NameOverrideManager : SetPropertyNotifyChanged
    {
        private static NameOverrideManager _instance;
        private static readonly object _instanceLock = new object();
        private ObservableCollection<NameOverrideEntry> _items;

        public Func<bool> Enabled { get; set; } = () => false;

        public ObservableCollection<NameOverrideEntry> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public static NameOverrideManager GetInstance()
        {
            // ReSharper disable once InvertIf
            if ( _instance == null )
            {
                lock ( _instanceLock )
                {
                    if ( _instance == null )
                    {
                        _instance = new NameOverrideManager();
                    }
                }
            }

            return _instance;
        }

        public bool CheckEntity( int serial, out string nameOverride )
        {
            nameOverride = string.Empty;
            NameOverrideEntry entry = Items.FirstOrDefault( e => e.Serial == serial && e.Enabled );

            if ( entry == null )
            {
                return false;
            }

            nameOverride = entry.Name;

            return true;
        }
    }
}
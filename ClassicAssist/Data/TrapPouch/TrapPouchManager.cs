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

using System;
using System.Collections.ObjectModel;
using ClassicAssist.Shared.UI;

namespace ClassicAssist.Data.TrapPouch
{
    public class TrapPouchManager : SetPropertyNotifyChanged
    {
        private static TrapPouchManager _instance;
        private static readonly object _lock = new object();
        private ObservableCollection<TrapPouchEntry> _items;

        public Action Use { get; set; }
        public Action Clear { get; set; }
        public Action<int> Add { get; set; }

        private TrapPouchManager()
        {
        }

        public ObservableCollection<TrapPouchEntry> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public static TrapPouchManager GetInstance()
        {
            // ReSharper disable once InvertIf
            if ( _instance == null )
            {
                lock ( _lock )
                {
                    if ( _instance != null )
                    {
                        return _instance;
                    }

                    _instance = new TrapPouchManager();
                    return _instance;
                }
            }

            return _instance;
        }
    }
}
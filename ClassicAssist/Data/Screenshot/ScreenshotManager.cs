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

using System;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.Data.Screenshot
{
    public class ScreenshotManager
    {
        private static ScreenshotManager _instance;
        private static readonly object _instanceLock = new object();
        public Action<Mobile> OnMobileDeath { get; set; }
        public Action<string> OnPlayerDeath { get; set; }
        public Func<bool?, string, string, string> TakeScreenshot { get; set; }

        public static ScreenshotManager GetInstance()
        {
            // ReSharper disable once InvertIf
            if ( _instance == null )
            {
                lock ( _instanceLock )
                {
                    if ( _instance == null )
                    {
                        _instance = new ScreenshotManager();
                    }
                }
            }

            return _instance;
        }
    }
}
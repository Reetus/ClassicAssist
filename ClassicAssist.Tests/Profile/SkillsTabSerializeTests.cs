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

using System.Linq;
using System.Windows.Input;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Data.Hotkeys.Commands;
using ClassicAssist.Misc;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Tests.Profile
{
    [TestClass]
    public class SkillsTabSerializeTests
    {
        [TestMethod]
        public void WillSerializeGlobalNonGlobalSkillKeys()
        {
            // Arrange
            SkillsTabViewModel viewModel = new SkillsTabViewModel
            {
                _hotkeyCategory = new HotkeyCommand
                {
                    Children = new ObservableCollectionEx<HotkeyEntry>
                    {
                        new HotkeyCommand { IsGlobal = true, Hotkey = new ShortcutKeys( SDLKeys.ModKey.None, Key.A ) },
                        new HotkeyCommand { IsGlobal = false, Hotkey = new ShortcutKeys( SDLKeys.ModKey.None, Key.B ) }
                    }
                }
            };
            JObject globalJson = new JObject();
            JObject json = new JObject();

            // Act
            viewModel.Serialize( globalJson, true );
            viewModel.Serialize( json );

            // Assert
            Assert.AreEqual( 1, globalJson["Skills"].Count() );
            Assert.AreEqual( 1, json["Skills"].Count() );
        }
    }
}
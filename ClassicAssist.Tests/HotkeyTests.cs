using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ClassicAssist.Shared;
using ClassicAssist.Data.Hotkeys.Commands;
using ClassicAssist.UI.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests
{
    [TestClass]
    public class HotkeyTests
    {
        [TestMethod]
        public void WillInstantiateAllHotkeysCommandsNoExceptions()
        {
            IEnumerable<Type> hotkeyCommands = Assembly.GetAssembly( typeof( Engine ) ).GetTypes()
                .Where( i => i.IsSubclassOf( typeof( HotkeyCommand ) ) );

            ObservableCollectionEx<HotkeyCommand> hotkeys = new ObservableCollectionEx<HotkeyCommand>();

            foreach ( Type hotkeyCommand in hotkeyCommands )
            {
                HotkeyCommand hkc = (HotkeyCommand) Activator.CreateInstance( hotkeyCommand );

                hotkeys.Add( hkc );
            }
        }
    }
}
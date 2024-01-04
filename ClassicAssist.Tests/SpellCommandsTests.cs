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

using System.IO;
using System.Threading;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Spells;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests
{
    [TestClass]
    public class SpellCommandsTests
    {
        [TestMethod]
        public void CastWillSetWaitingFalse()
        {
            SpellManager manager = SpellManager.GetInstance();

            SpellData spellData = manager.GetSpellData( "Clumsy" );

            manager.CastSpell( spellData );

            Assert.IsTrue( Engine.WaitingForTarget );

            Thread.Sleep( spellData.Timeout + 200 );

            Assert.IsFalse( Engine.WaitingForTarget );
        }

        [TestMethod]
        public void CastWillSetWaitingFalseFizzleDetection()
        {
            Engine.Player = new PlayerMobile( 0 );
            Engine.PacketWaitEntries = new PacketWaitEntries();
            Options.CurrentOptions.UseExperimentalFizzleDetection = true;

            SpellManager manager = SpellManager.GetInstance();

            SpellData spellData = manager.GetSpellData( "Flame Strike" );

            manager.CastSpell( spellData );

            Thread.Sleep( 100 );

            Assert.IsTrue( Engine.WaitingForTarget );

            PacketWriter pw = new PacketWriter( 19 );
            pw.Write( (byte) 0xC1 );
            pw.Seek( 14, SeekOrigin.Begin );
            pw.Write( 502632 );
            pw.Fill();

            Engine.PacketWaitEntries.CheckWait( pw.ToArray(), PacketDirection.Incoming );

            Thread.Sleep( 250 );

            Assert.IsFalse( Engine.WaitingForTarget );

            Engine.Player = null;
            Engine.PacketWaitEntries = null;
        }
    }
}
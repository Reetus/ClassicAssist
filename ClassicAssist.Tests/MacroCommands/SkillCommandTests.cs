#region License

// Copyright (C) 2021 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Data.Skills;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests.MacroCommands
{
    [TestClass]
    public class SkillCommandTests
    {
        [TestMethod]
        public void SkillWillReturnCorrectValue()
        {
            SkillManager manager = SkillManager.GetInstance();

            manager.Items.Add( new SkillEntry { Base = 80, Value = 100, Skill = new Skill { Name = "Hiding" } } );

            Assert.AreEqual( 80, SkillCommands.Skill( "Hiding", true ) );
            Assert.AreEqual( 100, SkillCommands.Skill( "Hiding" ) );
        }
    }
}
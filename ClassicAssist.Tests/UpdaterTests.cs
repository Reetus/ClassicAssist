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
using System.Collections.Generic;
using System.IO;
using ClassicAssist.Updater;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests
{
    [TestClass]
    public class UpdaterTests
    {
        [TestMethod]
        public void EnsureVerifyWriteAccess()
        {
            string guid = Guid.NewGuid().ToString();

            string path1 = Path.Combine( Path.GetTempPath(), $"{guid}-1" );
            string path2 = Path.Combine( Path.GetTempPath(), $"{guid}-2" );

            if ( !Directory.Exists( path1 ) )
            {
                Directory.CreateDirectory( path1 );
                Directory.CreateDirectory( Path.Combine( path1, "path" ) );
            }

            if ( !Directory.Exists( path2 ) )
            {
                Directory.CreateDirectory( path2 );
                Directory.CreateDirectory( Path.Combine( path2, "path" ) );
            }

            File.Create( Path.Combine( path1, guid ) ).Dispose();
            File.Create( Path.Combine( path2, guid ) ).Dispose();

            File.Create( Path.Combine( path1, "path", guid ) ).Dispose();
            File.Create( Path.Combine( path2, "path", guid ) ).Dispose();

            MainViewModel vm = new MainViewModel( true );

            List<string> failList = new List<string>();

            vm.VerifyWriteAccess( new DirectoryInfo( path1 ), new DirectoryInfo( path2 ), failList );

            Assert.AreEqual( 0, failList.Count );

            using ( File.OpenWrite( Path.Combine( path2, "path", guid ) ) )
            {
                vm.VerifyWriteAccess( new DirectoryInfo( path1 ), new DirectoryInfo( path2 ), failList );
            }

            Assert.AreNotEqual( 0, failList.Count );

            Directory.Delete( path1, true );
            Directory.Delete( path2, true );
        }
    }
}
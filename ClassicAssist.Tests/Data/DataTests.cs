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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Assistant;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests.Data
{
    [TestClass]
    public class DataTests
    {
        [TestMethod]
        public void PropertiesJsonNoDuplicateShortNames()
        {
            string filePath = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, "Data", "Properties.json" );

            if ( !File.Exists( filePath ) )
            {
                Assert.Fail( "Properties.json not found" );
            }

            var json = File.ReadAllText( filePath );

            if ( string.IsNullOrEmpty( json ) )
            {
                Assert.Fail( "Properties.json is empty" );
            }

            var properties = Newtonsoft.Json.JsonConvert.DeserializeObject<List<dynamic>>( json );

            if ( properties == null )
            {
                Assert.Fail( "Properties.json is invalid" );
            }

            var orderedProperties = properties.OrderBy( e => e.Name );

            var shortNames = new HashSet<string>();

            foreach ( var property in orderedProperties )
            {
                string shortName = property.ShortName;

                if ( string.IsNullOrEmpty( shortName ) )
                {
                    continue;
                }

                if ( shortNames.Contains( shortName ) )
                {
                    Assert.Fail( $"Duplicate short name: {shortName}" );
                }

                Debug.WriteLine( $"|{shortName}|{property.Name}|" );

                shortNames.Add( shortName );
            }

            Assert.IsTrue( shortNames.Count > 0 );
        }
    }
}
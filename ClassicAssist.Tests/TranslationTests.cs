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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using ClassicAssist.Data.Hotkeys.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests
{
    [TestClass]
    public class TranslationTests
    {
        private static readonly string[] _fileNames = { "Strings", "MacroCommandHelp" };
        private static readonly string[] _locales = { "en-AU", "en-GB", "it-IT", "pl-PL", "ko-KR", "cs", "zh" };

        [TestMethod]
        public void EnsureAllTranslationContainInputParameters()
        {
            foreach ( string fileName in _fileNames )
            {
                Dictionary<string, string> neutral = GetAllResXKeyValue( fileName );
                List<string> checkKeys = ( from keyValuePair in neutral
                    where Regex.Match( keyValuePair.Value, "{\\d+}" ).Success
                    select keyValuePair.Key ).ToList();

                foreach ( string locale in _locales )
                {
                    Dictionary<string, string> lang = GetAllResXKeyValue( $"{fileName}.{locale}" );

                    foreach ( string checkKey in from checkKey in checkKeys
                             let matches = Regex.Matches( neutral[checkKey], "{(\\d+)}" )
                             from Match match in matches
                             where lang.ContainsKey( checkKey )
                             where !Regex.IsMatch( lang[checkKey], "{(" + match.Groups[1].Value + ")}" )
                             select checkKey )
                    {
                        Assert.Fail( $"'{checkKey}' missing input param for language '{locale}'" );
                    }
                }
            }
        }

        [TestMethod]
        public void InstantiateAllHotkeysTranslationCheck()
        {
            // Constructor of HotkeyCommand will throw an exception if Name, Tooltip or Category is not translated...

            Assembly assembly = Assembly.LoadFile( Path.Combine( Environment.CurrentDirectory, "ClassicAssist.dll" ) );

            IEnumerable<Type> hotkeyCommands = assembly.GetTypes()
                .Where( i => i.IsSubclassOf( typeof( HotkeyCommand ) ) );

            foreach ( Type hotkeyCommand in hotkeyCommands )
            {
                Activator.CreateInstance( hotkeyCommand );
            }
        }

        private static Dictionary<string, string> GetAllResXKeyValue( string fileName )
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            string path = $@"..\..\..\..\ClassicAssist\Resources\{fileName}.resx";

            if ( !File.Exists( path ) )
            {
                path = $@"..\..\..\..\ClassicAssist.Shared\Resources\{fileName}.resx";
            }

            if ( !File.Exists( path ) )
            {
                Debug.WriteLine( $"Cannot find resx file: {fileName}" );
                return dictionary;
            }

            using ( ResXResourceReader resxReader = new ResXResourceReader( path ) )
            {
                foreach ( DictionaryEntry entry in resxReader )
                {
                    dictionary.Add( entry.Key.ToString(), entry.Value.ToString() );
                }
            }

            return dictionary;
        }
    }
}
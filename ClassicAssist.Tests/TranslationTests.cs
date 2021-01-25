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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests
{
    [TestClass]
    public class TranslationTests
    {
        private static readonly string[] _fileNames = { "Strings", "MacroCommandHelp" };
        private static readonly string[] _locales = { "en-AU", "en-GB", "it-IT", "pl-PL", "ko-KR" };

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

        private static Dictionary<string, string> GetAllResXKeyValue( string fileName )
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            if ( !File.Exists( $@"..\..\..\..\ClassicAssist\Resources\{fileName}.resx" ) )
            {
                Debug.WriteLine( $"Cannot find resx file: {fileName}" );
                return dictionary;
            }

            using ( ResXResourceReader resxReader =
                new ResXResourceReader( $@"..\..\..\..\ClassicAssist\Resources\{fileName}.resx" ) )
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
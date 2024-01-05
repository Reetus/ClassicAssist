using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ClassicAssist.Data;
using ClassicAssist.Helpers;

namespace ClassicAssist.UO.Data
{
    public static class Cliloc
    {
        private static Lazy<Dictionary<int, string>> _lazyClilocList =
            new Lazy<Dictionary<int, string>>( () => LoadClilocs() );

        private static Lazy<Dictionary<int, string>> _lazyENUClilocList =
            new Lazy<Dictionary<int, string>>( () => LoadClilocs( true ) );

        private static string _dataPath;

        public static bool CanUseCUOClilocLanguage { get; set; } = true;

        private static Dictionary<int, string> LoadClilocs( bool englishOnly = false )
        {
            string filename = Path.Combine( _dataPath, "Cliloc.enu" );

            if ( AssistantOptions.UseCUOClilocLanguage && !englishOnly )
            {
                dynamic settings = Reflection.GetTypeFieldValue<dynamic>( "ClassicUO.Configuration.Settings",
                    "GlobalSettings", null );

                if ( settings != null )
                {
                    dynamic clilocFile =
                        Reflection.GetTypePropertyValue<dynamic>( settings.GetType(), "ClilocFile", settings )
                            ?.ToString() ?? string.Empty;

                    if ( string.IsNullOrEmpty( clilocFile ) )
                    {
                        dynamic language =
                            Reflection.GetTypePropertyValue<dynamic>( settings.GetType(), "Language", settings )
                                ?.ToString() ?? string.Empty;

                        clilocFile = $"cliloc.{language}";
                    }


                    if ( string.IsNullOrEmpty( clilocFile ) && !File.Exists( Path.Combine( _dataPath, clilocFile ) ) )
                    {
                        CanUseCUOClilocLanguage = false;
                    }
                    else
                    {
                        filename = Path.Combine( _dataPath, clilocFile );
                    }
                }
            }

            if ( !File.Exists( filename ) )
            {
                throw new FileNotFoundException( "File not found.", filename );
            }

            byte[] fileBytes = File.ReadAllBytes( filename );

            Dictionary<int, string> clilocList = new Dictionary<int, string>( 100000 );

            ushort len;

            for ( int x = 6; x < fileBytes.Length; x += 7 + len )
            {
                len = BitConverter.ToUInt16( fileBytes, x + 5 );
                int cliloc = BitConverter.ToInt32( fileBytes, x );
                string value = Encoding.UTF8.GetString( fileBytes, x + 7, len );

                if ( !clilocList.ContainsKey( cliloc ) )
                {
                    clilocList.Add( cliloc, value );
                }
            }

            return clilocList;
        }

        public static string GetLocalString( string tokenizedString )
        {
            if ( tokenizedString.ToLower().Contains( "http://" ) || tokenizedString.ToLower().Contains( "https://" ) )
            {
                return tokenizedString;
            }

            while ( tokenizedString.Contains( "#" ) )
            {
                for ( int x = 0; x < tokenizedString.Length; x++ )
                {
                    if ( tokenizedString[x] != '#' || x >= tokenizedString.Length - 1 )
                    {
                        continue;
                    }

                    if ( !char.IsNumber( tokenizedString[x + 1] ) )
                    {
                        return tokenizedString;
                    }

                    int y;

                    for ( y = x + 1; y < tokenizedString.Length; y++ )
                    {
                        if ( !char.IsNumber( tokenizedString[y] ) )
                        {
                            break;
                        }
                    }

                    string token = tokenizedString.Substring( x, y - x );
                    string tokenNum = tokenizedString.Substring( x + 1, y - x - 1 );

                    if ( tokenNum.Length <= 0 )
                    {
                        continue;
                    }

                    int propertyNum;

                    try
                    {
                        propertyNum = Convert.ToInt32( tokenNum );
                    }
                    catch
                    {
                        return tokenizedString;
                    }

                    string property = GetProperty( propertyNum );
                    tokenizedString = tokenizedString.Replace( token, property );
                }
            }

            return tokenizedString;
        }

        public static string GetLocalString( int property, string[] arguments )
        {
            string propertyString = GetProperty( property );

            if ( arguments == null )
            {
                return propertyString;
            }

            //foreach (string s in arguments)
            for ( int x = 0; x < arguments.Length; x++ )
            {
                arguments[x] = GetLocalString( arguments[x] );
                bool found = false;
                int start = 0;
                int index = 0;

                foreach ( char c in propertyString )
                {
                    if ( c == '~' )
                    {
                        if ( found )
                        {
                            string subString = propertyString.Substring( start, index - start + 1 );
                            propertyString = propertyString.Replace( subString, arguments[x] );

                            break;
                        }

                        start = index;
                        found = true;
                    }

                    index++;
                }

                if ( !found )
                {
                    return propertyString;
                }
            }

            return propertyString;
        }

        public static void Initialize( string dataPath )
        {
            _dataPath = dataPath;
        }

        internal static void Initialize( Func<Dictionary<int, string>> customInitializer = null )
        {
            // For use in unit tests
            if ( customInitializer != null )
            {
                _lazyClilocList = new Lazy<Dictionary<int, string>>( customInitializer );
                _lazyENUClilocList = new Lazy<Dictionary<int, string>>( customInitializer );
            }
        }

        public static string GetProperty( int property )
        {
            if ( _lazyClilocList.Value.TryGetValue( property, out string propertyString ) )
            {
                return propertyString;
            }

            return _lazyENUClilocList.Value.TryGetValue( property, out string enuPropertyString )
                ? enuPropertyString
                : $"Localized string {property} not found!";
        }

        public static Dictionary<int, string> GetItems()
        {
            return new Dictionary<int, string>( _lazyClilocList.Value );
        }
    }
}
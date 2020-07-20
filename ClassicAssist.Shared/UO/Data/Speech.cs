using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ClassicAssist.UO.Data
{
    public static class Speech
    {
        private static readonly Lazy<SpeechEntry[]> _entries = new Lazy<SpeechEntry[]>( LoadEntries );

        private static string _dataPath;

        public static int[] GetKeywords( string input )
        {
            IEnumerable<SpeechEntry> matches =
                _entries.Value.Where( e => Regex.IsMatch( input, WildCardToRegular( e.Keywords ) ) );

            return matches.Select( e => e.Id ).Distinct().ToArray();
        }

        public static void Initialize( string dataPath )
        {
            _dataPath = dataPath;
        }

        private static SpeechEntry[] LoadEntries()
        {
            string fullPath = Path.Combine( _dataPath, "speech.mul" );

            using ( FileStream reader =
                new FileStream( fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) )
            {
                using ( BinaryReader binaryReader = new BinaryReader( reader ) )
                {
                    List<SpeechEntry> entries = new List<SpeechEntry>();

                    while ( reader.Position < reader.Length )
                    {
                        int id = ( binaryReader.ReadByte() << 8 ) | binaryReader.ReadByte();
                        int length = ( binaryReader.ReadByte() << 8 ) | binaryReader.ReadByte();

                        byte[] buffer = new byte[length];

                        binaryReader.Read( buffer, 0, length );

                        string text = Encoding.UTF8.GetString( buffer );

                        if ( text.Contains( "guards" ) )
                        {
                        }

                        entries.Add( new SpeechEntry { Id = id, Keywords = text } );
                    }

                    return entries.ToArray();
                }
            }
        }

        private static string WildCardToRegular( string value )
        {
            return "^" + Regex.Escape( value ).Replace( "\\*", ".*" ) + "$";
        }

        internal struct SpeechEntry
        {
            public int Id { get; set; }
            public string Keywords { get; set; }
        }
    }
}
#region License

// Copyright (C) 2020 Reetus
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
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ClassicAssist.UO.Data
{
    public class Statics
    {
        private static string _dataPath;

        private static readonly Lazy<StaticRecord[][]>[] _staticData = new Lazy<StaticRecord[][]>[6];

        public static int[,] _defaultMapSize { get; set; } =
        {
            { 7168, 4096 }, { 7168, 4096 }, { 2304, 1600 }, { 2560, 2048 }, { 1448, 1448 }, { 1280, 4096 }
        };

        internal static void Initialize( string dataPath )
        {
            _dataPath = dataPath;

            for ( int i = 0; i < 6; i++ )
            {
                int map = i;
                _staticData[i] = new Lazy<StaticRecord[][]>( () => LoadStatics( map ) );
            }
        }

        private static StaticRecord[][] LoadStatics( int map )
        {
            string staticIndexFile = Path.Combine( _dataPath, $"staidx{map}.mul" );
            string staticMulFile = Path.Combine( _dataPath, $"statics{map}.mul" );

            if ( !File.Exists( staticIndexFile ) )
            {
                throw new FileNotFoundException( "File not found!", staticIndexFile );
            }

            if ( !File.Exists( staticMulFile ) )
            {
                throw new FileNotFoundException( "File not found!", staticMulFile );
            }

            byte[] indexBytes = File.ReadAllBytes( staticIndexFile );

            StaticRecord[][] staticItems = new StaticRecord[indexBytes.Length / 12][];

            using ( FileStream fileStream =
                new FileStream( staticMulFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) )
            {
                using ( BinaryReader binaryReader = new BinaryReader( fileStream ) )
                {
                    for ( int x = 0; x < indexBytes.Length / 12; x++ )
                    {
                        int offset = x * 12;
                        int start = BitConverter.ToInt32( indexBytes, offset );
                        int length = BitConverter.ToInt32( indexBytes, offset + 4 );

                        if ( start == -1 )
                        {
                            continue;
                        }

                        fileStream.Seek( start, SeekOrigin.Begin );

                        staticItems[x] = new StaticRecord[length / 7];

                        for ( int y = 0; y < length / 7; y++ )
                        {
                            staticItems[x][y].ID = binaryReader.ReadUInt16();
                            staticItems[x][y].X = binaryReader.ReadByte();
                            staticItems[x][y].Y = binaryReader.ReadByte();
                            staticItems[x][y].Z = binaryReader.ReadSByte();
                            staticItems[x][y].Hue = binaryReader.ReadUInt16();
                        }
                    }
                }
            }

            return staticItems;
        }

        public static StaticTile[] GetStatics( int map, int x, int y )
        {
            if ( _staticData[map].Value == null )
            {
                return null;
            }

            int blockIndex = x / 8 * ( _defaultMapSize[map, 1] / 8 ) + y / 8;

            int cellX = x % 8;
            int cellY = y % 8;

            StaticRecord[] blockStatics = _staticData[map].Value[blockIndex];

            if ( blockStatics == null )
            {
                return null;
            }

            IEnumerable<StaticRecord> statics = blockStatics.Where( i => i.X == cellX && i.Y == cellY );

            List<StaticTile> tiles = new List<StaticTile>();

            foreach ( StaticRecord staticRecord in statics )
            {
                StaticTile tile = TileData.GetStaticTile( staticRecord.ID );
                tile.X = x;
                tile.Y = y;
                tile.Z = staticRecord.Z;
                tile.Hue = staticRecord.Hue;

                tiles.Add( tile );
            }

            return tiles.ToArray();
        }

        private struct StaticRecord
        {
            public ushort ID { get; set; }
            public byte X { get; set; }
            public byte Y { get; set; }
            public sbyte Z { get; set; }
            public int Hue { get; set; }
        }
    }
}
/**
 * This was mostly copied from the ClassicUO project, to harmonize the data structures and the ability to read the same files.
 */
using Assistant;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ClassicAssist.UO.Data
{
    public static class TileData
    {
        private static string _dataPath;
        private static StaticTile[] _staticData;
        private static LandTile[] _landData;

        public static void Initialize( string dataPath )
        {
            _dataPath = dataPath;
        }

        private static void Load()
        {
            if ( string.IsNullOrEmpty( _dataPath ) || !Directory.Exists( _dataPath ) )
            {
                // throwing an exception would be correct - but the unit tests depend on this behavior
                return;
            }

            string fileName = Path.Combine( _dataPath, "tiledata.mul" );

            if ( !File.Exists( fileName ) )
            {
                throw new FileNotFoundException( "File not found.", fileName );
            }

            using ( FileStream stream = File.Open( fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) )
            using ( BinaryReader tileData = new BinaryReader( stream ) )
            {
                bool isold = Engine.ClientVersion < Version.Parse( "7.0.9.0" );
                const int LAND_SIZE = 512;

                int land_group = isold ? Marshal.SizeOf<LandGroupOld>() : Marshal.SizeOf<LandGroupNew>();
                int static_group = isold ? Marshal.SizeOf<StaticGroupOld>() : Marshal.SizeOf<StaticGroupNew>();
                int staticscount = (int) ( ( stream.Length - LAND_SIZE * land_group ) / static_group );

                if ( staticscount > 2048 )
                {
                    staticscount = 2048;
                }

                stream.Seek( 0, System.IO.SeekOrigin.Begin );

                _landData = new LandTile[0x4000];
                _staticData = new StaticTile[staticscount * 32];

                Span<byte> buf = stackalloc byte[20];

                for ( int i = 0; i < 512; i++ )
                {
                    tileData.ReadUInt32();

                    for ( int j = 0; j < 32; j++ )
                    {
                        if ( stream.Position + ( isold ? 4 : 8 ) + 2 + 20 > stream.Length )
                        {
                            goto END;
                        }

                        int idx = i * 32 + j;
                        _landData[idx].Flags = (TileFlags) ( isold ? tileData.ReadUInt32() : tileData.ReadUInt64() );
                        _landData[idx].ID = tileData.ReadUInt16();

                        _landData[idx].Name = string.Intern( Encoding.UTF8.GetString( tileData.ReadBytes( 20 ) ).TrimEnd( '\0' ) );
                    }
                }

                END:

                for ( int i = 0; i < staticscount; i++ )
                {
                    if ( stream.Position >= stream.Length )
                    {
                        break;
                    }

                    tileData.ReadUInt32();

                    for ( int j = 0; j < 32; j++ )
                    {
                        if ( stream.Position + ( isold ? 4 : 8 ) + 13 + 20 > stream.Length )
                        {
                            goto END_2;
                        }

                        int idx = i * 32 + j;
                        _staticData[idx].ID = (ushort)idx;
                        _staticData[idx].Flags = (TileFlags) ( isold ? tileData.ReadUInt32() : tileData.ReadUInt64() );
                        _staticData[idx].Weight = tileData.ReadByte();
                        _staticData[idx].Layer = tileData.ReadByte();
                        _staticData[idx].Quantity = tileData.ReadInt32();
                        /*_staticData[idx].AnimId =*/
                        tileData.ReadUInt16();
                        _staticData[idx].Hue = tileData.ReadUInt16();
                        /*_staticData[idx].LightIndex =*/
                        tileData.ReadUInt16();
                        _staticData[idx].Height = tileData.ReadByte();

                        _staticData[idx].Name = string.Intern( Encoding.UTF8.GetString( tileData.ReadBytes( 20 ) ).TrimEnd( '\0' ) );
                    }
                }

            END_2:
                tileData.Dispose();
            }
        }

        public static LandTile GetLandTile( int index )
        {
            if (_landData == null )
            {
                Load();
            }
            return _landData[index];
        }

        public static StaticTile GetStaticTile( int index )
        {
            if ( _staticData == null )
            {
                Load();
            }
            try
            {
                return _staticData[index];
            }
            catch ( NullReferenceException )
            {
                return new StaticTile { Name = "unknown" };
            }
            catch ( IndexOutOfRangeException )
            {
                return _staticData[0];
            }
        }

        public static Layer GetLayer( int id )
        {
            StaticTile td = GetStaticTile( id );

            if ( !td.Flags.HasFlag( TileFlags.Wearable ) )
            {
                return Layer.Invalid;
            }

            Layer layer = (Layer) td.Layer;

            return layer;
        }
    }


    // old

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public struct LandGroupOld
    {
        public uint Unknown;
        [MarshalAs( UnmanagedType.ByValArray, SizeConst = 32 )]
        public LandTilesOld[] Tiles;
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public struct LandTilesOld
    {
        public uint Flags;
        public ushort TexID;
        [MarshalAs( UnmanagedType.LPStr, SizeConst = 20 )]
        public string Name;
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public struct StaticGroupOld
    {
        public uint Unk;
        [MarshalAs( UnmanagedType.ByValArray, SizeConst = 32 )]
        public StaticTilesOld[] Tiles;
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public struct StaticTilesOld
    {
        public uint Flags;
        public byte Weight;
        public byte Layer;
        public int Count;
        public ushort AnimID;
        public ushort Hue;
        public ushort LightIndex;
        public byte Height;
        [MarshalAs( UnmanagedType.LPStr, SizeConst = 20 )]
        public string Name;
    }

    // new

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public struct LandGroupNew
    {
        public uint Unknown;
        [MarshalAs( UnmanagedType.ByValArray, SizeConst = 32 )]
        public LandTilesNew[] Tiles;
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public struct LandTilesNew
    {
        public TileFlag Flags;
        public ushort TexID;
        [MarshalAs( UnmanagedType.LPStr, SizeConst = 20 )]
        public string Name;
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public struct StaticGroupNew
    {
        public uint Unk;
        [MarshalAs( UnmanagedType.ByValArray, SizeConst = 32 )]
        public StaticTilesNew[] Tiles;
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public struct StaticTilesNew
    {
        public TileFlag Flags;
        public byte Weight;
        public byte Layer;
        public int Count;
        public ushort AnimID;
        public ushort Hue;
        public ushort LightIndex;
        public byte Height;
        [MarshalAs( UnmanagedType.LPStr, SizeConst = 20 )]
        public string Name;
    }

    [Flags]
    public enum TileFlag : ulong
    {
        /// <summary>
        ///     Nothing is flagged.
        /// </summary>
        None = 0x00000000,
        /// <summary>
        ///     Not yet documented.
        /// </summary>
        Background = 0x00000001,
        /// <summary>
        ///     Not yet documented.
        /// </summary>
        Weapon = 0x00000002,
        /// <summary>
        ///     Not yet documented.
        /// </summary>
        Transparent = 0x00000004,
        /// <summary>
        ///     The tile is rendered with partial alpha-transparency.
        /// </summary>
        Translucent = 0x00000008,
        /// <summary>
        ///     The tile is a wall.
        /// </summary>
        Wall = 0x00000010,
        /// <summary>
        ///     The tile can cause damage when moved over.
        /// </summary>
        Damaging = 0x00000020,
        /// <summary>
        ///     The tile may not be moved over or through.
        /// </summary>
        Impassable = 0x00000040,
        /// <summary>
        ///     Not yet documented.
        /// </summary>
        Wet = 0x00000080,
        /// <summary>
        ///     Unknown.
        /// </summary>
        Unknown1 = 0x00000100,
        /// <summary>
        ///     The tile is a surface. It may be moved over, but not through.
        /// </summary>
        Surface = 0x00000200,
        /// <summary>
        ///     The tile is a stair, ramp, or ladder.
        /// </summary>
        Bridge = 0x00000400,
        /// <summary>
        ///     The tile is stackable
        /// </summary>
        Generic = 0x00000800,
        /// <summary>
        ///     The tile is a window. Like <see cref="TileFlag.NoShoot" />, tiles with this flag block line of sight.
        /// </summary>
        Window = 0x00001000,
        /// <summary>
        ///     The tile blocks line of sight.
        /// </summary>
        NoShoot = 0x00002000,
        /// <summary>
        ///     For single-amount tiles, the string "a " should be prepended to the tile name.
        /// </summary>
        ArticleA = 0x00004000,
        /// <summary>
        ///     For single-amount tiles, the string "an " should be prepended to the tile name.
        /// </summary>
        ArticleAn = 0x00008000,
        /// <summary>
        ///     Not yet documented.
        /// </summary>
        Internal = 0x00010000,
        /// <summary>
        ///     The tile becomes translucent when walked behind. Boat masts also have this flag.
        /// </summary>
        Foliage = 0x00020000,
        /// <summary>
        ///     Only gray pixels will be hued
        /// </summary>
        PartialHue = 0x00040000,
        /// <summary>
        ///     Unknown.
        /// </summary>
        NoHouse = 0x00080000,
        /// <summary>
        ///     The tile is a map--in the cartography sense. Unknown usage.
        /// </summary>
        Map = 0x00100000,
        /// <summary>
        ///     The tile is a container.
        /// </summary>
        Container = 0x00200000,
        /// <summary>
        ///     The tile may be equiped.
        /// </summary>
        Wearable = 0x00400000,
        /// <summary>
        ///     The tile gives off light.
        /// </summary>
        LightSource = 0x00800000,
        /// <summary>
        ///     The tile is animated.
        /// </summary>
        Animation = 0x01000000,
        /// <summary>
        ///     Gargoyles can fly over
        /// </summary>
        NoDiagonal = 0x02000000,
        /// <summary>
        ///     Unknown.
        /// </summary>
        Unknown2 = 0x04000000,
        /// <summary>
        ///     Not yet documented.
        /// </summary>
        Armor = 0x08000000,
        /// <summary>
        ///     The tile is a slanted roof.
        /// </summary>
        Roof = 0x10000000,
        /// <summary>
        ///     The tile is a door. Tiles with this flag can be moved through by ghosts and GMs.
        /// </summary>
        Door = 0x20000000,
        /// <summary>
        ///     Not yet documented.
        /// </summary>
        StairBack = 0x40000000,
        /// <summary>
        ///     Not yet documented.
        /// </summary>
        StairRight = 0x80000000,
        /// Blend Alphas, tile blending.
        AlphaBlend = 0x0100000000,
        /// Uses new art style?
        UseNewArt = 0x0200000000,
        /// Has art being used?
        ArtUsed = 0x0400000000,
        /// Disallow shadow on this tile, lightsource? lava?
        NoShadow = 0x1000000000,
        /// Let pixels bleed in to other tiles? Is this Disabling Texture Clamp?
        PixelBleed = 0x2000000000,
        /// Play tile animation once.
        PlayAnimOnce = 0x4000000000,
        /// Movable multi? Cool ships and vehicles etc?
        MultiMovable = 0x10000000000
    }
}
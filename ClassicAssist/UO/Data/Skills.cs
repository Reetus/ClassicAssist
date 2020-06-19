using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ClassicAssist.UO.Data
{
    public enum LockStatus : byte
    {
        Up,
        Down,
        Locked
    }

    public enum StatType : byte
    {
        Str,
        Dex,
        Int
    }

    internal class SkillData
    {
        public bool Invokable { get; internal set; }
        public string Name { get; internal set; }
    }

    public static class Skills
    {
        private static Lazy<Dictionary<int, SkillData>> _lazySkillData;
        private static string _dataPath;

        internal static void Initialize( string dataPath )
        {
            _dataPath = dataPath;
            _lazySkillData = new Lazy<Dictionary<int, SkillData>>( LoadSkills );
        }

        public static string GetSkillName( int skillID )
        {
            return _lazySkillData.Value.ContainsKey( skillID ) ? _lazySkillData.Value[skillID].Name : string.Empty;
        }

        public static bool IsInvokable( int skillID )
        {
            if ( skillID >= 0 && skillID <= _lazySkillData.Value.Count - 1 )
            {
                return _lazySkillData.Value[skillID].Invokable;
            }

            return false;
        }

        /// <summary>
        ///     Get string array of all skill names for UO installation defined in options.
        ///     Array returned will be indexed by skill ID.
        /// </summary>
        public static string[] GetSkillNames()
        {
            string[] skillNames = (string[]) _lazySkillData.Value.Values.Select( s => s.Name );

            return skillNames;
        }

        internal static SkillData[] GetSkillsArray()
        {
            return _lazySkillData?.Value.Values.ToArray();
        }

        internal static Dictionary<int, SkillData> GetSkills()
        {
            return _lazySkillData.Value;
        }

        internal static Dictionary<int, SkillData> LoadSkills()
        {
            string skillIndexFile = Path.Combine( _dataPath, "Skills.idx" );
            string skillMulFile = Path.Combine( _dataPath, "skills.mul" );
            Dictionary<int, SkillData> skills = new Dictionary<int, SkillData>();

            if ( !File.Exists( skillIndexFile ) )
            {
                throw new FileNotFoundException( "File not found!", skillIndexFile );
            }

            if ( !File.Exists( skillMulFile ) )
            {
                throw new FileNotFoundException( "File not found!", skillMulFile );
            }

            byte[] indexBytes = File.ReadAllBytes( skillIndexFile );
            byte[] mulBytes = File.ReadAllBytes( skillMulFile );

            for ( int x = 0; x < indexBytes.Length / 12; x++ )
            {
                int offset = x * 12;
                int start = BitConverter.ToInt32( indexBytes, offset );
                int length = BitConverter.ToInt32( indexBytes, offset + 4 );

                if ( length == 0 )
                {
                    break;
                }

                skills.Add( x,
                    new SkillData
                    {
                        Invokable = mulBytes[start] == 1,
                        Name = Encoding.ASCII.GetString( mulBytes, start + 1, length - 2 )
                    } );
            }

            return skills;
        }
    }
}
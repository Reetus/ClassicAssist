using System;
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

    internal class SkillData
    {
        public bool Invokable { get; internal set; }
        public string Name { get; internal set; }
    }

    public static class Skills
    {
        private static Lazy<SkillData[]> _lazySkillData;
        private static string _dataPath;

        internal static void Initialize( string dataPath )
        {
            _dataPath = dataPath;
            _lazySkillData = new Lazy<SkillData[]>( LoadSkills );
        }

        public static string GetSkillName( int skillID )
        {
            if ( skillID >= 0 && skillID <= _lazySkillData.Value.Length - 1 )
            {
                return _lazySkillData.Value[skillID].Name;
            }

            return "";
        }

        /// <summary>
        ///     Get string array of all skill names for UO installation defined in options.
        ///     Array returned will be indexed by skill ID.
        /// </summary>
        public static string[] GetSkillNames()
        {
            string[] skillNames = (string[]) _lazySkillData.Value.Select( s => s.Name );

            return skillNames;
        }

        internal static SkillData[] GetSkills()
        {
            return (SkillData[]) _lazySkillData?.Value.Clone();
        }

        internal static SkillData[] LoadSkills()
        {
            string skillIndexFile = Path.Combine( _dataPath, "Skills.idx" );
            string skillMulFile = Path.Combine( _dataPath, "skills.mul" );

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

            SkillData[] skillArray = new SkillData[indexBytes.Length / 12];

            for ( int x = 0; x < skillArray.Length; x++ )
            {
                int offset = x * 12;
                int start = BitConverter.ToInt32( indexBytes, offset );
                int length = BitConverter.ToInt32( indexBytes, offset + 4 );

                if ( length == 0 )
                {
                    SkillData[] newArray = new SkillData[x];
                    Array.Copy( skillArray, 0, newArray, 0, x );
                    skillArray = newArray;
                    break;
                }

                skillArray[x] = new SkillData
                {
                    Invokable = mulBytes[start] == 1,
                    Name = Encoding.ASCII.GetString( mulBytes, start + 1, length - 2 )
                };
            }

            return skillArray;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ClassicAssist.MacroBrowser.Models;
using LiteDB;
using Newtonsoft.Json;

namespace ClassicAssist.MacroBrowser.Data
{
    public class Database
    {
        private const string DATABASE_FILE = "macros.db";

        private const string MANIFEST_URL =
            "https://raw.githubusercontent.com/Reetus/ClassicAssist-Macros/master/Macros/metadata.json";

        private const string MACRO_BASEURL =
            "https://raw.githubusercontent.com/Reetus/ClassicAssist-Macros/master/Macros/";

        private static Database _instance;
        private static readonly object _instanceLock = new object();
        private readonly Github _github = Github.GetInstance();
        private readonly HttpClient _httpClient;
        private ILiteCollection<Manifest> _manifest;
        private ILiteCollection<Repository> _repository;

        private Database()
        {
            _httpClient = new HttpClient();
            string databasePath = Path.Combine( Environment.CurrentDirectory, DATABASE_FILE );
            LiteDatabase database = new LiteDatabase( $"Filename={databasePath};Mode=Shared" );

            InitCollections( database );
        }

        private void InitCollections( ILiteDatabase database )
        {
            _repository = database.GetCollection<Repository>( "repository" );
            _repository.EnsureIndex( x => x.Id );
            _manifest = database.GetCollection<Manifest>( "manifest" );
            _manifest.EnsureIndex( x => x.Id );
        }

        public async Task<string> GetLatestCommitHash()
        {
            Repository latest = _repository.Find( r => true ).OrderByDescending( r => r.DateTime ).FirstOrDefault() ??
                                new Repository();

            string hash = latest.CommitHash;

            if ( !string.IsNullOrEmpty( hash ) && DateTime.Now - latest.DateTime <= TimeSpan.FromMinutes( 5 ) )
            {
                return hash;
            }

            hash = await _github.GetLatestCommitHash();
            latest.CommitHash = hash;
            latest.DateTime = DateTime.Now;
            _repository.Upsert( latest );

            return hash;
        }

        public async Task<Manifest> GetManifest()
        {
            Manifest latest = _manifest.Find( r => true ).OrderByDescending( r => r.DateTime ).FirstOrDefault();

            if ( latest != null && !( DateTime.Now - latest?.DateTime >= TimeSpan.FromMinutes( 5 ) ) )
            {
                return latest;
            }

            latest = await FetchManifest();
            _manifest.Upsert( latest );

            return latest;
        }

        public async Task<string[]> GetMacros()
        {
            Manifest manifest = await GetManifest();

            return manifest.Files.Select( f => f.Name ).OrderBy( f => f ).ToArray();
        }

        public async Task<string[]> GetShards()
        {
            Manifest manifest = await GetManifest();

            return manifest.Files.Where( f => f.Shard != null ).Select( f => f.Shard ).OrderBy( f => f ).Distinct()
                .ToArray();
        }

        private async Task<Manifest> FetchManifest()
        {
            HttpResponseMessage response = await _httpClient.GetAsync( MANIFEST_URL );

            string json = await response.Content.ReadAsStringAsync();

            Metadata[] obj = JsonConvert.DeserializeObject<Metadata[]>( json );

            if ( obj == null )
            {
                return null;
            }

            Manifest manifest = new Manifest { Files = obj };

            return manifest;
        }

        public static Database GetInstance()
        {
            // ReSharper disable once InvertIf
            if ( _instance == null )
            {
                lock ( _instanceLock )
                {
                    if ( _instance == null )
                    {
                        _instance = new Database();
                    }
                }
            }

            return _instance;
        }

        public async Task<string[]> GetEras()
        {
            Manifest manifest = await GetManifest();

            return manifest.Files.Where( f => f.Era != null ).Select( f => f.Era ).OrderBy( f => f ).Distinct()
                .ToArray();
        }

        public async Task<string[]> GetAuthors()
        {
            Manifest manifest = await GetManifest();

            return manifest.Files.Where( f => f.Author != null ).Select( f => f.Author ).OrderBy( f => f ).Distinct()
                .ToArray();
        }

        public async Task<string[]> GetCategories()
        {
            Manifest manifest = await GetManifest();

            List<string> results = new List<string>();

            IEnumerable<string[]> categories = manifest.Files.Select( f => f.Categories ).OrderBy( f => f[0] )
                .ThenBy( f => f.Length ).Distinct();

            foreach ( string[] category in categories )
            {
                if ( category.Length == 0 )
                {
                    continue;
                }

                if ( !results.Contains( category[0] ) )
                {
                    results.Add( category[0] );
                }

                if ( category.Length == 1 )
                {
                    continue;
                }

                string res = $"--{category[1]}";

                if ( category.Length > 2 )
                {
                    for ( int i = 2; i < category.Length; i++ )
                    {
                        res += $" ˃ {category[i]}";
                    }
                }

                if ( !results.Contains( res ) )
                {
                    results.Add( res );
                }
            }

            return results.ToArray();
        }

        public async Task<string> GetMacroByName( string value )
        {
            Manifest manifest = await GetManifest();

            Metadata macro = manifest.Files.FirstOrDefault( f => f.Name == value );

            if ( macro == null )
            {
                return string.Empty;
            }

            string url = string.Concat( MACRO_BASEURL, macro.FileName.Replace( '\\', '/' ) );

            string result = await _httpClient.GetStringAsync( url );

            return result;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Assistant;
using ClassicAssist.Browser.Models;
using LiteDB;
using Newtonsoft.Json;
using Octokit;
using Repository = ClassicAssist.Browser.Models.Repository;

namespace ClassicAssist.Browser.Data
{
    public class Database
    {
        private const string DATABASE_FILE = "macros.db";

        private const string MANIFEST_URL =
            "https://raw.githubusercontent.com/Reetus/ClassicAssist-Macros/master/Macros/metadata.json";

        private const string MACRO_RAW_BASEURL =
            "https://raw.githubusercontent.com/Reetus/ClassicAssist-Macros/master/Macros/";

        private const string MACRO_BASEURL = "https://github.com/Reetus/ClassicAssist-Macros/tree/master/Macros/";

        private const string REPOSITORY_OWNER = "Reetus";
        private const string REPOSITORY_NAME = "ClassicAssist-Macros";

        private static Database _instance;
        private static readonly object _instanceLock = new object();
        private readonly GitHubClient _github;
        private readonly HttpClient _httpClient;
        private ILiteCollection<Macro> _macros;
        private ILiteCollection<Manifest> _manifest;
        private ILiteCollection<Repository> _repository;

        private Database()
        {
            _httpClient = new HttpClient();
            string databasePath = Path.Combine( Engine.StartupPath ?? Environment.CurrentDirectory, DATABASE_FILE );
            LiteDatabase database = new LiteDatabase( $"Filename={databasePath};Connection=Shared" );
            _github = new GitHubClient( new ProductHeaderValue( "ClassicAssist" ) );

            InitCollections( database );
        }

        private void InitCollections( ILiteDatabase database )
        {
            _repository = database.GetCollection<Repository>( "repository" );
            _repository.EnsureIndex( x => x.Id );
            _manifest = database.GetCollection<Manifest>( "manifest" );
            _manifest.EnsureIndex( x => x.Id );
            _macros = database.GetCollection<Macro>( "macros" );
            _macros.EnsureIndex( x => x.Id );
        }

        public string GetCommitHash()
        {
            Repository latest = _repository.Find( r => true ).OrderByDescending( r => r.DateTime ).FirstOrDefault() ??
                                new Repository();

            return latest.CommitHash;
        }

        public async Task<string> GetGithubHash()
        {
            Reference reference = await _github.Git.Reference.Get( REPOSITORY_OWNER, REPOSITORY_NAME, "heads/master" );

            string hash = reference.Object.Sha;

            Repository latest = _repository.Find( r => true ).OrderByDescending( r => r.DateTime ).FirstOrDefault() ??
                                new Repository();

            latest.CommitHash = hash;
            latest.DateTime = DateTime.Now;
            _repository.Upsert( latest );

            return hash;
        }

        public async Task<Manifest> GetManifest()
        {
            Manifest latest = _manifest.Find( r => true ).OrderByDescending( r => r.DateTime ).FirstOrDefault();

            if ( latest != null && !( DateTime.Now - latest.DateTime >= TimeSpan.FromMinutes( 15 ) ) )
            {
                return latest;
            }

            try
            {
                if ( latest != null && GetCommitHash() == await GetGithubHash() )
                {
                    return latest;
                }
            }
            catch ( RateLimitExceededException )
            {
                // Github throttle error
            }

            latest = await FetchManifest();
            _manifest.Upsert( latest );

            return latest;
        }

        public async Task<string[]> GetMacros( IEnumerable<Filter> filter = null )
        {
            Manifest manifest = await GetManifest();

            if ( filter == null )
            {
                return manifest.Files.Select( f => f.Name ).OrderBy( f => f ).ToArray();
            }

            string shardFilter = filter.FirstOrDefault( t => t.FilterType == FilterType.Shard )?.Value;
            string eraFilter = filter.FirstOrDefault( t => t.FilterType == FilterType.Era )?.Value;
            string authorFilter = filter.FirstOrDefault( t => t.FilterType == FilterType.Author )?.Value;

            Predicate<string[]> categoryPredicate =
                CategoryToPredicate( filter.FirstOrDefault( t => t.FilterType == FilterType.Category ) );

            IEnumerable<string> macros = manifest.Files.Where( m =>
                ( string.IsNullOrEmpty( shardFilter ) || m.Shard != null && m.Shard.Equals( shardFilter ) ) &&
                ( string.IsNullOrEmpty( eraFilter ) || m.Era == null || m.Era.Equals( eraFilter ) ) &&
                ( string.IsNullOrEmpty( authorFilter ) || m.Author == null || m.Author.Equals( authorFilter ) ) &&
                categoryPredicate( m.Categories ) ).Select( m => m.Name );

            return macros.ToArray();
        }

        private Predicate<string[]> CategoryToPredicate( Filter filter )
        {
            if ( filter.Category == null )
            {
                return f => true;
            }

            return m =>
            {
                if ( m.Length < filter.Category.Values.Length )
                {
                    return false;
                }

                for ( int i = 0; i < filter.Category.Values.Length; i++ )
                {
                    string macroCategory = m[i];
                    string filterCategory = filter.Category.Values[i];

                    if ( !macroCategory.Equals( filterCategory ) )
                    {
                        return false;
                    }
                }

                return true;
            };
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

        public async Task<Category[]> GetCategories()
        {
            Manifest manifest = await GetManifest();

            List<Category> results = new List<Category>();

            IEnumerable<string[]> categories = manifest.Files.Select( f => f.Categories ).OrderBy( f => f[0] )
                .ThenBy( f => f.Length ).Distinct();

            foreach ( string[] category in categories )
            {
                if ( category.Length == 0 )
                {
                    continue;
                }

                if ( results.All( e => e.Name != category[0] ) )
                {
                    results.Add( new Category { Name = category[0], Values = new[] { category[0] } } );
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

                if ( results.All( e => e.Name != res ) )
                {
                    results.Add( new Category { Name = res, Values = category } );
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

            Macro cached = _macros.Find( m => m.Name == macro.Name && m.SHA1 == macro.SHA1 ).FirstOrDefault();

            if ( cached != null )
            {
                return cached.Text;
            }

            string url = string.Concat( MACRO_RAW_BASEURL, macro.FileName.Replace( '\\', '/' ) );

            string result = await _httpClient.GetStringAsync( url );

            if ( string.IsNullOrEmpty( result ) )
            {
                return result;
            }

            Macro inserted = new Macro { Name = macro.Name, SHA1 = macro.SHA1, Text = result };
            _macros.Insert( inserted );

            return result;
        }

        public async void OpenGithubMacroURL( string value )
        {
            Manifest manifest = await GetManifest();

            Metadata macro = manifest.Files.FirstOrDefault( f => f.Name == value );

            if ( macro == null )
            {
                return;
            }

            string url = string.Concat( MACRO_BASEURL, macro.FileName.Replace( '\\', '/' ) );

            Process.Start( url );
        }
    }
}
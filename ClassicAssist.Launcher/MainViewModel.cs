using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using ClassicAssist.Launcher.Properties;
using ClassicAssist.Shared.Misc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Trinet.Core.IO.Ntfs;
using Application = System.Windows.Application;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace ClassicAssist.Launcher
{
    public class MainViewModel : BaseViewModel
    {
        private const string CONFIG_FILENAME = "Launcher.json";
        private ICommand _checkforUpdateCommand;
        private ObservableCollection<string> _clientPaths = new ObservableCollection<string>();
        private ICommand _closingCommand;
        private ObservableCollection<string> _dataPaths = new ObservableCollection<string>();
        private ICommand _optionsCommand;
        private ICommand _selectClientPathCommand;
        private ICommand _selectDataPathCommand;
        private string _selectedClientPath;
        private string _selectedDataPath;
        private ShardEntry _selectedShard;

        private ICommand _showShardsWindowCommand;
        private ICommand _startCommand;

        public MainViewModel()
        {
            RemoveAlternateDataStreams( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ) );

            string fullPath = Path.Combine( Environment.CurrentDirectory, CONFIG_FILENAME );

            if ( !File.Exists( fullPath ) )
            {
                return;
            }

            using ( JsonTextReader jtr = new JsonTextReader( new StreamReader( fullPath ) ) )
            {
                JObject config = (JObject) JToken.ReadFrom( jtr );

                if ( config["ClientPaths"] != null )
                {
                    foreach ( JToken token in config["ClientPaths"] )
                    {
                        string path = token.ToObject<string>();

                        if ( File.Exists( path ) )
                        {
                            ClientPaths.Add( path );
                        }
                    }
                }

                if ( config["SelectedClientPath"] != null )
                {
                    string path = config["SelectedClientPath"].ToObject<string>();

                    SelectedClientPath = File.Exists( path ) ? path : ClientPaths.FirstOrDefault();
                }

                if ( config["DataPaths"] != null )
                {
                    foreach ( JToken token in config["DataPaths"] )
                    {
                        string path = token.ToObject<string>();

                        if ( Directory.Exists( path ) )
                        {
                            DataPaths.Add( path );
                        }
                    }
                }

                if ( config["SelectedDataPath"] != null )
                {
                    string path = config["SelectedDataPath"].ToObject<string>();

                    SelectedDataPath = Directory.Exists( path ) ? path : DataPaths.FirstOrDefault();
                }

                ShardManager.OverridePresets = config["OverridePresets"]?.ToObject<bool>() ?? false;

                if ( ShardManager.OverridePresets && config["Presets"] != null )
                {
                    ShardManager.Shards.Clear();
                }

                ShardsHash = config["ShardsHash"]?.ToObject<string>() ?? string.Empty;
                ShardsDateTime = config["ShardsDateTime"]?.ToObject<DateTime?>();

                if ( config["Presets"] != null )
                {
                    foreach ( JToken token in config["Presets"] )
                    {
                        ShardEntry shard = new ShardEntry
                        {
                            Name = token["Name"]?.ToObject<string>() ?? "Unknown",
                            Address = token["Address"]?.ToObject<string>() ?? "localhost",
                            Port = token["Port"]?.ToObject<int>() ?? 2593,
                            HasStatusProtocol = token["HasStatusProtocol"]?.ToObject<bool>() ?? true,
                            Encryption = token["Encryption"]?.ToObject<bool>() ?? false,
                            Website = token["Website"]?.ToObject<string>(),
                            IsPreset = true
                        };

                        ShardManager.Shards.AddSorted( shard, new ShardEntryComparer() );
                    }
                }

                if ( config["Shards"] != null )
                {
                    foreach ( JToken token in config["Shards"] )
                    {
                        ShardEntry shard = new ShardEntry
                        {
                            Name = token["Name"]?.ToObject<string>() ?? "Unknown",
                            Address = token["Address"]?.ToObject<string>() ?? "localhost",
                            Port = token["Port"]?.ToObject<int>() ?? 2593,
                            Website = token["Website"]?.ToObject<string>(),
                            HasStatusProtocol = token["HasStatusProtocol"]?.ToObject<bool>() ?? true,
                            Encryption = token["Encryption"]?.ToObject<bool>() ?? false
                        };

                        ShardManager.Shards.AddSorted( shard, new ShardEntryComparer() );
                    }
                }

                if ( config["DeletedPresets"] != null )
                {
                    foreach ( JToken token in config["DeletedPresets"] )
                    {
                        ShardEntry shard = new ShardEntry
                        {
                            Name = token["Name"]?.ToObject<string>() ?? "Unknown", IsPreset = true
                        };

                        ShardEntry preset = ShardManager.Shards.FirstOrDefault( e => e.Equals( shard ) );

                        if ( preset != null )
                        {
                            preset.Deleted = true;
                        }
                    }
                }

                if ( config["SelectedShard"] != null )
                {
                    ShardEntry match = ShardManager.Shards.FirstOrDefault(
                        s => s.Name == config["SelectedShard"]?.ToObject<string>() );

                    if ( match != null )
                    {
                        SelectedShard = match;
                    }
                }

                if ( config["Plugins"] != null )
                {
                    foreach ( JToken token in config["Plugins"] )
                    {
                        string pluginPath = token.ToObject<string>();
                        Plugins.Add( new PluginEntry { Name = Path.GetFileName( pluginPath ), FullPath = pluginPath } );
                    }
                }

                ReadClassicOptions( config );

                if ( !ShardsDateTime.HasValue || DateTime.Now - ShardsDateTime >= TimeSpan.FromHours( 24 ) )
                {
                    CheckPresets().ConfigureAwait( false );
                }
            }
        }

        public ICommand CheckForUpdateCommand =>
            _checkforUpdateCommand ?? ( _checkforUpdateCommand = new RelayCommand( CheckForUpdate, UpdaterExists ) );

        public ClassicOptions ClassicOptions { get; set; } = new ClassicOptions();

        public ObservableCollection<string> ClientPaths
        {
            get => _clientPaths;
            set => SetProperty( ref _clientPaths, value );
        }

        public ICommand ClosingCommand =>
            _closingCommand ?? ( _closingCommand = new RelayCommand( Closing, o => true ) );

        public ObservableCollection<string> DataPaths
        {
            get => _dataPaths;
            set => SetProperty( ref _dataPaths, value );
        }

        public ICommand OptionsCommand =>
            _optionsCommand ?? ( _optionsCommand = new RelayCommand( ShowOptionsWindow, o => true ) );

        public List<PluginEntry> Plugins { get; set; } = new List<PluginEntry>();

        public ICommand SelectClientPathCommand =>
            _selectClientPathCommand ?? ( _selectClientPathCommand = new RelayCommand( SelectClientPath ) );

        public ICommand SelectDataPathCommand =>
            _selectDataPathCommand ?? ( _selectDataPathCommand = new RelayCommand( SelectDataPath ) );

        public string SelectedClientPath
        {
            get => _selectedClientPath;
            set => SetProperty( ref _selectedClientPath, value );
        }

        public string SelectedDataPath
        {
            get => _selectedDataPath;
            set => SetProperty( ref _selectedDataPath, value );
        }

        public ShardEntry SelectedShard
        {
            get => _selectedShard;
            set => SetProperty( ref _selectedShard, value );
        }

        public ShardManager ShardManager => ShardManager.GetInstance();

        public DateTime? ShardsDateTime { get; set; }

        public string ShardsHash { get; set; } = string.Empty;

        public ICommand ShowShardsWindowCommand =>
            _showShardsWindowCommand ?? ( _showShardsWindowCommand = new RelayCommand( ShowShardsWindow, o => true ) );

        public ICommand StartCommand =>
            _startCommand ?? ( _startCommand = new RelayCommandAsync( Start,
                o => !string.IsNullOrEmpty( SelectedClientPath ) && !string.IsNullOrEmpty( SelectedDataPath ) ) );

        private async Task CheckPresets()
        {
            try
            {
                string shardsHashUrl = ConfigurationManager.AppSettings["SHARDS_HASH_URL"];
                string shardsUrl = ConfigurationManager.AppSettings["SHARDS_URL"];

                if ( string.IsNullOrEmpty( shardsHashUrl ) || string.IsNullOrEmpty( shardsUrl ) )
                {
                    return;
                }

                string hash = await GetShardsHash( shardsHashUrl );

                if ( !string.IsNullOrEmpty( hash ) && !ShardsHash.Equals( hash ) )
                {
                    List<ShardEntry> shards = await GetShards( shardsUrl );

                    if ( shards != null )
                    {
                        foreach ( ShardEntry shardEntry in shards )
                        {
                            shardEntry.IsPreset = true;
                        }

                        ShardManager.ImportPresets( shards );
                        ShardsHash = hash;
                        ShardsDateTime = DateTime.Now;
                    }
                }
            }
            catch ( Exception )
            {
                // we tried
            }
        }

        private static async Task<string> GetShardsHash( string shardsHashUrl )
        {
            HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.GetAsync( shardsHashUrl );

            if ( !response.IsSuccessStatusCode )
            {
                return null;
            }

            string json = await response.Content.ReadAsStringAsync();

            JToken obj = JToken.Parse( json );

            string hash = obj["SHA1"]?.ToObject<string>();

            return hash;
        }

        private static async Task<List<ShardEntry>> GetShards( string shardsUrl )
        {
            HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.GetAsync( shardsUrl );

            if ( !response.IsSuccessStatusCode )
            {
                return null;
            }

            string json = await response.Content.ReadAsStringAsync();

            List<ShardEntry> shards = JsonConvert.DeserializeObject<List<ShardEntry>>( json );

            return shards;
        }

        private static void RemoveAlternateDataStreams( string path )
        {
            string manifestFile = Path.Combine( path, "MANIFEST.json" );

            if ( !File.Exists( manifestFile ) )
            {
                return;
            }

            string json = File.ReadAllText( manifestFile );

            IEnumerable<ManifestEntry> manifestEntries =
                JsonConvert.DeserializeObject<IEnumerable<ManifestEntry>>( json );

            if ( manifestEntries == null )
            {
                return;
            }

            foreach ( ManifestEntry manifestEntry in manifestEntries )
            {
                string filePath = Path.Combine( path, manifestEntry.Name );

                if ( !File.Exists( filePath ) )
                {
                    continue;
                }

                FileInfo file = new FileInfo( filePath );

                if ( file.AlternateDataStreamExists( "Zone.Identifier" ) )
                {
                    file.DeleteAlternateDataStream( "Zone.Identifier" );
                }
            }
        }

        private void ReadClassicOptions( JObject config )
        {
            PropertyInfo[] properties =
                typeof( ClassicOptions ).GetProperties( BindingFlags.Public | BindingFlags.Instance );

            foreach ( PropertyInfo property in properties )
            {
                string propName = property.Name;
                Type propType = property.PropertyType;

                object defaultValue = null;

                ClassicOptionAttribute attr = property.GetCustomAttribute<ClassicOptionAttribute>();

                if ( attr?.DefaultValue != null )
                {
                    defaultValue = attr.DefaultValue;
                }

                if ( defaultValue == null )
                {
                    defaultValue = Activator.CreateInstance( propType );
                }

                object val = config[propName]?.ToObject( propType ) ?? defaultValue;

                property.SetValue( ClassicOptions, val );
            }
        }

        private void ShowOptionsWindow( object arg )
        {
            OptionsViewModel vm = new OptionsViewModel( Plugins, ClassicOptions );

            OptionsWindow window = new OptionsWindow { DataContext = vm };
            window.ShowDialog();

            if ( vm.DialogResult != DialogResult.OK )
            {
                return;
            }

            Plugins.Clear();
            Plugins.AddRange( vm.Plugins );
            ClassicOptions = vm.ClassicOptions;
        }

        private static bool UpdaterExists( object arg )
        {
            return File.Exists( Path.Combine( Environment.CurrentDirectory, "ClassicAssist.Updater.exe" ) );
        }

        private static void CheckForUpdate( object obj )
        {
            string updaterPath = Path.Combine( Environment.CurrentDirectory, "ClassicAssist.Updater.exe" );

            if ( File.Exists( updaterPath ) )
            {
                Process.Start( updaterPath, $"--pid {Process.GetCurrentProcess().Id}" );
            }
        }

        private void SelectClientPath( object obj )
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                CheckFileExists = true,
                Multiselect = false,
                Filter = "ClassicUO.exe|ClassicUO.exe;TazUO.exe",
                Title = Resources.Select_a_client
            };

            bool? result = ofd.ShowDialog();

            if ( !result.HasValue || !result.Value )
            {
                return;
            }

            if ( !ClientPaths.Contains( ofd.FileName ) )
            {
                ClientPaths.Add( ofd.FileName );
            }

            SelectedClientPath = ofd.FileName;
        }

        private void SelectDataPath( object obj )
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
            {
                Description = Resources.Select_your_Ultima_Online_directory, ShowNewFolderButton = false
            };
            DialogResult result = folderBrowserDialog.ShowDialog();

            if ( result != DialogResult.OK )
            {
                return;
            }

            if ( !DataPaths.Contains( folderBrowserDialog.SelectedPath ) )
            {
                DataPaths.Add( folderBrowserDialog.SelectedPath );
            }

            SelectedDataPath = folderBrowserDialog.SelectedPath;
        }

        /*
         * Command line parameter documentation...
         * https://github.com/andreakarasho/ClassicUO/wiki/Distribuite-ClassicUO
         * https://github.com/andreakarasho/ClassicUO/wiki/Launch-Arguments
         */
        private async Task Start( object obj )
        {
            IPAddress ip = await Utility.ResolveAddress( SelectedShard.Address );

            if ( ip == null )
            {
                MessageBox.Show( Resources.Unable_to_resolve_shard_hostname_ );
                return;
            }

            StringBuilder args = new StringBuilder();

            List<string> pluginList = new List<string> { GetPluginPath() };

            foreach ( PluginEntry plugin in Plugins )
            {
                pluginList.Add( plugin.FullPath );
            }

            string plugins = string.Join( ",", pluginList.ToArray() );

            args.Append( $"-plugins \"{plugins}\" " );
            args.Append( $"-ip \"{ip}\" -port \"{SelectedShard.Port}\" " );
            args.Append( $"-uopath \"{SelectedDataPath}\" " );
            args.Append( $"-encryption {( SelectedShard.Encryption ? 1 : 0 )} " );
            args.Append( SelectedShard.ShardType > 0 ? $"-shard {SelectedShard.ShardType} " : "-shard 0 " );

            BuildClassicOptions( args );
            
            ProcessStartInfo psi = new ProcessStartInfo
            {
                WorkingDirectory =
                    Path.GetDirectoryName( SelectedClientPath ) ?? throw new InvalidOperationException(),
                FileName = SelectedClientPath,
                Arguments = args.ToString(),
                UseShellExecute = true
            };

            Process p = Process.Start( psi );

            if ( p != null && !p.HasExited )
            {
                Application.Current.Shutdown( 0 );
            }
        }

        private string GetPluginPath()
        {
            string plugin = Path.Combine( Environment.CurrentDirectory, "ClassicAssist.dll" );

            ( bool netAssembly, string version ) = Utility.GetExeType( SelectedClientPath );

            if ( netAssembly || !File.Exists( SelectedClientPath.Replace( ".exe", ".dll" ) ) )
            {
                return plugin;
            }

            ( netAssembly, version ) = Utility.GetExeType( SelectedClientPath.Replace( ".exe", ".dll" ) );

            if ( netAssembly && version.Contains( "NETCoreApp" ) )
            {
                plugin = Path.Combine( Environment.CurrentDirectory, "..", "net9.0-windows", "ClassicAssist.Plugin.dll" );
            }

            return plugin;
        }

        private void BuildClassicOptions( StringBuilder args )
        {
            PropertyInfo[] properties =
                typeof( ClassicOptions ).GetProperties( BindingFlags.Public | BindingFlags.Instance );

            foreach ( PropertyInfo property in properties )
            {
                ClassicOptionAttribute attr = property.GetCustomAttribute<ClassicOptionAttribute>();
                object val = property.GetValue( ClassicOptions );

                if ( attr == null )
                {
                    continue;
                }

                bool skip = val is bool b && b == false && !attr.IncludeIfFalse;
                bool canInclude = true;

                if ( !string.IsNullOrEmpty( attr.CanIncludeProperty ) )
                {
                    PropertyInfo canIncludeProperty = typeof( ClassicOptions ).GetProperty( attr.CanIncludeProperty );

                    if ( canIncludeProperty != null )
                    {
                        canInclude = (bool) canIncludeProperty.GetValue( ClassicOptions );
                    }
                }

                if ( !skip && canInclude )
                {
                    args.Append( $"{attr.Argument} {val} " );
                }
            }
        }

        private void ShowShardsWindow( object obj )
        {
            ShardsWindow window = new ShardsWindow();
            window.ShowDialog();

            if ( !( window.DataContext is ShardsViewModel vm ) || vm.DialogResult != DialogResult.OK ||
                 vm.SelectedShard == null )
            {
                return;
            }

            SelectedShard = vm.SelectedShard;
        }

        private void Closing( object obj )
        {
            JObject config = new JObject();

            JArray clientPathArray = new JArray();

            foreach ( string clientPath in ClientPaths )
            {
                clientPathArray.Add( clientPath );
            }

            config.Add( "ClientPaths", clientPathArray );
            config.Add( "SelectedClientPath", SelectedClientPath ?? string.Empty );

            JArray dataPathArray = new JArray();

            foreach ( string dataPath in DataPaths )
            {
                dataPathArray.Add( dataPath );
            }

            config.Add( "DataPaths", dataPathArray );
            config.Add( "SelectedDataPath", SelectedDataPath ?? string.Empty );
            config.Add( "SelectedShard", SelectedShard?.Name );

            IEnumerable<ShardEntry> shards = ShardManager.Shards.Where( s => !s.IsPreset );

            JArray shardArray = new JArray();

            foreach ( ShardEntry shard in shards )
            {
                JObject shardObj = new JObject
                {
                    { "Name", shard.Name },
                    { "Address", shard.Address },
                    { "Port", shard.Port },
                    { "HasStatusProtocol", shard.HasStatusProtocol },
                    { "Encryption", shard.Encryption }
                };

                shardArray.Add( shardObj );
            }

            config.Add( "Shards", shardArray );

            IEnumerable<ShardEntry> deletedPresets = ShardManager.Shards.Where( s => s.IsPreset && s.Deleted );

            JArray deletedArray = new JArray();

            foreach ( ShardEntry shard in deletedPresets )
            {
                JObject shardObj = new JObject { { "Name", shard.Name } };

                deletedArray.Add( shardObj );
            }

            config.Add( "DeletedPresets", deletedArray );

            JArray pluginsArray = new JArray();

            foreach ( PluginEntry plugin in Plugins )
            {
                pluginsArray.Add( plugin.FullPath );
            }

            config.Add( "Plugins", pluginsArray );

            config.Add( "OverridePresets", ShardManager.OverridePresets );
            config.Add( "ShardsHash", ShardsHash );
            config.Add( "ShardsDateTime", ShardsDateTime );

            if ( ShardManager.OverridePresets )
            {
                IEnumerable<ShardEntry> presets = ShardManager.Shards.Where( s => s.IsPreset );

                JArray presetsArray = new JArray();

                foreach ( ShardEntry shard in presets )
                {
                    JObject shardObj = new JObject
                    {
                        { "Name", shard.Name },
                        { "Address", shard.Address },
                        { "Port", shard.Port },
                        { "HasStatusProtocol", shard.HasStatusProtocol },
                        { "Website", shard.Website },
                        { "Encryption", shard.Encryption }
                    };

                    presetsArray.Add( shardObj );
                }

                config.Add( "Presets", presetsArray );
            }

            WriteClassicOptions( config );

            using ( JsonTextWriter jtw =
                   new JsonTextWriter(
                       new StreamWriter( Path.Combine( Environment.CurrentDirectory, CONFIG_FILENAME ) ) ) )
            {
                jtw.Formatting = Formatting.Indented;
                config.WriteTo( jtw );
            }
        }

        private void WriteClassicOptions( JObject config )
        {
            PropertyInfo[] properties =
                typeof( ClassicOptions ).GetProperties( BindingFlags.Public | BindingFlags.Instance );

            foreach ( PropertyInfo property in properties )
            {
                string propName = property.Name;
                object val = property.GetValue( ClassicOptions );

                if ( val != null )
                {
                    config.Add( propName, val.ToString() );
                }
            }
        }
    }
}
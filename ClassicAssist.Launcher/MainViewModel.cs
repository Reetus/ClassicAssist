using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using ClassicAssist.Launcher.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Application = System.Windows.Application;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace ClassicAssist.Launcher
{
    public class MainViewModel : BaseViewModel
    {
        private const string CONFIG_FILENAME = "Launcher.json";
        private readonly ShardManager _manager;
        private ICommand _checkforUpdateCommand;
        private ObservableCollection<string> _clientPaths = new ObservableCollection<string>();
        private ICommand _closingCommand;
        private ObservableCollection<string> _dataPaths = new ObservableCollection<string>();
        private ICommand _selectClientPathCommand;
        private ICommand _selectDataPathCommand;
        private string _selectedClientPath;
        private string _selectedDataPath;
        private ShardEntry _selectedShard;
        private ObservableCollection<ShardEntry> _shardEntries = new ObservableCollection<ShardEntry>();
        private ICommand _showShardsWindowCommand;
        private ICommand _startCommand;

        public MainViewModel()
        {
            _manager = ShardManager.GetInstance();

            _manager.Shards.CollectionChanged += ( sender, args ) => { ShardEntries = _manager.Shards; };

            ShardEntries = _manager.Shards;

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

                if ( config["Shards"] != null )
                {
                    foreach ( JToken token in config["Shards"] )
                    {
                        ShardEntry shard = new ShardEntry
                        {
                            Name = token["Name"]?.ToObject<string>() ?? "Unknown",
                            Address = token["Address"]?.ToObject<string>() ?? "localhost",
                            Port = token["Port"]?.ToObject<int>() ?? 2593,
                            HasStatusProtocol = token["HasStatusProtocol"]?.ToObject<bool>() ?? true
                        };

                        ShardEntries.Add( shard );
                    }
                }

                if ( config["SelectedShard"] != null )
                {
                    ShardEntry match = _manager.Shards.FirstOrDefault(
                        s => s.Name == config["SelectedShard"].ToObject<string>() );

                    if ( match != null )
                    {
                        SelectedShard = match;
                    }
                }
            }
        }

        public ICommand CheckForUpdateCommand =>
            _checkforUpdateCommand ?? ( _checkforUpdateCommand = new RelayCommand( CheckForUpdate, UpdaterExists ) );

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

        public ObservableCollection<ShardEntry> ShardEntries
        {
            get => _shardEntries;
            set => SetProperty( ref _shardEntries, value );
        }

        public ICommand ShowShardsWindowCommand =>
            _showShardsWindowCommand ?? ( _showShardsWindowCommand = new RelayCommand( ShowShardsWindow, o => true ) );

        public ICommand StartCommand =>
            _startCommand ?? ( _startCommand = new RelayCommandAsync( Start,
                o => !string.IsNullOrEmpty( SelectedClientPath ) && !string.IsNullOrEmpty( SelectedDataPath ) ) );

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
            OpenFileDialog ofd =
                new OpenFileDialog
                {
                    CheckFileExists = true,
                    Multiselect = false,
                    Filter = "ClassicUO.exe|ClassicUO.exe",
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
            FolderBrowserDialog folderBrowserDialog =
                new FolderBrowserDialog
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

            args.Append( $"-plugins \"{Path.Combine( Environment.CurrentDirectory, "ClassicAssist.dll" )}\" " );
            args.Append( $"-ip \"{ip}\" -port \"{SelectedShard.Port}\" " );
            args.Append( $"-uopath \"{SelectedDataPath}\" " );

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

            config.Add( "SelectedShard", SelectedShard.Name );

            IEnumerable<ShardEntry> shards = _manager.Shards.Where( s => !s.IsPreset );

            JArray shardArray = new JArray();

            foreach ( ShardEntry shard in shards )
            {
                JObject shardObj = new JObject
                {
                    { "Name", shard.Name },
                    { "Address", shard.Address },
                    { "Port", shard.Port },
                    { "HasStatusProtocol", shard.HasStatusProtocol }
                };

                shardArray.Add( shardObj );
            }

            config.Add( "Shards", shardArray );

            using ( JsonTextWriter jtw =
                new JsonTextWriter( new StreamWriter( Path.Combine( Environment.CurrentDirectory, CONFIG_FILENAME ) ) )
            )
            {
                jtw.Formatting = Formatting.Indented;
                config.WriteTo( jtw );
            }
        }
    }
}
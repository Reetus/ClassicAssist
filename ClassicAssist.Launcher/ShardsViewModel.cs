using System;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using ClassicAssist.Launcher.Properties;

namespace ClassicAssist.Launcher
{
    public class ShardsViewModel : BaseViewModel
    {
        private readonly ShardManager _manager;
        private ICommand _addCommand;
        private ICommand _cancelCommand;
        private bool _isRefreshing;
        private ICommand _okCommand;
        private ICommand _refreshCommand;
        private ICommand _removeCommand;
        private ShardEntry _selectedShard;
        private ObservableCollection<ShardEntry> _shards;

        public ShardsViewModel()
        {
            _manager = ShardManager.GetInstance();
            Shards = _manager.Shards;

            Refresh( this );
        }

        public ICommand AddCommand => _addCommand ?? ( _addCommand = new RelayCommand( Add, o => true ) );

        public ICommand CancelCommand => _cancelCommand ?? ( _cancelCommand = new RelayCommand( Cancel, o => true ) );

        public DialogResult DialogResult { get; set; }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty( ref _isRefreshing, value );
        }

        public ICommand OKCommand => _okCommand ?? ( _okCommand = new RelayCommand( OK, o => true ) );

        public ICommand RefreshCommand =>
            _refreshCommand ?? ( _refreshCommand = new RelayCommand( Refresh, o => !IsRefreshing ) );

        public ICommand RemoveCommand =>
            _removeCommand ?? ( _removeCommand =
                new RelayCommand( Remove, o => SelectedShard != null && !SelectedShard.IsPreset ) );

        public ShardEntry SelectedShard
        {
            get => _selectedShard;
            set => SetProperty( ref _selectedShard, value );
        }

        public ObservableCollection<ShardEntry> Shards
        {
            get => _shards;
            set
            {
                SetProperty( ref _shards, value );
                _manager.Shards = value;
            }
        }

        private void Remove( object obj )
        {
            if ( !( obj is ShardEntry entry ) )
            {
                return;
            }

            if ( entry.IsPreset )
            {
                MessageBox.Show( Resources.Cannot_remove_preset_shard, Resources.Error );
                return;
            }

            Shards.Remove( entry );
        }

        private void Add( object obj )
        {
            Shards.Add( new ShardEntry { Name = "Shard Name", Address = "localhost", Port = 2593 } );
        }

        private void Refresh( object obj )
        {
            try
            {
                foreach ( ShardEntry shard in Shards )
                {
                    Task.Run( async () =>
                    {
                        if ( !shard.HasStatusProtocol )
                        {
                            return "-";
                        }

                        string status = await GetStatus( shard );

                        return status;
                    } ).ContinueWith( t =>
                    {
                        if ( !string.IsNullOrEmpty( shard.StatusRegex ) )
                        {
                            Match matches = Regex.Match( t.Result, shard.StatusRegex );

                            shard.Status = matches.Success ? matches.Groups[1].Value : "-";
                        }
                        else
                        {
                            shard.Status = t.Result;
                        }

                        NotifyPropertyChanged( nameof( Shards ) );
                    } );
                }
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private void OK( object obj )
        {
            DialogResult = DialogResult.OK;
        }

        public async Task<string> GetStatus( ShardEntry shard )
        {
            if ( !shard.HasStatusProtocol )
            {
                return "Unknown";
            }

            TcpClient client = new TcpClient();

            Task connectTask = client.ConnectAsync( shard.Address, shard.Port );

            await Task.WhenAny( connectTask, Task.Delay( TimeSpan.FromSeconds( 5 ) ) );

            if ( !client.Connected )
            {
                return "Unknown";
            }

            byte[] packet = { 0x7F, 0x00, 0x00, 0x7F, 0xF1, 0x00, 0x04, 0xFF };
            client.Client.Send( packet );

            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[256];

            stream.ReadTimeout = 60000;
            await stream.ReadAsync( buffer, 0, buffer.Length );

            client.Close();

            string status = Encoding.ASCII.GetString( buffer ).TrimEnd( '\0' );

            return status;
        }

        private void Cancel( object obj )
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using ClassicAssist.Updater.Annotations;
using ClassicAssist.Updater.Properties;
using Exceptionless;
using Exceptionless.Models;
using Octokit;
using Application = System.Windows.Application;

namespace ClassicAssist.Updater
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly Dispatcher _dispatcher;
        private long _downloadSize;
        private bool _isIndeterminate = true;
        private bool _isUpdating;
        private ObservableCollection<string> _items = new ObservableCollection<string>();

        public MainViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;

            Task.Run( async () =>
            {
                if ( App.CurrentOptions.Stage == UpdaterStage.Initial )
                {
                    string path = await Task.Run( CheckForUpdate );

                    if ( !string.IsNullOrEmpty( path ) )
                    {
                        App.CurrentOptions.UpdatePath = path;
                    }
                }

                if ( !string.IsNullOrEmpty( App.CurrentOptions.UpdatePath ) )
                {
                    IsUpdating = true;

                    AddText( Resources.Updating_files___ );

                    DirectoryInfo source = new DirectoryInfo( App.CurrentOptions.UpdatePath );
                    DirectoryInfo destination = new DirectoryInfo( App.CurrentOptions.Path );

                    CopyAll( source, destination );

                    string modulesZip = Directory.EnumerateFiles( App.CurrentOptions.UpdatePath, "Modules.zip" )
                        .FirstOrDefault();

                    if ( !string.IsNullOrEmpty( modulesZip ) )
                    {
                        string modulesPath = Path.Combine( destination.FullName, "Modules" );

                        if ( !Directory.Exists( modulesPath ) )
                        {
                            Directory.CreateDirectory( modulesPath );
                        }

                        AddText( Resources.Extracting_modules___ );

                        try
                        {
                            using ( ZipArchive zipFile = ZipFile.OpenRead( modulesZip ) )
                            {
                                foreach ( ZipArchiveEntry entry in zipFile.Entries )
                                {
                                    AddText( string.Format( Resources.Copying__0____, entry.FullName ) );

                                    EnsurePathsExist( modulesPath, entry.FullName );

                                    entry.ExtractToFile( Path.Combine( modulesPath, entry.FullName ), true );
                                }
                            }
                        }
                        // ReSharper disable once EmptyGeneralCatchClause
#pragma warning disable 168
                        catch ( Exception e )
#pragma warning restore 168
                        {
#if !DEBUG
                            ExceptionlessClient.Default.SubmitException( e );
#endif
                        }
                    }

                    AddText( Resources.Done_ );

                    IsUpdating = false;
                }
            } );
        }

        private static void EnsurePathsExist( string modulesPath, string fullName )
        {
            //TODO may need to recurse check path

            string path = Path.Combine( modulesPath, Path.GetDirectoryName( fullName ) ?? string.Empty );

            if( !Directory.Exists( path ) )
            {
                Directory.CreateDirectory( path );
            }
        }

        public long DownloadSize
        {
            get => _downloadSize;
            set
            {
                SetProperty( ref _downloadSize, value );
                IsIndeterminate = value == 0;
            }
        }

        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            set => SetProperty( ref _isIndeterminate, value );
        }

        public bool IsUpdating
        {
            get => _isUpdating;
            set => SetProperty( ref _isUpdating, value );
        }

        public ObservableCollection<string> Items
        {
            get => _items;
            set => SetProperty( ref _items, value );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        //https://stackoverflow.com/questions/627504/what-is-the-best-way-to-recursively-copy-contents-in-c/627518#627518
        public void CopyAll( DirectoryInfo source, DirectoryInfo target )
        {
            //check if the target directory exists
            if ( Directory.Exists( target.FullName ) == false )
            {
                Directory.CreateDirectory( target.FullName );
            }

            //copy all the files into the new directory

            foreach ( FileInfo fi in source.GetFiles() )
            {
                try
                {
                    fi.CopyTo( Path.Combine( target.ToString(), fi.Name ), true );
                    AddText( string.Format( Resources.Copying__0____, fi.Name ) );
                }
                catch ( IOException ie )
                {
                    //handle it here
                    AddText( ie.Message );
                }
            }

            //copy all the sub directories using recursion

            foreach ( DirectoryInfo diSourceDir in source.GetDirectories() )
            {
                DirectoryInfo nextTargetDir = target.CreateSubdirectory( diSourceDir.Name );
                CopyAll( diSourceDir, nextTargetDir );
            }
        }

        private async Task<string> CheckForUpdate()
        {
            try
            {
                IsUpdating = true;

                AddText( Resources.Checking_for_latest_release___ );

                Release latestRelease = await GetLatestRelease();

                if ( latestRelease == null )
                {
                    throw new InvalidOperationException( Resources.Unable_to_locate_GitHub_release );
                }

                AddText( $"{Resources.Latest_Release_} {latestRelease.TagName}" );

                Version newVersion = Version.Parse( latestRelease.TagName );

                if ( newVersion > App.CurrentOptions.CurrentVersion || App.CurrentOptions.Force )
                {
                    ExceptionlessClient.Default.SubmitEvent( new Event
                    {
                        Message =
                            $"Update: Previous version: {App.CurrentOptions.CurrentVersion}, New version: {newVersion}"
                    } );

                    Process[] clients = GetRunningClients()?.ToArray();

                    if ( clients != null )
                    {
                        if ( clients.Length > 0 )
                        {
                            DialogResult result = DialogResult.Cancel;

                            _dispatcher.Invoke( () =>
                            {
                                ProcessesViewModel vm = new ProcessesViewModel( clients );
                                ProcessesView window = new ProcessesView { DataContext = vm };

                                window.ShowDialog();
                                result = vm.DialogResult;
                            } );

                            if ( result != DialogResult.OK )
                            {
                                AddText( Resources.Update_cancelled___ );
                                return null;
                            }
                        }

                        foreach ( Process process in clients )
                        {
                            try
                            {
                                process.Kill();
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                    }

                    if ( latestRelease.Assets.Count == 0 )
                    {
                        throw new InvalidOperationException( Resources.No_release_asset___ );
                    }

                    ReleaseAsset assest = latestRelease.Assets.FirstOrDefault( a =>
                        a.Name.EndsWith( ".zip", StringComparison.InvariantCultureIgnoreCase ) );

                    if ( assest == null )
                    {
                        AddText( Resources.Cannot_locate_update_package___ );
                        return null;
                    }

                    AddText( $"{Resources.Downloading} {assest.Name}..." );

                    string fileName = await DownloadFile( assest.BrowserDownloadUrl, assest.Size );

                    AddText( Resources.Extracting_package___ );

                    string updatePath = await ExtractPackage( fileName, newVersion );

                    if ( !File.Exists( Path.Combine( updatePath, "ClassicAssist.Updater.exe" ) ) )
                    {
                        return updatePath;
                    }

                    // if directory contains updater...

                    ProcessStartInfo psi = new ProcessStartInfo(
                        Path.Combine( updatePath, "ClassicAssist.Updater.exe" ),
                        $"--stage Install --updatepath \"{updatePath}\" --path \"{App.CurrentOptions.Path}\"" );
                    Process.Start( psi );

                    _dispatcher.Invoke( () => Application.Current.Shutdown() );

                    return null;
                }
                else
                {
                    AddText( Resources.No_new_release_available___ );
                }
            }
            catch ( RateLimitExceededException e )
            {
                AddText( string.Format( Resources.GitHub_rate_limit__try_again_after__0_, e.Reset.ToLocalTime() ) );
            }
            catch ( Exception e )
            {
                AddText( $"{Resources.Error_} {e.Message}" );
            }
            finally
            {
                IsUpdating = false;
            }

            return null;
        }

        private static async Task<Release> GetLatestRelease()
        {
            Release latestRelease;

            if ( string.IsNullOrEmpty( App.CurrentOptions.URL ) )
            {
                GitHubClient client = new GitHubClient( new ProductHeaderValue( "ClassicAssist" ) );

                IReadOnlyList<Release> releases =
                    await client.Repository.Release.GetAll( Settings.Default.RepositoryOwner,
                        Settings.Default.RepositoryName );

                latestRelease = releases?.FirstOrDefault();
            }
            else
            {
                // for testing only
                List<ReleaseAsset> assets = new List<ReleaseAsset>
                {
                    new ReleaseAsset( $"{App.CurrentOptions.URL}/ClassicAssist.zip", 0, string.Empty,
                        "ClassicAssist.zip", string.Empty, string.Empty, string.Empty, 0, 0, DateTimeOffset.Now,
                        DateTimeOffset.Now, $"{App.CurrentOptions.URL}/ClassicAssist.zip", null )
                };

                latestRelease = new Release( App.CurrentOptions.URL, App.CurrentOptions.URL, App.CurrentOptions.URL,
                    App.CurrentOptions.URL, 0, App.CurrentOptions.URL, "0.0.0", string.Empty, "ClassicAssist.zip",
                    string.Empty, false, false, DateTimeOffset.Now, DateTimeOffset.Now, null, App.CurrentOptions.URL,
                    App.CurrentOptions.URL, assets );
            }

            return latestRelease;
        }

        private static async Task<string> ExtractPackage( string fileName, Version newVersion )
        {
            string path = Path.Combine( Path.GetTempPath(), $"CAUpdate-{newVersion}" );

            try
            {
                await Task.Run( () => ZipFile.ExtractToDirectory( fileName, path ) );
            }
            catch ( Exception )
            {
                // ignored
            }

            return path;
        }

        private static async Task<string> DownloadFile( string browserDownloadUrl, int size )
        {
            using ( HttpClient http = new HttpClient() )
            {
                http.Timeout = TimeSpan.FromMinutes( 5 );

                string fileName = Path.Combine( Environment.CurrentDirectory, "Update.zip" );

                http.DefaultRequestHeaders.Add( "User-Agent", "ClassicAssist Updater" );

                byte[] response = await http.GetByteArrayAsync( browserDownloadUrl );

                if ( response.Length != size && !App.CurrentOptions.Force )
                {
                    throw new InvalidOperationException( Resources.Downloaded_size_doesn_t_match_expected___ );
                }

                File.WriteAllBytes( fileName, response );

                return fileName;
            }
        }

        private void AddText( string message )
        {
            _dispatcher?.Invoke( () => Items.Add( message ) );
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        // ReSharper disable once RedundantAssignment
        private void SetProperty<T>( ref T obj, T value, [CallerMemberName] string propertyName = null )
        {
            obj = value;
            OnPropertyChanged( propertyName );
        }

        private static IEnumerable<Process> GetRunningClients()
        {
            List<Process> result = new List<Process>();

            Process[] processes = Process.GetProcesses();

            string dllPath = Path.Combine( App.CurrentOptions.Path, "ClassicAssist.dll" );

            foreach ( Process process in processes )
            {
                try
                {
                    if ( process.Modules.Cast<ProcessModule>().Any( module =>
                        module.FileName.Equals( dllPath, StringComparison.InvariantCultureIgnoreCase ) ) )
                    {
                        result.Add( process );
                    }
                }
                catch ( Exception )
                {
                    // ignored
                }
            }

            return result;
        }
    }
}
#region License

// Copyright (C) 2023 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Data.Regions;
using ClassicAssist.Data.Screenshot;
using ClassicAssist.Data.Targeting;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Misc;
using ClassicAssist.Shared.UI;
using ClassicAssist.UI.ViewModels.Agents.Screenshot;
using ClassicAssist.UI.Views.Agents.Screenshot;
using ClassicAssist.UI.Views.OptionsTab;
using ClassicAssist.UO.Objects;
using Microsoft.Scripting.Utils;
using Newtonsoft.Json.Linq;
using static ClassicAssist.Misc.NativeMethods;
using FlowDirection = System.Windows.FlowDirection;

namespace ClassicAssist.UI.ViewModels.Agents
{
    public class ScreenshotTabViewModel : BaseViewModel, ISettingProvider
    {
        private const string SCREENSHOT_DIRECTORY_NAME = "Screenshots";

        private const string DEFAULT_FILENAME_FORMAT = "ClassicAssist-{date}-{longTime}";
        private readonly ScreenshotComparer _comparer = new ScreenshotComparer();
        private readonly string[] _extensions = { ".png", ".gif" };
        private bool _autoScreenshot;
        private Color _backgroundColor;
        private ICommand _configureFilterCommand;
        private int _distance;
        private string _filenameFormat;
        private Color _fontColor;
        private int _fontSize;
        private string _format;
        private bool _fullscreen;
        private bool _includeInfoBar;
        private bool _mobileDeath;
        private int _mobileDeathDelay;
        private List<ScreenshotMobileFilterEntry> _mobileDeathFilter = new List<ScreenshotMobileFilterEntry>();
        private bool _onlyIfEnemy;
        private ICommand _openFolderCommand;
        private RelayCommand _openScreenshotCommand;
        private bool _playerDeath;
        private int _playerDeathDelay;
        private string _screenshotPath;
        private ObservableCollection<ScreenshotEntry> _screenshots = new ObservableCollection<ScreenshotEntry>();
        private ICommand _setBackgroundColourCommand;
        private ICommand _setFontColourCommand;
        private FileSystemWatcher _watcher;

        public ScreenshotTabViewModel()
        {
            ScreenshotManager manager = ScreenshotManager.GetInstance();
            manager.TakeScreenshot = TakeScreenshot;
            manager.OnPlayerDeath = OnPlayerDeath;
            manager.OnMobileDeath = OnMobileDeath;
        }

        public bool AutoScreenshot
        {
            get => _autoScreenshot;
            set => SetProperty( ref _autoScreenshot, value );
        }

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set => SetProperty( ref _backgroundColor, value );
        }

        public ICommand ConfigureFilterCommand =>
            _configureFilterCommand ?? ( _configureFilterCommand = new RelayCommand( ConfigureFilter ) );

        public int Distance
        {
            get => _distance;
            set => SetProperty( ref _distance, value );
        }

        public string FilenameFormat
        {
            get => _filenameFormat;
            set => SetProperty( ref _filenameFormat, value );
        }

        public Color FontColor
        {
            get => _fontColor;
            set => SetProperty( ref _fontColor, value );
        }

        public int FontSize
        {
            get => _fontSize;
            set => SetProperty( ref _fontSize, value );
        }

        public string Format
        {
            get => _format;
            set => SetProperty( ref _format, value );
        }

        public bool Fullscreen
        {
            get => _fullscreen;
            set => SetProperty( ref _fullscreen, value );
        }

        public bool IncludeInfoBar
        {
            get => _includeInfoBar;
            set => SetProperty( ref _includeInfoBar, value );
        }

        public bool MobileDeath
        {
            get => _mobileDeath;
            set => SetProperty( ref _mobileDeath, value );
        }

        public int MobileDeathDelay
        {
            get => _mobileDeathDelay;
            set => SetProperty( ref _mobileDeathDelay, value );
        }

        public List<ScreenshotMobileFilterEntry> MobileDeathFilter
        {
            get => _mobileDeathFilter;
            set => SetProperty( ref _mobileDeathFilter, value );
        }

        public bool OnlyIfEnemy
        {
            get => _onlyIfEnemy;
            set => SetProperty( ref _onlyIfEnemy, value );
        }

        public ICommand OpenFolderCommand =>
            _openFolderCommand ?? ( _openFolderCommand = new RelayCommand( OpenFolder ) );

        public ICommand OpenScreenshotCommand =>
            _openScreenshotCommand ?? ( _openScreenshotCommand = new RelayCommand( OpenScreenshot, o => o != null ) );

        public bool PlayerDeath
        {
            get => _playerDeath;
            set => SetProperty( ref _playerDeath, value );
        }

        public int PlayerDeathDelay
        {
            get => _playerDeathDelay;
            set => SetProperty( ref _playerDeathDelay, value );
        }

        public ObservableCollection<ScreenshotEntry> Screenshots
        {
            get => _screenshots;
            set => SetProperty( ref _screenshots, value );
        }

        public ICommand SetBackgroundColourCommand =>
            _setBackgroundColourCommand ?? ( _setBackgroundColourCommand =
                new RelayCommand( SetBackgroundColour, o => IncludeInfoBar ) );

        public ICommand SetFontColourCommand =>
            _setFontColourCommand ?? ( _setFontColourCommand = new RelayCommand( SetFontColour, o => IncludeInfoBar ) );

        public void Serialize( JObject json, bool global = false )
        {
            if ( json == null )
            {
                return;
            }

            JObject obj = new JObject
            {
                { "Fullscreen", Fullscreen },
                { "FilenameFormat", FilenameFormat },
                { "IncludeInfoBar", IncludeInfoBar },
                { "Format", Format },
                { "FontSize", FontSize },
                { "FontColor", FontColor.ToString() },
                { "BackgroundColor", BackgroundColor.ToString() },
                { "AutoScreenshot", AutoScreenshot },
                { "PlayerDeath", PlayerDeath },
                { "PlayerDeathDelay", PlayerDeathDelay },
                { "MobileDeath", MobileDeath },
                { "MobileDeathDelay", MobileDeathDelay },
                { "Distance", Distance },
                { "OnlyIfEnemy", OnlyIfEnemy }
            };

            JArray filter = new JArray();

            foreach ( ScreenshotMobileFilterEntry entry in MobileDeathFilter )
            {
                filter.Add( new JObject { { "ID", entry.ID }, { "Note", entry.Note }, { "Enabled", entry.Enabled } } );
            }

            obj.Add( "MobileDeathFilter", filter );
            json.Add( "Screenshot", obj );
        }

        public void Deserialize( JObject json, Options options, bool global = false )
        {
            if ( _watcher != null )
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
            }

            _screenshotPath = Path.Combine( Engine.StartupPath, SCREENSHOT_DIRECTORY_NAME );

            if ( !Directory.Exists( _screenshotPath ) )
            {
                Directory.CreateDirectory( _screenshotPath );
            }

            string[] files = _extensions.SelectMany( ext => Directory.GetFiles( _screenshotPath, $"*{ext}" ) )
                .ToArray();

            foreach ( string file in files )
            {
                AddScreenshot( file );
            }

            _watcher = new FileSystemWatcher( _screenshotPath, "*.*" ) { EnableRaisingEvents = true };
            _watcher.Created += OnScreenshotCreated;
            _watcher.Deleted += OnScreenshotDeleted;

            if ( json?["Screenshot"] == null )
            {
                return;
            }

            Fullscreen = json["Screenshot"]["Fullscreen"]?.ToObject<bool>() ?? false;
            FilenameFormat = json["Screenshot"]["FilenameFormat"]?.ToObject<string>() ?? DEFAULT_FILENAME_FORMAT;
            IncludeInfoBar = json["Screenshot"]["IncludeInfoBar"]?.ToObject<bool>() ?? true;
            Format = json["Screenshot"]["Format"]?.ToObject<string>() ?? "{player} ({shard}) - {date} {time}";
            FontSize = json["Screenshot"]["FontSize"]?.ToObject<int>() ?? 16;
            BackgroundColor = json["Screenshot"]["BackgroundColor"]?.ToObject<Color>() ?? Colors.Black;
            FontColor = json["Screenshot"]["FontColor"]?.ToObject<Color>() ?? Colors.White;
            AutoScreenshot = json["Screenshot"]["AutoScreenshot"]?.ToObject<bool>() ?? false;
            PlayerDeath = json["Screenshot"]["PlayerDeath"]?.ToObject<bool>() ?? false;
            PlayerDeathDelay = json["Screenshot"]["PlayerDeathDelay"]?.ToObject<int>() ?? 2000;
            MobileDeath = json["Screenshot"]["MobileDeath"]?.ToObject<bool>() ?? false;
            MobileDeathDelay = json["Screenshot"]["MobileDeathDelay"]?.ToObject<int>() ?? 500;
            Distance = json["Screenshot"]["Distance"]?.ToObject<int>() ?? 12;
            OnlyIfEnemy = json["Screenshot"]["OnlyIfEnemy"]?.ToObject<bool>() ?? false;

            if ( json["Screenshot"]["MobileDeathFilter"] is JArray mobileIdArray )
            {
                MobileDeathFilter = mobileIdArray.Select( e =>
                {
                    JObject obj = (JObject) e;

                    return new ScreenshotMobileFilterEntry
                    {
                        ID = obj["ID"]?.ToObject<int>() ?? 0,
                        Note = obj["Note"]?.ToObject<string>() ?? string.Empty,
                        Enabled = obj["Enabled"]?.ToObject<bool>() ?? false
                    };
                } ).ToList();
            }
            else
            {
                MobileDeathFilter = GetDefaultMobileIDs();
            }
        }

        private static List<ScreenshotMobileFilterEntry> GetDefaultMobileIDs()
        {
            TargetManager targetManager = TargetManager.GetInstance();

            return targetManager.BodyData
                .Where( b => b.BodyType == TargetBodyType.Humanoid && !b.Name.Contains( "Dead" ) ).Select( b =>
                    new ScreenshotMobileFilterEntry { ID = b.Graphic, Note = b.Name, Enabled = true } ).ToList();
        }

        private void ConfigureFilter( object obj )
        {
            ScreenshotMobileFilterViewModel vm = new ScreenshotMobileFilterViewModel();

            vm.Items.AddRange( MobileDeathFilter );

            ScreenshotMobileFilterWindow window = new ScreenshotMobileFilterWindow { DataContext = vm };

            window.ShowDialog();

            if ( vm.DialogResult == DialogResult.OK )
            {
                MobileDeathFilter = vm.Items.ToList();
            }
        }

        private void OnMobileDeath( Mobile mobile )
        {
            if ( !AutoScreenshot || !MobileDeath || !MobileDeathFilter.Any( e => e.ID == mobile.ID && e.Enabled ) ||
                 mobile.Distance > Distance )
            {
                return;
            }

            if ( OnlyIfEnemy )
            {
                int enemy = AliasCommands.GetAlias( "enemy" );

                if ( enemy != mobile.Serial )
                {
                    return;
                }
            }

            try
            {
                Task.Run( async () =>
                {
                    if ( MobileDeathDelay > 0 )
                    {
                        await Task.Delay( MobileDeathDelay );
                    }

                    TakeScreenshot( null, mobile.Name );
                } );
            }
            catch ( Exception )
            {
                // ignored
            }
        }

        public void OnPlayerDeath( string name )
        {
            if ( !AutoScreenshot || !PlayerDeath )
            {
                return;
            }

            try
            {
                Task.Run( async () =>
                {
                    if ( PlayerDeathDelay > 0 )
                    {
                        await Task.Delay( PlayerDeathDelay );
                    }

                    TakeScreenshot( null, name );
                } );
            }
            catch ( Exception )
            {
                // ignored
            }
        }

        private void SetBackgroundColour( object obj )
        {
            if ( !( obj is Color colour ) )
            {
                return;
            }

            MacrosGumpTextColorSelectorViewModel vm =
                new MacrosGumpTextColorSelectorViewModel { SelectedColor = colour };

            MacrosGumpTextColorSelectorWindow window = new MacrosGumpTextColorSelectorWindow { DataContext = vm };

            window.ShowDialog();

            if ( !vm.Result )
            {
                return;
            }

            BackgroundColor = vm.SelectedColor;
        }

        private void SetFontColour( object obj )
        {
            if ( !( obj is Color colour ) )
            {
                return;
            }

            MacrosGumpTextColorSelectorViewModel vm =
                new MacrosGumpTextColorSelectorViewModel { SelectedColor = colour };

            MacrosGumpTextColorSelectorWindow window = new MacrosGumpTextColorSelectorWindow { DataContext = vm };

            window.ShowDialog();

            if ( !vm.Result )
            {
                return;
            }

            FontColor = vm.SelectedColor;
        }

        private void OnScreenshotDeleted( object sender, FileSystemEventArgs e )
        {
            ScreenshotEntry screenshot = Screenshots.FirstOrDefault( s => s.Path.Equals( e.FullPath ) );

            if ( screenshot != null )
            {
                _dispatcher.Invoke( () => { Screenshots.Remove( screenshot ); } );
            }
        }

        private void AddScreenshot( string file )
        {
            _dispatcher.Invoke( () =>
            {
                if ( Screenshots.Any( s => s.Path.Equals( file ) ) )
                {
                    return;
                }

                Screenshots.AddSorted(
                    new ScreenshotEntry
                    {
                        Path = file,
                        BitmapSource = new Lazy<BitmapSource>( () => LoadBitmap( file ) ),
                        CreatedDate = File.GetCreationTime( file ),
                        Extension = Path.GetExtension( file ).Replace( ".", string.Empty ).ToUpper()
                    }, _comparer );
            } );
        }

        private static BitmapSource LoadBitmap( string file )
        {
            if ( !File.Exists( file ) )
            {
                return new BitmapImage();
            }

            try
            {
                using ( FileStream stream = new FileStream( file, FileMode.Open ) )
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    return bitmap;
                }
            }
            catch
            {
                return new BitmapImage();
            }
        }

        private void OnScreenshotCreated( object sender, FileSystemEventArgs e )
        {
            if ( !_extensions.Contains( Path.GetExtension( e.FullPath ) ) )
            {
                return;
            }

            Task.Delay( 1000 ).ContinueWith( t => AddScreenshot( e.FullPath ) );
        }

        private static void OpenScreenshot( object obj )
        {
            if ( !( obj is ScreenshotEntry screenshot ) )
            {
                return;
            }

            try
            {
                Process.Start( screenshot.Path );
            }
            catch
            {
                // We tried
            }
        }

        private void OpenFolder( object obj )
        {
            if ( !Directory.Exists( _screenshotPath ) )
            {
                return;
            }

            try
            {
                Process.Start( _screenshotPath );
            }
            catch
            {
                // We tried
            }
        }

        public string TakeScreenshot( bool? fullscreen = null, string mobileName = null, string filename = null )
        {
            IntPtr screenDC;
            int width, height;

            if ( fullscreen.HasValue && fullscreen.Value || Fullscreen )
            {
                screenDC = GetDC( IntPtr.Zero );
                width = (int) SystemParameters.VirtualScreenWidth;
                height = (int) SystemParameters.VirtualScreenHeight;
            }
            else
            {
                screenDC = GetDC( Engine.WindowHandle );
                GetClientRect( Engine.WindowHandle, out RECT rect );
                width = rect.Right - rect.Left;
                height = rect.Bottom - rect.Top;
            }

            IntPtr memDC = CreateCompatibleDC( screenDC );
            IntPtr hBitmap = CreateCompatibleBitmap( screenDC, width, height );
            SelectObject( memDC, hBitmap );

            BitBlt( memDC, 0, 0, width, height, screenDC, 0, 0, TernaryRasterOperations.SRCCOPY );
            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap( hBitmap, IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions() );

            DeleteObject( hBitmap );
            ReleaseDC( IntPtr.Zero, screenDC );
            ReleaseDC( IntPtr.Zero, memDC );

            DateTime now = DateTime.Now;

            string filePath =
                $"{GetFormattedText( !string.IsNullOrEmpty( filename ) ? filename : FilenameFormat, now, mobileName, true )}.png";

            DrawingVisual drawingVisual = new DrawingVisual();

            ImageSource screenshotLogo = Properties.Resources.screenshot_logo.ToImageSource();

            using ( DrawingContext drawingContext = drawingVisual.RenderOpen() )
            {
                drawingContext.DrawImage( bitmapSource, new Rect( 0, 0, bitmapSource.Width, bitmapSource.Height ) );

                ImageBrush imageBrush = new ImageBrush( screenshotLogo ) { Opacity = 0.6 };

                drawingContext.DrawRectangle( imageBrush, null,
                    new Rect( bitmapSource.Width - screenshotLogo.Width - 5, 5, screenshotLogo.Width,
                        screenshotLogo.Height ) );

                if ( IncludeInfoBar )
                {
                    string text = GetFormattedText( Format, now, mobileName );

                    FormattedText formattedText = new FormattedText( text, CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight, new Typeface( "Arial" ), FontSize, new SolidColorBrush( FontColor ),
                        96 );

                    drawingContext.DrawRoundedRectangle( new SolidColorBrush( BackgroundColor ), null,
                        new Rect( 0, 0, formattedText.Width + 10, formattedText.Height + 10 ), 5, 5 );
                    drawingContext.DrawText( formattedText, new Point( 5, 5 ) );
                }
            }

            RenderTargetBitmap result = new RenderTargetBitmap( (int) bitmapSource.Width, (int) bitmapSource.Height, 96,
                96, PixelFormats.Pbgra32 );
            result.Render( drawingVisual );

            if ( !Path.IsPathRooted( filePath ) )
            {
                string path = Path.Combine( Engine.StartupPath, "Screenshots" );

                if ( !Directory.Exists( path ) )
                {
                    Directory.CreateDirectory( path );
                }

                filePath = Path.Combine( path, filePath );
            }

            using ( FileStream fileStream = new FileStream( filePath, FileMode.Create ) )
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add( BitmapFrame.Create( result ) );
                encoder.Save( fileStream );
            }

            return filePath;
        }

        private static string GetFormattedText( string format, DateTime now, string mobileName,
            bool filenameChars = false )
        {
            if ( filenameChars && string.IsNullOrEmpty( format ) )
            {
                format = DEFAULT_FILENAME_FORMAT;
            }

            Dictionary<string, Func<string>> replacements = new Dictionary<string, Func<string>>
            {
                { "player", () => Engine.Player?.Name },
                { "shard", () => Engine.CurrentShard?.Name },
                { "mobile", () => mobileName },
                { "date", now.ToShortDateString },
                { "time", now.ToShortTimeString },
                { "longDate", now.ToLongDateString },
                { "longTime", now.ToLongTimeString },
                { "isoDate", () => now.ToString( "O" ) },
                { "x", () => Engine.Player?.X.ToString() },
                { "y", () => Engine.Player?.Y.ToString() },
                { "map", () => Engine.Player?.Map.ToString() },
                { "region", () => Regions.GetRegion( Engine.Player )?.Name },
                { "ticks", now.Ticks.ToString }
            };

            return Regex.Replace( format, "{(.*?)}", match =>
            {
                string key = match.Groups[1].Value;
                string replacementValue =
                    replacements.TryGetValue( key, out Func<string> replacement ) ? replacement() : key;
                string str = string.IsNullOrEmpty( replacementValue ) ? string.Empty : replacementValue;

                if ( filenameChars )
                {
                    str = Path.GetInvalidFileNameChars().Aggregate( str, ( current, c ) => current.Replace( c, '-' ) );
                }

                return str;
            } ).Trim();
        }

        public class ScreenshotComparer : IComparer<ScreenshotEntry>
        {
            public int Compare( ScreenshotEntry x, ScreenshotEntry y )
            {
                if ( ReferenceEquals( x, y ) )
                {
                    return 0;
                }

                if ( ReferenceEquals( null, y ) )
                {
                    return 1;
                }

                if ( ReferenceEquals( null, x ) )
                {
                    return -1;
                }

                return y.CreatedDate.CompareTo( x.CreatedDate );
            }
        }

        public class ScreenshotEntry
        {
            public Lazy<BitmapSource> BitmapSource { get; set; }
            public DateTime CreatedDate { get; set; }
            public string Extension { get; set; }
            public string Path { get; set; }
        }
    }
}
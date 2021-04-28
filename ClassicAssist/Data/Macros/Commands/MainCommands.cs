using System;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Assistant;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UI.ViewModels;
using ClassicAssist.UI.Views;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;
using static ClassicAssist.Misc.NativeMethods;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class MainCommands
    {
        [CommandsDisplay( Category = nameof( Strings.Main ), Parameters = new[] { nameof( ParameterType.OnOff ) } )]
        public static void SetQuietMode( bool onOff )
        {
            MacroManager.QuietMode = onOff;
        }

        [CommandsDisplay( Category = nameof( Strings.Main ), Parameters = new[] { nameof( ParameterType.String ) } )]
        [CommandsDisplayStringSeeAlso( new[] { nameof( Virtues ) } )]
        public static void InvokeVirtue( string virtue )
        {
            Virtues v = Utility.GetEnumValueByName<Virtues>( virtue );

            if ( v == Virtues.None )
            {
                return;
            }

            Engine.SendPacketToServer( new InvokeVirtue( v ) );
        }

        [CommandsDisplay( Category = nameof( Strings.Main ) )]
        public static void Resync()
        {
            UOC.Resync();
        }

        [CommandsDisplay( Category = nameof( Strings.Main ), Parameters = new[] { nameof( ParameterType.Timeout ) } )]
        public static void Pause( int milliseconds )
        {
            try
            {
                Thread.Sleep( milliseconds );
            }
            catch ( ThreadInterruptedException )
            {
                // Squash
            }
        }

        [CommandsDisplay( Category = nameof( Strings.Main ),
            Parameters = new[] { nameof( ParameterType.String ), nameof( ParameterType.Hue ) } )]
        public static void SysMessage( string text, int hue = 0x03b2 )
        {
            UOC.SystemMessage( text, hue );
        }

        [CommandsDisplay( Category = nameof( Strings.Main ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ) } )]
        public static void Info( object obj = null )
        {
            int serial = 0;

            if ( obj == null )
            {
                serial = UOC.GetTargetSerialAsync( Strings.Target_object___ ).Result;

                if ( serial == 0 )
                {
                    return;
                }
            }

            serial = AliasCommands.ResolveSerial( serial != 0 ? serial : obj );

            if ( serial == 0 )
            {
                return;
            }

            Entity entity = UOMath.IsMobile( serial )
                ? Engine.Mobiles.GetMobile( serial )
                : (Entity) Engine.Items.GetItem( serial );

            if ( entity == null )
            {
                UOC.SystemMessage( Strings.Cannot_find_item___ );
                return;
            }

            Thread t = new Thread( () =>
            {
                ObjectInspectorWindow window =
                    new ObjectInspectorWindow { DataContext = new ObjectInspectorViewModel( entity ) };

                window.ShowDialog();
            } ) { IsBackground = true };

            t.SetApartmentState( ApartmentState.STA );
            t.Start();
        }

        [CommandsDisplay( Category = nameof( Strings.Main ), Parameters = new[] { nameof( ParameterType.OnOff ) } )]
        public static void Hotkeys( string onOff = "toggle" )
        {
            HotkeyManager manager = HotkeyManager.GetInstance();

            switch ( onOff.Trim().ToLower() )
            {
                case "on":
                {
                    manager.Enabled = true;
                    break;
                }
                case "off":
                {
                    manager.Enabled = false;
                    break;
                }
                default:
                {
                    manager.Enabled = !manager.Enabled;
                    break;
                }
            }

            UOC.SystemMessage( manager.Enabled ? Strings.Hotkeys_enabled___ : Strings.Hotkeys_disabled___,
                manager.Enabled ? 0x3F : 36 );
        }

        [CommandsDisplay( Category = nameof( Strings.Main ), Parameters = new[] { nameof( ParameterType.OnOff ) } )]
        public static void WarMode( string onOff = "toggle" )
        {
            if ( Engine.Player == null )
            {
                return;
            }

            string onOffNormalized = onOff.Trim().ToLower();

            if ( onOffNormalized != "toggle" )
            {
                switch ( onOffNormalized )
                {
                    case "on" when Engine.Player.Status.HasFlag( MobileStatus.WarMode ):
                    case "off" when !Engine.Player.Status.HasFlag( MobileStatus.WarMode ):
                        return;
                }
            }

            Engine.SendPacketToServer( Engine.Player.Status.HasFlag( MobileStatus.WarMode )
                ? new WarMode( false )
                : new WarMode( true ) );
        }

        [CommandsDisplay( Category = nameof( Strings.Main ),
            Parameters = new[] { nameof( ParameterType.String ), nameof( ParameterType.String ) } )]
        public static void MessageBox( string title, string body )
        {
            System.Windows.MessageBox.Show( body, title, MessageBoxButton.OK, MessageBoxImage.Information );
        }

        [CommandsDisplay( Category = nameof( Strings.Main ) )]
        public static void PlaySound( object param, bool playSync = true )
        {
            switch ( param )
            {
                case int id:
                    Engine.SendPacketToClient( new PlaySound( id ) );
                    break;
                case string soundFile:
                {
                    string fullPath = Path.Combine( Engine.StartupPath, "Sounds", soundFile );

                    if ( !File.Exists( fullPath ) )
                    {
                        UOC.SystemMessage( Strings.Cannot_find_sound_file___ );
                        return;
                    }

                    SoundPlayer soundPlayer = new SoundPlayer( fullPath );

                    if ( playSync )
                    {
                        soundPlayer.PlaySync();
                    }
                    else
                    {
                        soundPlayer.Play();
                    }

                    break;
                }
            }
        }

        [CommandsDisplay( Category = nameof( Strings.Main ) )]
        public static bool Playing()
        {
            MacroManager manager = MacroManager.GetInstance();

            return manager.CurrentMacro != null && ( manager.CurrentMacro.IsRunning || manager.Replay );
        }

        [CommandsDisplay( Category = nameof( Strings.Main ), Parameters = new[] { nameof( ParameterType.MacroName ) } )]
        public static bool Playing( string macroName )
        {
            MacroManager manager = MacroManager.GetInstance();

            MacroEntry macro = manager.Items.FirstOrDefault( m => m.Name.Equals( macroName ) );

            return macro != null && ( macro.IsRunning || manager.Replay );
        }

        [CommandsDisplay( Category = nameof( Strings.Main ),
            Parameters = new[]
            {
                nameof( ParameterType.XCoordinate ), nameof( ParameterType.YCoordinate ),
                nameof( ParameterType.Boolean )
            } )]
        public static void DisplayQuestPointer( int x, int y, bool enabled = true )
        {
            Engine.SendPacketToClient( new DisplayQuestPointer( enabled, x, y ) );
        }

        [CommandsDisplay( Category = nameof( Strings.Main ),
            Parameters = new[]
            {
                nameof( ParameterType.IntegerValue ), nameof( ParameterType.Boolean ),
                nameof( ParameterType.String )
            } )]
        public static bool Snapshot( int delay = 0, bool fullscreen = false, string fileName = "" )
        {
            try
            {
                if ( delay > 0 )
                {
                    Thread.Sleep( delay );
                }

                IntPtr screenDC;
                int width, height;

                if ( fullscreen )
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
                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap( hBitmap, IntPtr.Zero,
                    Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions() );

                DeleteObject( hBitmap );
                ReleaseDC( IntPtr.Zero, screenDC );
                ReleaseDC( IntPtr.Zero, memDC );

                DateTime now = DateTime.Now;

                if ( string.IsNullOrEmpty( fileName ) )
                {
                    fileName =
                        $"ClassicAssist-{now.Year}-{now.Month}-{now.Day}-{now.Hour}-{now.Minute}-{now.Second}.png";
                }

                string filePath = fileName;

                if ( !Path.IsPathRooted( fileName ) )
                {
                    string path = Path.Combine( Engine.StartupPath, "Screenshots" );

                    if ( !Directory.Exists( path ) )
                    {
                        Directory.CreateDirectory( path );
                    }

                    filePath = Path.Combine( path, fileName );
                }

                using ( FileStream fileStream = new FileStream( filePath, FileMode.Create ) )
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add( BitmapFrame.Create( bitmapSource ) );
                    encoder.Save( fileStream );
                }

                return true;
            }
            catch ( Exception e )
            {
                UOC.SystemMessage( e.Message, (int) UOC.SystemMessageHues.Red );
                return false;
            }
        }
    }
}
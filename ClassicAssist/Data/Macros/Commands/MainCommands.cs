using System.IO;
using System.Media;
using System.Threading;
using System.Windows;
using Assistant;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Misc;
using ClassicAssist.Resources;
using ClassicAssist.UI.ViewModels;
using ClassicAssist.UI.Views;
using ClassicAssist.UO;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class MainCommands
    {
        [CommandsDisplay( Category = "Main",
            Description = "Set quiet mode True/False, True reduces the number of messages macro commands emit.",
            InsertText = "SetQuietMode(True)" )]
        public static void SetQuietMode( bool onOff )
        {
            MacroManager.QuietMode = onOff;
        }

        [CommandsDisplay( Category = "Main", Description = "Use a virtue by name.",
            InsertText = "InvokeVirtue(\"Honor\")" )]
        public static void InvokeVirtue( string virtue )
        {
            Virtues v = Utility.GetEnumValueByName<Virtues>( virtue );

            if ( v == Virtues.None )
            {
                return;
            }

            Engine.SendPacketToServer( new InvokeVirtue( v ) );
        }

        [CommandsDisplay( Category = "Main", Description = "Sends Resync request to server.", InsertText = "Resync()" )]
        public static void Resync()
        {
            UOC.Resync();
        }

        [CommandsDisplay( Category = "Main", Description = "Pauses execution for the given amount in milliseconds.",
            InsertText = "Pause(1000)" )]
        public static void Pause( int milliseconds )
        {
            Thread.Sleep( milliseconds );
        }

        [CommandsDisplay( Category = "Main", Description = "Send a text message.",
            InsertText = "SysMessage(\"hello\")" )]
        public static void SysMessage( string text )
        {
            UOC.SystemMessage( text );
        }

        [CommandsDisplay( Category = "Main",
            Description =
                "Show object inspector for supplied serial / alias, will prompt for target if no parameter given.",
            InsertText = "Info(\"self\")" )]
        public static void Info( object obj = null )
        {
            int serial = 0;

            if ( obj == null )
            {
                serial = UOC.GetTargeSerialAsync( Strings.Target_object___ ).Result;

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

        [CommandsDisplay( Category = "Main", Description = "Enable and disable hotkeys.", InsertText = "Hotkeys()" )]
        public static void Hotkeys()
        {
            HotkeyManager manager = HotkeyManager.GetInstance();

            manager.Enabled = !manager.Enabled;

            UOC.SystemMessage( manager.Enabled ? Strings.Hotkeys_enabled___ : Strings.Hotkeys_disabled___, 0x3F );
        }

        [CommandsDisplay( Category = "Main",
            Description =
                "Sets war mode status, parameter on, off, or toggle, defaults to toggle if no parameter given.",
            InsertText = "WarMode(\"on\")" )]
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

        [CommandsDisplay( Category = "Main", Description = "Show a simple message box with a custom title and body.",
            InsertText = "MessageBox(\"title\", \"message\")" )]
        public static void MessageBox( string title, string body )
        {
            System.Windows.MessageBox.Show( body, title, MessageBoxButton.OK, MessageBoxImage.Information );
        }

        [CommandsDisplay( Category = "Main", Description = "Play sound by id or system .wav file.",
            InsertText = "PlaySound(\"Bike Horn.wav\")" )]
        public static void PlaySound( object param )
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
                    soundPlayer.PlaySync();
                    break;
                }
            }
        }
    }
}
using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Reflection;
using System.Windows;
using System.Windows.Interactivity;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Shared;
using Sentry;

namespace ClassicAssist.UI.Misc
{
    public class LoadOptionsOnWindowLoaded : Behavior<Window>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += OnLoaded;
            AssociatedObject.Closing += OnClosing;
        }

        private static void OnClosing( object sender, CancelEventArgs e )
        {
            Options.Save( Options.CurrentOptions );
            AssistantOptions.Save();
        }

        private static void OnLoaded( object sender, RoutedEventArgs e )
        {
            AssistantOptions.OnWindowLoaded();
            SentrySdk.Init( new SentryOptions
            {
                Dsn = Settings.Default.SentryDsn,
                BeforeSend = SentryBeforeSend,
                AutoSessionTracking = true,
                Release = VersionHelpers.GetProductVersion( Assembly.GetExecutingAssembly() ).ToString()
            } );
        }

        private static SentryEvent SentryBeforeSend( SentryEvent args )
        {
            if ( args.Exception is AggregateException && args.Exception.InnerException is SocketException )
            {
                return null;
            }

            if ( args.Exception?.TargetSite?.Module.Assembly == Engine.ClassicAssembly ||
                 ( args.Exception?.TargetSite?.Module.Assembly.ToString().Contains( "FNA" ) ?? false ) )
            {
                return null;
            }
#if DEBUG
            return null;
#else
            args.User = new User { Id = AssistantOptions.UserId };
            args.SetTag( "SessionId", AssistantOptions.SessionId );
            args.SetExtra( "PlayerName", Engine.Player?.Name ?? "Unknown" );
            args.SetExtra( "PlayerSerial", Engine.Player?.Serial ?? 0 );
            args.SetExtra( "Shard", Engine.CurrentShard?.Name ?? "Unknown" );
            args.SetExtra( "ShardFeatures", Engine.Features.ToString() );
            args.SetExtra( "CharacterListFlags", Engine.CharacterListFlags.ToString() );
            args.SetExtra( "Connected", Engine.Connected );
            args.SetExtra( "ClientVersion",
                Engine.ClientVersion == null ? "Unknown" : Engine.ClientVersion.ToString() );
            args.SetExtra( "ClassicUO Version", Engine.ClassicAssembly?.GetName().Version.ToString() ?? "Unknown" );

            return args;
#endif
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= OnLoaded;
            AssociatedObject.Closing -= OnClosing;
            base.OnDetaching();
        }
    }
}
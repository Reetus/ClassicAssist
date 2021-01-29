using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using Assistant;
using ClassicAssist.Data;
using Sentry;
using Sentry.Protocol;

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
#if !DEBUG
            SentrySdk.Init( new SentryOptions
            {
                Dsn = new Dsn( "https://7a7c44cd07e64058a3b434e1c86e4c02@o369765.ingest.sentry.io/5325425" ),
                BeforeSend = SentryBeforeSend
            } );
#endif
        }

        private static SentryEvent SentryBeforeSend( SentryEvent args )
        {
            args.User = new User { Id = AssistantOptions.UserId };
            args.SetTag( "SessionId", AssistantOptions.SessionId );
            args.SetExtra( "PlayerName", Engine.Player?.Name ?? "Unknown" );
            args.SetExtra( "PlayerSerial", Engine.Player?.Serial ?? 0 );
            args.SetExtra( "Shard", Engine.CurrentShard?.Name ?? "Unknown" );
            args.SetExtra( "ShardFeatures", Engine.Features.ToString() );
            args.SetExtra( "Connected", Engine.Connected );
            args.SetExtra( "ClientVersion",
                Engine.ClientVersion == null ? "Unknown" : Engine.ClientVersion.ToString() );
            args.SetExtra( "KeyboardLayout", InputLanguageManager.Current?.CurrentInputLanguage?.Name ?? "Unknown" );

            return args;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= OnLoaded;
            AssociatedObject.Closing -= OnClosing;
            base.OnDetaching();
        }
    }
}
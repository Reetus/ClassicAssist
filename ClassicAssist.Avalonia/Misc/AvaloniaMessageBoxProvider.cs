using System.Threading.Tasks;
using Assistant;
using ClassicAssist.Misc;
using ClassicAssist.UI.ViewModels;
using SEngine = ClassicAssist.Shared.Engine;

namespace ClassicAssist.Avalonia.Misc
{
    internal class AvaloniaMessageBoxProvider : IMessageBoxProvider
    {
        public Task<MessageBoxResult> Show( string text, string caption = null,
            MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxImage? icon = null )
        {
            TaskCompletionSource<MessageBoxResult> promise = new TaskCompletionSource<MessageBoxResult>();

            SEngine.Dispatcher.Invoke( async () =>
            {
                DialogWindow window = new DialogWindow();
                DialogWindowViewModel model = new DialogWindowViewModel( new SharedWindow
                {
                    Window = window,
                    Type = WindowType.Avalonia,
                    Caption = caption,
                    Text = text,
                    Buttons = buttons,
                    Icon = icon
                } );
                window.DataContext = model;

                await window.ShowDialog( Engine.MainWindow );
                promise.TrySetResult( model.Result );
            } );

            return promise.Task;
        }
    }
}
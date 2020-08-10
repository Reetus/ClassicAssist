using System.Threading.Tasks;
using System.Windows.Forms;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
using MessageBoxButtons = ClassicAssist.Misc.MessageBoxButtons;
using WMessageBox = System.Windows.Forms.MessageBox;
using WMessageBoxButtons = System.Windows.Forms.MessageBoxButtons;

namespace ClassicAssist.UI.Misc
{
    internal class WPFMessageBoxProvider : IMessageBoxProvider
    {
        public Task<MessageBoxResult> Show( string text, string caption = null,
            MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxImage? icon = null )
        {
            if (string.IsNullOrEmpty( caption ))
            {
                caption = Strings.Error;
            }

            DialogResult result = WMessageBox.Show( text, caption,
                buttons == MessageBoxButtons.YesNo ? WMessageBoxButtons.YesNo : WMessageBoxButtons.OK );

            switch (result)
            {
                case DialogResult.OK: return Task.FromResult( MessageBoxResult.OK );
                case DialogResult.Yes: return Task.FromResult( MessageBoxResult.Yes );
                default: return Task.FromResult( MessageBoxResult.No );
            }
        }
    }
}
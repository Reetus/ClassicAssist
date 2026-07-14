using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassicAssist.Data.Hotkeys;
using static ClassicAssist.Misc.SDLKeys;

namespace ClassicAssist.UI.Controls
{
    /// <summary>
    ///     A read-only <see cref="TextBox" /> that captures a key, mouse-button or mouse-wheel shortcut
    ///     and exposes it through the <see cref="Shortcut" /> property.
    /// </summary>
    public class TextBoxKey : TextBox
    {
        public static readonly DependencyProperty ShortcutProperty = DependencyProperty.Register( nameof( Shortcut ),
            typeof( ShortcutKeys ), typeof( TextBoxKey ),
            new FrameworkPropertyMetadata( default( ShortcutKeys ),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnShortcutChanged ) );

        private readonly Key[] _modifiers =
        {
            Key.LeftCtrl, Key.RightCtrl, Key.LeftShift, Key.RightShift, Key.LeftAlt, Key.RightAlt
        };

        public TextBoxKey()
        {
            IsReadOnly = true;
            IsReadOnlyCaretVisible = false;
            IsUndoEnabled = false;

            // WPF implicit styles only match the exact type, so a TextBox subclass does not inherit
            // the theme's implicit { x:Type TextBox } style. Resolve it explicitly so we keep the
            // dark theme wherever DarkTheme.xaml is in scope.
            SetResourceReference( StyleProperty, typeof( TextBox ) );
        }

        public Key Key { get; private set; }
        public ModKey Modifier { get; set; }

        public ShortcutKeys Shortcut
        {
            get => (ShortcutKeys) GetValue( ShortcutProperty );
            set => SetValue( ShortcutProperty, value );
        }

        private static void OnShortcutChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            if ( d is TextBoxKey textBoxKey )
            {
                textBoxKey.Text = e.NewValue?.ToString() ?? string.Empty;
            }
        }

        private ModKey CheckModifiers()
        {
            return KeymodFromKeyList( _modifiers.Where( Keyboard.IsKeyDown ) );
        }

        private bool IsModifier( Key key )
        {
            return _modifiers.Any( k => key == k );
        }

        protected override void OnPreviewKeyDown( KeyEventArgs e )
        {
            e.Handled = true;
            Key key = e.Key;

            if ( key == Key.System )
            {
                key = e.SystemKey;
            }

            if ( IsModifier( key ) )
            {
                return;
            }

            Modifier = CheckModifiers();

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch ( key )
            {
                case Key.DeadCharProcessed:
                    key = e.DeadCharProcessedKey;
                    break;
                case Key.ImeProcessed:
                    key = e.ImeProcessedKey;
                    break;
                case Key.System:
                    key = e.SystemKey;
                    break;
            }

            Key = key;

            Shortcut = new ShortcutKeys { Modifier = Modifier, Key = Key };
        }

        protected override void OnPreviewMouseDown( MouseButtonEventArgs e )
        {
            if ( e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Right )
            {
                return;
            }

            e.Handled = true;

            Modifier = CheckModifiers();

            Shortcut = new ShortcutKeys { Modifier = Modifier, Mouse = (MouseOptions) e.ChangedButton };
        }

        protected override void OnMouseWheel( MouseWheelEventArgs e )
        {
            e.Handled = true;

            Modifier = CheckModifiers();

            Shortcut = e.Delta < 0
                ? new ShortcutKeys { Mouse = MouseOptions.MouseWheelDown, Modifier = Modifier }
                : new ShortcutKeys { Mouse = MouseOptions.MouseWheelUp, Modifier = Modifier };
        }
    }
}

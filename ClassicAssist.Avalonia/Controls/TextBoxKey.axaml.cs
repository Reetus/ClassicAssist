using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ClassicAssist.Data.Hotkeys;
using CKey = ClassicAssist.Misc.Key;

namespace ClassicAssist.Avalonia.Controls
{
    public class TextBoxKey : UserControl
    {
        public static DirectProperty<TextBoxKey, ShortcutKeys> ShortcutProperty =
            AvaloniaProperty.RegisterDirect<TextBoxKey, ShortcutKeys>( nameof( Shortcut ), o => o.Shortcut,
                ( o, v ) => o.Shortcut = v, defaultBindingMode: BindingMode.TwoWay );

        private readonly CKey[] _modifiers =
        {
            CKey.LeftCtrl, CKey.RightCtrl, CKey.LeftShift, CKey.RightShift, CKey.LeftAlt, CKey.RightAlt
        };

        private ShortcutKeys _shortcut;

        public TextBoxKey()
        {
            InitializeComponent();
        }

        public bool Enabled => Shortcut != null;

        public ShortcutKeys Shortcut
        {
            get => _shortcut;
            set => SetAndRaise( ShortcutProperty, ref _shortcut, value );
        }

        private bool IsModifier( CKey key )
        {
            return _modifiers.Any( k => key == k );
        }

        private static CKey CheckModifiers( KeyModifiers modifiers )
        {
            switch ( modifiers )
            {
                case KeyModifiers.None:
                    break;
                case KeyModifiers.Alt:
                    return CKey.LeftAlt;
                case KeyModifiers.Control:
                    return CKey.LeftCtrl;
                case KeyModifiers.Shift:
                    return CKey.LeftShift;
                case KeyModifiers.Meta:
                    break;
                default:
                    throw new ArgumentOutOfRangeException( nameof( modifiers ), modifiers, null );
            }

            return CKey.None;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load( this );
        }

        protected override void OnKeyDown( KeyEventArgs e )
        {
            if ( Shortcut == null )
            {
                return;
            }

            e.Handled = true;

            CKey key = (CKey) e.Key;

            if ( IsModifier( key ) )
            {
                e.Handled = true;

                return;
            }

            CKey modifier = CheckModifiers( e.KeyModifiers );

            Shortcut = new ShortcutKeys( modifier, key );
        }

        protected override void OnPointerWheelChanged( PointerWheelEventArgs e )
        {
            if ( Shortcut == null )
            {
                return;
            }

            e.Handled = true;

            CKey modifier = CheckModifiers( e.KeyModifiers );

            Shortcut = e.Delta.Y < 0
                ? new ShortcutKeys { Mouse = MouseOptions.MouseWheelDown, Modifier = modifier }
                : new ShortcutKeys { Mouse = MouseOptions.MouseWheelUp, Modifier = modifier };
        }

        protected override void OnPointerPressed( PointerPressedEventArgs e )
        {
            if ( Shortcut == null )
            {
                return;
            }

            PointerPointProperties properties = e.GetCurrentPoint( this ).Properties;

            if ( properties.IsLeftButtonPressed | properties.IsRightButtonPressed )
            {
                return;
            }

            e.Handled = true;

            MouseOptions button = MouseOptions.None;

            switch ( properties.PointerUpdateKind )
            {
                case PointerUpdateKind.LeftButtonPressed:
                    break;
                case PointerUpdateKind.MiddleButtonPressed:
                    button = MouseOptions.MiddleButton;
                    break;
                case PointerUpdateKind.RightButtonPressed:
                    break;
                case PointerUpdateKind.XButton1Pressed:
                    button = MouseOptions.XButton1;
                    break;
                case PointerUpdateKind.XButton2Pressed:
                    button = MouseOptions.XButton2;
                    break;
                case PointerUpdateKind.LeftButtonReleased:
                case PointerUpdateKind.MiddleButtonReleased:
                case PointerUpdateKind.RightButtonReleased:
                case PointerUpdateKind.XButton1Released:
                case PointerUpdateKind.XButton2Released:
                case PointerUpdateKind.Other:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Shortcut = new ShortcutKeys { Mouse = button };
        }
    }
}
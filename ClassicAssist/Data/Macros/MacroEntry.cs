using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Shared.Resources;
using DraggableTreeView;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Newtonsoft.Json;

namespace ClassicAssist.Data.Macros
{
    public class MacroEntry : HotkeyEntry, IComparable<MacroEntry>, IDraggableEntry, IComparable
    {
        private readonly Dispatcher _dispatcher;
        private Dictionary<string, int> _aliases = new Dictionary<string, int>();
        private bool _doNotAutoInterrupt;
        private bool _global;
        private string _group;
        private bool _isAutostart;
        private bool _isBackground;
        private bool _isRunning;
        private bool _loop;
        private string _macro = string.Empty;
        private MacroInvoker _macroInvoker = new MacroInvoker();
        private string _name;

        public MacroEntry()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _macroInvoker.ExceptionEvent += OnExceptionEvent;
            _macroInvoker.StoppedEvent += OnStoppedEvent;
        }

        public Dictionary<string, int> Aliases
        {
            get => _aliases;
            set => SetProperty( ref _aliases, value );
        }

        public bool DoNotAutoInterrupt
        {
            get => _doNotAutoInterrupt;
            set => SetProperty( ref _doNotAutoInterrupt, value );
        }

        public bool Global
        {
            get => _global;
            set => SetProperty( ref _global, value );
        }

        public string Group
        {
            get => _group;
            set => SetProperty( ref _group, value );
        }

        public bool IsAutostart
        {
            get => _isAutostart;
            set => SetProperty( ref _isAutostart, value );
        }

        public bool IsBackground
        {
            get => _isBackground;
            set => SetProperty( ref _isBackground, value );
        }

        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty( ref _isRunning, value );
        }

        public bool Loop
        {
            get => _loop;
            set => SetProperty( ref _loop, value );
        }

        public string Macro
        {
            get => _macro;
            set => SetProperty( ref _macro, value );
        }

        [JsonIgnore]
        public MacroInvoker MacroInvoker
        {
            get => _macroInvoker;
            set => SetProperty( ref _macroInvoker, value );
        }

        public DateTime StartedOn { get; set; }

        public int CompareTo( object obj )
        {
            if ( !( obj is IDraggable entry ) )
            {
                return 1;
            }

            if ( obj.GetType() != GetType() )
            {
                return 1;
            }

            if ( ReferenceEquals( this, entry ) )
            {
                return 0;
            }

            if ( ReferenceEquals( null, entry ) )
            {
                return 1;
            }

            return string.Compare( Name, entry.Name, StringComparison.Ordinal );
        }

        public int CompareTo( MacroEntry other )
        {
            if ( ReferenceEquals( this, other ) )
            {
                return 0;
            }

            if ( ReferenceEquals( null, other ) )
            {
                return 1;
            }

            int hotkeyEntryComparison = base.CompareTo( other );

            if ( hotkeyEntryComparison != 0 )
            {
                return hotkeyEntryComparison;
            }

            return string.Compare( _name, other._name, StringComparison.Ordinal );
        }

        public override string Name
        {
            get => _name;
            set => SetName( _name, value );
        }

        private void OnStoppedEvent()
        {
            _dispatcher.Invoke( () => IsRunning = false );

            if ( IsBackground )
            {
                UO.Commands.SystemMessage( string.Format( Strings.Background_macro___0___stopped___, Name ), true );
            }
        }

        public override string ToString()
        {
            return Name;
        }

        private void SetName( string name, string value )
        {
            MacroManager manager = MacroManager.GetInstance();

            bool exists = manager.Items.Any( m => m.Name == value && !ReferenceEquals( m, this ) );

            if ( exists && name == null )
            {
                SetName( null, $"{value}-" );
                return;
            }

            if ( exists )
            {
                MessageBox.Show( Strings.Macro_name_must_be_unique_, Strings.Error );
                return;
            }

            SetProperty( ref _name, value );
        }

        public void Execute()
        {
            _dispatcher.Invoke( () => IsRunning = true );

            if ( IsBackground )
            {
                UO.Commands.SystemMessage( string.Format( Strings.Background_macro___0___started___, Name ), true );
            }

            StartedOn = DateTime.Now;
            _macroInvoker.Execute( this );
        }

        public void Stop()
        {
            if ( IsRunning )
            {
                _macroInvoker.Stop();
                _dispatcher.Invoke( () => IsRunning = false );
            }
        }

        private static void OnExceptionEvent( Exception exception )
        {
            UO.Commands.SystemMessage( string.Format( Strings.Macro_error___0_, exception.Message ) );

            if ( exception is SyntaxErrorException syntaxError )
            {
                UO.Commands.SystemMessage( $"{Strings.Line_Number}: {syntaxError.RawSpan.Start.Line}" );
            }
            else
            {
                DynamicStackFrame sf = PythonOps.GetDynamicStackFrames( exception ).FirstOrDefault();

                if ( sf != null )
                {
                    string fileName = sf.GetFileName();

                    if ( fileName != "<string>" )
                    {
                        UO.Commands.SystemMessage( $"{Strings.Module}: {fileName}" );
                    }

                    UO.Commands.SystemMessage( $"{Strings.Line_Number}: {sf.GetFileLineNumber()}" );
                }
            }
        }
    }
}
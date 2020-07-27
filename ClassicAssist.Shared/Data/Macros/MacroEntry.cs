﻿using System;
using System.Collections.Generic;
using System.Linq;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Shared;
using ClassicAssist.Shared.Resources;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Newtonsoft.Json;

namespace ClassicAssist.Data.Macros
{
    public class MacroEntry : HotkeyEntry, IComparable<MacroEntry>
    {
        //private readonly Dispatcher _dispatcher;
        private Dictionary<string, int> _aliases = new Dictionary<string, int>();
        private bool _doNotAutoInterrupt;
        private bool _global;
        private bool _isAutostart;
        private bool _isBackground;
        private bool _isRunning;
        private bool _loop;
        private string _macro = string.Empty;
        private MacroInvoker _macroInvoker = new MacroInvoker();
        private string _name;

        public MacroEntry()
        {
            //TODO
            //_dispatcher = Dispatcher.CurrentDispatcher;
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

        public override string Name
        {
            get => _name;
            set => SetName( _name, value );
        }

        public int CompareTo( MacroEntry other )
        {
            return string.Compare( Name, other.Name, StringComparison.OrdinalIgnoreCase );
        }

        private void OnStoppedEvent()
        {
            Engine.Dispatcher.Invoke( () => IsRunning = false );

            if ( IsBackground && !MacroManager.QuietMode )
            {
                Shared.UO.Commands.SystemMessage( string.Format( Strings.Background_macro___0___stopped___, Name ) );
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
                //TODO
                //MessageBox.Show( Strings.Macro_name_must_be_unique_, Strings.Error );
                return;
            }

            SetProperty( ref _name, value );
        }

        public void Execute()
        {
            Engine.Dispatcher.Invoke( () => IsRunning = true );

            if ( IsBackground && !MacroManager.QuietMode )
            {
                Shared.UO.Commands.SystemMessage( string.Format( Strings.Background_macro___0___started___, Name ) );
            }

            _macroInvoker.Execute( this );
        }

        public void Stop()
        {
            if ( IsRunning )
            {
                _macroInvoker.Stop();
                Engine.Dispatcher.Invoke( () => IsRunning = false );
            }
        }

        private static void OnExceptionEvent( Exception exception )
        {
            Shared.UO.Commands.SystemMessage( string.Format( Strings.Macro_error___0_, exception.Message ) );

            if ( exception is SyntaxErrorException syntaxError )
            {
                Shared.UO.Commands.SystemMessage( $"{Strings.Line_Number}: {syntaxError.RawSpan.Start.Line}" );
            }
            else
            {
                DynamicStackFrame sf = PythonOps.GetDynamicStackFrames( exception ).FirstOrDefault();

                if ( sf != null )
                {
                    Shared.UO.Commands.SystemMessage( $"{Strings.Line_Number}: {sf.GetFileLineNumber()}" );
                }
            }
        }
    }
}
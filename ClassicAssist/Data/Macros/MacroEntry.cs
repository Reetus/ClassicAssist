#region License

// Copyright (C) 2025 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using ClassicAssist.Controls.DraggableTreeView;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UO.Data;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Data.Macros
{
    public sealed class MacroEntry : HotkeyEntry, IComparable<MacroEntry>, IDraggableEntry, IComparable
    {
        private readonly Dispatcher _dispatcher;
        private Dictionary<string, int> _aliases = new Dictionary<string, int>();
        private AutoResetEvent _autoResetEvent;
        private ObservableCollection<int> _breakpoints = new ObservableCollection<int>();
        private bool _doNotAutoInterrupt;
        private string _filePath;
        private Dictionary<string, object> _frameVariables;
        private bool _global;
        private string _group;
        private string _id;
        private bool _isAutostart;
        private bool _isBackground;
        private bool _isPaused;
        private bool _isRunning;
        private bool _isSelected;
        private Exception _lastException;
        private bool _loop;
        private string _macro = string.Empty;
        private MacroInvoker _macroInvoker = new MacroInvoker();
        private Dictionary<string, string> _metadata = new Dictionary<string, string>();
        private string _name;
        private int _pausedLinedNumber;
        private Version _languageVersion;

        public MacroEntry( JToken token = null )
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _macroInvoker.ExceptionEvent += OnExceptionEvent;
            _macroInvoker.StartedEvent += OnStartedEvent;
            _macroInvoker.StoppedEvent += OnStoppedEvent;
            _macroInvoker.PausedEvent += OnPausedEvent;

            if ( token == null )
            {
                return;
            }

            Id = GetJsonValue<string>( token, "Id", null );
            Name = GetJsonValue( token, "Name", string.Empty );
            Loop = GetJsonValue( token, "Loop", false );
            DoNotAutoInterrupt = GetJsonValue( token, "DoNotAutoInterrupt", false );
            FilePath = GetJsonValue<string>( token, "FilePath", null );

            string embeddedMacro = GetJsonValue( token, "Macro", string.Empty );

            if ( IsFileBacked && !string.IsNullOrEmpty( embeddedMacro ) )
            {
                // The last save couldn't write the backing file and embedded the newer content in
                // the profile instead - prefer it and re-attempt the file write on the next save.
                Macro = embeddedMacro;
                BackingFileWritePending = true;
            }
            else if ( IsFileBacked && File.Exists( FilePath ) )
            {
                try
                {
                    Macro = File.ReadAllText( FilePath );
                }
                catch
                {
                    // Unreadable backing file - load the entry without content rather than failing
                    // the whole profile; the folder scan will reload it once readable.
                    Macro = string.Empty;
                    BackingFileReadFailed = true;
                }
            }
            else
            {
                Macro = embeddedMacro;
            }

            PassToUO = GetJsonValue( token, "PassToUO", true );
            IsBackground = GetJsonValue( token, "IsBackground", false );
            IsAutostart = GetJsonValue( token, "IsAutostart", false );
            Disableable = GetJsonValue( token, "Disableable", true );
            Group = GetJsonValue( token, "Group", string.Empty );
            Global = GetJsonValue( token, "Global", false );
            Breakpoints = GetJsonValue( token, "Breakpoints", new ObservableCollection<int>() );

            /* Keys aren't done here, because of logic global vs normal */

            if ( token["Metadata"] != null )
            {
                foreach ( JToken jToken in token["Metadata"] )
                {
                    string key = jToken["Key"]?.ToObject<string>() ?? string.Empty;
                    string value = jToken["Value"]?.ToObject<string>() ?? string.Empty;

                    if ( !string.IsNullOrEmpty( key ) )
                    {
                        Metadata.Add( key, value );
                    }
                }
            }

            if ( token["Aliases"] == null )
            {
                return;
            }

            foreach ( JToken aliasToken in token["Aliases"] )
            {
                if ( aliasToken.Type == JTokenType.Property )
                {
                    JProperty jProperty = (JProperty) aliasToken;

                    Aliases.Add( jProperty.Name, jProperty.Value.ToObject<int>() );
                }
                else
                {
                    string key = aliasToken["Key"]?.ToObject<string>() ?? string.Empty;
                    int value = aliasToken["Value"]?.ToObject<int>() ?? 0;

                    if ( string.IsNullOrEmpty( key ) )
                    {
                        continue;
                    }

                    Aliases.Add( key, value );
                }
            }
        }

        public Dictionary<string, int> Aliases
        {
            get => _aliases;
            set => SetProperty( ref _aliases, value );
        }

        public AutoResetEvent AutoResetEvent
        {
            get => _autoResetEvent;
            set => SetProperty( ref _autoResetEvent, value );
        }

        public ObservableCollection<int> Breakpoints
        {
            get => _breakpoints;
            set => SetProperty( ref _breakpoints, value );
        }

        public bool DoNotAutoInterrupt
        {
            get => _doNotAutoInterrupt;
            set => SetProperty( ref _doNotAutoInterrupt, value );
        }

        public string FilePath
        {
            get => _filePath;
            set
            {
                SetProperty( ref _filePath, value );
                OnPropertyChanged( nameof( IsFileBacked ) );
            }
        }

        public bool IsFileBacked => !string.IsNullOrEmpty( FilePath );

        /// <summary>
        ///     The backing file couldn't be read when the profile loaded; saves must not write
        ///     (empty) content over it until the folder scan has reloaded it.
        /// </summary>
        public bool BackingFileReadFailed { get; set; }

        /// <summary>
        ///     The in-memory content is newer than the backing file (the last file write failed and
        ///     the content was embedded in the profile instead); the folder scan must not reload
        ///     over it and the next save should retry the file write.
        /// </summary>
        public bool BackingFileWritePending { get; set; }

        public Dictionary<string, object> FrameVariables
        {
            get => _frameVariables;
            set => SetProperty( ref _frameVariables, value );
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

        public string Hash => _macro.SHA1();

        public string Id
        {
            get => _id ?? ( _id = Guid.NewGuid().ToString() );
            set => SetProperty( ref _id, value );
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

        public bool IsPaused
        {
            get => _isPaused;
            set => SetProperty( ref _isPaused, value );
        }

        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty( ref _isRunning, value );
        }

        public Exception LastException
        {
            get => _lastException;
            set => SetProperty( ref _lastException, value );
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

        public Dictionary<string, string> Metadata
        {
            get => _metadata;
            set => SetProperty( ref _metadata, value );
        }

        public int PausedLinedNumber
        {
            get => _pausedLinedNumber;
            set => SetProperty( ref _pausedLinedNumber, value );
        }

        public DateTime StartedOn { get; set; }

        public int CompareTo( object obj )
        {
            if ( !( obj is MacroEntry entry ) )
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

            return string.Compare( _id, entry._id, StringComparison.Ordinal );
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

            return string.Compare( _id, other._id, StringComparison.Ordinal );
        }

        public override string Name
        {
            get => _name;
            set => SetName( _name, value );
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty( ref _isSelected, value );
        }

        public Version LanguageVersion
        {
            get => _languageVersion;
            set => SetProperty( ref _languageVersion, value );
        }

        private void OnPausedEvent( int lineNumber, AutoResetEvent autoResetEvent, Dictionary<string, object> frameVariables )
        {
            UO.Commands.SystemMessage( string.Format( Strings.Debugger_paused_on_line__0_, lineNumber ), SystemMessageHues.Yellow );

            _dispatcher.Invoke( () =>
            {
                IsPaused = true;
                PausedLinedNumber = lineNumber;
                AutoResetEvent = autoResetEvent;
                FrameVariables = frameVariables;
            } );
        }

        private static T2 GetJsonValue<T2>( JToken json, string name, T2 defaultValue )
        {
            if ( json == null )
            {
                return defaultValue;
            }

            return json[name] == null ? defaultValue : json[name].ToObject<T2>();
        }

        private void OnStartedEvent()
        {
            // Fires on the run's own thread. Mark running only while this is still the invoker's
            // current run, so a superseded run can't flip state for the one that replaced it. This
            // also re-asserts the running state after a restart, in case the previous run's stop
            // notification lands after the new run started.
            Thread startedThread = Thread.CurrentThread;

            _dispatcher.BeginInvoke( (Action) ( () =>
            {
                if ( ReferenceEquals( _macroInvoker.Thread, startedThread ) )
                {
                    IsRunning = true;
                }
            } ) );
        }

        private void OnStoppedEvent()
        {
            bool wasPaused = IsPaused;

            if ( wasPaused )
            {
                AutoResetEvent.Set();
            }

            // Captured on the macro thread that is stopping. If a restart has already begun, the
            // invoker's Thread will point at the new run, so this (superseded) stop must not clear
            // IsRunning - otherwise the macro keeps running but shows as stopped.
            Thread stoppingThread = Thread.CurrentThread;

            _dispatcher.BeginInvoke( (Action) ( () =>
            {
                if ( !ReferenceEquals( _macroInvoker.Thread, stoppingThread ) )
                {
                    return;
                }

                IsRunning = false;

                if ( wasPaused )
                {
                    IsPaused = false;
                }
            } ) );

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

        public void Execute( object[] parameters = null )
        {
            _dispatcher.Invoke( () =>
            {
                LastException = null;
                IsRunning = true;
            } );

            if ( IsBackground )
            {
                UO.Commands.SystemMessage( string.Format( Strings.Background_macro___0___started___, Name ), true );
            }

            StartedOn = DateTime.Now;
            _macroInvoker.Execute( this, parameters );
        }

        public void Stop()
        {
            if ( IsRunning )
            {
                Thread stoppingThread = _macroInvoker.Thread;

                _macroInvoker.Stop();

                _dispatcher.BeginInvoke( (Action) ( () =>
                {
                    // Don't clear running state if a restart has already swapped in a new run.
                    if ( ReferenceEquals( _macroInvoker.Thread, stoppingThread ) )
                    {
                        IsRunning = false;
                    }
                } ) );
            }
        }

        private void OnExceptionEvent( Exception exception )
        {
            _dispatcher.BeginInvoke( (Action) ( () => { LastException = exception; } ) );

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

        public JObject ToJObject()
        {
            JObject entry = new JObject
            {
                { "Id", Id },
                { "Name", Name },
                { "Loop", Loop },
                { "DoNotAutoInterrupt", DoNotAutoInterrupt },
                // File-backed macros keep their content in the .py file, not the profile, unless
                // the backing file couldn't be written - then embed the content so it isn't lost.
                { "Macro", IsFileBacked && !BackingFileWritePending ? string.Empty : Macro },
                { "PassToUO", PassToUO },
                { "Keys", Hotkey.ToJObject() },
                { "IsBackground", IsBackground },
                { "IsAutostart", IsAutostart },
                { "Disableable", Disableable },
                { "Group", Group },
                { "Global", Global },
                { "LastSavedHash", Hash }
            };

            if ( IsFileBacked )
            {
                entry.Add( "FilePath", FilePath );
            }

            if ( Metadata?.Count > 0 )
            {
                JArray metadataArray = new JArray();

                foreach ( JObject metadata in from keyValuePair in Metadata select new JObject { { "Key", keyValuePair.Key }, { "Value", keyValuePair.Value } } )
                {
                    metadataArray.Add( metadata );
                }

                entry.Add( "Metadata", metadataArray );
            }

            if ( !Global )
            {
                JArray aliasesArray = new JArray();

                foreach ( JObject aliasObj in Aliases.Select( kvp => new JObject { { "Key", kvp.Key }, { "Value", kvp.Value } } ) )
                {
                    aliasesArray.Add( aliasObj );
                }

                entry.Add( "Aliases", aliasesArray );
            }
            else
            {
                /*
                 * Write global macro aliases as properties for backwards compatibility (for now)
                 */
                JObject aliases = new JObject();

                foreach ( KeyValuePair<string, int> keyValuePair in Aliases )
                {
                    aliases.Add( keyValuePair.Key, keyValuePair.Value );
                }

                entry.Add( "Aliases", aliases );
            }

            if ( Breakpoints != null )
            {
                JArray breakpointsArray = new JArray();

                foreach ( int breakpoint in Breakpoints )
                {
                    breakpointsArray.Add( breakpoint );
                }

                entry.Add( "Breakpoints", breakpointsArray );
            }

            return entry;
        }

        public void Resume()
        {
            AutoResetEvent.Set();

            _dispatcher.Invoke( () => IsPaused = false );
        }

        public void Step()
        {
            AutoResetEvent.Set();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using ClassicAssist.Controls.DraggableTreeView;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
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
        private bool _doNotAutoInterrupt;
        private bool _global;
        private string _group;
        private string _id;
        private bool _isAutostart;
        private bool _isBackground;
        private bool _isRunning;
        private string _lastSavedHash;
        private DateTime _lastSavedOn;
        private bool _loop;
        private string _macro = string.Empty;
        private MacroInvoker _macroInvoker = new MacroInvoker();
        private string _name;

        public MacroEntry( JToken token = null )
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _macroInvoker.ExceptionEvent += OnExceptionEvent;
            _macroInvoker.StoppedEvent += OnStoppedEvent;

            if ( token == null )
            {
                return;
            }

            Id = GetJsonValue<string>( token, "Id", null );
            Name = GetJsonValue( token, "Name", string.Empty );
            Loop = GetJsonValue( token, "Loop", false );
            DoNotAutoInterrupt = GetJsonValue( token, "DoNotAutoInterrupt", false );
            Macro = GetJsonValue( token, "Macro", string.Empty );
            PassToUO = GetJsonValue( token, "PassToUO", true );
            IsBackground = GetJsonValue( token, "IsBackground", false );
            IsAutostart = GetJsonValue( token, "IsAutostart", false );
            Disableable = GetJsonValue( token, "Disableable", true );
            Group = GetJsonValue( token, "Group", string.Empty );
            Global = GetJsonValue( token, "Global", false );
            LastSavedOn = GetJsonValue( token, "LastSavedOn", DateTime.Now );
            LastSavedHash = GetJsonValue( token, "LastSavedHash", string.Empty );

            /* Keys aren't done here, because of logic global vs normal */

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

        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty( ref _isRunning, value );
        }

        public string LastSavedHash
        {
            get => _lastSavedHash;
            set => SetProperty( ref _lastSavedHash, value );
        }

        public DateTime LastSavedOn
        {
            get => _lastSavedOn;
            set => SetProperty( ref _lastSavedOn, value );
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

        private static T2 GetJsonValue<T2>( JToken json, string name, T2 defaultValue )
        {
            if ( json == null )
            {
                return defaultValue;
            }

            return json[name] == null ? defaultValue : json[name].ToObject<T2>();
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

        public JObject ToJObject()
        {
            JObject entry = new JObject
            {
                { "Id", Id },
                { "Name", Name },
                { "Loop", Loop },
                { "DoNotAutoInterrupt", DoNotAutoInterrupt },
                { "Macro", Macro },
                { "PassToUO", PassToUO },
                { "Keys", Hotkey.ToJObject() },
                { "IsBackground", IsBackground },
                { "IsAutostart", IsAutostart },
                { "Disableable", Disableable },
                { "Group", Group },
                { "Global", Global },
                { "LastSavedOn", DateTime.Now },
                { "LastSavedHash", Hash }
            };

            if ( !Global )
            {
                JArray aliasesArray = new JArray();

                foreach ( JObject aliasObj in Aliases.Select( kvp =>
                    new JObject { { "Key", kvp.Key }, { "Value", kvp.Value } } ) )
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

            return entry;
        }
    }
}
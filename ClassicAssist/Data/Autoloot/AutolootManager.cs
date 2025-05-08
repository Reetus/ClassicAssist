#region License

// Copyright (C) 2020 Reetus
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
using System.Diagnostics;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Autoloot
{
    public class AutolootManager
    {
        private static AutolootManager _instance;
        private static readonly object _lock = new object();
        private ScriptEngine _engine;
        private ScriptScope _scope;
        public Action<int, bool> CheckContainer { get; set; }

        public Func<List<AutolootEntry>> GetEntries { get; set; } = () => new List<AutolootEntry>();
        public Func<string> GetPythonFunctionText { get; set; }

        public Func<bool> IsEnabled { get; set; }
        public Func<bool> IsRunning { get; set; }
        public Func<bool> MatchTextValue { get; set; }
        public Action<bool> SetEnabled { get; set; }
        public Action<string> SetPythonFunctionText { get; set; }

        public static AutolootManager GetInstance()
        {
            // ReSharper disable once InvertIf
            if ( _instance == null )
            {
                lock ( _lock )
                {
                    if ( _instance != null )
                    {
                        return _instance;
                    }

                    _instance = new AutolootManager();
                    return _instance;
                }
            }

            return _instance;
        }

        public bool ExecuteFunction( string function, Item item )
        {
#if DEBUG
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
#endif

            if ( _engine == null )
            {
                InitializeEngine();
            }

            dynamic func = _scope.GetVariable( function );

            if ( func == null )
            {
                UOC.SystemMessage( string.Format( Strings.Predicate_function___0___not_found___, function ), (int) SystemMessageHues.Red, throttleRepeating: true );

                return true;
            }

            try
            {
                dynamic result = _engine?.Operations.Invoke( func, item );

                return !( result is bool res ) || res;
            }
            catch ( Exception exception )
            {
                UOC.SystemMessage( string.Format( Strings.Predicate_function___0___error___1_, function, exception.Message ), (int) SystemMessageHues.Red,
                    throttleRepeating: true );

                if ( exception is SyntaxErrorException syntaxError )
                {
                    UOC.SystemMessage( $"{Strings.Line_Number}: {syntaxError.RawSpan.Start.Line}", (int) SystemMessageHues.Red, throttleRepeating: true );
                }
            }
            finally
            {
#if DEBUG
                stopWatch.Stop();
                UOC.SystemMessage( $"Predicate Time = {stopWatch.Elapsed}" );
#endif
            }

            return true;
        }

        public void InitializeEngine()
        {
            _engine = Python.CreateEngine();
            _scope = _engine.CreateScope();

            try
            {
                ScriptSource source = _engine.CreateScriptSourceFromString( GetPythonFunctionText() );
                source.Execute( _scope );
            }
            catch ( Exception exception )
            {
                UOC.SystemMessage( exception.Message );                
            }
        }
    }
}
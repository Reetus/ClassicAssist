using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ClassicAssist.Misc;
using ClassicAssist.Resources;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class TimerCommands
    {
        private static readonly ConcurrentDictionary<string, OffsetStopwatch> _timers = new ConcurrentDictionary<string, OffsetStopwatch>();

        [CommandsDisplay( Category = nameof( Strings.Timers ),
            Parameters = new[] { nameof( ParameterType.TimerName ) } )]
        public static void CreateTimer( string name )
        {
            OffsetStopwatch timer = new OffsetStopwatch( TimeSpan.Zero );

            _timers.AddOrUpdate( name, t => timer, (t,o) => timer );
        }

        [CommandsDisplay( Category = nameof( Strings.Timers ),
            Parameters = new[] { nameof( ParameterType.TimerName ) } )]
        public static bool TimerExists( string name )
        {
            return _timers.ContainsKey( name );
        }

        [CommandsDisplay( Category = nameof( Strings.Timers ),
            Parameters = new[] { nameof( ParameterType.TimerName ) } )]
        public static void RemoveTimer( string name )
        {
            if ( _timers.TryGetValue( name, out OffsetStopwatch sw ) )
            {
                sw?.Stop();
            }

            _timers.TryRemove( name, out _ );
        }

        [CommandsDisplay( Category = nameof( Strings.Timers ),
            Parameters = new[] { nameof( ParameterType.SerialOrAlias ), nameof( ParameterType.IntegerValue ) } )]
        public static void SetTimer( string name, int milliseconds = 0 )
        {
            if ( !TimerExists( name ) )
            {
                CreateTimer( name );
            }

            if ( _timers.TryGetValue( name, out OffsetStopwatch sw ) )
            {
                sw?.Reset( TimeSpan.FromMilliseconds( milliseconds ) );
            }
        }

        [CommandsDisplay( Category = nameof( Strings.Timers ),
            Parameters = new[] { nameof( ParameterType.TimerName ) } )]
        public static long Timer( string name )
        {
            return !_timers.ContainsKey( name ) ? 0 : _timers[name].ElapsedMilliseconds;
        }

        [CommandsDisplay( Category = nameof( Strings.Timers ),
            Parameters = new[] { nameof( ParameterType.TimerName ) } )]
        public static void TimerMsg( string name )
        {
            if ( !_timers.ContainsKey( name ) )
            {
                UOC.SystemMessage( Strings.Unknown_timer___ );
                return;
            }

            UOC.SystemMessage( _timers[name].ToString() );
        }

        internal static Dictionary<string, OffsetStopwatch> GetAllTimers()
        {
            return _timers.ToDictionary( t => t.Key, t => t.Value );
        }
    }
}
using System;
using System.Collections.Generic;
using ClassicAssist.Misc;
using ClassicAssist.Resources;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class TimerCommands
    {
        private static readonly Dictionary<string, OffsetStopwatch> _timers = new Dictionary<string, OffsetStopwatch>();

        [CommandsDisplay( Category = nameof( Strings.Timers ) )]
        public static void CreateTimer( string name )
        {
            if ( _timers.ContainsKey( name ) )
            {
                RemoveTimer( name );
            }

            OffsetStopwatch timer = new OffsetStopwatch( TimeSpan.Zero );
            timer.Start();

            _timers.Add( name, timer );
        }

        [CommandsDisplay( Category = nameof( Strings.Timers ) )]
        public static bool TimerExists( string name )
        {
            return _timers.ContainsKey( name );
        }

        [CommandsDisplay( Category = nameof( Strings.Timers ) )]
        public static void RemoveTimer( string name )
        {
            if ( _timers.TryGetValue( name, out OffsetStopwatch sw ) )
            {
                sw?.Stop();
            }

            _timers.Remove( name );
        }

        [CommandsDisplay( Category = nameof( Strings.Timers ) )]
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

        [CommandsDisplay( Category = nameof( Strings.Timers ) )]
        public static long Timer( string name )
        {
            return !_timers.ContainsKey( name ) ? 0 : _timers[name].ElapsedMilliseconds;
        }

        [CommandsDisplay( Category = nameof( Strings.Timers ) )]
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
            return _timers;
        }
    }
}
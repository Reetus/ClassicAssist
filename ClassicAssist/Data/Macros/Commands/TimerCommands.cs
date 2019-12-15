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

        [CommandsDisplay( Category = "Timers", Description = "Create a new named timer.",
            InsertText = "CreateTimer(\"shmoo\")" )]
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

        [CommandsDisplay( Category = "Timers", Description = "Returns true if the timer exists.",
            InsertText = "if TimerExists(\"shmoo\"):" )]
        public static bool TimerExists( string name )
        {
            return _timers.ContainsKey( name );
        }

        [CommandsDisplay( Category = "Timers", Description = "Removes the named timer.",
            InsertText = "RemoveTimer(\"shmoo\")" )]
        public static void RemoveTimer( string name )
        {
            if ( _timers.TryGetValue( name, out OffsetStopwatch sw ) )
            {
                sw?.Stop();
            }

            _timers.Remove( name );
        }

        [CommandsDisplay( Category = "Timers", Description = "Set a timer value and create in case it does not exist.",
            InsertText = "SetTimer(\"shmoo\", 0)" )]
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

        [CommandsDisplay( Category = "Timers", Description = "Check for a named timer value.",
            InsertText = "if Timer(\"shmoo\") > 10000:" )]
        public static long Timer( string name )
        {
            return !_timers.ContainsKey( name ) ? 0 : _timers[name].ElapsedMilliseconds;
        }

        [CommandsDisplay( Category = "Timers", Description = "Outputs the elapsed timer value as a SystemMessage",
            InsertText = "TimerMsg(\"shmoo\"" )]
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
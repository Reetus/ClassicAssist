using System;
using System.Diagnostics;

namespace ClassicAssist.Misc
{
    public class OffsetStopwatch : Stopwatch
    {
        private TimeSpan _offset;

        public OffsetStopwatch( TimeSpan offset )
        {
            _offset = offset;
        }

        public new long ElapsedMilliseconds => base.ElapsedMilliseconds + (long) _offset.TotalMilliseconds;
        public new long ElapsedTicks => base.ElapsedTicks + _offset.Ticks;

        public void Reset( TimeSpan offset )
        {
            _offset = offset;
            Reset();
            Start();
        }

        public override string ToString()
        {
            return $"Elapsed: {TimeSpan.FromMilliseconds( ElapsedMilliseconds )}";
        }
    }
}
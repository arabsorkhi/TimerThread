/*
 * TickProvider.cs
 * Copyright 2017 Terry McIntyre
 * This source code is free software, per GNU General Public License, version 3.  
 * See http://www.gnu.org/licenses for details.
 * E-mail feedback to Terry McIntyre (mcinterry@gmail.com)
 */

namespace TimerThread
{
    /*
     * Base class for timer "tick" provider. Derived classes fill in blanks
     *   with specific tick counter implementation details.
     *   This assumes at least a milliseconds per tick precision.
     */ 
    public abstract class TickProvider
    {
        // ABSTRACT METHODS...

        // Return the current tick count
        public abstract long GetTickCount();

        // Return the number of ticks per milisecond
        public abstract long GetTicksPerMS();

        // Return the maximum tick count value (to deal with "wraparound")
        public abstract long GetMaxTick();

        // IMPLEMENTATION METHODS...

        // Tick/millisecond conversions...
        public uint TicksToMS(long ticks)
        {
            return (uint)(ticks / GetTicksPerMS());
        }

        public long MSToTicks(uint ms)
        {
            return (long)(ms * GetTicksPerMS());
        }

        // Tick math functions that can deal with "wraparound"
        public long GetElapsedTicks(long start, long stop)
        {
            // If the start is a "little" before the max tick (per GetWraparoundMargin()),
            //   and stop is little after the zero tick (within same margin), presume we've
            //   wrapped around the tick counter, and comoute the span accordinginy
            if ((start > stop) &&
                (start >= (GetMaxTick() - GetWraparoundMargin())) &&
                (stop <= GetWraparoundMargin()))
            {
                return (GetMaxTick() - start) + stop + 1;
            }
            // Otherwise, simple delta
            else
            {
                return stop - start;
            }
        }

        public long AddTicks(long ticksStart, long ticksElapsed)
        {
            // Initial simple result
            long result = ticksStart + ticksElapsed;
            // If the addition will cause wraparound, deal with it accordingly
            if ((GetMaxTick() - ticksStart) < ticksElapsed)
            {
                result = ticksElapsed - (GetMaxTick() - ticksStart) - 1;
            }

            return result;
        }

        // Presumed margin of nearness to "maximum tick" wrapround, so the tick math 
        //   functinos above can deal with a start tick time a little before the
        //   maximun tick, and a stop time a little after the wraparound "zero".
        private long GetWraparoundMargin()
        {
            // Presuming one hour of ticks on either side will work
            return GetTicksPerMS() * 1000 * 60 * 60;
        }

    }
}

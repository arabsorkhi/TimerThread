/*
 * TickTimer.cs
 * Copyright 2017 Terry McIntyre
 * This source code is free software, per GNU General Public License, version 3.  
 * See http://www.gnu.org/licenses for details.
 * E-mail feedback to Terry McIntyre (mcinterry@gmail.com)
 */

using System;
using System.Diagnostics;

namespace TimerThread
{
    /*
     * Timer class to represent a potentially recurring timer alarm. 
     *   The timer is identified with a numeric ID. There is an initial delay, timer period, 
     *     and optional finite count for specifying the alarm times.
     *   When an alarm is triggered, a delegate handler function is called.  
    */
    public class TickTimer : IComparable<TickTimer>
    {
    // CONSTANTS...
        //   Infinite repeat count
        public const int COUNT_INFINITE = -1;
        
    // CLASS PROPERTIES...
        // How many milliseconds can a timer be "overdue" (relative to curren time)
        //  and still be triggered rather than skipped? Default is 10 ms.
        public static uint OverdueToleranceMS
        {
            get { return _OverdueToleranceMS; }
            set { _OverdueToleranceMS = value; }
        }

        // Tick count provider implementation. Wil use a default if not explicitly set. 
        public static TickProvider TickProvider { get; set;  }
        
     // METHODS...
        // Construction/initialization...
        public TickTimer()
        {
            Clear();
        }

        public TickTimer(uint id)
        {
            Clear();
            ID = id;
        }

        public TickTimer(uint id, uint delay, uint period)
        {
            Initialize(id, delay, period, COUNT_INFINITE);
        }

        public TickTimer(uint id, uint delay, uint period, int count)
        {
            Initialize(id, delay, period, count);
        }

        public void Initialize(uint id, uint delay, uint period, int count)
        {
            Clear();
            ID = id;
            Delay = delay;
            Period = period;
            Count = count;
        }

        public void Clear()
        {
            CheckTickProvider();
            ID = 0;
            Delay = 0;
            Period = 0;
            Count = COUNT_INFINITE;
            _TickStart = -1;
            _TickNext = -1;
        }

        // Sort comparison
        public int CompareTo(TickTimer other)
        {
            return (int)TickProvider.GetElapsedTicks(other._TickNext, _TickNext);
        }

        // Start the timer...
        //   ...using current time
        public void Start()
        {
            Start(TickProvider.GetTickCount());
        }

        //   ...using provided time
        public void Start(long tickStart)
        {
            _TickStart = tickStart;
            UpdateNextAlarmTick();
        }

        // Returns the next alarm time tick
        public long GetNextAlarmTick()
        {
            return _TickNext;
        }

        //  Returns the number of milliseconds to wait until the next alarm occurence,
        //   or -1 if there is no next occurence
        public long GetNextAlarmWait()
        {
            long result = -1;

            // Immediate timers are just that - zero wait
            if (IsImmediate())
            {
                result = 0;
            }
            else
            {
                // Get the time difference between now and next alarm (may be negative)
                long tickNextAlarm = GetNextAlarmTick();
                long tickNow = TickProvider.GetTickCount();
                long delta = TickProvider.GetElapsedTicks(tickNow, tickNextAlarm);
                // Use the difference if in future
                if (delta >= 0)
                {
                    result = delta;
                }
                // Otherwise check for the next occurence being within "overdue tolerance"
                else 
                {
                    if (-delta <= TickProvider.MSToTicks(OverdueToleranceMS))
                    {
                        Debug.WriteLine("TickTimer.GetNextAlarmWait, tick delta is " + delta + ", within tolerance.");
                        result = 0;
                    }
                }
            }

            // Result so far is in ticks, convert to milliseconds as needed
            if (result >= 0)
            {
                result = TickProvider.TicksToMS(result);
            }

            return result;
        }


        // Returns true if this timer's next alarm is due
        public bool AlarmDue()
        {
            return (TickProvider.GetElapsedTicks(_TickNext, TickProvider.GetTickCount()) >= 0);
        }
    
        // Updates the timer for an alarm occurence.
        // Does NOT call the handler function, just updates the internal timekeeping members.
        public void UpdateForAlarm()
        {
            // If timer is not expired
            if (!IsExpired())
            {
                // If not an inifinte timer, decrement occurence count
                if (Count > 0)
                {
                    Count = Count - 1;
                }

                // Set next alarm time if still active
                if (!IsExpired())
                {
                    UpdateNextAlarmTick();
                }
            }
        }
    
        // Updates the "next tick" value relative to the current time
        public void UpdateNextAlarmTick()
        {
            // Don't bother if expired
            if (IsExpired())
            {
                return;
            }
            // A single immediate timer, always due now
            if (IsImmediate())
            {
                _TickNext = TickProvider.GetTickCount();
            }
            else
            {
                // Get the ticks elapsed since the timer start. 
                long tickNow = TickProvider.GetTickCount();
                long elapsedTicks = TickProvider.GetElapsedTicks(_TickStart, tickNow);
                uint elapsedMS = TickProvider.TicksToMS(elapsedTicks);
        
                // Determine the wait between the now and the next alarm 
                uint nextWait = 0;
                if (elapsedMS <= Delay)
                {
                    nextWait = Delay - elapsedMS;
                }
                else
                {
                    nextWait = Period - ((elapsedMS - Delay) % Period);
                }
        
                // If the result is zero, set it to the length of the period (presuming this timer is already being set off)
                if (nextWait == 0)
                {
                    nextWait = Period;
                }

                // The next alarm is the current tick count plus the next alarm tick span
                _TickNext = TickProvider.AddTicks(tickNow, TickProvider.MSToTicks(nextWait));
            }
        }
        
        // Returns true if this timer is expired
        public bool IsExpired()
        {
            return (Count == 0);
        }
    
        // Set this timer to be a single immediate timer
        public void SetImmediate()
        {
            Period = 0;
            Delay = 0;
            Count = 1;
        }
    
        // Returns true if this timer is a single immediate one
        public bool IsImmediate()
        {
            return ((Period == 0) &&
                    (Delay == 0) &&
                    (Count == 1));
        }

        // Set the TickProvider class property to default implementation if not yet set
        private void CheckTickProvider()
        {
            if (TickProvider == null)
            {
                TickProvider = new TickProviderDateTime();
                Debug.WriteLine("TickTimer, using default TickProvider.");
            }
        }


        // DELEGATES...
        // Timer event delegate type
        public delegate void HandlerType(uint timerID);
        public HandlerType Handler; 

        // PROPERTIES...
        // Timer ID
        public uint ID { get; private set; }
        
        // Initial alarm delay, in milliseconds
        public uint Delay { get; private set; }
        
        // Timer period, in milliseconds 
        public uint Period { get; private set; }
        
        // Timer count (can be COUNT_INFINITE for infinitely repeating timer)
        public int Count { get; private set; }

        // DATA...
        // Internal property value for "overdue" alarm tolerance
        private static uint _OverdueToleranceMS = 10;

        // Timer starting tick count
        private long _TickStart;
        
        // Next alarm tick count
        private long _TickNext;
    }
}

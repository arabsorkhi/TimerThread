/*
 * TickTimerQueue.cs
 * Copyright 2017 Terry McIntyre
 * This source code is free software, per GNU General Public License, version 3.  
 * See http://www.gnu.org/licenses for details.
 * E-mail feedback to Terry McIntyre (mcinterry@gmail.com)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace TimerThread
{
    /*
     * Thread-safe priority queue of TickTimer objects.
     * Represents a set of timers, sorted in order of the next occurence.
     */ 
    public class TickTimerQueue
    {
    // METHODS...
        // Construction...
        public TickTimerQueue()
        {
        }

        // Basic queue operations...
        public bool IsEmpty()
        {
            lock(_Lock)
            {
                return (_Timers.Count == 0);
            }
        }

        public void Add(TickTimer timer)
        {
            lock(_Lock)
            {
                if (!IsPresent(timer.ID))
                {
                    timer.Start();
                    _Timers.Add(timer);
                    _Timers.Sort();
                }
            }
        }

        public bool Remove(uint timerID)
        {
            bool result = false;
            lock(_Lock)
            {
                int index = Find(timerID);
                if (index >= 0)
                {
                    _Timers.RemoveAt(index);
                    result = true;
                }
            }

            return result;
        }

        public void RemoveAll()
        {
            lock(_Lock)
            {
                _Timers.Clear();
            }
        }

        public bool IsPresent(uint timerID)
        {
            return (Find(timerID) >= 0);
        }

        private int Find(uint timerID)
        {
            lock (_Lock)
            {
                return _Timers.FindIndex(timer => timer.ID == timerID);
            }
        }

        // Start all timers, sync-ed to the same starting tick value
        public void StartAll()
        {
            lock (_Lock)
            {
                long tick = TickTimer.TickProvider.GetTickCount();
                foreach (TickTimer timer in _Timers)
                {
                    timer.Start(tick);
                }
            }
        }

        // Return the time to wait for the next timer alarm (in milliseconds), or Timeout.Infinite
        //   if no alarms are due.
        public long GetNextAlarmWait()
        {
            long result = Timeout.Infinite;
            lock(_Lock)
            {
                // Any timers present?
                if (_Timers.Count > 0)
                {
                    // Look at the current front timer wait
                    TickTimer timerFront = _Timers[0];
                    result = timerFront.GetNextAlarmWait();
                    // If front is expired, update/re-sort all timers and try again with the new front one
                    if (result < 0)
                    {
                        Debug.WriteLine("TickTimerQueue.GetNextAlarmWait, front is expired. Updating all alarms.");
                        UpdateAllAlarms();
                        timerFront = _Timers[0];
                        result = timerFront.GetNextAlarmWait();                    
                    }
                }
            }

            return result;
        }

        // Calls the handlers for alarms that are due, and updates all timers
        //   for the next alarm.
        public void NotifyNext()
        {
            lock(_Lock)
            {
                // Make a list copy of timers that are due, and update those to next
                //   alarm settings.
                List<TickTimer> timersDue = new List<TickTimer>();
                foreach (TickTimer timer in _Timers)
                {
                    if (timer.AlarmDue())
                    {
                        timersDue.Add(timer);
                        timer.UpdateForAlarm();
                    }
                    // Stop the loop once past due timers
                    else 
                    {
                        break;
                    }
                }
        
                // Remove all timers from main list that are now expired
                _Timers.RemoveAll(timer => timer.IsExpired());

                // Loop through the due timers and call their delegates
                //   This is done from a separate list in case the handlers themselves affect the
                //   queue contents.
                foreach (TickTimer dueTimer in timersDue)
                {
                    dueTimer.Handler.Invoke(dueTimer.ID);
                }

                // Re-sort the timer list
                _Timers.Sort();
            }
        }

        // Update all timers for their next alarm occurence, and re-sort the queue
        private void UpdateAllAlarms()
        {
            lock(_Lock)
            {
                foreach (TickTimer timer in _Timers)
                {
                    timer.UpdateNextAlarmTick();
                }
                _Timers.Sort();
            }
        }
    
    // DATA...
        // The timer list
        private List<TickTimer> _Timers = new List<TickTimer>();

        // Critical section lock object
        private Object _Lock = new Object();
    }
}

/*
 * TickTimerThread.cs
 * Copyright 2017 Terry McIntyre
 * This source code is free software, per GNU General Public License, version 3.  
 * See http://www.gnu.org/licenses for details.
 * E-mail feedback to Terry McIntyre (mcinterry@gmail.com)
 */

using System.Diagnostics;
using System.Threading;

namespace TimerThread
{
    /*
     * Timer Thread class. 
     *  Contains a TickTimerQueue to maintain a set of timers, and runs a background thread
     *  to trigger them at the approiproiate times.
     */
    public class TickTimerThread : ThreadBase
    {
    // METHODS...
        public TickTimerThread()
        {
        }

        // Timer maintenance...
        public void AddTimer(TickTimer timer)
        {
            _Timers.Add(timer);
            SignalTimerChange();
        }

        public void RemoveTimer(uint timerID)
        {
            _Timers.Remove(timerID);
            SignalTimerChange();
        }

        public void RemoveAllTimers()
        {
            _Timers.RemoveAll();
            SignalTimerChange();
        }

        public bool IsTimerPresent(uint timerID)
        {
            return _Timers.IsPresent(timerID);
        }
    
        // Starts all timers synced to the same start time
        public void StartAllTimers()
        {
            _Timers.StartAll();
        }

        // Thread start override
        public override void Start()
        {
            // Start the thread
            base.Start();

            // Start all timers
            StartAllTimers();
        }

        // The thread procedure
        protected override void Procedure()
        {
            Debug.WriteLine("TickTimerThread.Procedure starting.");

            // Event handles to wait on. These need to be in Handles enum order
            WaitHandle[] events = new WaitHandle[2] 
            {
                GetStopHandle(),
                _EventTimerChange,
            };

            // Get initial timer wait
            int wait = GetNextWait();

            // Infinite loop until explicitly broken...
            while (true)
            {
                Debug.WriteLine("TickTimerThread.Procedure next wait = " + wait);

                // Wait on events, with timer wait timeout
                int waitResult = WaitHandle.WaitAny(events, wait);
                
                // Timeout: 
                if (waitResult == WaitHandle.WaitTimeout)
                {
                    Debug.WriteLine("TickTimerThread.Procedure, timeout elapsed.");
                    // Trigger the next timer alarm(s)
                    _Timers.NotifyNext();
                }
                // Stop event signalled, break out of loop
                else if (waitResult == (int)Handles.StopThread)
                {
                    Debug.WriteLine("TickTimerThread.Procedure, stop event signalled.");
                    break;
                }
                // Timers have changed, just proceed to next loop iteration
                else if (waitResult == (int)Handles.TimerChange)
                {
                    Debug.WriteLine("TickTimerThread.Procedure, timer change event signalled.");
                }

                // Get next timer wait
                wait = GetNextWait();
            } // while

            Debug.WriteLine("TickTimerThread.Procedure ending.");
        }

        // Set event to signal that the timers have changed
        private void SignalTimerChange()
        {
            _EventTimerChange.Set();
        }

        // Gets the next timer alarm wait from the timer queue (will be in milliseconds)
        private int GetNextWait()
        {
            return (int)_Timers.GetNextAlarmWait();
        }

        // Enumeration of handles to wait on in main procedure
        private enum Handles { StopThread, TimerChange };

    // DATA...
        // The timer queue
        private TickTimerQueue  _Timers = new TickTimerQueue();

        // Inter-thread signal event for when timer queue is changed
        private AutoResetEvent  _EventTimerChange = new AutoResetEvent(false);
    }
}

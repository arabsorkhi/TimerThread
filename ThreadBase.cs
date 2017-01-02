/*
 * ThreadBase.cs
 * Copyright 2017 Terry McIntyre
 * This source code is free software, per GNU General Public License, version 3.  
 * See http://www.gnu.org/licenses for details.
 * E-mail feedback to Terry McIntyre (mcinterry@gmail.com)
 */

using System;
using System.Threading;

namespace TimerThread
{
    /*
     * Basic thread class. Has an abstract member procedure method that derived classes
     *   implement. Also has a "stop" event that is singaled when the controling thread wants
     *   this to stop. For use in a continuous loop worker thread implementation.
     */
    abstract public class ThreadBase
    {
    // METHODS...
        // Construction...
        public ThreadBase()
        { 
        }

        // Thread control methods...
	    public virtual void Start()
        {
            _Thread.Start(this);
        }

	    public virtual bool Stop(int wait = -1)
        {
            bool result = true;
            // Signal the "stop" event, and wait for the thread to complete
            _EventStop.Set();
            if (wait >= 0)
            {
                result = _Thread.Join(wait);
            }
            else
            {
                _Thread.Join();
            }

            return result;
        }

        public virtual void Abort()
        {
            _Thread.Abort();
        }

        public bool IsAlive
        {
            get { return _Thread.IsAlive; }
        }

        // Abstract procedure method to be implemented in derived classes
        protected abstract void Procedure();

        // See if the "stop" event has been signalled
        protected bool CheckStop(int wait)
        {
            return _EventStop.WaitOne(wait);
        }

        // Get the "stop" event handle for use in other "wait" functions
        protected EventWaitHandle GetStopHandle()
        {
            return _EventStop;
        }

        // Static thread function. "obj" parameter is really "this", and the 
        //   "Procedure" implementation is called.
        private static void StaticProcedure(Object obj)
        {
            Console.WriteLine("ThreadBase.StaticProcedure started.");
            ThreadBase This = (ThreadBase)obj;
            This.Procedure();
            Console.WriteLine("ThreadBase.StaticProcedure ending.");
        }

// DATA...
        // The actual thread
        private Thread  _Thread = new Thread(StaticProcedure);

        // Inter-thread event for telling this one to stop (for usi in continuous loop
        //   thread function implementations)
        private AutoResetEvent _EventStop = new AutoResetEvent(false);
    }
}

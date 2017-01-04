# TimerThread
C# Timer Thread project. Enables variable timers (a la System.Threading.Timer), but handled on a single dedicated thread instead of variable threads from the managed .Net thread pool. Also allows for customized timekeeping provider implementations.

#Key Classes
*TickProvider - Abstract class for "tick" timekeeping support. Derived classes use a specific "tick counter" mechanism to provide the current time used for timers, and support timekeeping arithmetic. Includes support to deal with tick counter "wraparound", where the counter hits the maximum value and starts over at zero. There are currently two specific implementations: TickProviderDateTime (the default one), and TickProviderEnvironment.

*TickTimer - Timer class. Similar in intent to System.Threading.Timer, with an initial delay, timer period, and optionally finite count. Includes a handler delegate called when the timer alarm is triggered. Has a static TickProvider property for timekeeping and time arithmetic support.

*TickTimerQueue - TickTimer priority queue class. Keeps the contained TickTimer objects sorted in order of next alarm occurrence.

*ThreadBase - Simple thread base class, that has an abstract "Procedure" method that is run on the thread. Includes a "stop" event for derived classes to use for stopping a continuously looping Procedure implementation.

*TickTimerThread - The thread to run TickTimers. Derived from ThreadBase. Contains a TickTimerQueue, and continuously loops, waiting for either the next TickTimer alarm to invoke, a change to the timers, or the "stop" event signal.

#Sample Usage

    // Common timer handler procedure
    private void TimerProc(uint timerID)
    {
       Console.WriteLine("TimerProc(" + timerID + ")");
    }

    // Sample use code
    public static void Main()
    {
        TickTimerThread timerThread = new TickTimerThread();
    
        // Timer A: triggered at 100, 600, and 1100 milliseconds
        TickTimer timerA = new TickTimer(1, 100, 500, 3);
        timerA.Handler = TimerProcBasic;
    
        // Timer B: triggered at 200 and 1200 milliseconds  
        TickTimer timerB = new TickTimer(2, 200, 1000, 2);
        // Note: this example has both timers using the same handler, but they can use separate handlers as well
        timerB.Handler = TimerProcBasic;

        // Add timers to thread and start them
        timerThread.AddTimer(timerA);
        timerThread.AddTimer(timerB);
        timerThread.Start();

        // Wait for timers to run
        Thread.Sleep(2000);
    
        // Stop the thread
        timerThread.Stop();
    }

# Sample Output

    TimerProc(1)
    TimerProc(2)
    TimerProc(1)
    TimerProc(1)
    TimerProc(2)

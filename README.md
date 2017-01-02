# TimerThread
C# Timer Thread project. Enables variable timers (a la System.Threading.Timer), but handled on a single dedicated thread instead of variable threads from the managed .Net thread pool. Also allows for customized timekeeping provider implementations.

#Key classes
TickProvider - Abstract class for "tick" timekeeping support. Derived classes use a specific "tick counter" mechanism to provide the current time used for timers, and support timekeeping arithmetic. Includes support to deal with tick counter "wraparound", where the counter hits the maximum value and starts over at zero. There are currently two specific implementations: TickProviderDateTime (the default one), and TickProviderEnvironment.

TickTimer - Timer class. Similar in intent to System.Threading.Timer, with an initial delay, timer period, and optionally finite count. Includes a handler delegate called when the timer alarm is triggered. Has a static TickProvider property for timekeeping and time arithmetic support.

TickTimerQueue - TickTimer priority queue class. Keeps the contained TickTimer objects sorted in order of next alarm occurrence.

ThreadBase - Simple thread base class, that has an abstract "Procedure" method that is run on the thread. Includes a "stop" event for derived classes to use for stopping a continuously looping Procedure implementation.

TickTimerThread - The thread to run TickTimers. Derived from ThreadBase. Contains a TickTimerQueue, and continuously loops, waiting for either the next TickTimer alarm to invoke, a change to the timers, or the "stop" event signal.

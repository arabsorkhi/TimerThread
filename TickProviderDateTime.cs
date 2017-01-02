/*
 * TickProviderDateTime.cs
 * Copyright 2017 Terry McIntyre
 * This source code is free software, per GNU General Public License, version 3.  
 * See http://www.gnu.org/licenses for details.
 * E-mail feedback to Terry McIntyre (mcinterry@gmail.com)
 */

using System;

namespace TimerThread
{
    /**
     * TickProvider implementation using DateTime.Now.Ticks 
     */
    public class TickProviderDateTime : TickProvider
    {
        public override long GetTickCount()
        {
            return DateTime.Now.Ticks;
        }

        public override long GetTicksPerMS()
        {
            return 10000;
        }

        public override long GetMaxTick()
        {
            return (long)0x7FFFFFFFFFFFFFFF;
        }
    }

}

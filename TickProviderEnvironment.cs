/*
 * TickProviderEnvironment.cs
 * Copyright 2017 Terry McIntyre
 * This source code is free software, per GNU General Public License, version 3.  
 * See http://www.gnu.org/licenses for details.
 * E-mail feedback to Terry McIntyre (mcinterry@gmail.com)
 */

using System;

namespace TimerThread
{
    /**
     * TickProvider implementation using Environment.TickCount 
     */
    public class TickProviderEnvironment : TickProvider
    {
        // METHODS...
        public override long GetTickCount()
        {
            return Environment.TickCount;
        }

        public override long GetTicksPerMS()
        {
            return 1;
        }

        public override long GetMaxTick()
        {
            return (long)0x7FFFFFFF;
        }
    }
}

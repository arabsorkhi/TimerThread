using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using TimerThread;

namespace TimerThreadTest
{
    /**
     * Unit tests for TickTimerThread class
     */
    [TestClass]
    public class TickTimerThreadTest
    {
    // METHODS...
        // Basic test of a few timers
        [TestMethod]
        public void TestBasic()
        {
            TickTimer.TickProvider = new TickProviderDateTime();
            int threadID = Thread.CurrentThread.ManagedThreadId;
            Debug.WriteLine("Main thread ID = " + threadID);
            _TestText.Clear();
            TickTimerThread timerThread = new TickTimerThread();
            
            // A: 100, 600, 1100
            TickTimer timerA = new TickTimer(1, 100, 500, 3);
            timerA.Handler = TimerProcBasic;

            // B: 205, 1205
            TickTimer timerB = new TickTimer(2, 205, 1000, 2);
            timerB.Handler = TimerProcBasic;

            // C: 310, 510, 710, 910, 1110
            TickTimer timerC = new TickTimer(3, 310, 200, 5);
            timerC.Handler = TimerProcBasic;

            timerThread.AddTimer(timerA);
            timerThread.AddTimer(timerB);
            timerThread.AddTimer(timerC);
            timerThread.Start();

            Thread.Sleep(750);
            String textSoFar = _TestText.ToString();
            Debug.WriteLine("TickTimerThreadTest.TestBasic, after 750 ms, test text = " + textSoFar);
            Assert.AreEqual("ABCCAC", textSoFar);

            Thread.Sleep(550);
            textSoFar = _TestText.ToString();
            Debug.WriteLine("TickTimerThreadTest.TestBasic, after 1300 ms, test text = " + textSoFar);
            Assert.AreEqual("ABCCACCACB", textSoFar);

            timerThread.Stop();
        }

        // Test timers with excessively long handlers that causes skipped alarms
        [TestMethod]
        public void TestLongHandler()
        {
            TickTimer.TickProvider = new TickProviderDateTime();
            _TestText.Clear();
            TickTimerThread timerThread = new TickTimerThread();
            
            // A: 100, 600, 1100, 2100
            TickTimer timerA = new TickTimer(1, 100, 500, 4);
            timerA.Handler = TimerProcLongHandler;

            // B: 305, 1305, 2305, 3305
            TickTimer timerB = new TickTimer(2, 305, 1000, 4);
            timerB.Handler = TimerProcLongHandler;

            // AB: *100, 305, *600, *1100, 1305, *2100, 2305, *3305
            timerThread.AddTimer(timerA);
            timerThread.AddTimer(timerB);
            timerThread.Start();

            // TO REVIEW: Waiting 3500 adds an extra 'B'. Why is this? Look at precise timing of handler calls.
            Thread.Sleep(3000);
            String textSoFar = _TestText.ToString();
            Debug.WriteLine("TickTimerThreadTest.TestLongHandler, after wait, test text = " + textSoFar);
            Assert.AreEqual("AAAAB", textSoFar);

            timerThread.Stop();
        }

        // Test a timer thread that has handlers that change the timers
        [TestMethod]
        public void TestChangeQueue()
        {
            TickTimer.TickProvider = new TickProviderEnvironment();
            _TestText.Clear();
            
            // A: 100, 600, 1100, 1600
            TickTimer timerA = new TickTimer(1, 100, 500, 4);
            timerA.Handler = TimerProcChangeQueue;

            // B: 305, 1305, 2305, 3305
            TickTimer timerB = new TickTimer(2, 305, 1000, 4);
            timerB.Handler = TimerProcChangeQueue;

            // AB: 100, 305, 600, 1100, 1305, 1600, 2305, 3305
            // ABCD: 100-AC, 305-B, 600-AC, 805-D, 1100-AC, 1305-B, 1805-D, 2305-B, 2805-D, 3305-B, 3805-D
            _ChangeThread.AddTimer(timerA);
            _ChangeThread.AddTimer(timerB);
            _ChangeThread.Start();

            Thread.Sleep(5000);
            String textSoFar = _TestText.ToString();
            Debug.WriteLine("TickTimerThreadTest.TestChangeQueue, after wait, test text = " + textSoFar);
            Assert.AreEqual("ACBACDABDBDBD", textSoFar);

            _ChangeThread.Stop();

        }

        private void AddToTestText(uint timerID)
        {
            char nextChar = (char)('A' + (timerID - 1));
            _TestText.Append(nextChar);
        }

        private void TimerProcBasic(uint timerID)
        {
            uint tickMS = TickTimer.TickProvider.TicksToMS(TickTimer.TickProvider.GetTickCount());
            Debug.WriteLine("[" + tickMS + "] TickTimerThreadTest.TimerProcBasic(" + timerID + ")");
            AddToTestText(timerID);
        }

        private void TimerProcLongHandler(uint timerID)
        {
            uint tickMSBefore = TickTimer.TickProvider.TicksToMS(TickTimer.TickProvider.GetTickCount());
            Debug.WriteLine("[" + tickMSBefore + "] TickTimerThreadTest.TimerProcLongHandler(" + timerID + ")");
            char nextChar = (char)('A' + (timerID - 1));
            AddToTestText(timerID);

            uint tickMSAfter = TickTimer.TickProvider.TicksToMS(TickTimer.TickProvider.GetTickCount());
            uint elapsed = tickMSAfter - tickMSBefore;
            const int TOTAL_ELAPSE = 400;
            int wait = 0;
            if (elapsed < TOTAL_ELAPSE)
            {
                wait = TOTAL_ELAPSE - Convert.ToInt32(elapsed);
            }
            Thread.Sleep(wait);
        }

        private void TimerProcChangeQueue(uint timerID)
        {
            uint tickMS = TickTimer.TickProvider.TicksToMS(TickTimer.TickProvider.GetTickCount());
            Debug.WriteLine("[" + tickMS + "] TickTimerThreadTest.TimerProcChangeQueue(" + timerID + ")");
            AddToTestText(timerID);
            if (timerID == 1)
            {
                String testText = _TestText.ToString();
                var countA = testText.Count(c => c == 'A');
                if (countA > 2)
                {
                    _ChangeThread.RemoveTimer(1);
                }
                else
                {
                    TickTimer timerC = new TickTimer(3);
                    timerC.SetImmediate();
                    timerC.Handler = TimerProcBasic;
                    _ChangeThread.AddTimer(timerC);
                }
            }
            else if (timerID == 2)
            {
                TickTimer timerD = new TickTimer();
                timerD.Initialize(4, 500, 0, 1);
                timerD.Handler = TimerProcBasic;
                _ChangeThread.AddTimer(timerD);
            }
        }

    // DATA...

        // Text that is built up by timer alarms
        private StringBuilder _TestText = new StringBuilder();

        // Member timer thread for testing handlers that change the timers
        private TickTimerThread _ChangeThread = new TickTimerThread();
    }
}

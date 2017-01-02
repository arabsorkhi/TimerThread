using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimerThread;


namespace TimerThreadTest
{
    /**
     * Unit tests for TickTimerQueue class
     */ 
    [TestClass]
    public class TickTimerQueueTest
    {
        [TestMethod]
        public void TestList()
        {
            TickTimerQueue q = new TickTimerQueue();
            Assert.IsTrue(q.IsEmpty());

            TickTimer timerA = new TickTimer(1, 500, 1000);
            q.Add(timerA);
            Assert.IsFalse(q.IsEmpty());

            TickTimer timerB = new TickTimer(2, 1000, 500);
            q.Add(timerB);

            TickTimer timerC = new TickTimer(3, 1000, 500);
            q.Add(timerC);

            Assert.IsTrue(q.IsPresent(2));
            q.Remove(2);
            Assert.IsFalse(q.IsPresent(2));

            q.RemoveAll();
            Assert.IsTrue(q.IsEmpty());
            Assert.IsFalse(q.IsPresent(1));
        }

        [TestMethod]
        public void TestNextAlarm()
        {
            TickTimerQueue q = new TickTimerQueue();
            TickTimer timerA = new TickTimer(1, 5000, 1000);
            q.Add(timerA);

            TickTimer timerB = new TickTimer(2, 500, 2000);
            q.Add(timerB);

            TickTimer timerC = new TickTimer(3, 2000, 500);
            q.Add(timerC);

            long wait = q.GetNextAlarmWait();
            
            Assert.IsTrue(wait <= 500);
        }
    }
}

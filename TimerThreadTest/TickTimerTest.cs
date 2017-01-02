using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using TimerThread;

namespace TimerThreadTest
{
    /**
     * Unit tests for TickTimer class
     */ 
    [TestClass]
    public class TestTickTimer
    {
        [TestMethod]
        public void TestCompareTo()
        {
            TickTimer timerA = new TickTimer(1, 500, 1000, 1);

            TickTimer timerB = new TickTimer(2, 1000, 500, 1);

            TickTimer timerC = timerB;

            timerA.Start();
            timerB.Start();
            timerC.Start();

            Assert.IsTrue(timerA.CompareTo(timerB) < 0);
            Assert.IsTrue(timerB.CompareTo(timerA) > 0);
            Assert.IsTrue(timerB.CompareTo(timerB) == 0);
            Assert.IsTrue(timerB.CompareTo(timerC) == 0);
        }

        [TestMethod]
        public void TestAlarmDue()
        {
            TickTimer timerA = new TickTimer(1, 500, 1000, 1);

            timerA.Start();
            System.Threading.Thread.Sleep(200);
            Assert.IsFalse(timerA.AlarmDue());
            System.Threading.Thread.Sleep(400);
            Assert.IsTrue(timerA.AlarmDue());
        }

        [TestMethod]
        public void TestExpired()
        {
            TickTimer timerA = new TickTimer(1, 500, 1000, 1);
            timerA.Start();
            Thread.Sleep(700);
            timerA.UpdateForAlarm();
            Assert.IsTrue(timerA.IsExpired());

            TickTimer timerB = new TickTimer(2, 0, 250, 2);

            timerB.Start();
            Thread.Sleep(100);
            timerB.UpdateForAlarm();
            Assert.IsFalse(timerB.IsExpired());
            Thread.Sleep(300);
            timerB.UpdateForAlarm();
            Assert.IsTrue(timerB.IsExpired());

        }

        [TestMethod]
        public void TestImmediate()
        {
            TickTimer timerA = new TickTimer(1, 500, 1000, 1);

            Assert.IsFalse(timerA.IsImmediate());

            TickTimer timerB = new TickTimer();
            timerB.SetImmediate();
            Assert.IsTrue(timerB.IsImmediate());
            Assert.IsTrue(timerB.GetNextAlarmWait() == 0);
        }

    }
}

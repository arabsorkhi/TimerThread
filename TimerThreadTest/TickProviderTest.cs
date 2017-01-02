using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimerThread;

namespace TimerThreadTest
{
    /**
     * Unit tests for TickProvider and implementations
     */
    [TestClass]
    public class TickProviderTest
    {
        private void TestElapsed(TickProvider provider)
        {
            Assert.AreEqual(100, provider.GetElapsedTicks(100, 200));
            Assert.AreEqual(-100, provider.GetElapsedTicks(200, 100));
            long tickStart = provider.GetMaxTick() - 1000;
            long tickStop = 1000;
            Assert.AreEqual(2001, provider.GetElapsedTicks(tickStart, tickStop));
        }

        [TestMethod]
        public void TestElapsed()
        {
            TickProvider provider = new TickProviderDateTime();
            TestElapsed(provider);

            provider = new TickProviderEnvironment();
            TestElapsed(provider);
        }

        private void TestAdd(TickProvider provider)
        {
            Assert.AreEqual(200, provider.AddTicks(100, 100));
            Assert.AreEqual(900, provider.AddTicks(1000, -100));
            long tickStart = provider.GetMaxTick() - 1000;
            Assert.AreEqual(1000, provider.AddTicks(tickStart, 2001));
        }

        [TestMethod]
        public void TestAdd()
        {
            TickProvider provider = new TickProviderDateTime();
            TestAdd(provider);

            provider = new TickProviderEnvironment();
            TestAdd(provider);
        }
    }
}

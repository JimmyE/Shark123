using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharkService;
using Microsoft.ServiceBus.Messaging;

namespace AzureServiceBusUnitTest
{
    [TestClass]
    public class UnderwriteQueueTest
    {
        private UnderwriteQueue _underwriteQueue;

        [TestInitialize]
        public void Setup()
        {
            _underwriteQueue = new UnderwriteQueue();
        }


        [TestMethod]
        public void TestMethod1()
        {
            var msg = new BrokeredMessage();
            _underwriteQueue.ProcessMessage(msg);
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharkData;
using SharkService;

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
            UnderwriteRequestDto dto = new UnderwriteRequestDto
            {
                Name = "test-name",
                Age = 30
            };
            _underwriteQueue.ProcessMessage(dto);

            Assert.IsNotNull(dto);

        }
    }
}

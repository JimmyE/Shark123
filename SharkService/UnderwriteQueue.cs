using System;
using System.Diagnostics;
using Microsoft.ServiceBus.Messaging;

namespace SharkService
{

    public interface IUnderwriteRequestProcessor
    {
        void ProcessMessage(BrokeredMessage receivedMessage);
    }

    public class UnderwriteQueue : IUnderwriteRequestProcessor
    {
        public void ProcessMessage(BrokeredMessage receivedMessage)
        {
            try
            {
                Trace.WriteLine("Processing Service Bus message: " + receivedMessage.SequenceNumber);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("ERROR ** Service Bus message: " + ex.Message);
            }
        }
    }
}

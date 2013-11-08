using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Microsoft.ServiceBus.Messaging;
using SharkData;

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
                var ser = new DataContractSerializer(typeof(UnderwriteRequestDto));
                var foo = receivedMessage.GetBody<UnderwriteRequestDto>(ser);
                //var msg = receivedMessage.Properties["mydata"];
                Trace.WriteLine("Processing Service Bus message: " + receivedMessage.SequenceNumber + " : " + foo.Name);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("ERROR ** Service Bus message: " + ex.Message);
            }
        }
    }
}

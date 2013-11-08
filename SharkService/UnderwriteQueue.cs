using System;
using System.Diagnostics;
using SharkData;

namespace SharkService
{

    public interface IUnderwriteRequestProcessor
    {
        UnderwriteResponseDto ProcessMessage(UnderwriteRequestDto receivedMessage);
    }

    public class UnderwriteQueue : IUnderwriteRequestProcessor
    {
        public UnderwriteResponseDto ProcessMessage(UnderwriteRequestDto receivedMessage)
        {
            try
            {
                //var ser = new DataContractSerializer(typeof(UnderwriteRequestDto));
                //var foo = receivedMessage.GetBody<UnderwriteRequestDto>(ser);
                //var msg = receivedMessage.Properties["mydata"];

                return new UnderwriteResponseDto {Status = 0, StatusMessage = "success"};
            }
            catch (Exception ex)
            {
                Trace.WriteLine("ERROR ** Service Bus message: " + ex.Message);
                return new UnderwriteResponseDto {Status = 1, StatusMessage = ex.Message};
            }
        }
    }
}

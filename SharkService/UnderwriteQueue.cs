using System;
using System.Diagnostics;
using System.Threading;
using SharkData;

namespace SharkService
{

    public interface IUnderwriteRequestProcessor
    {
        UnderwriteResponseDto ProcessMessage(UnderwriteRequestDto requestDto);
    }

    public class UnderwriteQueue : IUnderwriteRequestProcessor
    {
        public UnderwriteResponseDto ProcessMessage(UnderwriteRequestDto requestDto)
        {
            try
            {
                Thread.Sleep(TimeSpan.FromSeconds(3));

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

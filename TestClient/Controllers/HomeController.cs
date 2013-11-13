using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Web.Mvc;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using SharkData;

namespace TestClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
        //private const string UnderwriteQueueName = "underwriterequest";

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult SendMsgSync()
        {
            string msg = "";
            try
            {
                msg = SendTestMessageToQueue(true);
            }
            catch (Exception ex)
            {
                msg = "Error: " + ex.Message;
            }

            var result = new {message = msg};
            return Json(result);
        }
        
        public JsonResult SendMsgAsync()
        {
            string msg = "";
            try
            {
                msg = SendTestMessageToQueue(false);
            }
            catch (Exception ex)
            {
                msg = "Error: " + ex.Message;
            }

            var result = new {message = msg};
            return Json(result);
        }

        private string SendTestMessageToQueue(bool isSync)
        {
            try
            {
                //ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.Http;
                //var client = QueueClient.CreateFromConnectionString(_connectionString, UnderwriteQueueName);
                var client = QueueClient.CreateFromConnectionString(_connectionString, AppConstants.RequestQueueNameUnderwrite);

                TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                DateTime timeStamp = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, easternZone);

                var dto = new UnderwriteRequestDto
                {
                    //Name = "Spongebob + " + DateTime.Now.ToString("h:mm:ss.fff tt"),
                    Name = "Eddie: " + timeStamp.ToString("h:mm:ss.fff tt"),
                    Age = 10,
                    City = "Bikini Bottom"
                };

                var ser = new DataContractSerializer(typeof(UnderwriteRequestDto));
                var msg = new BrokeredMessage(dto, ser);
                client.Send(msg);

                //msg.Properties["mydata"] = "Hello world";
                //client.Send(msg);

                Trace.TraceWarning("Client() - send message");
                return string.Format("Success!    timestamp: {0}",  DateTime.Now.ToString("hh:mm:ss.fff tt"));
            }
            catch (Exception ex)
            {
                return "Error.  Exception: " + ex.Message + "   " + DateTime.Now.ToString("G");
            }

        }

    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using SharkData;
using SharkService;

namespace QueueReaderWorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly string _connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
        private const string RequestQueueName = "underwriterequest";
        private const string ResponseQueueName = "underwriteresponse";

        private readonly DataContractSerializer _requestSerializer = new DataContractSerializer(typeof(UnderwriteRequestDto));
        private readonly DataContractSerializer _responseSerializer = new DataContractSerializer(typeof(UnderwriteRequestDto));
        ManualResetEvent _completedEvent = new ManualResetEvent(false);

        private UnityContainer _unityContainer;
        private QueueClient _requestQueue;
        //private QueueClient _responseQueue;
        private IUnderwriteRequestProcessor _underwriteRequest;

        public override void Run()
        {
            ConfigureUnityResolver();

<<<<<<< HEAD
            var underwriteRequest = _unityContainer.Resolve<IUnderwriteRequestProcessor>();
            _client.OnMessage(underwriteRequest.ProcessMessage);
            //_client.OnMessage(ProcessMessage);
=======
            _underwriteRequest = _unityContainer.Resolve<IUnderwriteRequestProcessor>();

            _requestQueue.OnMessage(ProcessMessage);
>>>>>>> 508b2bdbff8b549c3ff18023003f816181a05536

            _completedEvent.WaitOne();
        }

        private void ProcessMessage(BrokeredMessage requestMsg)
        {
<<<<<<< HEAD
            _unityContainer = new UnityContainer();
            _unityContainer.RegisterType<IUnderwriteRequestProcessor, UnderwriteQueue>();
=======

            var dto = requestMsg.GetBody<UnderwriteRequestDto>(_requestSerializer);

           Trace.WriteLine("Processing Service Bus message: " + requestMsg.SequenceNumber + " : " + dto.Name);

            var result = _underwriteRequest.ProcessMessage(dto);

            //var response = new BrokeredMessage(result, _responseSerializer) {SessionId = requestMsg.ReplyToSessionId, MessageId = requestMsg.MessageId};
            //_responseQueue.Send(response);
>>>>>>> 508b2bdbff8b549c3ff18023003f816181a05536
        }

        private void ConfigureUnityResolver()
        {
            _unityContainer = new UnityContainer();
            _unityContainer.RegisterType<IUnderwriteRequestProcessor, UnderwriteQueue>();
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            var namespaceManager = NamespaceManager.CreateFromConnectionString(_connectionString);

            _requestQueue = CreateQueueIfNecessary(namespaceManager, RequestQueueName);
            //_responseQueue = CreateQueueIfNecessary(namespaceManager, ResponseQueueName);

            return base.OnStart();
        }

        private QueueClient CreateQueueIfNecessary(NamespaceManager namespaceManager, string queueName)
        {
            if (!namespaceManager.QueueExists(queueName))
            {
                namespaceManager.CreateQueue(queueName);
            }
            
            return QueueClient.CreateFromConnectionString(_connectionString, queueName);
        }

        public override void OnStop()
        {
            // Close the connection to Service Bus Queue
            _requestQueue.Close();
            _completedEvent.Set();
            base.OnStop();
        }
    }
}

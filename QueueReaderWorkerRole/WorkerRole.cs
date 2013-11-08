using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using SharkService;

namespace QueueReaderWorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        // The name of your queue
        const string QueueName = "ProcessingQueue";
        QueueClient _client;
        readonly ManualResetEvent _completedEvent = new ManualResetEvent(false);
        private UnityContainer _unityContainer;

        public override void Run()
        {
            ConfigureUnityResolver();
            Trace.WriteLine("Starting processing of messages");

            var underwriteRequest = _unityContainer.Resolve<IUnderwriteRequestProcessor>();
            _client.OnMessage(underwriteRequest.ProcessMessage);
            //_client.OnMessage(ProcessMessage);

            Trace.WriteLine("Finished processing of messages");
            _completedEvent.WaitOne();
        }

        private void ConfigureUnityResolver()
        {
            _unityContainer = new UnityContainer();
            _unityContainer.RegisterType<IUnderwriteRequestProcessor, UnderwriteQueue>();
        }

        private void ProcessMessage(BrokeredMessage receivedMessage)
        {
            try
            {
                // Process the message
                Trace.WriteLine("Processing Service Bus message: " + receivedMessage.SequenceNumber);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("ERROR ** Service Bus message: " + ex.Message);
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // Create the queue if it does not exist already
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            if (!namespaceManager.QueueExists(QueueName))
            {
                namespaceManager.CreateQueue(QueueName);
            }

            // Initialize the connection to Service Bus Queue
            _client = QueueClient.CreateFromConnectionString(connectionString, QueueName);
            return base.OnStart();
        }

        public override void OnStop()
        {
            // Close the connection to Service Bus Queue
            _client.Close();
            _completedEvent.Set();
            base.OnStop();
        }
    }
}

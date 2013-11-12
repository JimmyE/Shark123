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
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using SharkData;
using SharkService;

namespace QueueReaderWorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly string _connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
        //private const string RequestQueueNameUnderwrite = "underwriterequest";
        private const string ResponseQueueNameUnderwrite = "underwriteresponse";
        private const string RequestQueueNameOnboarding = "onboardingrequest";

        private readonly DataContractSerializer _requestSerializer = new DataContractSerializer(typeof(UnderwriteRequestDto));
        private readonly DataContractSerializer _responseSerializer = new DataContractSerializer(typeof(UnderwriteRequestDto));
        ManualResetEvent _completedEvent = new ManualResetEvent(false);

        private UnityContainer _unityContainer;
        private QueueClient _requestQueueUnderwrite;
        private QueueClient _responseQueueUnderwrite;
        private QueueClient _requestQueueOnboarding;
        private IUnderwriteRequestProcessor _underwriteRequest;

        public override void Run()
        {
            //Console.WriteLine("Foo() Run started (console.writeline msg)");
            //Trace.TraceError("WorkRole.Run() starting (error level msg)");
            Trace.TraceWarning("WorkRole.Run() starting (warning level msg)");
            ConfigureUnityResolver();

            _underwriteRequest = _unityContainer.Resolve<IUnderwriteRequestProcessor>();

            _requestQueueUnderwrite.OnMessage(ProcessUnderwritingRequest);

            //_requestQueueOnboarding.OnMessage(ProcessOnboardingRequest);

            _completedEvent.WaitOne();
        }

        private void ProcessUnderwritingRequest(BrokeredMessage requestMsg)
        {
            UnderwriteRequestDto dto = requestMsg.GetBody<UnderwriteRequestDto>(_requestSerializer);

           //Trace.WriteLine("Processing Underwriting Request: " + requestMsg.SequenceNumber + " : " + dto.Name);
           Trace.TraceError("Processing Underwriting Request (new): " + requestMsg.SequenceNumber + " : " + dto.Name);

            var result = _underwriteRequest.ProcessMessage(dto);

            var response = new BrokeredMessage(result, _responseSerializer)
            {
                SessionId = requestMsg.ReplyToSessionId,
                MessageId = requestMsg.MessageId,
                ReplyToSessionId = requestMsg.SessionId
            };

            requestMsg.Complete();
            //_responseQueueUnderwrite.Send(response);
        }

        private void ProcessOnboardingRequest(BrokeredMessage requestMsg)
        {
            Trace.WriteLine("Processing Onboarding Request: " + requestMsg.SequenceNumber);
        }

        private void ConfigureUnityResolver()
        {
            _unityContainer = new UnityContainer();
            _unityContainer.RegisterType<IUnderwriteRequestProcessor, UnderwriteQueue>();
        }

        public override bool OnStart()
        {
            Trace.TraceError("OnStart() started");
            try
            {
                // Set the maximum number of concurrent connections 
                ServicePointManager.DefaultConnectionLimit = 12;

                // ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.Http;

                var namespaceManager = NamespaceManager.CreateFromConnectionString(_connectionString);

                //_requestQueueUnderwrite = CreateQueueIfNecessary(namespaceManager, RequestQueueNameUnderwrite);
                _requestQueueUnderwrite = CreateQueueIfNecessary(namespaceManager, AppConstants.RequestQueueNameUnderwrite);
                _responseQueueUnderwrite = CreateQueueIfNecessary(namespaceManager, ResponseQueueNameUnderwrite);
                _requestQueueOnboarding = CreateQueueIfNecessary(namespaceManager, RequestQueueNameOnboarding);

                return base.OnStart();
            }
            catch (Exception ex)
            {
                Trace.TraceError("OnStart() error: " + ex.Message);
                throw;
            }
        }

        private QueueClient CreateQueueIfNecessary(NamespaceManager namespaceManager, string queueName)
        {
            //Trace.TraceWarning("Does queue exist: " + queueName);
            if (!namespaceManager.QueueExists(queueName))
            {
                Trace.TraceWarning("CreateQueue: " + _connectionString + " " + queueName);
                namespaceManager.CreateQueue(queueName);
            }
            
            Trace.TraceWarning("Get QueueClient from ConnectionString: " + _connectionString + " " + queueName);
            var qc = QueueClient.CreateFromConnectionString(_connectionString, queueName);
            if (qc == null)
            {
                Trace.TraceError("CreateQueue() failed - queueClient is null");
            }
            return qc;
            //return QueueClient.CreateFromConnectionString(_connectionString, queueName);
        }

        private void ConfigDiagnostics()
        {
            DiagnosticMonitorConfiguration config = DiagnosticMonitor.GetDefaultInitialConfiguration();
            config.ConfigurationChangePollInterval = TimeSpan.FromMinutes(1d);
            config.Logs.BufferQuotaInMB = 500;
            config.Logs.ScheduledTransferLogLevelFilter = LogLevel.Verbose;
            config.Logs.ScheduledTransferPeriod = TimeSpan.FromMinutes(1d);

            DiagnosticMonitor.Start( "Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString", config);
        }

        public override void OnStop()
        {
            // Close the connection to Service Bus Queue
            _requestQueueUnderwrite.Close();
            _completedEvent.Set();
            base.OnStop();
        }
    }
}

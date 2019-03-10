using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using PHmiClient.Utils.Runner;
using PHmiClient.Wcf;

namespace PHmiRunner.Utils.Wcf {
    internal class ServiceRunner : IRunner {
        private readonly IDefaultBindingFactory _defaultBindingFactory;
        private readonly string _server;
        private readonly IServerUriFactory _serverUriFactory;
        private readonly IService _service;
        private ServiceHost _host;

        public ServiceRunner(IService service, string server, IDefaultBindingFactory defaultBindingFactory,
            IServerUriFactory serverUriFactory) {
            _service = service;
            _server = server;
            _defaultBindingFactory = defaultBindingFactory;
            _serverUriFactory = serverUriFactory;
        }

        public void Start() {
            Uri address = _serverUriFactory.Create(_server);
            _host = new ServiceHost(_service, address);
            Binding binding = _defaultBindingFactory.Create();
            _host.AddServiceEndpoint(typeof(IService), binding, "");
            _host.Open();
        }

        public void Stop() {
            ServiceHost host = _host;
            if (host != null && host.State != CommunicationState.Faulted)
                host.Close();
        }
    }
}
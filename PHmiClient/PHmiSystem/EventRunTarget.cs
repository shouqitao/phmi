using System;
using PHmiClient.Loc;
using PHmiClient.Utils;
using PHmiClient.Wcf;

namespace PHmiClient.PHmiSystem {
    internal class EventRunTarget : IEventRunTarget {
        private readonly IDispatcherService _dispatcherService;

        public EventRunTarget(IDispatcherService dispatcherService) {
            _dispatcherService = dispatcherService;
        }

        public void Run(IService service) {
            _dispatcherService.Invoke(Raise);
        }

        public void Clean() { }

        public string Name {
            get { return Res.AfterUpdateEvent; }
        }

        public event EventHandler Runned;

        private void Raise() {
            EventHelper.Raise(ref Runned, this, EventArgs.Empty);
        }
    }
}
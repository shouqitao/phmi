using PHmiClient.Loc;
using PHmiClient.Utils;
using PHmiClient.Wcf;
using PHmiClient.Wcf.ServiceTypes;

namespace PHmiClient.PHmiSystem {
    internal class UpdateStatusRunTarget : IUpdateStatusRunTarget {
        private readonly ITimeService _timeService;

        public UpdateStatusRunTarget(ITimeService timeService) {
            _timeService = timeService;
        }

        public void Run(IService service) {
            UpdateStatusResult updateStatusResult = service.UpdateStatus();
            _timeService.UtcTime = updateStatusResult.Time;
        }

        public void Clean() { }

        public string Name {
            get { return Res.StatusService; }
        }
    }
}
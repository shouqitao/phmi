using System.Collections.Generic;
using System.Linq;
using PHmiClient.Loc;
using PHmiClient.Utils.Notifications;
using PHmiClient.Wcf;
using PHmiClient.Wcf.ServiceTypes;

namespace PHmiClient.Tags {
    internal class TagService : ITagService {
        private readonly IList<IoDeviceAbstract> _ioDevices = new List<IoDeviceAbstract>();
        private readonly IReporter _reporter;

        public TagService(IReporter reporter) {
            _reporter = reporter;
        }

        public void Add(IoDeviceAbstract ioDevice) {
            _ioDevices.Add(ioDevice);
        }

        public void Run(IService service) {
            var ioDevices = new List<IoDeviceAbstract>();
            var parameters = new List<RemapTagsParameter>();
            foreach (IoDeviceAbstract ioDevice in _ioDevices) {
                RemapTagsParameter parameter = ioDevice.CreateRemapParameter();
                if (parameter == null)
                    continue;
                parameters.Add(parameter);
                ioDevices.Add(ioDevice);
            }

            if (parameters.Any()) {
                var result = service.RemapTags(parameters.ToArray());
                for (var i = 0; i < ioDevices.Count; i++) ApplyResult(ioDevices[i], result[i]);
            }
        }

        public void Clean() {
            foreach (IoDeviceAbstract ioDevice in _ioDevices) ioDevice.ApplyRemapResult(null);
        }

        public string Name {
            get { return Res.TagService; }
        }

        private void ApplyResult(IoDeviceAbstract ioDevice, RemapTagsResult result) {
            _reporter.Report(result.Notifications);
            ioDevice.ApplyRemapResult(result);
        }
    }
}
using PHmiClient.Utils.Notifications;
using PHmiClient.Wcf;

namespace PHmiClient.PHmiSystem {
    internal class PHmiRunTargetFactory : IPHmiRunTargetFactory {
        public IPHmiRunTarget Create(
            INotificationReporter reporter,
            IServiceClientFactory clientFactory,
            params IServiceRunTarget[] targets) {
            return new PHmiRunTarget(reporter, clientFactory, targets);
        }
    }
}
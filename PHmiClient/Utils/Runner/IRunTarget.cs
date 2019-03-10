using PHmiClient.Utils.Notifications;

namespace PHmiClient.Utils.Runner {
    public interface IRunTarget {
        string Name { get; }
        INotificationReporter Reporter { get; }

        void Run();

        void Clean();
    }
}
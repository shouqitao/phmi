using PHmiClient.Wcf;

namespace PHmiClient.PHmiSystem {
    internal interface IServiceRunTarget {
        string Name { get; }

        void Run(IService service);

        void Clean();
    }
}
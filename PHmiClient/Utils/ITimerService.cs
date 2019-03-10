using System;

namespace PHmiClient.Utils {
    public interface ITimerService {
        TimeSpan TimeSpan { get; set; }

        event EventHandler Elapsed;

        void Start();

        void Stop();
    }
}
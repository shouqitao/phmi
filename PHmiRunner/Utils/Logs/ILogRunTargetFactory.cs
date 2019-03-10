using PHmiClient.Utils;
using PHmiModel.Entities;

namespace PHmiRunner.Utils.Logs {
    public interface ILogRunTargetFactory {
        ILogMaintainer Create(string connectionString, Log log, ITimeService timeService);
    }
}
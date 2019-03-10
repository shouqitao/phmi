using Npgsql;
using PHmiClient.Utils;
using PHmiModel.Entities;
using PHmiTools.Utils.Npg;

namespace PHmiRunner.Utils.Logs {
    public class LogMaintainerFactory : ILogRunTargetFactory {
        public ILogMaintainer Create(string connectionString, Log log, ITimeService timeService) {
            var npgsqlConnectionFactory = new NpgsqlConnectionFactory(connectionString);
            var logRepository = new LogRepository(log.Id);
            using (NpgsqlConnection connection = npgsqlConnectionFactory.Create()) {
                logRepository.EnsureTable(connection);
            }

            return new LogMaintainer(
                log,
                logRepository,
                timeService,
                npgsqlConnectionFactory);
        }
    }
}
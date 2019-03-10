using System;
using System.Linq;
using Npgsql;
using PHmiClient.Logs;
using PHmiClient.Utils;
using PHmiClient.Utils.Pagination;
using PHmiTools.Utils.Npg;
using Log = PHmiModel.Entities.Log;

namespace PHmiRunner.Utils.Logs {
    public class LogMaintainer : ILogMaintainer {
        private readonly INpgsqlConnectionFactory _connectionFactory;
        private readonly DateTime _defaultTime = new DateTime();
        private readonly ILogRepository _repository;
        private readonly ITimeService _timeService;
        private readonly TimeSpan? _timeToStore;

        public LogMaintainer(
            Log log,
            ILogRepository repository,
            ITimeService timeService,
            INpgsqlConnectionFactory connectionFactory) {
            _repository = repository;
            _timeService = timeService;
            _connectionFactory = connectionFactory;
            _timeToStore = log.TimeToStoreDb.HasValue
                ? new TimeSpan(log.TimeToStoreDb.Value) as TimeSpan?
                : null;
        }

        public DateTime Save(LogItem item) {
            using (NpgsqlConnection connection = _connectionFactory.Create()) {
                DeleteOld(connection);
                if (item.Time == _defaultTime) {
                    item.Time = _timeService.UtcTime;
                    _repository.Insert(connection, item);
                } else if (!_repository.Update(connection, item)) {
                    _repository.Insert(connection, item);
                }

                return item.Time;
            }
        }

        public void Delete(DateTime[] times) {
            using (NpgsqlConnection connection = _connectionFactory.Create()) {
                foreach (DateTime time in times) _repository.Delete(connection, time);
            }
        }

        public LogItem[][] GetItems(Tuple<CriteriaType, DateTime, int, bool>[] parameters) {
            using (NpgsqlConnection connection = _connectionFactory.Create()) {
                return parameters
                    .Select(p => _repository.GetItems(connection, p.Item1, p.Item2, p.Item3, p.Item4))
                    .ToArray();
            }
        }

        private void DeleteOld(NpgsqlConnection connection) {
            if (!_timeToStore.HasValue)
                return;
            DateTime oldTime = _timeService.UtcTime - _timeToStore.Value;
            _repository.DeleteOld(connection, oldTime);
        }
    }
}
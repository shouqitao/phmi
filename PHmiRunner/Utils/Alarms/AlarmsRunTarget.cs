using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using PHmiClient.Alarms;
using PHmiClient.Users;
using PHmiClient.Utils;
using PHmiClient.Utils.Notifications;
using PHmiClient.Utils.Pagination;
using PHmiModel.Entities;
using PHmiResources.Loc;
using PHmiRunner.Utils.IoDeviceRunner;
using PHmiTools.Utils.Npg;

namespace PHmiRunner.Utils.Alarms {
    public class AlarmsRunTarget : IAlarmsRunTarget {
        private readonly IDictionary<int, AlarmStatus> _alarmDigitalValues;
        private readonly IDictionary<int, int?> _alarmPrivileges = new Dictionary<int, int?>();
        private readonly INpgsqlConnectionFactory _connectionFactory;
        private readonly IProject _project;

        private readonly IAlarmsRepository _repository;
        private readonly ITimeService _timeService;
        private readonly TimeSpan? _timeToStore;
        private readonly Action _updateAlarmTagsDigitalValues;
        private NpgsqlConnection _connection;

        public AlarmsRunTarget(
            AlarmCategory alarmCategory,
            INotificationReporter reporter,
            IAlarmsRepository repository,
            IProject project,
            ITimeService timeService,
            INpgsqlConnectionFactory connectionFactory) {
            Name = string.Format("{0} \"{1}\"", Res.Alarms, alarmCategory.Name);
            Reporter = reporter;
            _timeService = timeService;
            _repository = repository;
            _project = project;
            _connectionFactory = connectionFactory;
            if (alarmCategory.TimeToStoreDb.HasValue)
                _timeToStore = new TimeSpan(alarmCategory.TimeToStoreDb.Value);

            _alarmDigitalValues = new Dictionary<int, AlarmStatus>(alarmCategory.AlarmTags.Count);
            foreach (AlarmTag t in alarmCategory.AlarmTags) {
                _alarmDigitalValues.Add(t.Id, new AlarmStatus(t.Acknowledgeable));
                _alarmPrivileges.Add(t.Id, t.Privilege);
            }

            _updateAlarmTagsDigitalValues = () => UpdateAlarmDigitalValues(GetIoDeviceGroups(alarmCategory));
        }

        public string Name { get; }

        public INotificationReporter Reporter { get; }

        public void Run() {
            if (_connection == null) _connection = _connectionFactory.Create();
            DeleteOldAlarms();
            DateTime time = _timeService.UtcTime;
            _updateAlarmTagsDigitalValues();
            ProcessAlarms(time);
        }

        public void Clean() {
            NpgsqlConnection connection = _connection;
            _connection = null;
            connection.Dispose();
        }

        public Alarm[] GetCurrentAlarms(CriteriaType criteriaType, AlarmSampleId criteria, int maxCount) {
            using (NpgsqlConnection connection = _connectionFactory.Create()) {
                return _repository.GetCurrentAlarms(connection, criteriaType, criteria, maxCount);
            }
        }

        public Alarm[] GetHistoryAlarms(CriteriaType criteriaType, AlarmSampleId criteria, int maxCount) {
            using (NpgsqlConnection connection = _connectionFactory.Create()) {
                return _repository.GetHistoryAlarms(connection, criteriaType, criteria, maxCount);
            }
        }

        public Tuple<bool, bool> GetHasActiveAndUnacknowledged() {
            using (NpgsqlConnection connection = _connectionFactory.Create()) {
                bool hasActive = _repository.HasActiveAlarms(connection);
                bool hasUnacknowledged = _repository.HasUnacknowledgedAlarms(connection);
                return new Tuple<bool, bool>(hasActive, hasUnacknowledged);
            }
        }

        public void Acknowledge(AlarmSampleId[] alarms, Identity identity) {
            var privilege = _project.UsersRunner.GetPrivilege(identity);
            var alarmsToAcknowledge = (from a in alarms
                let p = GetPrivilege(a.AlarmId)
                where !p.HasValue || privilege.HasValue && (p.Value & privilege.Value) != 0
                select a).ToArray();
            if (!alarmsToAcknowledge.Any())
                return;
            var userId = identity == null ? null : identity.UserId as long?;
            using (NpgsqlConnection connection = _connectionFactory.Create()) {
                _repository.Update(connection, alarmsToAcknowledge, _timeService.UtcTime, userId);
            }
        }

        private static IEnumerable<Tuple<int, Tuple<int, int>[]>> GetIoDeviceGroups(
            AlarmCategory alarmCategory) {
            var ioDeviceGroups = alarmCategory.AlarmTags
                .GroupBy(a => a.DigTag.IoDevice)
                .Select(g => new Tuple<int, Tuple<int, int>[]>(
                    g.Key.Id,
                    g.Select(a => new Tuple<int, int>(a.DigTag.Id, a.Id))
                        .ToArray()))
                .ToArray();
            return ioDeviceGroups;
        }

        private void UpdateAlarmDigitalValues(IEnumerable<Tuple<int, Tuple<int, int>[]>> ioDeviceGroups) {
            foreach (var g in ioDeviceGroups) {
                IIoDeviceRunTarget ioDev = _project.IoDeviceRunTargets[g.Item1];
                ioDev.EnterReadLock();
                try {
                    foreach (var t in g.Item2)
                        _alarmDigitalValues[t.Item2].Value = ioDev.GetDigitalValue(t.Item1);
                } finally {
                    ioDev.ExitReadLock();
                }
            }
        }

        private void DeleteOldAlarms() {
            if (!_timeToStore.HasValue)
                return;
            _repository.DeleteNotActive(_connection, _timeService.UtcTime - _timeToStore.Value);
        }

        private void ProcessAlarms(DateTime time) {
            var idsToInsert = new List<Tuple<DateTime, int, DateTime?>>();
            var idsToReset = new List<AlarmSampleId>();
            var activeIds = _repository.GetActiveIds(_connection).ToLookup(t => t.AlarmId);
            foreach (var alarmValue in _alarmDigitalValues)
                if (alarmValue.Value.Value == true) {
                    if (activeIds.Contains(alarmValue.Key))
                        idsToReset.AddRange(activeIds[alarmValue.Key].Skip(1));
                    else
                        idsToInsert.Add(new Tuple<DateTime, int, DateTime?>(
                            time,
                            alarmValue.Key,
                            alarmValue.Value.Acknowledgeable ? null : time as DateTime?));
                } else {
                    if (activeIds.Contains(alarmValue.Key)) idsToReset.AddRange(activeIds[alarmValue.Key]);
                }

            foreach (var ids in activeIds.Where(ids => !_alarmDigitalValues.ContainsKey(ids.Key)))
                idsToReset.AddRange(ids);
            if (idsToReset.Any()) _repository.Update(_connection, idsToReset.ToArray(), time);
            if (idsToInsert.Any()) _repository.Insert(_connection, idsToInsert.ToArray());
        }

        private int? GetPrivilege(int alarmId) {
            int? privilege;
            return _alarmPrivileges.TryGetValue(alarmId, out privilege) ? privilege : null;
        }

        private class AlarmStatus {
            public AlarmStatus(bool acknowledgeable) {
                Acknowledgeable = acknowledgeable;
            }

            public bool Acknowledgeable { get; }

            public bool? Value { get; set; }
        }
    }
}
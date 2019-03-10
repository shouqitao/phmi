using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using PHmiClient.Users;
using PHmiClient.Utils;
using PHmiClient.Utils.Pagination;

namespace PHmiClient.Alarms {
    public class AlarmCategoryBase : AlarmCategoryAbstract {
        private readonly IDictionary<int, Tuple<Func<string>, Func<string>>> _alarmInfos
            = new Dictionary<int, Tuple<Func<string>, Func<string>>>();

        private readonly IList<Tuple<AlarmSampleId[], Identity>> _alarmsToAcknowledge
            = new List<Tuple<AlarmSampleId[], Identity>>();

        private readonly IList<Tuple<CriteriaType, AlarmSampleId, int, Action<Alarm[]>>> _currentQueries
            = new List<Tuple<CriteriaType, AlarmSampleId, int, Action<Alarm[]>>>();

        private readonly Func<string> _descriptionGetter;

        private readonly Tuple<Func<string>, Func<string>> _emptyAlarmInfo
            = new Tuple<Func<string>, Func<string>>(() => null, () => null);

        private readonly IList<Tuple<CriteriaType, AlarmSampleId, int, Action<Alarm[]>>> _historyQueries
            = new List<Tuple<CriteriaType, AlarmSampleId, int, Action<Alarm[]>>>();

        private bool? _hasActive;
        private bool? _hasUnacknowledged;
        private Func<Identity> _identity;
        private int _isRead;

        protected internal AlarmCategoryBase(int id, string name, Func<string> descriptionGetter) {
            Id = id;
            Name = name;
            _descriptionGetter = descriptionGetter;
        }

        internal override int Id { get; }

        public override string Name { get; }

        public override string Description {
            get { return _descriptionGetter(); }
        }

        public override bool? HasActive {
            get {
                _isRead = 1;
                return _hasActive;
            }
            internal set {
                _hasActive = value;
                OnPropertyChanged("HasActive");
            }
        }

        public override bool? HasUnacknowledged {
            get {
                _isRead = 1;
                return _hasUnacknowledged;
            }
            internal set {
                _hasUnacknowledged = value;
                OnPropertyChanged("HasUnacknowledged");
            }
        }

        internal override void SetIdentityGetter(Func<Identity> getter) {
            _identity = getter;
        }

        protected override void AddAlarmInfo(int id, Func<string> locationGetter,
            Func<string> descriptionGetter) {
            _alarmInfos.Add(id, new Tuple<Func<string>, Func<string>>(locationGetter, descriptionGetter));
        }

        internal override bool HasAlarm(int id) {
            return _alarmInfos.ContainsKey(id);
        }

        internal override Tuple<Func<string>, Func<string>> GetAlarmInfo(int id) {
            Tuple<Func<string>, Func<string>> result;
            return _alarmInfos.TryGetValue(id, out result) ? result : _emptyAlarmInfo;
        }

        public override void GetCurrent(CriteriaType criteriaType, AlarmSampleId criteria, int maxCount,
            Action<Alarm[]> callback) {
            lock (_currentQueries) {
                _currentQueries.Add(new Tuple<CriteriaType, AlarmSampleId, int, Action<Alarm[]>>(
                    criteriaType, criteria, maxCount, callback));
            }
        }

        internal override Tuple<CriteriaType, AlarmSampleId, int, Action<Alarm[]>>[] QueriesForCurrent() {
            lock (_currentQueries) {
                var result = _currentQueries.ToArray();
                _currentQueries.Clear();
                return result;
            }
        }

        public override void GetHistory(CriteriaType criteriaType, AlarmSampleId criteria, int maxCount,
            Action<Alarm[]> callback) {
            lock (_historyQueries) {
                _historyQueries.Add(new Tuple<CriteriaType, AlarmSampleId, int, Action<Alarm[]>>(
                    criteriaType, criteria, maxCount, callback));
            }
        }

        internal override Tuple<CriteriaType, AlarmSampleId, int, Action<Alarm[]>>[] QueriesForHistory() {
            lock (_historyQueries) {
                var result = _historyQueries.ToArray();
                _historyQueries.Clear();
                return result;
            }
        }

        internal override bool IsRead() {
            bool isRead = Interlocked.Exchange(ref _isRead, 0) == 1;
            return isRead;
        }

        public override void Acknowledge(Alarm[] alarms) {
            lock (_alarmsToAcknowledge) {
                _alarmsToAcknowledge.Add(new Tuple<AlarmSampleId[], Identity>(
                    alarms.Select(a => new AlarmSampleId(a.StartTime, a.AlarmId)).ToArray(),
                    _identity()));
            }
        }

        internal override Tuple<AlarmSampleId[], Identity>[] AlarmsToAcknowledge() {
            lock (_alarmsToAcknowledge) {
                var result = _alarmsToAcknowledge.ToArray();
                if (result.Any()) { }

                _alarmsToAcknowledge.Clear();
                return result;
            }
        }

        public override event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            EventHelper.Raise(ref PropertyChanged, this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
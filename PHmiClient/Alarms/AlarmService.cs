using System;
using System.Collections.Generic;
using System.Linq;
using PHmiClient.Loc;
using PHmiClient.Users;
using PHmiClient.Utils;
using PHmiClient.Utils.Notifications;
using PHmiClient.Utils.Pagination;
using PHmiClient.Wcf;
using PHmiClient.Wcf.ServiceTypes;

namespace PHmiClient.Alarms {
    internal class AlarmService : IAlarmService {
        private readonly IDictionary<int, AlarmCategoryAbstract> _categories =
            new Dictionary<int, AlarmCategoryAbstract>();

        private readonly IDispatcherService _dispatcherService;
        private readonly IReporter _reporter;
        private AlarmCategoryAbstract _commonCategory;
        private ParametersInfo _lastInfo;

        public AlarmService(IReporter reporter) {
            _dispatcherService = new DispatcherService();
            _reporter = reporter;
        }

        public void Add(AlarmCategoryAbstract category) {
            if (category.Id == 0)
                _commonCategory = category;
            else
                _categories.Add(category.Id, category);
        }

        public void Run(IService service) {
            _lastInfo = GetParameters();
            if (_lastInfo.Parameters.Any()) {
                var result = service.RemapAlarms(_lastInfo.Parameters.Select(p => p.Item1).ToArray());
                ApplyResult(_lastInfo, result);
            } else {
                ApplyResult(new ParametersInfo(), new RemapAlarmResult[0]);
            }
        }

        public void Clean() {
            CleanStatus();
            CleanQueries();
        }

        public string Name {
            get { return Res.AlarmService; }
        }

        private ParametersInfo GetParameters() {
            var r = new ParametersInfo();
            var commonAcknowledgeParameters = _commonCategory.AlarmsToAcknowledge();
            var commonCurrentQueries = _commonCategory.QueriesForCurrent();
            r.CommonCurrentInfo.AddRange(commonCurrentQueries.Select(q =>
                new Tuple<Action<Alarm[]>, CriteriaType, int>(q.Item4, q.Item1, q.Item3)));
            var commonHistoryQueries = _commonCategory.QueriesForHistory();
            r.CommonHistoryInfo.AddRange(commonHistoryQueries.Select(q =>
                new Tuple<Action<Alarm[]>, CriteriaType, int>(q.Item4, q.Item1, q.Item3)));
            bool commonStatusIsRead = _commonCategory.IsRead();

            foreach (AlarmCategoryAbstract c in _categories.Values) {
                var parameter = new RemapAlarmsParameter {
                    CategoryId = c.Id
                };
                var addToParameters = false;

                parameter.AcknowledgeParameters = GetAcknowledgeParameters(c, commonAcknowledgeParameters);
                if (parameter.AcknowledgeParameters.Any()) addToParameters = true;

                if (c.IsRead() || commonStatusIsRead) {
                    parameter.GetStatus = true;
                    addToParameters = true;
                }

                var currentQueries = c.QueriesForCurrent();
                var currentCallbacks = currentQueries.Select(q => q.Item4).ToArray();
                parameter.CurrentParameters = commonCurrentQueries.Concat(currentQueries)
                    .Select(q => new Tuple<CriteriaType, AlarmSampleId, int>(q.Item1, q.Item2, q.Item3))
                    .ToArray();
                if (parameter.CurrentParameters.Any()) addToParameters = true;

                var historyQueries = c.QueriesForHistory();
                var historyCallbacks = historyQueries.Select(q => q.Item4).ToArray();
                parameter.HistoryParameters = commonHistoryQueries.Concat(historyQueries)
                    .Select(q => new Tuple<CriteriaType, AlarmSampleId, int>(q.Item1, q.Item2, q.Item3))
                    .ToArray();
                if (parameter.HistoryParameters.Any()) addToParameters = true;

                if (addToParameters)
                    r.Parameters.Add(new Tuple<RemapAlarmsParameter, Action<Alarm[]>[], Action<Alarm[]>[]>(
                        parameter, currentCallbacks, historyCallbacks));
            }

            return r;
        }

        private static Tuple<AlarmSampleId[], Identity>[] GetAcknowledgeParameters(
            AlarmCategoryAbstract category,
            IEnumerable<Tuple<AlarmSampleId[], Identity>> commonAcknowledgeParameters) {
            var alarmsToAcknowledge = new List<Tuple<AlarmSampleId, Identity>>();
            foreach (var tuple in commonAcknowledgeParameters)
                alarmsToAcknowledge.AddRange(from alarmSampleId in tuple.Item1
                    where category.HasAlarm(alarmSampleId.AlarmId)
                    select new Tuple<AlarmSampleId, Identity>(alarmSampleId, tuple.Item2));
            var acknowledgeParameters = category.AlarmsToAcknowledge();
            foreach (var tuple in acknowledgeParameters)
                alarmsToAcknowledge.AddRange(from alarmSampleId in tuple.Item1
                    where category.HasAlarm(alarmSampleId.AlarmId)
                    select new Tuple<AlarmSampleId, Identity>(alarmSampleId, tuple.Item2));
            return alarmsToAcknowledge
                .GroupBy(t => t.Item2)
                .Select(g => new Tuple<AlarmSampleId[], Identity>(g.Select(i => i.Item1).ToArray(), g.Key))
                .ToArray();
        }

        private void ApplyResult(ParametersInfo info, IList<RemapAlarmResult> result) {
            _reporter.Report(result.SelectMany(r => r.Notifications));
            _dispatcherService.Invoke(() => ApplyStatus(info, result));
            ApplyCurrentQueries(info, result);
            ApplyHistoryQueries(info, result);
        }

        private void ApplyStatus(ParametersInfo info, IList<RemapAlarmResult> result) {
            var categoriesSet = new HashSet<AlarmCategoryAbstract>();
            for (var i = 0; i < result.Count; i++) {
                var p = info.Parameters[i];
                RemapAlarmResult r = result[i];
                AlarmCategoryAbstract c = _categories[p.Item1.CategoryId];
                categoriesSet.Add(c);
                if (p.Item1.GetStatus) {
                    c.HasActive = r.HasActive;
                    c.HasUnacknowledged = r.HasUnacknowledged;
                } else {
                    c.HasActive = null;
                    c.HasUnacknowledged = null;
                }
            }

            foreach (AlarmCategoryAbstract unAppliedCategory in _categories.Values.Where(c =>
                !categoriesSet.Contains(c))) {
                unAppliedCategory.HasActive = null;
                unAppliedCategory.HasUnacknowledged = null;
            }

            ApplyCommonStatus();
        }

        private void ApplyCommonStatus() {
            if (_categories.Values.Any(c => !c.HasActive.HasValue))
                _commonCategory.HasActive = null;
            else
                _commonCategory.HasActive = _categories.Values.Any(c => c.HasActive == true);
            if (_categories.Values.Any(c => !c.HasUnacknowledged.HasValue))
                _commonCategory.HasUnacknowledged = null;
            else
                _commonCategory.HasUnacknowledged = _categories.Values.Any(c => c.HasUnacknowledged == true);
        }

        private void ApplyCurrentQueries(ParametersInfo info, IList<RemapAlarmResult> result) {
            for (var i = 0; i < result.Count; i++) {
                var p = info.Parameters[i];
                RemapAlarmResult r = result[i];
                AlarmCategoryAbstract alarmCategory = _categories[p.Item1.CategoryId];
                foreach (Alarm a in r.Current.SelectMany(alarms => alarms)) {
                    var alarmInfo = alarmCategory.GetAlarmInfo(a.AlarmId);
                    a.SetAlarmInfo(alarmInfo.Item1, alarmInfo.Item2, alarmCategory);
                }

                var callbacks = p.Item2;
                for (var j = 0; j < callbacks.Length; j++) {
                    var callback = callbacks[j];
                    var alarms = r.Current[info.CommonCurrentInfo.Count + j];
                    callback(alarms);
                }
            }

            for (var i = 0; i < info.CommonCurrentInfo.Count; i++) {
                var currentInfo = info.CommonCurrentInfo[i];
                int i1 = i;
                var alarms = result.SelectMany(r => r.Current[i1]);
                switch (currentInfo.Item2) {
                    case CriteriaType.DownFromInfinity:
                    case CriteriaType.DownFrom:
                    case CriteriaType.DownFromOrEqual:
                        alarms = alarms.OrderByDescending(a => a.StartTime).ThenByDescending(a => a.AlarmId)
                            .Take(currentInfo.Item3);
                        break;

                    case CriteriaType.UpFromInfinity:
                    case CriteriaType.UpFrom:
                    case CriteriaType.UpFromOrEqual:
                        alarms = alarms.OrderBy(a => a.StartTime).ThenBy(a => a.AlarmId)
                            .Take(currentInfo.Item3).Reverse();
                        break;
                }

                var callback = currentInfo.Item1;
                var alarmsArray = alarms.ToArray();
                callback(alarmsArray);
            }
        }

        private void ApplyHistoryQueries(ParametersInfo info, IList<RemapAlarmResult> result) {
            for (var i = 0; i < result.Count; i++) {
                var p = info.Parameters[i];
                RemapAlarmResult r = result[i];
                AlarmCategoryAbstract alarmCategory = _categories[p.Item1.CategoryId];
                foreach (Alarm a in r.History.SelectMany(alarms => alarms)) {
                    var alarmInfo = alarmCategory.GetAlarmInfo(a.AlarmId);
                    a.SetAlarmInfo(alarmInfo.Item1, alarmInfo.Item2, alarmCategory);
                }

                var callbacks = p.Item3;
                for (var j = 0; j < callbacks.Length; j++) {
                    var callback = callbacks[j];
                    var alarms = r.History[info.CommonCurrentInfo.Count + j];
                    callback(alarms);
                }
            }

            for (var i = 0; i < info.CommonHistoryInfo.Count; i++) {
                var historyInfo = info.CommonHistoryInfo[i];
                int i1 = i;
                var alarms = result.SelectMany(r => r.History[i1]);
                switch (historyInfo.Item2) {
                    case CriteriaType.DownFromInfinity:
                    case CriteriaType.DownFrom:
                    case CriteriaType.DownFromOrEqual:
                        alarms = alarms.OrderByDescending(a => a.StartTime).ThenByDescending(a => a.AlarmId)
                            .Take(historyInfo.Item3);
                        break;

                    case CriteriaType.UpFromInfinity:
                    case CriteriaType.UpFrom:
                    case CriteriaType.UpFromOrEqual:
                        alarms = alarms.OrderBy(a => a.StartTime).ThenBy(a => a.AlarmId)
                            .Take(historyInfo.Item3).Reverse();
                        break;
                }

                var callback = historyInfo.Item1;
                var alarmsArray = alarms.ToArray();
                callback(alarmsArray);
            }
        }

        private void CleanStatus() {
            ApplyStatus(new ParametersInfo(), new RemapAlarmResult[0]);
            ApplyCommonStatus();
        }

        private void CleanQueries() {
            var emptyAlarms = new Alarm[0];

            foreach (AlarmCategoryAbstract c in _categories.Values) {
                foreach (var t in c.QueriesForCurrent()) t.Item4(emptyAlarms);
                foreach (var t in c.QueriesForHistory()) t.Item4(emptyAlarms);
            }

            if (_lastInfo != null) {
                foreach (var t in _lastInfo.CommonCurrentInfo) t.Item1(emptyAlarms);
                foreach (var t in _lastInfo.CommonHistoryInfo) t.Item1(emptyAlarms);
                foreach (var p in _lastInfo.Parameters) {
                    foreach (var callback in p.Item2) callback(emptyAlarms);
                    foreach (var callback in p.Item3) callback(emptyAlarms);
                }

                _lastInfo = null;
            }
        }

        private class ParametersInfo {
            public ParametersInfo() {
                Parameters = new List<Tuple<RemapAlarmsParameter, Action<Alarm[]>[], Action<Alarm[]>[]>>();
                CommonCurrentInfo = new List<Tuple<Action<Alarm[]>, CriteriaType, int>>();
                CommonHistoryInfo = new List<Tuple<Action<Alarm[]>, CriteriaType, int>>();
            }

            public List<Tuple<RemapAlarmsParameter, Action<Alarm[]>[], Action<Alarm[]>[]>> Parameters { get; }

            public List<Tuple<Action<Alarm[]>, CriteriaType, int>> CommonCurrentInfo { get; }

            public List<Tuple<Action<Alarm[]>, CriteriaType, int>> CommonHistoryInfo { get; }
        }
    }
}
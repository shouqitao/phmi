using System;
using System.Collections.Generic;
using System.Linq;
using PHmiClient.Utils;
using PHmiClient.Utils.Notifications;
using PHmiClient.Utils.Pagination;
using PHmiModel.Entities;
using PHmiResources.Loc;

namespace PHmiRunner.Utils.Trends {
    public class TrendsRunTarget : ITrendsRunTarget {
        private readonly ITrendsRepositoryFactory _repositoryFactory;
        private readonly ITrendTableSelector _tableSelector;
        private readonly ITimeService _timeService;
        private readonly TimeSpan? _timeToStore;
        private readonly IDictionary<int, TrendTagInfo> _trendsInfo = new Dictionary<int, TrendTagInfo>();

        public TrendsRunTarget(
            TrendCategory trendCategory,
            INotificationReporter reporter,
            ITrendsRepositoryFactory repositoryFactory,
            IProject project,
            ITimeService timeService,
            ITrendTableSelector tableSelector) {
            Name = string.Format("{0} \"{1}\"", Res.Trends, trendCategory.Name);
            _timeToStore = trendCategory.TimeToStoreDb.HasValue
                ? new TimeSpan(trendCategory.TimeToStoreDb.Value) as TimeSpan?
                : null;
            foreach (TrendTag t in trendCategory.TrendTags.ToArray()) {
                Func<bool> triggerValueGetter;
                if (t.Trigger == null) {
                    triggerValueGetter = () => true;
                } else {
                    int trIoDevId = t.Trigger.IoDevice.Id;
                    int trId = t.Trigger.Id;
                    triggerValueGetter = () =>
                        project.IoDeviceRunTargets[trIoDevId].GetDigitalValue(trId) == true;
                }

                int ioDeviceId = t.NumTag.IoDevice.Id;
                int tagId = t.NumTag.Id;
                var trendInfo = new TrendTagInfo(
                    t.Id,
                    triggerValueGetter,
                    () => project.IoDeviceRunTargets[ioDeviceId].GetNumericValue(tagId));
                _trendsInfo.Add(t.Id, trendInfo);
            }

            Reporter = reporter;
            _repositoryFactory = repositoryFactory;
            _timeService = timeService;
            _tableSelector = tableSelector;
        }

        public void Run() {
            using (ITrendsRepository repository = _repositoryFactory.Create()) {
                DeleteOldTrendSamples(repository);
                ProcessTrends(repository);
            }
        }

        public void Clean() { }

        public string Name { get; }

        public INotificationReporter Reporter { get; }

        public Tuple<DateTime, double?[]>[] GetPage(int[] trendTagIds, CriteriaType criteriaType,
            DateTime criteria, int maxCount) {
            using (ITrendsRepository repository = _repositoryFactory.Create()) {
                return repository.GetPage(trendTagIds, criteriaType, criteria, maxCount);
            }
        }

        public Tuple<DateTime, double?[]>[] GetSamples(int[] trendTagIds, DateTime startTime,
            DateTime? endTime, int rarerer) {
            using (ITrendsRepository repository = _repositoryFactory.Create()) {
                return repository.GetSamples(trendTagIds, startTime, endTime, rarerer);
            }
        }

        private void DeleteOldTrendSamples(ITrendsRepository repository) {
            if (!_timeToStore.HasValue)
                return;
            repository.DeleteOld(_timeService.UtcTime - _timeToStore.Value);
        }

        private void ProcessTrends(ITrendsRepository repository) {
            DateTime time = _timeService.UtcTime;
            var infoToInsert = (from info in _trendsInfo.Values
                where info.TriggerValue()
                let value = info.Value()
                where value.HasValue
                select new Tuple<int, double>(info.Id, value.Value)).ToArray();
            if (infoToInsert.Any()) repository.Insert(time, _tableSelector.NextTable(), infoToInsert);
        }

        private class TrendTagInfo {
            public TrendTagInfo(int id, Func<bool> triggerValue, Func<double?> value) {
                Id = id;
                TriggerValue = triggerValue;
                Value = value;
            }

            public int Id { get; }

            public Func<bool> TriggerValue { get; }

            public Func<double?> Value { get; }
        }
    }
}
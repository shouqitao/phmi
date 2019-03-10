﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using PHmiClient.Utils.Pagination;

namespace PHmiClient.Logs {
    public class Log : LogAbstract {
        private readonly ConcurrentQueue<LogItem> _itemsToSave = new ConcurrentQueue<LogItem>();

        private readonly List<Tuple<CriteriaType, DateTime, int, bool, Action<LogItem[]>>> _parametersForGet
            = new List<Tuple<CriteriaType, DateTime, int, bool, Action<LogItem[]>>>();

        private readonly List<DateTime> _timesForDelete = new List<DateTime>();

        internal Log(int id, string name) {
            Id = id;
            Name = name;
        }

        internal override int Id { get; }

        public override string Name { get; }

        public override void GetItems(CriteriaType criteriaType, DateTime criteria, int maxCount,
            bool includeBytes, Action<LogItem[]> callback) {
            lock (_parametersForGet) {
                _parametersForGet.Add(new Tuple<CriteriaType, DateTime, int, bool, Action<LogItem[]>>(
                    criteriaType, criteria, maxCount, includeBytes, callback));
            }
        }

        public override void Save(LogItem item) {
            _itemsToSave.Enqueue(item);
        }

        public override void Delete(LogItem item) {
            lock (_timesForDelete) {
                _timesForDelete.Add(item.Time);
            }
        }

        internal override Tuple<CriteriaType, DateTime, int, bool, Action<LogItem[]>>[] GetInfo() {
            lock (_parametersForGet) {
                var info = _parametersForGet.ToArray();
                _parametersForGet.Clear();
                return info;
            }
        }

        internal override LogItem ItemToSave() {
            LogItem item;
            return _itemsToSave.TryDequeue(out item) ? item : null;
        }

        internal override DateTime[] TimesForDelete() {
            lock (_timesForDelete) {
                var items = _timesForDelete.ToArray();
                _timesForDelete.Clear();
                return items;
            }
        }
    }
}
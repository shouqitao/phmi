﻿using System.Collections.Generic;
using System.Linq;
using PHmiClient.Loc;
using PHmiClient.Utils.Notifications;
using PHmiClient.Wcf;
using PHmiClient.Wcf.ServiceTypes;

namespace PHmiClient.Trends {
    internal class TrendsService : ITrendsService {
        public const int MaxRarerer = 10;
        private readonly IList<TrendsCategoryAbstract> _categories = new List<TrendsCategoryAbstract>();
        private readonly IReporter _reporter;

        public TrendsService(IReporter reporter) {
            _reporter = reporter;
        }

        public void Run(IService service) {
            var categories = new List<TrendsCategoryAbstract>();
            var parameters = new List<RemapTrendsParameter>();
            foreach (TrendsCategoryAbstract category in _categories) {
                RemapTrendsParameter parameter = category.CreateRemapParameter();
                if (parameter == null)
                    continue;
                parameters.Add(parameter);
                categories.Add(category);
            }

            if (parameters.Any()) {
                var result = service.RemapTrends(parameters.ToArray());
                for (var i = 0; i < categories.Count; i++) ApplyResult(categories[i], result[i]);
            }
        }

        public void Clean() {
            foreach (TrendsCategoryAbstract category in _categories) category.ApplyRemapResult(null);
        }

        public string Name {
            get { return Res.TrendsService; }
        }

        public void Add(TrendsCategoryAbstract category) {
            _categories.Add(category);
        }

        private void ApplyResult(TrendsCategoryAbstract category, RemapTrendsResult result) {
            _reporter.Report(result.Notifications);
            category.ApplyRemapResult(result);
        }
    }
}
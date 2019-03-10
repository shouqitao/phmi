﻿using System;
using PHmiClient.Utils.Pagination;

namespace PHmiClient.Trends {
    public interface ITrendTag {
        TrendsCategoryAbstract Category { get; }
        string Name { get; }
        string Description { get; }
        string Format { get; }
        string EngUnit { get; }

        double? MinValue { get; }
        double? MaxValue { get; }

        void GetSamples(DateTime startTime, DateTime? endTime, int rarerer,
            Action<Tuple<DateTime, double>[]> callback);

        void GetPage(
            CriteriaType criteriaType,
            DateTime criteria,
            int maxCount,
            Action<Tuple<DateTime, double>[]> callback);
    }
}
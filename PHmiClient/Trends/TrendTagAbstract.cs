using System;
using PHmiClient.Utils.Pagination;

namespace PHmiClient.Trends {
    public abstract class TrendTagAbstract : ITrendTag {
        internal abstract int Id { get; }
        public abstract TrendsCategoryAbstract Category { get; }

        public abstract string Name { get; }

        public abstract string Description { get; }

        public abstract string Format { get; }

        public abstract string EngUnit { get; }

        public abstract void GetSamples(
            DateTime startTime, DateTime? endTime, int rarerer, Action<Tuple<DateTime, double>[]> callback);

        public abstract void GetPage(CriteriaType criteriaType, DateTime criteria, int maxCount,
            Action<Tuple<DateTime, double>[]> callback);

        public abstract double? MinValue { get; }

        public abstract double? MaxValue { get; }
    }
}
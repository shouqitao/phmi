using System;
using PHmiClient.Utils.Pagination;

namespace PHmiClient.Trends {
    public class TrendTag : TrendTagAbstract {
        private readonly TrendsCategoryAbstract _category;
        private readonly Func<string> _descriptionGetter;
        private readonly Func<string> _engUnitGetter;
        private readonly Func<string> _formatGetter;
        private readonly int _id;

        internal TrendTag(
            TrendsCategoryAbstract category,
            int id,
            string name,
            Func<string> descriptionGetter,
            Func<string> formatGetter,
            Func<string> engUnitGetter,
            double? minValue,
            double? maxValue) {
            _category = category;
            _id = id;
            Name = name;
            _descriptionGetter = descriptionGetter;
            _formatGetter = formatGetter;
            _engUnitGetter = engUnitGetter;
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public override TrendsCategoryAbstract Category {
            get { return _category; }
        }

        public override string Format {
            get { return _formatGetter(); }
        }

        internal override int Id {
            get { return _id; }
        }

        public override string Name { get; }

        public override string Description {
            get { return _descriptionGetter(); }
        }

        public override string EngUnit {
            get { return _engUnitGetter(); }
        }

        public override double? MinValue { get; }

        public override double? MaxValue { get; }

        public override void GetSamples(DateTime startTime, DateTime? endTime, int rarerer,
            Action<Tuple<DateTime, double>[]> callback) {
            _category.GetSamples(_id, startTime, endTime, rarerer, callback);
        }

        public override void GetPage(CriteriaType criteriaType, DateTime criteria, int maxCount,
            Action<Tuple<DateTime, double>[]> callback) {
            _category.GetPage(_id, criteriaType, criteria, maxCount, callback);
        }
    }
}
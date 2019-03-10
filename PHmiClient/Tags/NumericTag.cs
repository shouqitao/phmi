using System;
using PHmiClient.Utils;

namespace PHmiClient.Tags {
    public class NumericTag : Tag<double?>, INumericTag {
        private readonly Func<string> _engUnitGetter;
        private readonly Func<string> _formatGetter;

        internal NumericTag(IDispatcherService dispatcherService, int id, string name,
            Func<string> descriptionGetter,
            Func<string> formatGetter, Func<string> engUnitGetter, double? minValue, double? maxValue)
            : base(dispatcherService, id, name, descriptionGetter) {
            _formatGetter = formatGetter;
            _engUnitGetter = engUnitGetter;
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public double? MinValue { get; }

        public double? MaxValue { get; }

        public string ValueString {
            get {
                var value = Value;
                return value.HasValue
                    ? value.Value.ToString(_formatGetter.Invoke()) + _engUnitGetter.Invoke()
                    : null;
            }
        }

        public override double? Value {
            get { return base.Value; }
            set {
                if (value > MaxValue)
                    base.Value = MaxValue;
                else if (value < MinValue)
                    base.Value = MinValue;
                else
                    base.Value = value;
            }
        }

        protected override void OnPropertyChanged(string property) {
            base.OnPropertyChanged(property);
            if (property == "Value") OnPropertyChanged("ValueString");
        }
    }
}
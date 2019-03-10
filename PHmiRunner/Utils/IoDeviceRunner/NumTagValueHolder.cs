using System;
using PHmiModel.Entities;

namespace PHmiRunner.Utils.IoDeviceRunner {
    public class NumTagValueHolder : TagValueHolder<double?> {
        private readonly NumTagValueConverter _converter;

        public NumTagValueHolder(NumTag tag) {
            _converter = new NumTagValueConverter(tag);
            Address = tag.Device;
        }

        public override string Address { get; }

        protected override double? RawToEng(object value) {
            return _converter.RawToEng(value);
        }

        protected override object EngToRaw(double? value) {
            if (!value.HasValue)
                throw new Exception("Value is null");
            return _converter.EngToRaw(value.Value);
        }
    }
}
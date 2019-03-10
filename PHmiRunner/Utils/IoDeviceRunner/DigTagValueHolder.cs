using System;
using PHmiModel.Entities;

namespace PHmiRunner.Utils.IoDeviceRunner {
    public class DigTagValueHolder : TagValueHolder<bool?> {
        public DigTagValueHolder(DigTag tag) {
            Address = tag.Device;
        }

        public override string Address { get; }

        protected override bool? RawToEng(object value) {
            return value as bool?;
        }

        protected override object EngToRaw(bool? value) {
            if (!value.HasValue)
                throw new Exception("Value is null");
            return value.Value;
        }
    }
}
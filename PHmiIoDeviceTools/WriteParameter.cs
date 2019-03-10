using System;

namespace PHmiIoDeviceTools {
    [Serializable]
    public class WriteParameter {
        public WriteParameter(string address, object value) {
            Address = address;
            Value = value;
        }

        public string Address { get; }

        public object Value { get; }
    }
}
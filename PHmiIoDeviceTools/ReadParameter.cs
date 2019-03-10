using System;

namespace PHmiIoDeviceTools {
    [Serializable]
    public class ReadParameter {
        public ReadParameter(string address, Type valueType) {
            Address = address;
            ValueType = valueType;
        }

        public string Address { get; }

        public Type ValueType { get; }
    }
}
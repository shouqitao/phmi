using System;
using System.Collections.Generic;
using System.Reflection;
using PHmiIoDevice.Generic.Loc;
using PHmiIoDeviceTools;

namespace PHmiIoDevice.Generic {
    public class GenericIoDevice : IIoDevice {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        public GenericIoDevice(string options) { }

        public void Dispose() { }

        public void Open() { }

        public object[] Read(ReadParameter[] readParameters) {
            var values = new object[readParameters.Length];
            for (var i = 0; i < readParameters.Length; i++) {
                ReadParameter readParameter = readParameters[i];
                object value;
                if (_values.TryGetValue(readParameter.Address, out value)) {
                    if (value == null || value.GetType() == readParameter.ValueType)
                        values[i] = value;
                    else
                        throw new Exception(string.Format(
                            Res.TypeMismatchMessage,
                            readParameter.Address,
                            readParameter.ValueType,
                            value.GetType()));
                } else {
                    if (readParameter.ValueType.IsClass) {
                        ConstructorInfo constructor = readParameter.ValueType.GetConstructor(new Type[0]);
                        if (constructor == null)
                            throw new Exception(string.Format(
                                Res.ConstructorErrorMessage,
                                readParameter.Address,
                                readParameter.ValueType));
                        values[i] = constructor.Invoke(new object[0]);
                    } else {
                        value = Activator.CreateInstance(readParameter.ValueType);
                        values[i] = value;
                    }
                }
            }

            return values;
        }

        public void Write(WriteParameter[] writeParameters) {
            foreach (WriteParameter writeParameter in writeParameters)
                if (_values.ContainsKey(writeParameter.Address))
                    _values[writeParameter.Address] = writeParameter.Value;
                else
                    _values.Add(writeParameter.Address, writeParameter.Value);
        }
    }
}
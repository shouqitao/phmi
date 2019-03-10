using System;
using System.IO;
using System.Reflection;
using PHmiIoDeviceTools;
using PHmiResources.Loc;

namespace PHmiRunner.Utils.IoDeviceRunner {
    public class IoDeviceWrapper : MarshalByRefObject, IIoDeviceWrapper {
        private IIoDevice _ioDevice;

        public void Create(string filePath, string options) {
            if (!File.Exists(filePath)) throw new Exception(string.Format(Res.FileNotFoundMessage, filePath));
            Assembly assembly = GetAssembly(filePath);
            if (assembly == null) throw new Exception(string.Format(Res.FileNotAssemblyMessage, filePath));
            Type type = GetType(assembly);
            if (type == null)
                throw new Exception(string.Format(Res.AssemblyDoesNotContainInterfaceImplementantionMessage,
                    assembly.FullName, typeof(IIoDevice).Name));
            ConstructorInfo constructor = GetConstructor(type);
            if (constructor == null)
                throw new Exception(string.Format(Res.TypeDoesNotHaveConstructorMessage, type.Name));
            _ioDevice = (IIoDevice) constructor.Invoke(new object[] {options});
            _ioDevice.Open();
        }

        public object[] Read(ReadParameter[] readParameters) {
            return _ioDevice.Read(readParameters);
        }

        public void Write(WriteParameter[] writeParameters) {
            _ioDevice.Write(writeParameters);
        }

        public void Dispose() {
            IIoDevice ioDevice = _ioDevice;
            if (ioDevice != null)
                ioDevice.Dispose();
        }

        private static Assembly GetAssembly(string file) {
            try {
                return Assembly.LoadFrom(file);
            } catch {
                return null;
            }
        }

        private static Type GetType(Assembly assembly) {
            try {
                var types = assembly.GetTypes();
                Type ioDeviceType = typeof(IIoDevice);
                foreach (Type t in types)
                    if (ioDeviceType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsNotPublic)
                        return t;
            } catch {
                return null;
            }

            return null;
        }

        public static ConstructorInfo GetConstructor(Type type) {
            try {
                return type.GetConstructor(new[] {typeof(string)});
            } catch {
                return null;
            }
        }
    }
}
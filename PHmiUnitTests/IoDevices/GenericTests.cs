using System;
using NUnit.Framework;
using PHmiIoDevice.Generic;
using PHmiIoDeviceTools;

namespace PHmiUnitTests.IoDevices {
    [TestFixture]
    public class GenericTests {
        [SetUp]
        public void SetUp() {
            _ioDevice = new GenericIoDevice(null);
        }

        private class ParameterLessConstructorObject {
            public ParameterLessConstructorObject(string arg) { }
        }

        private class PrivateConstructorObject {
            private PrivateConstructorObject() { }
        }

        private GenericIoDevice _ioDevice;

        [Test]
        public void ReadDoesNotReturnParameterlessConstructorObject() {
            var parameters = new[] {
                new ReadParameter("Addr0", typeof(ParameterLessConstructorObject))
            };
            Assert.Throws<Exception>(() => _ioDevice.Read(parameters));
        }

        [Test]
        public void ReadDoesNotReturnPrivateConstructorObject() {
            var parameters = new[] {
                new ReadParameter("Addr0", typeof(PrivateConstructorObject))
            };
            Assert.Throws<Exception>(() => _ioDevice.Read(parameters));
        }

        [Test]
        public void ReadReturnsNewObject() {
            var parameters = new[] {
                new ReadParameter("Addr0", typeof(object))
            };
            var result = _ioDevice.Read(parameters);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(typeof(object), result[0].GetType());
        }

        [Test]
        public void ReadReturnsNewStruct() {
            var parameters = new[] {
                new ReadParameter("Addr0", typeof(short))
            };
            var result = _ioDevice.Read(parameters);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(typeof(short), result[0].GetType());
        }

        [Test]
        public void ReadTwoValues() {
            var parameters = new[] {
                new ReadParameter("Addr0", typeof(int)),
                new ReadParameter("Addr1", typeof(object))
            };
            var result = _ioDevice.Read(parameters);
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(typeof(int), result[0].GetType());
            Assert.AreEqual(typeof(object), result[1].GetType());
        }

        [Test]
        public void ReadTypeMismatchTest() {
            var writeParameters = new[] {
                new WriteParameter("Addr0", 16)
            };
            _ioDevice.Write(writeParameters);

            var readParameters = new[] {
                new ReadParameter("Addr0", typeof(short))
            };
            Assert.Throws<Exception>(() => _ioDevice.Read(readParameters));
        }

        [Test]
        public void WriteTest() {
            var writeParameters = new[] {
                new WriteParameter("Addr0", 16)
            };
            _ioDevice.Write(writeParameters);

            var readParameters = new[] {
                new ReadParameter("Addr0", typeof(int))
            };
            var readResult = _ioDevice.Read(readParameters);
            Assert.AreEqual(1, readResult.Length);
            Assert.AreEqual(16, readResult[0]);

            writeParameters = new[] {
                new WriteParameter("Addr0", -1)
            };
            _ioDevice.Write(writeParameters);

            readParameters = new[] {
                new ReadParameter("Addr0", typeof(int))
            };
            readResult = _ioDevice.Read(readParameters);
            Assert.AreEqual(1, readResult.Length);
            Assert.AreEqual(-1, readResult[0]);
        }
    }
}
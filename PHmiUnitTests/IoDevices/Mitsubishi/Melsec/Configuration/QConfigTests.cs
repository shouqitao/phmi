using NUnit.Framework;
using PHmiIoDevice.Melsec.Configuration;

namespace PHmiUnitTests.IoDevices.Mitsubishi.Melsec.Configuration {
    [TestFixture]
    public class QConfigTests {
        [Test]
        public void GetSetXmlTest() {
            var config = new QConfig {
                Address = "Address",
                Port = 123,
                NetworkNumber = 234,
                PcNumber = 21
            };
            var config2 = new QConfig();
            config2.SetXml(config.GetXml());
            string actual = config.GetXml();
            string expected = config2.GetXml();
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
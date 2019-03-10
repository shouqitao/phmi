using NUnit.Framework;
using PHmiClient.Converters;

namespace PHmiClientUnitTests.Client.Converters {
    [TestFixture]
    public class EmptyToFalseConverterTests {
        [SetUp]
        public void SetUp() {
            _converter = new EmptyToFalseConverter();
        }

        private EmptyToFalseConverter _converter;

        private object Convert(object value) {
            return _converter.Convert(value, null, null, null);
        }

        [Test]
        public void EmptyCollectionReturnsFalse() {
            Assert.IsFalse((bool) Convert(new object[0]));
        }

        [Test]
        public void EmptyStringReturnsFalse() {
            Assert.IsFalse((bool) Convert(string.Empty));
        }

        [Test]
        public void NotEmptyCollectionReturnsTrue() {
            Assert.IsTrue((bool) Convert(new object[1]));
        }

        [Test]
        public void NotEmptyStringReturnsTrue() {
            Assert.IsTrue((bool) Convert("1"));
        }

        [Test]
        public void NotNullReturnsTrue() {
            Assert.IsTrue((bool) Convert(new object()));
        }

        [Test]
        public void NullReturnsFalse() {
            Assert.IsFalse((bool) Convert(null));
        }
    }
}
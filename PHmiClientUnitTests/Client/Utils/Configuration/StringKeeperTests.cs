using System.Configuration;
using System.Linq;
using NUnit.Framework;
using PHmiClient.Utils.Configuration;

namespace PHmiClientUnitTests.Client.Utils.Configuration {
    [TestFixture]
    public class StringKeeperTests {
        [SetUp]
        [TearDown]
        public void CleanUp() {
            System.Configuration.Configuration config = CreateConfig();
            config.AppSettings.Settings.Clear();
            config.Save();
        }

        private const string Prefix = "Settings";
        private const string Key = "Key";
        private const string ConfigKey = Prefix + "_" + Key;
        private const string Value = "Value";

        private static System.Configuration.Configuration CreateConfig() {
            return ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        private static StringKeeper CreateStringKeeper() {
            return new StringKeeper(Prefix);
        }

        [Test]
        public void GetReturnsNullWhenConfigEmpty() {
            StringKeeper keeper = CreateStringKeeper();
            Assert.IsNull(keeper.Get(Key));
        }

        [Test]
        public void GetReturnsValueWhenConfigStoresValue() {
            System.Configuration.Configuration config = CreateConfig();
            config.AppSettings.Settings.Add(ConfigKey, Value);
            config.Save();

            StringKeeper settings = CreateStringKeeper();
            Assert.AreEqual(Value, settings.Get(Key));
        }

        [Test]
        public void SetAndSaveReplacesEntryInConfig() {
            const string value = "Value";
            const string anotherValue = "Another value";

            System.Configuration.Configuration config = CreateConfig();
            config.AppSettings.Settings.Add(ConfigKey, value);
            config.Save();

            StringKeeper keeper = CreateStringKeeper();
            keeper.Set(Key, anotherValue);
            keeper.Save();

            config = CreateConfig();
            Assert.IsTrue(config.AppSettings.Settings.AllKeys.Count() == 1);
            Assert.AreEqual(anotherValue, config.AppSettings.Settings[ConfigKey].Value);
        }

        [Test]
        public void SetAndSaveSavesStringToConfig() {
            StringKeeper keeper = CreateStringKeeper();
            keeper.Set(Key, Value);
            keeper.Save();

            System.Configuration.Configuration config = CreateConfig();
            Assert.IsTrue(config.AppSettings.Settings.AllKeys.Count() == 1);
            Assert.AreEqual(Value, config.AppSettings.Settings[ConfigKey].Value);
        }

        [Test]
        public void SetNullAndSaveDoesNothingWhenConfigEmpty() {
            StringKeeper keeper = CreateStringKeeper();
            keeper.Set(Key, null);
            keeper.Save();

            System.Configuration.Configuration config = CreateConfig();
            Assert.IsFalse(config.AppSettings.Settings.AllKeys.Any());
        }

        [Test]
        public void SetNullAndSaveRemovesEntryFromConfig() {
            System.Configuration.Configuration config = CreateConfig();
            config.AppSettings.Settings.Add(ConfigKey, Value);
            config.Save();

            StringKeeper keeper = CreateStringKeeper();
            keeper.Set(Key, null);
            keeper.Save();

            config = CreateConfig();
            Assert.IsFalse(config.AppSettings.Settings.AllKeys.Any());
        }
    }
}
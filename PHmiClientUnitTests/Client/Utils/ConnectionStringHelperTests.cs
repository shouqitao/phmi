using System.Configuration;
using NUnit.Framework;
using PHmiClient.Utils;

namespace PHmiClientUnitTests.Client.Utils {
    [TestFixture]
    public class ConnectionStringHelperTests {
        [SetUp]
        [TearDown]
        public void CleanUp() {
            System.Configuration.Configuration config =
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.ConnectionStrings.SectionInformation.UnprotectSection();
            config.ConnectionStrings.ConnectionStrings.Clear();
            config.Save();
        }

        private static void PrepareConnectionString(string name, string connectionString) {
            System.Configuration.Configuration config =
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.ConnectionStrings.ConnectionStrings.Add(
                new ConnectionStringSettings(name, connectionString));
            config.ConnectionStrings.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
            config.Save();
        }

        [Test]
        public void GetReturnsConnectionString() {
            const string name = "Name";
            const string connectionString = "ConnectionString";
            PrepareConnectionString(name, connectionString);
            var helper = new ConnectionStringHelper();
            Assert.AreEqual(connectionString, helper.Get(name));
        }

        [Test]
        public void GetReturnsNullIfConnectionStringIsNotPresent() {
            const string name = "Name";
            var helper = new ConnectionStringHelper();
            Assert.IsNull(helper.Get(name));
        }

        [Test]
        public void ProtectProtectsConnectionStrings() {
            var helper = new ConnectionStringHelper();
            helper.Protect();
            System.Configuration.Configuration config =
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            Assert.IsTrue(config.ConnectionStrings.SectionInformation.IsProtected);
        }

        [Test]
        public void SetAddsConnectionString() {
            const string name = "Name";
            const string connectionString = "ConnectionString";
            var helper = new ConnectionStringHelper();
            helper.Set(name, connectionString);
            System.Configuration.Configuration config =
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            Assert.AreEqual(1, config.ConnectionStrings.ConnectionStrings.Count);
            Assert.AreEqual(connectionString,
                config.ConnectionStrings.ConnectionStrings[name].ConnectionString);
        }

        [Test]
        public void SetNullRemovesConnectionString() {
            const string name = "Name";
            const string connectionString = "ConnectionString";
            PrepareConnectionString(name, connectionString);
            var helper = new ConnectionStringHelper();
            helper.Set(name, null);
            System.Configuration.Configuration config =
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            Assert.AreEqual(0, config.ConnectionStrings.ConnectionStrings.Count);
        }

        [Test]
        public void SetOverridesConnectionString() {
            const string name = "Name";
            const string connectionString = "ConnectionString";
            PrepareConnectionString(name, "Old connection string");
            var helper = new ConnectionStringHelper();
            helper.Set(name, connectionString);
            System.Configuration.Configuration config =
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            Assert.AreEqual(1, config.ConnectionStrings.ConnectionStrings.Count);
            Assert.AreEqual(connectionString,
                config.ConnectionStrings.ConnectionStrings[name].ConnectionString);
        }

        [Test]
        public void SetProtectsConnectionStrings() {
            const string name = "Name";
            const string connectionString = "ConnectionString";
            var helper = new ConnectionStringHelper();
            helper.Set(name, connectionString);
            System.Configuration.Configuration config =
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            Assert.IsTrue(config.ConnectionStrings.SectionInformation.IsProtected);
        }
    }
}
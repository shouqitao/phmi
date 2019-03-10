using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace PHmiClient.Utils {
    public class ConnectionStringHelper : IConnectionStringHelper {
        private const string ProtectionProvider = "DataProtectionConfigurationProvider";

        public void Set(string name, string connectionString) {
            try {
                SetThatThrows(name, connectionString);
            } catch (ConfigurationErrorsException) {
                RemoveConnectionStrings();
                SetThatThrows(name, connectionString);
            }
        }

        public string Get(string name) {
            try {
                System.Configuration.Configuration config =
                    ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                ConnectionStringSettings section = config.ConnectionStrings.ConnectionStrings[name];
                return section == null ? null : section.ConnectionString;
            } catch (ConfigurationErrorsException) {
                return null;
            }
        }

        public void Protect() {
            try {
                ProtectThatThrows();
            } catch (ConfigurationErrorsException) {
                RemoveConnectionStrings();
                ProtectThatThrows();
            }
        }

        private static void SetThatThrows(string name, string connectionString) {
            System.Configuration.Configuration config =
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (connectionString == null) {
                if (config.ConnectionStrings.ConnectionStrings[name] != null)
                    config.ConnectionStrings.ConnectionStrings.Remove(name);
            } else {
                if (config.ConnectionStrings.ConnectionStrings[name] == null)
                    config.ConnectionStrings.ConnectionStrings.Add(
                        new ConnectionStringSettings(name, connectionString));
                else
                    config.ConnectionStrings.ConnectionStrings[name].ConnectionString = connectionString;
            }

            config.ConnectionStrings.SectionInformation.ProtectSection(ProtectionProvider);
            config.Save();
        }

        private static void RemoveConnectionStrings() {
            string location = Assembly.GetEntryAssembly().Location + ".Config";
            var document = new XmlDocument();
            document.Load(location);
            var configurations = document.GetElementsByTagName("configuration").OfType<XmlNode>();
            foreach (XmlNode configuration in configurations)
            foreach (XmlNode childNode in configuration.ChildNodes.OfType<XmlNode>())
                if (childNode.Name == "connectionStrings") {
                    configuration.RemoveChild(childNode);
                    break;
                }

            document.Save(location);
        }

        private static void ProtectThatThrows() {
            System.Configuration.Configuration config =
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (!config.ConnectionStrings.SectionInformation.IsProtected) {
                config.ConnectionStrings.SectionInformation.ProtectSection(ProtectionProvider);
                config.Save();
            }
        }
    }
}
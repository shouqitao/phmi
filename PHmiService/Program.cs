using System;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;
using PHmiClient.Utils;
using PHmiResources.Loc;
using PHmiTools;

namespace PHmiService {
    internal static class Program {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        private static void Main(string[] args) {
            string arguments = string.Join(" ", args);
            if (string.IsNullOrEmpty(arguments)) {
                var servicesToRun = new ServiceBase[] {
                    new Service()
                };
                ServiceBase.Run(servicesToRun);
            } else if (arguments.StartsWith("--i ")) {
                try {
                    string connectionString = arguments.Substring("--i ".Length);
                    var connectionStringHelper = new ConnectionStringHelper();
                    connectionStringHelper.Set(PHmiConstants.PHmiConnectionStringName, connectionString);
                    connectionStringHelper.Protect();

                    ManagedInstallerClass.InstallHelper(new[] {Assembly.GetExecutingAssembly().Location});
                } catch (Exception exception) {
                    Console.WriteLine(exception.ToString());
                }

                Console.WriteLine(Res.PressAnyKeyToContinue);
                Console.ReadKey();
            } else if (arguments.StartsWith("--u")) {
                try {
                    ManagedInstallerClass.InstallHelper(
                        new[] {"/u", Assembly.GetExecutingAssembly().Location});
                } catch (Exception exception) {
                    Console.WriteLine(exception.ToString());
                }

                Console.WriteLine(Res.PressAnyKeyToContinue);
                Console.ReadKey();
            }
        }
    }
}
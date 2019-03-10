using System.ComponentModel;
using System.Configuration.Install;
using PHmiService.Utils;

namespace PHmiService {
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer {
        public ProjectInstaller() {
            InitializeComponent();
            var postgresNames = ServiceHelper.GetServiceNames("postgresql");
            if (postgresNames != null) serviceInstaller1.ServicesDependedOn = postgresNames;
        }
    }
}
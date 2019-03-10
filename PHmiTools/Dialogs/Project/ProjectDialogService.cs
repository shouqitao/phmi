using PHmiClient.Utils;
using PHmiTools.Utils;
using PHmiTools.Utils.Npg;

namespace PHmiTools.Dialogs.Project {
    public class ProjectDialogService : IProjectDialogService {
        public IDialogHelper DialogHelper { get; } = new DialogHelper();

        public IConnectionStringHelper ConnectionStringHelper { get; } = new ConnectionStringHelper();

        public INpgConnectionParameters ConnectionParameters { get; } = new NpgConnectionParameters();
    }
}
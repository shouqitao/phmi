using PHmiClient.Utils;
using PHmiTools.Utils;
using PHmiTools.Utils.Npg;

namespace PHmiTools.Dialogs.Project {
    public class OpenProjectDialogService : ProjectDialogService, IOpenProjectDialogService {
        public INpgHelper NpgHelper { get; } = new NpgHelper();

        public IActionHelper ActionHelper { get; } = new ActionHelper();

        public IPHmiDatabaseHelper DatabaseHelper { get; } = new PHmiDatabaseHelper();
    }
}
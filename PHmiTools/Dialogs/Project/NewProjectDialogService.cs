using PHmiClient.Utils;
using PHmiTools.Utils.Npg;

namespace PHmiTools.Dialogs.Project {
    public class NewProjectDialogService : ProjectDialogService, INewProjectDialogService {
        public INpgHelper NpgHelper { get; } = new NpgHelper();

        public IActionHelper ActionHelper { get; } = new ActionHelper();

        public INpgScriptHelper ScriptHelper { get; } = new NpgScriptHelper();
    }
}
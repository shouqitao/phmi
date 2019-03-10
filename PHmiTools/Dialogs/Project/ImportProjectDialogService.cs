using PHmiClient.Utils;
using PHmiTools.Utils.Npg;
using PHmiTools.Utils.Npg.ExIm;

namespace PHmiTools.Dialogs.Project {
    public class ImportProjectDialogService : ProjectDialogService,
        IImportProjectDialogService {
        public INpgHelper NpgHelper { get; } = new NpgHelper();

        public IActionHelper ActionHelper { get; } = new ActionHelper();

        public INpgScriptHelper ScriptHelper { get; } = new NpgScriptHelper();

        public INpgExImHelper ExImHelper { get; } = new NpgExImHelper();
    }
}
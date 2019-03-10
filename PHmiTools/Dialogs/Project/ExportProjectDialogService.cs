using PHmiClient.Utils;
using PHmiTools.Utils;
using PHmiTools.Utils.Npg;
using PHmiTools.Utils.Npg.ExIm;

namespace PHmiTools.Dialogs.Project {
    public class ExportProjectDialogService : IExportProjectDialogService {
        public IActionHelper ActionHelper { get; } = new ActionHelper();

        public INpgExImHelper ExImHelper { get; } = new NpgExImHelper();

        public IDialogHelper DialogHelper { get; } = new DialogHelper();

        public INpgScriptHelper ScriptHelper { get; } = new NpgScriptHelper();

        public INpgHelper NpgHelper { get; } = new NpgHelper();
    }
}
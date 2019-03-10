using PHmiClient.Utils;
using PHmiModel;
using PHmiModel.Interfaces;
using PHmiTools.Utils;

namespace PHmiConfigurator.Modules {
    public class ModuleService : IModuleService {
        public IDialogHelper DialogHelper { get; } = new DialogHelper();

        public IModelContextFactory ContextFactory { get; } = new PHmiModelContextFactory();

        public IEditorHelper EditorHelper { get; } = new EditorHelper();

        public IClipboardHelper ClipboardHelper { get; } = new ClipboardHelper();

        public IActionHelper ActionHelper { get; } = new ActionHelper();
    }
}
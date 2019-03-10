using PHmiTools.Utils;

namespace PHmiConfigurator {
    public class MainWindowService : IMainWindowService {
        public IDialogHelper DialogHelper { get; } = new DialogHelper();
    }
}
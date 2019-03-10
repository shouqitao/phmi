using System;
using System.Windows.Input;

namespace PHmiConfigurator.Modules {
    public interface IModuleViewModel {
        Module View { get; set; }

        string ConnectionString { get; set; }

        bool HasChanges { get; }
        bool IsValid { get; }

        ICommand ReloadCommand { get; }
        ICommand SaveCommand { get; }
        ICommand CloseCommand { get; }

        event EventHandler Closed;

        bool Reload();

        bool Save();
    }
}
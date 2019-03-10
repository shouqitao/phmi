using System;
using PHmiTools.Views;

namespace PHmiConfigurator.Modules {
    public interface IModule : IView {
        string ConnectionString { get; set; }

        bool HasChanges { get; }
        bool IsValid { get; }

        event EventHandler Closed;

        void Save();
    }
}
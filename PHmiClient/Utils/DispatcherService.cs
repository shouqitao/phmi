using System;
using System.Windows;
using System.Windows.Threading;

namespace PHmiClient.Utils {
    public class DispatcherService : IDispatcherService {
        public void Invoke(Action action) {
            Application app = Application.Current;
            if (app == null) {
                action.Invoke();
                return;
            }

            Dispatcher dispatcher = app.Dispatcher;
            if (dispatcher == null) {
                action.Invoke();
                return;
            }

            if (dispatcher.CheckAccess()) {
                action.Invoke();
                return;
            }

            dispatcher.Invoke(action);
        }
    }
}
using System.Windows;

namespace PHmiRunner {
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public string Args { get; set; }

        protected override void OnStartup(StartupEventArgs e) {
            Args = string.Join(" ", e.Args);
            base.OnStartup(e);
        }
    }
}
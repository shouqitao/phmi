using System.ComponentModel;
using System.Windows;

namespace PHmiClient.Utils.ViewInterfaces {
    public interface IWindow {
        Rect RestoreBounds { get; }

        double Top { get; set; }
        double Left { get; set; }
        double Width { get; set; }
        double Height { get; set; }
        WindowState WindowState { get; set; }
        WindowStartupLocation WindowStartupLocation { get; set; }

        bool? DialogResult { get; set; }

        string Title { get; set; }

        event CancelEventHandler Closing;

        void Close();

        bool? ShowDialog();
    }
}
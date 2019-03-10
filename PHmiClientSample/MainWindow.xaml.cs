using System.Windows;
using PHmiClient.Utils;

namespace PHmiClientSample {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            WindowExtentions.UpdateLanguage(this);
        }
    }
}
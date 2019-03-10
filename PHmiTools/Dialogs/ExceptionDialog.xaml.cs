using System;
using System.Media;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using PHmiClient.Controls.Input;

namespace PHmiTools.Dialogs {
    /// <summary>
    ///     Interaction logic for ExceptionDialog.xaml
    /// </summary>
    public partial class ExceptionDialog : Window {
        private ExceptionDialog(Exception e) {
            InitializeComponent();
            SystemSounds.Asterisk.Play();
            ErrorTextBox.Text = e.Message;
            DetailsTextBox.Text = e.ToString();
            OkCommand = new DelegateCommand(OkCommandExecuted);
            CancelCommand = new DelegateCommand(CancelCommandExecuted);
            DataContext = this;
            OkButton.Focus();
        }

        public ICommand OkCommand { get; }

        public ICommand CancelCommand { get; }

        public static bool Show(Exception exception, object owner = null) {
            var depObj = owner as DependencyObject;
            var result = false;
            SendOrPostCallback action =
                obj => {
                    var w = new ExceptionDialog((Exception) obj);
                    try {
                        w.Owner = depObj == null ? Application.Current.MainWindow : GetWindow(depObj);
                    } catch (InvalidOperationException) { }

                    result = w.ShowDialog() == true;
                };
            var context = new DispatcherSynchronizationContext(Application.Current.Dispatcher);
            context.Send(action, exception);
            return result;
        }

        private void OkCommandExecuted(object obj) {
            DialogResult = true;
        }

        private void CancelCommandExecuted(object obj) {
            DialogResult = false;
        }
    }
}
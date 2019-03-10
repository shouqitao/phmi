using System;
using System.ComponentModel;
using System.Drawing;
using System.Media;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using PHmiClient.Controls.Input;
using PHmiTools.Views;

namespace PHmiTools.Dialogs {
    /// <summary>
    ///     Interaction logic for MessageDialog.xaml
    /// </summary>
    public partial class MessageDialog : Window {
        private bool? _result;

        private MessageDialog(
            string message,
            string header,
            MessageBoxButton button,
            MessageBoxImage image) {
            InitializeComponent();

            Title = header;
            tb.Text = message;
            Icon icon = GetIcon(image);
            if (icon != null) {
                BitmapSource bitmap = Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                iMessage.Source = bitmap;
                if (Icon == null)
                    Icon = bitmap;
            }

            PositiveCommand = new DelegateCommand(PositiveCommandExecuted);
            NegativeCommand = new DelegateCommand(NegativeCommandExecuted);
            NeutralCommand = new DelegateCommand(NeutralCommandExecuted);

            switch (button) {
                case MessageBoxButton.OK:
                    bOk.Visibility = Visibility.Visible;
                    bYes.Visibility = Visibility.Collapsed;
                    bNo.Visibility = Visibility.Collapsed;
                    bCancel.Visibility = Visibility.Collapsed;
                    bOk.Focus();
                    break;

                case MessageBoxButton.OKCancel:
                    bOk.Visibility = Visibility.Visible;
                    bYes.Visibility = Visibility.Collapsed;
                    bNo.Visibility = Visibility.Collapsed;
                    bCancel.Visibility = Visibility.Visible;
                    bCancel.Focus();
                    break;

                case MessageBoxButton.YesNo:
                    bOk.Visibility = Visibility.Collapsed;
                    bYes.Visibility = Visibility.Visible;
                    bNo.Visibility = Visibility.Visible;
                    bCancel.Visibility = Visibility.Collapsed;
                    bNo.Focus();
                    break;

                case MessageBoxButton.YesNoCancel:
                    bOk.Visibility = Visibility.Collapsed;
                    bYes.Visibility = Visibility.Visible;
                    bNo.Visibility = Visibility.Visible;
                    bCancel.Visibility = Visibility.Visible;
                    bCancel.Focus();
                    break;
            }

            DataContext = this;
            Closing += MessageDialogClosing;
        }

        private void MessageDialogClosing(object sender, CancelEventArgs e) {
            if (_result != null)
                return;
            if (bCancel.Visibility == Visibility.Visible)
                return;
            _result = bNo.Visibility != Visibility.Visible;
        }

        private static Icon GetIcon(MessageBoxImage image) {
            switch (image) {
                case MessageBoxImage.Warning:
                    return SystemIcons.Warning;

                case MessageBoxImage.Information:
                    return SystemIcons.Asterisk;

                case MessageBoxImage.Error:
                    return SystemIcons.Error;

                case MessageBoxImage.Question:
                    return SystemIcons.Question;
            }

            return null;
        }

        public static bool? Show(
            string message,
            string header,
            MessageBoxButton button = MessageBoxButton.OK,
            MessageBoxImage image = MessageBoxImage.Asterisk,
            object owner = null) {
            switch (button) {
                case MessageBoxButton.OK:
                    SystemSounds.Asterisk.Play();
                    break;

                case MessageBoxButton.OKCancel:
                case MessageBoxButton.YesNo:
                case MessageBoxButton.YesNoCancel:
                    SystemSounds.Exclamation.Play();
                    break;
            }

            bool? result = null;
            SendOrPostCallback action = obj => {
                var depObj = owner as DependencyObject;
                var w = new MessageDialog(message, header, button, image);
                Window windowOwner = depObj == null ? Application.Current.MainWindow : GetWindow(depObj);
                try {
                    w.Owner = windowOwner;
                } catch (InvalidOperationException) { }

                var view = owner as IView;
                if (view != null && view.ImageSource != null)
                    w.Icon = view.ImageSource;
                else if (owner != null && windowOwner != null)
                    w.Icon = windowOwner.Icon;
                w.ShowDialog();
                result = w._result;
            };
            var context = new DispatcherSynchronizationContext(Application.Current.Dispatcher);
            context.Send(action, null);
            return result;
        }

        #region PositiveCommand

        public ICommand PositiveCommand { get; }

        private void PositiveCommandExecuted(object obj) {
            _result = true;
            Close();
        }

        #endregion PositiveCommand

        #region NegativeCommand

        public ICommand NegativeCommand { get; }

        private void NegativeCommandExecuted(object obj) {
            _result = false;
            Close();
        }

        #endregion NegativeCommand

        #region CancelCommand

        public ICommand NeutralCommand { get; }

        private void NeutralCommandExecuted(object obj) {
            _result = null;
            Close();
        }

        #endregion CancelCommand
    }
}
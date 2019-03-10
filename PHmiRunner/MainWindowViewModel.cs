using System;
using System.Windows;
using System.Windows.Input;
using PHmiClient.Controls.Input;
using PHmiClient.Utils;
using PHmiClient.Utils.Notifications;
using PHmiClient.Utils.ViewInterfaces;
using PHmiModel;
using PHmiRunner.Utils;
using PHmiTools;
using PHmiTools.Dialogs;
using PHmiTools.Dialogs.Project;
using PHmiTools.Utils.Npg;
using PHmiTools.ViewModels;

namespace PHmiRunner {
    public class MainWindowViewModel : ViewModelBase<IWindow> {
        private readonly IActionHelper _actionHelper;
        private readonly DelegateCommand _closeCommand;
        private readonly IProjectRunnerFactory _runnerFactory;
        private ICommand _aboutCommand;
        private bool _busy;
        private INpgConnectionParameters _connectionParameters;
        private ICommand _exitCommand;
        private ICommand _importCommand;
        private ICommand _openCommand;
        private IProjectRunner _runner;

        internal MainWindowViewModel(
            IActionHelper actionHelper,
            ITimeService timeService,
            INotificationReporter reporter,
            IProjectRunnerFactory runnerFactory) {
            _actionHelper = actionHelper;
            Reporter = reporter ?? new NotificationReporter(timeService);
            Reporter.ExpirationTime = TimeSpan.FromSeconds(1);
            _runnerFactory = runnerFactory ??
                             new ProjectRunnerFactory(timeService, Reporter, new PHmiModelContextFactory(),
                                 new NpgHelper());
            _closeCommand = new DelegateCommand(CloseCommandExecuted, CloseCommandCanExecute);
        }

        public MainWindowViewModel() : this(new ActionHelper(), new TimeService(), null, null) { }

        public INpgConnectionParameters ConnectionParameters {
            get { return _connectionParameters; }
            set {
                _connectionParameters = value;
                RestartRunner();
                OnPropertyChanged(this, v => v.ConnectionParameters);
                OnPropertyChanged(this, v => v.Title);
                _closeCommand.RaiseCanExecuteChanged();
            }
        }

        public INotificationReporter Reporter { get; }

        public string Title {
            get {
                return string.Format("{0}{1}",
                    ConnectionParameters != null ? ConnectionParameters.Database + " - " : string.Empty,
                    PHmiConstants.PHmiRunnerName);
            }
        }

        public bool Busy {
            get { return _busy; }
            set {
                _busy = value;
                OnPropertyChanged(this, v => v.Busy);
            }
        }

        public ICommand ExitCommand {
            get { return _exitCommand ?? (_exitCommand = new DelegateCommand(ExitCommandExecuted)); }
        }

        public ICommand CloseCommand {
            get { return _closeCommand; }
        }

        public ICommand OpenCommand {
            get { return _openCommand ?? (_openCommand = new DelegateCommand(OpenCommandExecuted)); }
        }

        public ICommand ImportCommand {
            get { return _importCommand ?? (_importCommand = new DelegateCommand(ImportCommandExecuted)); }
        }

        public ICommand AboutCommand {
            get { return _aboutCommand ?? (_aboutCommand = new DelegateCommand(AboutCommandExecuted)); }
        }

        private void RestartRunner() {
            if (_runner == null) {
                if (_connectionParameters != null) {
                    _runner = _runnerFactory.Create(_connectionParameters.Database,
                        _connectionParameters.ConnectionString);
                    Busy = true;
                    _actionHelper.Async(StartRunner);
                }
            } else {
                Busy = true;
                _actionHelper.Async(StopRunner);
            }
        }

        private void StartRunner() {
            _runner.Start();
            Busy = false;
        }

        private void StopRunner() {
            _runner.Stop();
            Busy = false;
            _runner = null;
            RestartRunner();
        }

        private void ExitCommandExecuted(object parameter) {
            IWindow view = View;
            if (view != null)
                view.Close();
        }

        private void CloseCommandExecuted(object parameter) {
            ConnectionParameters = null;
        }

        private bool CloseCommandCanExecute(object obj) {
            return ConnectionParameters != null;
        }

        private void OpenCommandExecuted(object parameter) {
            var dialog = new OpenProjectDialog();
            if (dialog.ShowDialog() == true) ConnectionParameters = dialog.ViewModel.ConnectionParameters;
        }

        private void ImportCommandExecuted(object obj) {
            var dialog = new ImportProjectDialog();
            if (dialog.ShowDialog() == true) ConnectionParameters = dialog.ViewModel.ConnectionParameters;
        }

        private void AboutCommandExecuted(object obj) {
            var w = new AboutDialog {Owner = View as Window};
            w.ShowDialog();
        }
    }
}
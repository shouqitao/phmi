using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using PHmiClient.Controls.Input;
using PHmiClient.Utils.ViewInterfaces;
using PHmiConfigurator.Dialogs;
using PHmiConfigurator.Modules;
using PHmiConfigurator.Modules.Collection;
using PHmiResources.Loc;
using PHmiTools;
using PHmiTools.Dialogs;
using PHmiTools.Dialogs.Project;
using PHmiTools.Utils.Npg;
using PHmiTools.ViewModels;

namespace PHmiConfigurator {
    public class MainWindowViewModel : ViewModelBase<IWindow> {
        private readonly DelegateCommand _buildClientCommand;
        private readonly DelegateCommand _closeCommand;
        private readonly DelegateCommand _exportCommand;
        private readonly ObservableCollection<object> _modules = new ObservableCollection<object>();
        private readonly DelegateCommand _openModuleCommand;
        private readonly DelegateCommand _runProjectCommand;
        private readonly IMainWindowService _service;
        private readonly DelegateCommand _uninstallServiceCommand;
        private INpgConnectionParameters _connectionParameters;
        private IModuleViewModel _selectedModuleViewModel;

        public MainWindowViewModel() : this(new MainWindowService()) { }

        internal MainWindowViewModel(IMainWindowService service) {
            _service = service;
            Modules = new ReadOnlyObservableCollection<object>(_modules);
            _openModuleCommand = new DelegateCommand(OpenModuleCommandExecuted, OpenModuleCommandCanExecute);
            ExitCommand = new DelegateCommand(ExitCommandExecuted);
            NewProjectCommand = new DelegateCommand(NewProjectCommandExecuted);
            OpenCommand = new DelegateCommand(OpenCommandExecuted);
            _closeCommand = new DelegateCommand(CloseCommandExecuted, CloseCommandCanExecute);
            _exportCommand = new DelegateCommand(ExportCommandExecuted, ExportCommandCanExecute);
            ImportCommand = new DelegateCommand(ImportCommandExecuted);
            QuickStartCommand = new DelegateCommand(QuickStartExecuted);
            AboutCommand = new DelegateCommand(AboutCommandExecuted);
            _buildClientCommand =
                new DelegateCommand(BuildClientCommandExecuted, BuildClientCommandCanExecute);
            _runProjectCommand = new DelegateCommand(RunProjectCommandExecuted, RunProjectCommandCanExecute);
            InstallServiceCommand = new DelegateCommand(InstallServiceCommandExecuted);
            _uninstallServiceCommand = new DelegateCommand(UninstallServiceCommandExecuted);

            ReloadModuleCommand = new DelegateCommand(ReloadModuleCommandExecuted);
            SaveModuleCommand = new DelegateCommand(SaveModuleCommandExecuted);
            CloseModuleCommand = new DelegateCommand(CloseModuleCommandExecuted);
            AddModuleCommand = new DelegateCommand(AddModuleCommandExecuted);
            EditModuleCommand = new DelegateCommand(EditModuleCommandExecuted);
            DeleteModuleCommand = new DelegateCommand(DeleteModuleCommandExecuted);
            CopyModuleCommand = new DelegateCommand(CopyModuleCommandExecuted);
            PasteModuleCommand = new DelegateCommand(PasteModuleCommandExecuted);
            UnselectModuleCommand = new DelegateCommand(UnselectModuleCommandExecuted);
        }

        public INpgConnectionParameters ConnectionParameters {
            get { return _connectionParameters; }
            set {
                var modules = _modules.OfType<IModule>().Where(m => m.HasChanges).ToArray();
                if (modules.Any()) {
                    var result = _service.DialogHelper.Message(
                        string.Format(
                            "{0}{1}{2}", Res.ThereAreChangesMessage, Environment.NewLine,
                            Res.SaveChangesQuestion),
                        Title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question, View);
                    if (result == true) {
                        if (modules.Any(m => !m.IsValid)) {
                            _service.DialogHelper.Message(
                                Res.ValidationErrorMessage, Res.ValidationError, MessageBoxButton.OK,
                                MessageBoxImage.Error, View);
                            return;
                        }

                        foreach (IModule module in modules) module.Save();
                    }

                    if (result == null) return;
                }

                _connectionParameters = value;
                _modules.Clear();
                OnPropertyChanged(this, v => v.ConnectionParameters);
                _openModuleCommand.RaiseCanExecuteChanged();
                _closeCommand.RaiseCanExecuteChanged();
                _exportCommand.RaiseCanExecuteChanged();
                _buildClientCommand.RaiseCanExecuteChanged();
                _runProjectCommand.RaiseCanExecuteChanged();
                OnPropertyChanged(this, v => v.Title);
            }
        }

        public string Title {
            get {
                return string.Format(
                    "{0}{1}",
                    ConnectionParameters == null ? string.Empty : ConnectionParameters.Database + " - ",
                    PHmiConstants.PHmiConfiguratorName);
            }
        }

        #region Modules

        public ReadOnlyObservableCollection<object> Modules { get; }

        public void OpenModule<T>() {
            Type type = typeof(T);
            OpenModule(type);
        }

        private void OpenModule(Type type) {
            object present = _modules.FirstOrDefault(m => m.GetType() == type);
            if (present != null) {
                TryFocus(present);
                return;
            }

            ConstructorInfo constructor = type.GetConstructor(new Type[0]);
            if (constructor == null)
                throw new ArgumentException("type must have a parameterless constructor");
            object item = constructor.Invoke(new object[0]);
            _modules.Add(item);
            var module = item as IModule;
            if (module != null) {
                module.ConnectionString = ConnectionParameters.ConnectionString;
                module.Closed += ModuleClosed;
            }

            TryFocus(item);
        }

        private static void TryFocus(object module) {
            var inputElement = module as IInputElement;
            if (inputElement != null)
                inputElement.Focus();
        }

        private void ModuleClosed(object sender, EventArgs e) {
            var module = (IModule) sender;
            module.Closed -= ModuleClosed;
            _modules.Remove(sender);
        }

        #region OpenModuleCommand

        public ICommand OpenModuleCommand {
            get { return _openModuleCommand; }
        }

        private bool OpenModuleCommandCanExecute(object obj) {
            return ConnectionParameters != null;
        }

        private void OpenModuleCommandExecuted(object parameter) {
            if (parameter == null)
                throw new ArgumentNullException("parameter");
            var type = parameter as Type;
            if (type == null)
                throw new ArgumentException("parameter must be a Type");
            OpenModule(type);
        }

        #endregion OpenModuleCommand

        public bool HasChanges {
            get { return _modules.OfType<IModule>().Any(m => m.HasChanges); }
        }

        public bool IsValid {
            get { return _modules.OfType<IModule>().All(m => m.IsValid); }
        }

        public void Save() {
            foreach (IModule module in _modules.OfType<IModule>()) module.Save();
        }

        #endregion Modules

        #region ExitCommand

        public ICommand ExitCommand { get; }

        private void ExitCommandExecuted(object parameter) {
            View.Close();
        }

        #endregion ExitCommand

        #region NewProjectCommand

        public ICommand NewProjectCommand { get; }

        private void NewProjectCommandExecuted(object obj) {
            var dialog = new NewProjectDialog();
            if (dialog.ShowDialog() == true) ConnectionParameters = dialog.ViewModel.ConnectionParameters;
        }

        #endregion NewProjectCommand

        #region OpenCommand

        public ICommand OpenCommand { get; }

        private void OpenCommandExecuted(object obj) {
            var dialog = new OpenProjectDialog();
            if (dialog.ShowDialog() == true) ConnectionParameters = dialog.ViewModel.ConnectionParameters;
        }

        #endregion OpenCommand

        #region CloseCommand

        public ICommand CloseCommand {
            get { return _closeCommand; }
        }

        private bool CloseCommandCanExecute(object obj) {
            return ConnectionParameters != null;
        }

        private void CloseCommandExecuted(object obj) {
            ConnectionParameters = null;
        }

        #endregion CloseCommand

        #region ExportCommand

        public ICommand ExportCommand {
            get { return _exportCommand; }
        }

        private bool ExportCommandCanExecute(object obj) {
            return ConnectionParameters != null;
        }

        private void ExportCommandExecuted(object obj) {
            var w = new ExportProjectDialog(ConnectionParameters);
            w.ShowDialog();
        }

        #endregion ExportCommand

        #region ImportCommand

        public ICommand ImportCommand { get; }

        private void ImportCommandExecuted(object obj) {
            var dialog = new ImportProjectDialog();
            if (dialog.ShowDialog() == true) ConnectionParameters = dialog.ViewModel.ConnectionParameters;
        }

        #endregion ImportCommand

        #region QuickStartCommand

        public ICommand QuickStartCommand { get; }

        private void QuickStartExecuted(object obj) {
            try {
                string assemblyLocation = Assembly.GetAssembly(typeof(MainWindowViewModel)).Location;
                string dirPath = Path.GetDirectoryName(assemblyLocation);
                if (dirPath == null)
                    return;
                string filePath = Path.Combine(dirPath, "PHmi Quick start guide.pdf");
                Process.Start(filePath);
            } catch (Exception exception) {
                ExceptionDialog.Show(exception, View);
            }
        }

        #endregion QuickStartCommand

        #region AboutCommand

        public ICommand AboutCommand { get; }

        private void AboutCommandExecuted(object obj) {
            var w = new AboutDialog {Owner = View as Window};
            w.ShowDialog();
        }

        #endregion AboutCommand

        #region BuildClientCommand

        public ICommand BuildClientCommand {
            get { return _buildClientCommand; }
        }

        private void BuildClientCommandExecuted(object obj) {
            var w = new BuildClient {
                ConnectionString = ConnectionParameters.ConnectionString,
                Owner = View as Window
            };
            w.ShowDialog();
        }

        private bool BuildClientCommandCanExecute(object obj) {
            return ConnectionParameters != null;
        }

        #endregion BuildClientCommand

        #region RunProjectCommand

        public ICommand RunProjectCommand {
            get { return _runProjectCommand; }
        }

        private bool RunProjectCommandCanExecute(object obj) {
            return ConnectionParameters != null;
        }

        private void RunProjectCommandExecuted(object obj) {
            try {
                var proc = new ProcessStartInfo {
                    FileName = string.Format("{0}.exe", PHmiConstants.PHmiRunnerName),
                    Arguments = ConnectionParameters.ConnectionString
                };
                Process.Start(proc);
            } catch (Win32Exception) { } catch (Exception exception) {
                _service.DialogHelper.Exception(exception, View);
            }
        }

        #endregion RunProjectCommand

        #region InstallServiceCommand

        public ICommand InstallServiceCommand { get; }

        private void InstallServiceCommandExecuted(object obj) {
            var w = new InstallService {
                ProjectConnectionString =
                    ConnectionParameters == null ? null : ConnectionParameters.ConnectionString,
                Owner = View as Window
            };
            w.ShowDialog();
        }

        #endregion InstallServiceCommand

        #region UninstallServiceCommand

        public ICommand UninstallServiceCommand {
            get { return _uninstallServiceCommand; }
        }

        private void UninstallServiceCommandExecuted(object obj) {
            try {
                var proc = new ProcessStartInfo {
                    FileName = string.Format("{0}.exe", PHmiConstants.PHmiServiceName),
                    Arguments = "--u "
                };
                Process.Start(proc);
            } catch (Win32Exception) { } catch (Exception exception) {
                _service.DialogHelper.Exception(exception, View);
            }
        }

        #endregion UninstallServiceCommand

        #region SelectedModuleViewModel

        public IModuleViewModel SelectedModuleViewModel {
            get { return _selectedModuleViewModel; }
            set {
                _selectedModuleViewModel = value;
                OnPropertyChanged(this, v => v.SelectedModuleViewModel);
            }
        }

        #region ReloadModuleCommand

        public ICommand ReloadModuleCommand { get; }

        private void ReloadModuleCommandExecuted(object obj) {
            if (SelectedModuleViewModel != null && SelectedModuleViewModel.ReloadCommand.CanExecute(null))
                SelectedModuleViewModel.ReloadCommand.Execute(null);
        }

        #endregion ReloadModuleCommand

        #region SaveModuleCommand

        public ICommand SaveModuleCommand { get; }

        private void SaveModuleCommandExecuted(object obj) {
            if (SelectedModuleViewModel != null && SelectedModuleViewModel.SaveCommand.CanExecute(null))
                SelectedModuleViewModel.SaveCommand.Execute(null);
        }

        #endregion SaveModuleCommand

        #region CloseModuleCommand

        public ICommand CloseModuleCommand { get; }

        private void CloseModuleCommandExecuted(object obj) {
            if (SelectedModuleViewModel != null && SelectedModuleViewModel.CloseCommand.CanExecute(null))
                SelectedModuleViewModel.CloseCommand.Execute(null);
        }

        #endregion CloseModuleCommand

        #region AddModuleCommand

        public ICommand AddModuleCommand { get; }

        private void AddModuleCommandExecuted(object obj) {
            var collectionViewModel = SelectedModuleViewModel as ICollectionViewModel;
            if (collectionViewModel != null && collectionViewModel.AddCommand.CanExecute(null))
                collectionViewModel.AddCommand.Execute(null);
        }

        #endregion AddModuleCommand

        #region EditModuleCommand

        public ICommand EditModuleCommand { get; }

        private void EditModuleCommandExecuted(object obj) {
            var collectionViewModel = SelectedModuleViewModel as ICollectionViewModel;
            if (collectionViewModel != null && collectionViewModel.EditCommand.CanExecute(null))
                collectionViewModel.EditCommand.Execute(null);
        }

        #endregion EditModuleCommand

        #region DeleteModuleCommand

        public ICommand DeleteModuleCommand { get; }

        private void DeleteModuleCommandExecuted(object obj) {
            var collectionViewModel = SelectedModuleViewModel as ICollectionViewModel;
            if (collectionViewModel != null && collectionViewModel.DeleteCommand.CanExecute(null))
                collectionViewModel.DeleteCommand.Execute(null);
        }

        #endregion DeleteModuleCommand

        #region CopyModuleCommand

        public ICommand CopyModuleCommand { get; }

        private void CopyModuleCommandExecuted(object obj) {
            var collectionViewModel = SelectedModuleViewModel as ICollectionViewModel;
            if (collectionViewModel != null && collectionViewModel.CopyCommand.CanExecute(null))
                collectionViewModel.CopyCommand.Execute(null);
        }

        #endregion CopyModuleCommand

        #region PasteModuleCommand

        public ICommand PasteModuleCommand { get; }

        private void PasteModuleCommandExecuted(object obj) {
            var collectionViewModel = SelectedModuleViewModel as ICollectionViewModel;
            if (collectionViewModel != null && collectionViewModel.PasteCommand.CanExecute(null))
                collectionViewModel.PasteCommand.Execute(null);
        }

        #endregion PasteModuleCommand

        #region UnselectModuleCommand

        public ICommand UnselectModuleCommand { get; }

        private void UnselectModuleCommandExecuted(object obj) {
            var collectionViewModel = SelectedModuleViewModel as ICollectionViewModel;
            if (collectionViewModel != null && collectionViewModel.UnselectCommand.CanExecute(null))
                collectionViewModel.UnselectCommand.Execute(null);
        }

        #endregion UnselectModuleCommand

        #endregion SelectedModuleViewModel
    }
}
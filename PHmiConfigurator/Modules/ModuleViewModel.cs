using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Input;
using PHmiClient.Controls.Input;
using PHmiClient.Utils;
using PHmiModel.Interfaces;
using PHmiResources.Loc;
using PHmiTools.ViewModels;

namespace PHmiConfigurator.Modules {
    public abstract class ModuleViewModel : ViewModelBase<Module>, IModuleViewModel {
        private readonly IModuleService _service;

        protected ModuleViewModel(IModuleService service) {
            _service = service ?? new ModuleService();
            CloseCommand = new DelegateCommand(CloseCommandExecuted);
            _saveCommand = new DelegateCommand(SaveCommandExecuted, SaveCommandCanExecute);
            ReloadCommand = new DelegateCommand(ReloadCommandExecuted);
        }

        public abstract string Name { get; }
        public abstract string Error { get; }

        public virtual bool IsValid {
            get { return string.IsNullOrEmpty(Error); }
        }

        public string ConnectionString { get; set; }

        protected override void OnPropertyChanged<TDescender>(
            TDescender obj, Expression<Func<TDescender, object>> expression) {
            base.OnPropertyChanged(obj, expression);
            if (PropertyHelper.GetPropertyName(expression) ==
                PropertyHelper.GetPropertyName(this, v => v.HasChanges))
                _saveCommand.RaiseCanExecuteChanged();
        }

        #region Context

        private IModelContext _context;

        public IModelContext Context {
            get { return _context; }
            private set {
                if (_context != null)
                    _context.PropertyChanged -= ContextPropertyChanged;
                _context = value;
                _context.PropertyChanged += ContextPropertyChanged;
                OnPropertyChanged(this, v => v.Context);
            }
        }

        private void ContextPropertyChanged(object sender, PropertyChangedEventArgs args) {
            if (args.PropertyName == PropertyHelper.GetPropertyName<IModelContext>(c => c.HasChanges))
                OnPropertyChanged(this, v => v.HasChanges);
        }

        public bool HasChanges {
            get { return _context != null && _context.HasChanges; }
        }

        #endregion Context

        #region Save

        public bool Save() {
            try {
                if (!IsValid) {
                    string errorMessage = string.IsNullOrEmpty(Error) ? Res.ValidationErrorMessage : Error;
                    _service.DialogHelper.Message(errorMessage, Res.ValidationError, MessageBoxButton.OK,
                        MessageBoxImage.Error, View);
                    return false;
                }

                _context.Save();
                return true;
            } catch (Exception exception) {
                _service.DialogHelper.Exception(exception, View);
                return false;
            }
        }

        private readonly DelegateCommand _saveCommand;

        public ICommand SaveCommand {
            get { return _saveCommand; }
        }

        private bool SaveCommandCanExecute(object obj) {
            return HasChanges;
        }

        private void SaveCommandExecuted(object obj) {
            Save();
        }

        #endregion Save

        #region Reload

        public bool Reload() {
            try {
                if (HasChanges)
                    if (_service.DialogHelper.Message(
                            string.Format("{0}{1}{2}", Res.HasChangesMessage, Environment.NewLine,
                                Res.ContinueQuestion),
                            Name, MessageBoxButton.YesNo, MessageBoxImage.Question, View) != true)
                        return false;
                if (Context != null)
                    Context.Dispose();
                Context = _service.ContextFactory.Create(ConnectionString, true);
                PostReloadAction();
                OnPropertyChanged(this, v => v.HasChanges);
                return true;
            } catch (Exception exception) {
                _service.DialogHelper.Exception(exception, View);
                RaiseClosed();
                return false;
            }
        }

        protected abstract void PostReloadAction();

        public ICommand ReloadCommand { get; }

        private void ReloadCommandExecuted(object obj) {
            Reload();
        }

        #endregion Reload

        #region Close

        public event EventHandler Closed;

        protected void RaiseClosed() {
            if (Context != null)
                Context.Dispose();
            EventHelper.Raise(ref Closed, this, EventArgs.Empty);
        }

        public ICommand CloseCommand { get; }

        private void CloseCommandExecuted(object obj) {
            if (HasChanges) {
                var result = _service.DialogHelper.Message(
                    string.Format("{0}{1}{2}", Res.HasChangesMessage, Environment.NewLine,
                        Res.SaveChangesQuestion),
                    Name, MessageBoxButton.YesNoCancel, MessageBoxImage.Question, View);
                if (result == true) {
                    if (Save())
                        RaiseClosed();
                    return;
                }

                if (result == null)
                    return;
            }

            RaiseClosed();
        }

        #endregion Close

        #region Progress

        private bool _inProgress;

        public bool InProgress {
            get { return _inProgress; }
            protected set {
                _inProgress = value;
                OnPropertyChanged(this, v => v.InProgress);
            }
        }

        private int _progressMax;

        public int ProgressMax {
            get { return _progressMax; }
            protected set {
                _progressMax = value;
                OnPropertyChanged(this, v => v.ProgressMax);
            }
        }

        private int _progress;

        public int Progress {
            get { return _progress; }
            protected set {
                _progress = value;
                OnPropertyChanged(this, v => v.Progress);
            }
        }

        private bool _progressIsIndeterminate;

        public bool ProgressIsIndeterminate {
            get { return _progressIsIndeterminate; }
            protected set {
                _progressIsIndeterminate = value;
                OnPropertyChanged(this, v => v.ProgressIsIndeterminate);
            }
        }

        #endregion Progress
    }
}
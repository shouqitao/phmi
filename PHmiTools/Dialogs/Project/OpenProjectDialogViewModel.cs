using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Npgsql;
using PHmiClient.Controls.Input;
using PHmiClient.Utils;
using PHmiResources.Loc;
using PHmiTools.Utils.Npg;

namespace PHmiTools.Dialogs.Project {
    public class OpenProjectDialogViewModel : ProjectDialogViewModel {
        private readonly ObservableCollection<string> _databases = new ObservableCollection<string>();

        private readonly IOpenProjectDialogService _service;

        public OpenProjectDialogViewModel() : this(null) { }

        public OpenProjectDialogViewModel(IOpenProjectDialogService service) : base(service) {
            _service = service ?? new OpenProjectDialogService();
            LoadDatabasesCommand = new DelegateCommand(LoadDatabasesCommandExecuted);
            Databases = new ReadOnlyObservableCollection<string>(_databases);
            ConnectionParameters.PropertyChanged += ConnectionParametersPropertyChanged;
        }

        protected override void OkCommandExecuted(object obj) {
            InProgress = true;
            ProgressIsIndeterminate = true;
            _service.ActionHelper.Async(() => {
                try {
                    if (_service.DatabaseHelper.IsPHmiDatabase(ConnectionParameters))
                        _service.ActionHelper.Dispatch(() => base.OkCommandExecuted(obj));
                    else
                        MessageDialog.Show(
                            string.Format(Res.NotPHmiDatabaseMessage, ConnectionParameters.Database),
                            Res.Error, owner: View);
                } catch (Exception exception) {
                    ExceptionDialog.Show(exception, View);
                } finally {
                    _service.ActionHelper.Dispatch(() => {
                        InProgress = false;
                        ProgressIsIndeterminate = false;
                    });
                }
            });
        }

        private void ConnectionParametersPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName != PropertyHelper.GetPropertyName<INpgConnectionParameters>(p => p.Database))
                _databases.Clear();
        }

        #region Databases

        public ReadOnlyObservableCollection<string> Databases { get; }

        public ICommand LoadDatabasesCommand { get; }

        private void LoadDatabasesCommandExecuted(object obj) {
            InProgress = true;
            ProgressIsIndeterminate = true;
            _databases.Clear();
            _service.ActionHelper.Async(() => {
                try {
                    var databases = _service.NpgHelper.GetDatabases(ConnectionParameters);
                    _service.ActionHelper.Dispatch(() => {
                        ProgressIsIndeterminate = false;
                        ProgressMax = databases.Length;
                        Progress = 0;
                    });
                    for (var i = 0; i < databases.Length; i++) {
                        try {
                            if (_service.DatabaseHelper.IsPHmiDatabase(
                                ConnectionParameters.ConnectionStringWithoutDatabase, databases[i])) {
                                int i1 = i;
                                _service.ActionHelper.Dispatch(() => _databases.Add(databases[i1]));
                            }
                        } catch (NpgsqlException) { }

                        int progress = i;
                        _service.ActionHelper.Dispatch(() => { Progress = progress; });
                    }
                } catch (Exception exception) {
                    ExceptionDialog.Show(exception, View);
                } finally {
                    _service.ActionHelper.Dispatch(() => {
                        InProgress = false;
                        ProgressIsIndeterminate = false;
                    });
                }
            });
        }

        #endregion Databases
    }
}
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using PHmiClient.Controls.Input;
using PHmiClient.Utils;
using PHmiConfigurator.Dialogs;
using PHmiModel.Interfaces;
using PHmiResources.Loc;

namespace PHmiConfigurator.Modules.Collection {
    public abstract class CollectionViewModel<T, TMeta> : ModuleViewModel, ICollectionViewModel
        where T : class, IDataErrorInfo, INotifyPropertyChanged, INamedEntity, new()
        where TMeta : class, IDataErrorInfo, new() {
        private readonly ICollectionService _service;

        protected readonly ObservableCollection<T> List = new ObservableCollection<T>();

        protected CollectionViewModel(ICollectionService service) : base(service) {
            _service = service ?? new CollectionService();
            Collection = new ReadOnlyObservableCollection<T>(List);
            _addCommand = new DelegateCommand(AddCommandExecuted, AddCommandCanExecute);
            _editCommand = new DelegateCommand(EditCommandExecuted, EditCommandCanExecute);
            _deleteCommand = new DelegateCommand(DeleteCommandExecuted, DeleteCommandCanExecute);
            _copyCommand = new DelegateCommand(CopyCommandExecuted, CopyCommandCanExecute);
            PasteCommand = new DelegateCommand(PasteCommandExecuted);
            _unselectCommand = new DelegateCommand(UnselectCommandExecuted, UnselectCommandCanExecute);
            SelectedItems.CollectionChanged += SelectedItemsCollectionChanged;
        }

        public override string Error {
            get {
                var names = List.GroupBy(i => i.Name).Where(g => g.Count() > 1)
                    .Select(g => "\"" + g.Key + "\"").ToArray();
                string error = string.Empty;
                if (names.Any())
                    error =
                        string.Format(Res.UniqueErrorMessage, ReflectionHelper.GetDisplayName<T>(t => t.Name))
                        + Environment.NewLine
                        + string.Join(", ", names) + ".";
                return error;
            }
        }

        public ReadOnlyObservableCollection<T> Collection { get; }

        public override bool IsValid {
            get { return base.IsValid && List.All(d => string.IsNullOrEmpty(d.Error)); }
        }

        protected override void PostReloadAction() {
            List.Clear();
            var result = Context.Get<T>().OrderBy(i => i.Id).ToArray();
            foreach (T i in result) List.Add(i);
        }

        protected abstract IEditDialog<TMeta> CreateAddDialog();

        protected abstract IEditDialog<TMeta> CreateEditDialog();

        #region SelectedItem

        private T _selectedItem;

        public T SelectedItem {
            get { return _selectedItem; }
            set {
                _selectedItem = value;
                OnPropertyChanged(this, v => v.SelectedItem);
                _editCommand.RaiseCanExecuteChanged();
                _unselectCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion SelectedItem

        #region SelectedItems

        public ObservableCollection<T> SelectedItems { get; } = new ObservableCollection<T>();

        private void SelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            _deleteCommand.RaiseCanExecuteChanged();
            _copyCommand.RaiseCanExecuteChanged();
        }

        #endregion SelectedItems

        #region AddCommand

        private readonly DelegateCommand _addCommand;

        protected void RaiseAddCommandCanExecuteChanged() {
            _addCommand.RaiseCanExecuteChanged();
        }

        public ICommand AddCommand {
            get { return _addCommand; }
        }

        protected virtual bool AddCommandCanExecute(object obj) {
            return true;
        }

        private void AddCommandExecuted(object obj) {
            var dialog = CreateAddDialog();
            var entity = new T();
            object meta = _service.EditorHelper.Clone(entity);
            dialog.Entity = (TMeta) meta;
            if (dialog.ShowDialog() == true) {
                _service.EditorHelper.Update(meta, entity);
                List.Add(entity);
                OnBeforeAddedToContext(entity);
                Context.AddTo(entity);
                SelectedItem = entity;
            }
        }

        protected virtual void OnBeforeAddedToContext(T entity) { }

        #endregion AddCommand

        #region EditCommand

        private readonly DelegateCommand _editCommand;

        public ICommand EditCommand {
            get { return _editCommand; }
        }

        private bool EditCommandCanExecute(object obj) {
            return SelectedItem != null;
        }

        private void EditCommandExecuted(object obj) {
            var dialog = CreateEditDialog();
            var meta = (TMeta) _service.EditorHelper.Clone(SelectedItem);
            dialog.Entity = meta;
            if (dialog.ShowDialog() == true) _service.EditorHelper.Update(meta, SelectedItem);
        }

        #endregion EditCommand

        #region DeleteCommand

        private readonly DelegateCommand _deleteCommand;

        public ICommand DeleteCommand {
            get { return _deleteCommand; }
        }

        private bool DeleteCommandCanExecute(object obj) {
            return SelectedItems.Any();
        }

        private void DeleteCommandExecuted(object obj) {
            if (_service.DialogHelper.Message(Res.DeleteRowsQuestion, Name, MessageBoxButton.YesNo,
                    MessageBoxImage.Question, View) != true)
                return;
            var entitiesToDelete = SelectedItems.ToArray();
            Action action = () => {
                int length = entitiesToDelete.Length;
                _service.ActionHelper.Dispatch(() => {
                    InProgress = true;
                    ProgressIsIndeterminate = false;
                    ProgressMax = length;
                    Progress = 0;
                });
                for (var i = 0; i < length; i++) {
                    T entity = entitiesToDelete[i];
                    int progress = i + 1;
                    _service.ActionHelper.Dispatch(() => {
                        List.Remove(entity);
                        Context.DeleteObject(entity);
                        Progress = progress;
                    });
                }

                _service.ActionHelper.Dispatch(() => { InProgress = false; });
            };
            _service.ActionHelper.Async(action);
        }

        #endregion DeleteCommand

        #region CopyCommand

        private readonly DelegateCommand _copyCommand;

        public ICommand CopyCommand {
            get { return _copyCommand; }
        }

        private bool CopyCommandCanExecute(object obj) {
            return SelectedItems.Any();
        }

        private void CopyCommandExecuted(object obj) {
            var selectedItems = SelectedItems.ToArray();
            string header = string.Join("\t",
                ReflectionHelper.GetDisplayName<T>(i => i.Id),
                ReflectionHelper.GetDisplayName<T>(i => i.Name),
                string.Join("\t", GetCopyHeaders()));
            var text = selectedItems
                .Select(i => string.Join("\t", i.Id + "\t" + i.Name, string.Join("\t", GetCopyData(i))))
                .ToArray();
            _service.ClipboardHelper.SetText(string.Join("\r\n", header, string.Join("\r\n", text)));
        }

        protected abstract string[] GetCopyData(T item);

        protected abstract string[] GetCopyHeaders();

        #endregion CopyCommand

        #region PasteCommand

        public ICommand PasteCommand { get; }

        private void PasteCommandExecuted(object obj) {
            InProgress = true;
            ProgressIsIndeterminate = true;
            string text = _service.ClipboardHelper.GetText();
            Action action = () => {
                try {
                    var rows = text.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
                    _service.ActionHelper.Dispatch(() => {
                        ProgressIsIndeterminate = false;
                        ProgressMax = rows.Length;
                        Progress = 0;
                    });
                    var items = List.ToDictionary(i => i.Id);
                    for (var index = 0; index < rows.Length; index++) {
                        string row = rows[index];
                        var columns = row.Split(new[] {"\t"}, StringSplitOptions.None);
                        if (columns.Length != GetCopyHeaders().Length + 2)
                            throw new Exception(Res.ColumnsCountNotMatchMessage);
                        int id;
                        T item;
                        var isNewItem = false;
                        if (int.TryParse(columns[0], out id)) {
                            if (!items.TryGetValue(id, out item))
                                throw new Exception(string.Format(Res.ItemWithIdNotFoundMessage, id));
                        } else if (string.IsNullOrEmpty(columns[0])) {
                            item = new T();
                            isNewItem = true;
                        } else if (columns[0] == ReflectionHelper.GetDisplayName<T>(i => i.Id)) {
                            continue;
                        } else {
                            throw new Exception(string.Format(Res.NotValidIdMessage, columns[0]));
                        }

                        int progress = index + 1;
                        Exception toThrow = null;
                        _service.ActionHelper.Dispatch(() => {
                            Progress = progress;
                            try {
                                item.Name = columns[1];
                                SetCopyData(item, columns.Skip(2).ToArray());
                                if (isNewItem) {
                                    OnBeforeAddedToContext(item);
                                    Context.AddTo(item);
                                    List.Add(item);
                                }
                            } catch (Exception exception) {
                                toThrow = exception;
                            }
                        });
                        if (toThrow != null)
                            throw new Exception(
                                string.Format(
                                    Res.PasteRowErrorMessage,
                                    ReflectionHelper.GetDisplayName<T>(t => t.Id),
                                    columns[0],
                                    ReflectionHelper.GetDisplayName<T>(t => t.Name),
                                    columns[1],
                                    toThrow.Message),
                                toThrow);
                    }
                } catch (Exception exception) {
                    _service.DialogHelper.Exception(exception, View);
                } finally {
                    _service.ActionHelper.Dispatch(() => {
                        InProgress = false;
                        ProgressIsIndeterminate = false;
                    });
                }
            };
            _service.ActionHelper.Async(action);
        }

        protected abstract void SetCopyData(T item, string[] data);

        #endregion PasteCommand

        #region UnselectCommand

        private readonly DelegateCommand _unselectCommand;

        public ICommand UnselectCommand {
            get { return _unselectCommand; }
        }

        private bool UnselectCommandCanExecute(object obj) {
            return SelectedItem != null;
        }

        private void UnselectCommandExecuted(object obj) {
            SelectedItem = null;
        }

        #endregion UnselectCommand
    }
}
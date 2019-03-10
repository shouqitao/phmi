using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using PHmiModel.Interfaces;
using PHmiResources.Loc;

namespace PHmiConfigurator.Modules.Collection.Selectable {
    public abstract class SelectableCollectionViewModel<T, TMeta, TSelector> : CollectionViewModel<T, TMeta>
        where T : class, IDataErrorInfo, INotifyPropertyChanged, INamedEntity, new()
        where TMeta : class, IDataErrorInfo, new()
        where TSelector : class, INamedEntity, IRepository {
        private readonly ISelectableCollectionService _service;

        protected readonly ObservableCollection<TSelector> SelectorsList =
            new ObservableCollection<TSelector>();

        private TSelector _currentSelector;

        protected SelectableCollectionViewModel(ISelectableCollectionService service) : base(service) {
            _service = service ?? new SelectableCollectionService();
            Selectors = new ReadOnlyObservableCollection<TSelector>(SelectorsList);
        }

        public ReadOnlyObservableCollection<TSelector> Selectors { get; }

        public TSelector CurrentSelector {
            get { return _currentSelector; }
            set {
                if (HasChanges) {
                    _service.DialogHelper.Message(Res.CantChangeSelectorMessage, Name, owner: View);
                    return;
                }

                _currentSelector = value;
                OnPropertyChanged(this, v => v.CurrentSelector);
                RaiseAddCommandCanExecuteChanged();
                try {
                    LoadCollection();
                } catch (Exception exception) {
                    _service.DialogHelper.Exception(exception, View);
                    RaiseClosed();
                }
            }
        }

        protected override void PostReloadAction() {
            TSelector oldSelector = CurrentSelector;
            var selectors = Context.Get<TSelector>().OrderBy(i => i.Name).ToArray();
            SelectorsList.Clear();
            foreach (TSelector i in selectors) SelectorsList.Add(i);
            CurrentSelector = oldSelector == null
                ? null
                : selectors.FirstOrDefault(i => i.Id == oldSelector.Id);
            if (CurrentSelector == null && oldSelector == null && selectors.Count() == 1)
                CurrentSelector = selectors.FirstOrDefault();
            LoadCollection();
        }

        private void LoadCollection() {
            List.Clear();
            TSelector selector = CurrentSelector;
            if (selector == null)
                return;
            foreach (T d in selector.GetRepository<T>().OrderBy(d => d.Id)) List.Add(d);
        }

        protected override void OnBeforeAddedToContext(T entity) {
            CurrentSelector.GetRepository<T>().Add(entity);
        }

        protected override bool AddCommandCanExecute(object obj) {
            return base.AddCommandCanExecute(obj) && CurrentSelector != null;
        }
    }
}
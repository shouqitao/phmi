using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Markup;

namespace PHmiClient.Utils.CollectionUniters {
    [ContentProperty("Collections")]
    public class CollectionUniter<T> : ReadOnlyObservableCollection<T> {
        private readonly List<IEnumerable<T>> _collectionList = new List<IEnumerable<T>>();

        private readonly Func<T, T, int> _comparer;

        public CollectionUniter(Func<T, T, int> comparer) : base(new ObservableCollection<T>()) {
            Collections.CollectionChanged += CollectionsChanged;
            _comparer = comparer;
        }

        public ObservableCollection<IEnumerable<T>> Collections { get; } =
            new ObservableCollection<IEnumerable<T>>();

        private void CollectionsChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (object newItem in e.NewItems) AddCollection(newItem);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (object oldItem in e.OldItems) RemoveCollection(oldItem);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (object oldItem in e.OldItems) RemoveCollection(oldItem);
                    foreach (object newItem in e.NewItems) AddCollection(newItem);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    ResetCollections();
                    break;
            }
        }

        private void AddCollection(object newCollection) {
            var enumerable = (IEnumerable<T>) newCollection;
            var collection = (INotifyCollectionChanged) enumerable;
            foreach (T i in enumerable) Insert(i);
            collection.CollectionChanged += OneOfCollectionsChanged;
            _collectionList.Add(enumerable);
        }

        private void RemoveCollection(object oldCollection) {
            var enumerable = (IEnumerable<T>) oldCollection;
            var collection = (INotifyCollectionChanged) enumerable;
            foreach (T i in enumerable) Remove(i);
            collection.CollectionChanged -= OneOfCollectionsChanged;
            _collectionList.Remove(enumerable);
        }

        private void ResetCollections() {
            foreach (var collection in _collectionList.ToArray()) RemoveCollection(collection);
        }

        private void OneOfCollectionsChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (object newItem in e.NewItems) Insert((T) newItem);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (object oldItem in e.OldItems) Remove((T) oldItem);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (object oldItem in e.OldItems) Remove((T) oldItem);
                    foreach (object newItem in e.NewItems) Insert((T) newItem);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    Reset();
                    break;
            }
        }

        private void Insert(T item) {
            int index = Items.BinarySearch(item, _comparer);
            if (index < 0)
                index = ~index;
            Items.Insert(index, item);
        }

        private void Remove(T item) {
            Items.Remove(item);
        }

        private void Reset() {
            var itemsToRemove = Items.ToList();
            foreach (T item in Collections.SelectMany(c => c)) itemsToRemove.Remove(item);
            foreach (T item in itemsToRemove) Items.Remove(item);
        }
    }
}
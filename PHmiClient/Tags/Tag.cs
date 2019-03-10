using System;
using System.ComponentModel;
using System.Threading;
using PHmiClient.Utils;

namespace PHmiClient.Tags {
    public abstract class Tag<T> : TagAbstract<T> {
        private readonly Func<string> _descriptionGetter;
        private readonly IDispatcherService _dispatcherService;
        private readonly object _writeLockObj = new object();
        private int _isRead;
        private bool _isWritten;
        private T _value;

        protected Tag(IDispatcherService dispatcherService, int id, string name,
            Func<string> descriptionGetter) {
            _dispatcherService = dispatcherService;
            Id = id;
            Name = name;
            _descriptionGetter = descriptionGetter;
        }

        internal override int Id { get; }

        public override string Name { get; }

        public override string Description {
            get { return _descriptionGetter.Invoke(); }
        }

        public override T Value {
            get {
                _isRead = 1;
                return _value;
            }
            set {
                lock (_writeLockObj) {
                    _value = value;
                    _isWritten = true;
                }

                OnPropertyChanged("Value");
            }
        }

        internal override bool IsWritten {
            get { return _isWritten; }
        }

        internal override bool IsRead {
            get { return Interlocked.Exchange(ref _isRead, 0) == 1; }
        }

        internal override void UpdateValue(T value) {
            lock (_writeLockObj) {
                if (!_isWritten)
                    _value = value;
            }

            _dispatcherService.Invoke(() => OnPropertyChanged("Value"));
        }

        internal override T GetWrittenValue() {
            lock (_writeLockObj) {
                _isWritten = false;
                return _value;
            }
        }

        public override event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string property) {
            EventHelper.Raise(ref PropertyChanged, this, new PropertyChangedEventArgs(property));
        }
    }
}
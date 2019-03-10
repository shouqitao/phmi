using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace PHmiClient.Utils.Notifications {
    public class NotificationReporter : INotificationReporter {
        private readonly IDispatcherService _dispatcherService = new DispatcherService();
        private readonly ISet<Notification> _expiredNotifications = new HashSet<Notification>();

        private readonly IDictionary<Key, NotificationWithLastTime> _notificationDic =
            new Dictionary<Key, NotificationWithLastTime>();

        private readonly ObservableCollection<Notification> _notificationList;
        private readonly ITimerService _timerService;
        private readonly UniqueTimeService _timeService;
        private bool _containsActiveNotifications;

        internal NotificationReporter(ITimeService timeService, ITimerService timerService) {
            _notificationList = new ObservableCollection<Notification>();
            Notifications = new ReadOnlyObservableCollection<Notification>(_notificationList);
            _timeService = new UniqueTimeService(timeService);
            _timerService = timerService;
            _timerService.TimeSpan = TimeSpan.FromSeconds(1);
            ExpirationTime = TimeSpan.FromSeconds(25);
            LifeTime = TimeSpan.FromDays(7);
            _timerService.Elapsed += TimerServiceElapsed;
            _timerService.Start();
        }

        public NotificationReporter(ITimeService timeService) : this(timeService, new TimerService()) { }

        public TimeSpan ExpirationTime { get; set; }

        public TimeSpan LifeTime { get; set; }

        public ReadOnlyObservableCollection<Notification> Notifications { get; }

        public void Report(string message, string shortDescription = null, string longDescription = null) {
            var key = new Key(message, shortDescription, longDescription);
            NotificationWithLastTime value = GetValue(key);
            DateTime currentTime = _timeService.UtcTime;
            if (value == null) {
                var notification = new Notification(currentTime, message, shortDescription, longDescription);
                value = new NotificationWithLastTime(notification, currentTime);
                AddValue(key, value);
            } else {
                value.LastTime = currentTime;
            }

            ContainsActiveNotifications = true;
        }

        public void Report(string message, Exception exception) {
            Report(message, exception.Message, exception.ToString());
        }

        public void Reset(Notification notification) {
            lock (_expiredNotifications) {
                if (_expiredNotifications.Contains(notification)) {
                    _expiredNotifications.Remove(notification);
                    _dispatcherService.Invoke(() => _notificationList.Remove(notification));
                }
            }
        }

        public void ResetAll() {
            foreach (Notification notification in _expiredNotifications.ToArray()) Reset(notification);
        }

        public bool ContainsActiveNotifications {
            get { return _containsActiveNotifications; }
            private set {
                if (_containsActiveNotifications == value)
                    return;
                _containsActiveNotifications = value;
                OnPropertyChanged("ContainsActiveNotifications");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void TimerServiceElapsed(object sender, EventArgs e) {
            DateTime now = _timeService.UtcTime;
            foreach (var item in _notificationDic.ToArray()
                .Where(item => now > item.Value.LastTime + ExpirationTime)) {
                item.Value.Notification.EndTime = item.Value.LastTime;
                _notificationDic.Remove(item);
                _expiredNotifications.Add(item.Value.Notification);
            }

            ContainsActiveNotifications = _notificationDic.Any();
            foreach (Notification msg in _expiredNotifications.ToArray()
                .Where(msg => now > msg.EndTime + LifeTime)) Reset(msg);
        }

        private NotificationWithLastTime GetValue(Key key) {
            NotificationWithLastTime tuple;
            return _notificationDic.TryGetValue(key, out tuple) ? tuple : null;
        }

        private void AddValue(Key key, NotificationWithLastTime item) {
            _notificationDic.Add(key, item);
            _dispatcherService.Invoke(() => _notificationList.Add(item.Notification));
        }

        protected virtual void OnPropertyChanged(string property) {
            EventHelper.Raise(ref PropertyChanged, this, new PropertyChangedEventArgs(property));
        }

        private class Key : Tuple<string, string, string> {
            public Key(string message, string shortDescription, string longDescription) : base(message,
                shortDescription, longDescription) { }
        }

        private class NotificationWithLastTime {
            public readonly Notification Notification;
            public DateTime LastTime;

            public NotificationWithLastTime(Notification notification, DateTime lastTime) {
                Notification = notification;
                LastTime = lastTime;
            }
        }
    }
}
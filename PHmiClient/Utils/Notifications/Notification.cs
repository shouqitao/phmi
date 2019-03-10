using System;
using System.ComponentModel;

namespace PHmiClient.Utils.Notifications {
    public class Notification : INotifyPropertyChanged {
        private DateTime? _endTime;

        internal Notification(DateTime startTime, string message, string shortDescription,
            string longDescription) {
            StartTime = startTime;
            Message = message;
            ShortDescription = shortDescription;
            LongDescription = longDescription ?? ShortDescription;
        }

        public DateTime StartTime { get; }

        public DateTime? EndTime {
            get { return _endTime; }
            internal set {
                _endTime = value;
                OnPropertyChanged(PropertyHelper.GetPropertyName(this, m => m.EndTime));
            }
        }

        public string Message { get; }

        public string ShortDescription { get; }

        public string LongDescription { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName) {
            EventHelper.Raise(ref PropertyChanged, this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
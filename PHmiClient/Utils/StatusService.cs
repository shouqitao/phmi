using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace PHmiClient.Utils {
    public class StatusService : IStatusService {
        private readonly ITimerService _timer;
        private string _message;

        internal StatusService(ITimerService timer) {
            _timer = timer;
            LifeTime = TimeSpan.FromSeconds(15);
            _timer.Elapsed += TimerElapsed;
        }

        public StatusService() : this(new TimerService()) { }

        public TimeSpan LifeTime {
            get { return _timer.TimeSpan; }
            set {
                _timer.TimeSpan = value;
                OnPropertyChanged(s => s.LifeTime);
            }
        }

        public string Message {
            get { return _message; }
            set {
                _message = value;
                _timer.Stop();
                _timer.Start();
                OnPropertyChanged(s => s.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void TimerElapsed(object sender, EventArgs e) {
            Message = null;
        }

        private void OnPropertyChanged(Expression<Func<StatusService, object>> getPropertyExpression) {
            EventHelper.Raise(ref PropertyChanged, this,
                new PropertyChangedEventArgs(PropertyHelper.GetPropertyName(getPropertyExpression)));
        }
    }
}
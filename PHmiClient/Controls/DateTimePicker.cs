using System;
using System.Windows;
using System.Windows.Controls;

namespace PHmiClient.Controls {
    public class DateTimePicker : Control {
        static DateTimePicker() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DateTimePicker),
                new FrameworkPropertyMetadata(typeof(DateTimePicker)));
        }

        private void Update(DateTime? newValue) {
            if (!newValue.HasValue) {
                Date = DateTimeDate = null;
                Hour = DateTimeHour = null;
                Minute = DateTimeMinute = null;
                Second = DateTimeSecond = null;
                Millisecond = DateTimeMillisecond = null;
                return;
            }

            DateTime dateTime = newValue.Value;

            Date = DateTimeDate = dateTime.Date;
            Hour = DateTimeHour = dateTime.Hour;
            Minute = DateTimeMinute = dateTime.Minute;
            Second = DateTimeSecond = dateTime.Second;
            Millisecond = DateTimeMillisecond = dateTime.Millisecond;
        }

        private void UpdateDate(DateTime? newDate) {
            if (!newDate.HasValue) {
                DateTime = null;
                return;
            }

            if (!DateTime.HasValue) {
                DateTime = newDate;
                return;
            }

            DateTime dateTime = DateTime.Value;
            DateTime = newDate + dateTime.TimeOfDay;
        }

        private void UpdateHour(int? newHour) {
            if (!newHour.HasValue) {
                DateTime = null;
                return;
            }

            int hour = newHour.Value;
            DateTime dateTime = DateTime.HasValue ? DateTime.Value : System.DateTime.Now;
            TimeSpan difference = TimeSpan.FromHours(hour - dateTime.Hour);
            DateTime = dateTime + difference;
        }

        private void UpdateMinute(int? newMinute) {
            if (!newMinute.HasValue) {
                DateTime = null;
                return;
            }

            int minute = newMinute.Value;
            DateTime dateTime = DateTime.HasValue ? DateTime.Value : System.DateTime.Now;
            TimeSpan difference = TimeSpan.FromMinutes(minute - dateTime.Minute);
            DateTime = dateTime + difference;
        }

        private void UpdateSecond(int? newSecond) {
            if (!newSecond.HasValue) {
                DateTime = null;
                return;
            }

            int second = newSecond.Value;
            DateTime dateTime = DateTime.HasValue ? DateTime.Value : System.DateTime.Now;
            TimeSpan difference = TimeSpan.FromSeconds(second - dateTime.Second);
            DateTime = dateTime + difference;
        }

        private void UpdateMillisecond(int? newMillisecond) {
            if (!newMillisecond.HasValue) {
                DateTime = null;
                return;
            }

            int millisecond = newMillisecond.Value;
            DateTime dateTime = DateTime.HasValue ? DateTime.Value : System.DateTime.Now;
            TimeSpan difference = TimeSpan.FromMilliseconds(millisecond - dateTime.Millisecond);
            DateTime = dateTime + difference;
        }

        #region DateTime

        public DateTime? DateTime {
            get { return (DateTime?) GetValue(DateTimeProperty); }
            set { SetValue(DateTimeProperty, value); }
        }

        public static readonly DependencyProperty DateTimeProperty =
            DependencyProperty.Register("DateTime", typeof(DateTime?), typeof(DateTimePicker),
                new PropertyMetadata(null, DateTimePropertyChanged));

        private static void
            DateTimePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var p = (DateTimePicker) d;
            p.Update(e.NewValue as DateTime?);
        }

        #endregion DateTime

        #region Date

        public DateTime? Date {
            get { return (DateTime?) GetValue(DateProperty); }
            set { SetValue(DateProperty, value); }
        }

        public static readonly DependencyProperty DateProperty =
            DependencyProperty.Register("Date", typeof(DateTime?), typeof(DateTimePicker),
                new PropertyMetadata(null, DatePropertyChanged));

        private static void DatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var p = (DateTimePicker) d;
            p.UpdateDate(e.NewValue as DateTime?);
        }

        #endregion Date

        #region DateTimeDate

        public DateTime? DateTimeDate {
            get { return (DateTime?) GetValue(DateTimeDateProperty); }
            set { SetValue(DateTimeDateProperty, value); }
        }

        public static readonly DependencyProperty DateTimeDateProperty =
            DependencyProperty.Register("DateTimeDate", typeof(DateTime?), typeof(DateTimePicker));

        #endregion DateTimeDate

        #region Hour

        public int? Hour {
            get { return (int?) GetValue(HourProperty); }
            set { SetValue(HourProperty, value); }
        }

        public static readonly DependencyProperty HourProperty =
            DependencyProperty.Register("Hour", typeof(int?), typeof(DateTimePicker),
                new PropertyMetadata(null, HourPropertyChanged));

        private static void HourPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var p = (DateTimePicker) d;
            var newHour = e.NewValue as int?;
            if (newHour.HasValue && (newHour.Value < 0 || newHour.Value > 23))
                p.Hour = e.OldValue as int?;
            else
                p.UpdateHour(e.NewValue as int?);
        }

        #endregion Hour

        #region DateTimeHour

        public int? DateTimeHour {
            get { return (int?) GetValue(DateTimeHourProperty); }
            set { SetValue(DateTimeHourProperty, value); }
        }

        public static readonly DependencyProperty DateTimeHourProperty =
            DependencyProperty.Register("DateTimeHour", typeof(int?), typeof(DateTimePicker));

        #endregion DateTimeHour

        #region Munute

        public int? Minute {
            get { return (int?) GetValue(MinuteProperty); }
            set { SetValue(MinuteProperty, value); }
        }

        public static readonly DependencyProperty MinuteProperty =
            DependencyProperty.Register("Minute", typeof(int?), typeof(DateTimePicker),
                new PropertyMetadata(null, MinutePropertyChanged));

        private static void MinutePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var p = (DateTimePicker) d;
            var newMinute = e.NewValue as int?;
            if (newMinute.HasValue && (newMinute < 0 || newMinute > 59))
                p.Minute = e.OldValue as int?;
            else
                p.UpdateMinute(newMinute);
        }

        #endregion Munute

        #region DateTimeMinute

        public int? DateTimeMinute {
            get { return (int?) GetValue(DateTimeMinuteProperty); }
            set { SetValue(DateTimeMinuteProperty, value); }
        }

        public static readonly DependencyProperty DateTimeMinuteProperty =
            DependencyProperty.Register("DateTimeMinute", typeof(int?), typeof(DateTimePicker));

        #endregion DateTimeMinute

        #region Second

        public int? Second {
            get { return (int?) GetValue(SecondProperty); }
            set { SetValue(SecondProperty, value); }
        }

        public static readonly DependencyProperty SecondProperty =
            DependencyProperty.Register("Second", typeof(int?), typeof(DateTimePicker),
                new PropertyMetadata(null, SecondPropertyChanged));

        private static void SecondPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var p = (DateTimePicker) d;
            var newSecond = e.NewValue as int?;
            if (newSecond.HasValue && (newSecond < 0 || newSecond > 59))
                p.Second = e.OldValue as int?;
            else
                p.UpdateSecond(newSecond);
        }

        #endregion Second

        #region DateTimeSecond

        public int? DateTimeSecond {
            get { return (int?) GetValue(DateTimeSecondProperty); }
            set { SetValue(DateTimeSecondProperty, value); }
        }

        public static readonly DependencyProperty DateTimeSecondProperty =
            DependencyProperty.Register("DateTimeSecond", typeof(int?), typeof(DateTimePicker));

        #endregion DateTimeSecond

        #region Millisecond

        public int? Millisecond {
            get { return (int?) GetValue(MillisecondProperty); }
            set { SetValue(MillisecondProperty, value); }
        }

        public static readonly DependencyProperty MillisecondProperty =
            DependencyProperty.Register("Millisecond", typeof(int?), typeof(DateTimePicker),
                new PropertyMetadata(null, MillisecondPropertyChanged));

        private static void MillisecondPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e) {
            var p = (DateTimePicker) d;
            var newMillisecond = e.NewValue as int?;
            if (newMillisecond.HasValue && (newMillisecond < 0 || newMillisecond > 999))
                p.Millisecond = e.OldValue as int?;
            else
                p.UpdateMillisecond(newMillisecond);
        }

        #endregion Millisecond

        #region DateTimeMillisecond

        public int? DateTimeMillisecond {
            get { return (int?) GetValue(DateTimeMillisecondProperty); }
            set { SetValue(DateTimeMillisecondProperty, value); }
        }

        public static readonly DependencyProperty DateTimeMillisecondProperty =
            DependencyProperty.Register("DateTimeMillisecond", typeof(int?), typeof(DateTimePicker));

        #endregion DateTimeMillisecond
    }
}
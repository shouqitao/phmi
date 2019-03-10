using System;
using System.Windows;
using System.Windows.Controls;
using PHmiIoDevice.Generic.Loc;
using PHmiIoDeviceTools;

namespace PHmiIoDevice.Generic {
    public class GenericOptionsEditor : TextBox, IOptionsEditor {
        public void SetOptions(string options) {
            IsReadOnly = true;
            BorderThickness = new Thickness(0);
            Text = Res.NoOptionsRequired;
            Loaded += OptionsEditorLoaded;
        }

        public string GetOptions() {
            return string.Empty;
        }

        public event EventHandler OptionsChanged;

        private void OptionsEditorLoaded(object sender, RoutedEventArgs e) {
            EventHandler changed = OptionsChanged;
            if (changed != null) changed(this, EventArgs.Empty);
        }
    }
}
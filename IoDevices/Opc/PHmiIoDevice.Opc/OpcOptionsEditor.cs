﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Opc;
using OpcCom;
using PHmiIoDeviceTools;

namespace PHmiIoDevice.Opc {
    public class OpcOptionsEditor : ComboBox, IOptionsEditor {
        public OpcOptionsEditor() {
            VerticalAlignment = VerticalAlignment.Top;
            IsEditable = true;
            AddHandler(
                TextBoxBase.TextChangedEvent,
                new TextChangedEventHandler(ComboBox_TextChanged));
            Loaded += OpcOptionsEditor_Loaded;
        }

        public void SetOptions(string options) {
            if (string.IsNullOrEmpty(options) || !options.StartsWith("opcda://"))
                Text = "opcda://localhost/OpcServer";
            else
                Text = options;
        }

        public string GetOptions() {
            return Text;
        }

        public event EventHandler OptionsChanged;

        private void OpcOptionsEditor_Loaded(object sender, RoutedEventArgs e) {
            Dispatcher dispatcher = Dispatcher;
            var action = new Action(() => {
                try {
                    IDiscovery discovery = new ServerEnumerator();
                    var localservers =
                        discovery.GetAvailableServers(Specification.COM_DA_20)
                            .Select(s => s.Url)
                            .ToArray();
                    dispatcher.Invoke(new Action(() => { ItemsSource = localservers; }));
                } catch (Exception exception) {
                    MessageBox.Show(exception.Message);
                }
            });
            action.BeginInvoke(action.EndInvoke, null);
        }

        private void ComboBox_TextChanged(object sender, TextChangedEventArgs e) {
            EventHandler changed = OptionsChanged;
            if (changed != null) changed(this, EventArgs.Empty);
        }
    }
}
using System;
using System.ComponentModel;
using System.Windows;
using PHmiClient.Utils;
using PHmiClient.Utils.Configuration;
using PHmiIoDeviceTools;
using PHmiModel.Entities;
using PHmiTools.Controls;
using Settings = PHmiClient.Utils.Configuration.Settings;

namespace PHmiConfigurator.Dialogs {
    /// <summary>
    ///     Interaction logic for EditIoDevice.xaml
    /// </summary>
    public partial class EditIoDevice : IEditDialog<IoDevice.IoDeviceMetadata> {
        private const string EditIoDeviceWindowTypeEditor = "EditIoDeviceWindowTypeEditor";

        private const string RowHeight = "RowHeight";

        public EditIoDevice() {
            this.UpdateLanguage();
            InitializeComponent();
            WindowPositionSettings.UseWith(this, "EditIoDeviceWindowPosition");
            ViewModel.View = this;
            tbName.Focus();
            UpdateTypeEditorRowHeight();
            Closing += EditIoDeviceClosing;
        }

        public EditIoDeviceViewModel ViewModel {
            get { return (EditIoDeviceViewModel) Resources["ViewModel"]; }
        }

        public IoDevice.IoDeviceMetadata Entity {
            get { return ViewModel.Entity; }
            set { ViewModel.Entity = value; }
        }

        private void EditIoDeviceClosing(object sender, CancelEventArgs e) {
            SaveTypeEditorRowHeight();
        }

        private void IoDeviceTypeEditorSelectedItemChanged(object sender, EventArgs e) {
            var oldEditor = svOptionsEditorHolder.Content as IOptionsEditor;
            if (oldEditor != null) oldEditor.OptionsChanged -= OptionsEditorOptionsChanged;
            var editor = (IoDeviceTypeEditor) sender;
            IOptionsEditor optionsEditor = editor.CreateOptionsEditor();
            if (optionsEditor == null) {
                tbOptions.Visibility = Visibility.Visible;
                svOptionsEditorHolder.Content = null;
                svOptionsEditorHolder.Visibility = Visibility.Collapsed;
            } else {
                tbOptions.Visibility = Visibility.Collapsed;
                svOptionsEditorHolder.Content = optionsEditor;
                optionsEditor.OptionsChanged += OptionsEditorOptionsChanged;
                optionsEditor.SetOptions(Entity.Options);
                svOptionsEditorHolder.Visibility = Visibility.Visible;
            }
        }

        private void OptionsEditorOptionsChanged(object sender, EventArgs e) {
            var editor = (IOptionsEditor) sender;
            Entity.Options = editor.GetOptions();
        }

        private void UpdateTypeEditorRowHeight() {
            var settings = new Settings(EditIoDeviceWindowTypeEditor);
            var height = settings.GetDouble(RowHeight);
            if (height.HasValue)
                typeEditorRow.Height = new GridLength(height.Value);
        }

        private void SaveTypeEditorRowHeight() {
            var settings = new Settings(EditIoDeviceWindowTypeEditor);
            settings.SetDouble(RowHeight, typeEditorRow.ActualHeight);
            settings.Save();
        }
    }
}
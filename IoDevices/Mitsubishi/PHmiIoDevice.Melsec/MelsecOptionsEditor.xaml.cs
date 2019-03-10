using System;
using PHmiClient.Utils;
using PHmiIoDevice.Melsec.Configuration;
using PHmiIoDeviceTools;

namespace PHmiIoDevice.Melsec {
    /// <summary>
    ///     Interaction logic for MelsecOptionsEditor.xaml
    /// </summary>
    public partial class MelsecOptionsEditor : IOptionsEditor {
        public MelsecOptionsEditor() {
            InitializeComponent();
            ViewModel.ConfigChanged += (sender, args) => OnConfigChanged();
        }

        public MelsecOptionsEditorViewModel ViewModel { get; } = new MelsecOptionsEditorViewModel();

        public void SetOptions(string options) {
            try {
                ViewModel.Config = ConfigHelper.GetConfig(options);
            } catch {
                ViewModel.Config = new QConfig();
            }
        }

        public string GetOptions() {
            return ViewModel.Config.GetXml();
        }

        public event EventHandler OptionsChanged;

        private void OnConfigChanged() {
            EventHelper.Raise(ref OptionsChanged, this, EventArgs.Empty);
        }
    }
}
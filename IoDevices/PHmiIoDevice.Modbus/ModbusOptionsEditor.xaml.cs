using System;
using PHmiClient.Utils;
using PHmiIoDevice.Modbus.Configuration;
using PHmiIoDeviceTools;

namespace PHmiIoDevice.Modbus {
    /// <summary>
    ///     Interaction logic for ModbusOptionsEditor.xaml
    /// </summary>
    public partial class ModbusOptionsEditor : IOptionsEditor {
        public ModbusOptionsEditor() {
            InitializeComponent();
            ViewModel.ConfigChanged += (sender, args) => OnConfigChanged();
        }

        public ModbusOptionsEditorViewModel ViewModel { get; } = new ModbusOptionsEditorViewModel();

        public void SetOptions(string options) {
            try {
                ViewModel.Config = ConfigHelper.GetConfig(options);
            } catch {
                ViewModel.Config = new TcpConfig();
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
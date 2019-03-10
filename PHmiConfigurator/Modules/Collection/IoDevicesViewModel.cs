using System.Windows;
using PHmiClient.Utils;
using PHmiConfigurator.Dialogs;
using PHmiModel.Entities;
using PHmiResources.Loc;

namespace PHmiConfigurator.Modules.Collection {
    public class IoDevicesViewModel : CollectionViewModel<IoDevice, IoDevice.IoDeviceMetadata> {
        public IoDevicesViewModel() : base(null) { }

        public override string Name {
            get { return Res.IoDevices; }
        }

        protected override IEditDialog<IoDevice.IoDeviceMetadata> CreateAddDialog() {
            return new EditIoDevice {Title = Res.AddIoDevice, Owner = Window.GetWindow(View)};
        }

        protected override IEditDialog<IoDevice.IoDeviceMetadata> CreateEditDialog() {
            return new EditIoDevice {Title = Res.EditIoDevice, Owner = Window.GetWindow(View)};
        }

        protected override string[] GetCopyData(IoDevice item) {
            return new[] {
                item.Type,
                item.Options
            };
        }

        protected override string[] GetCopyHeaders() {
            return new[] {
                ReflectionHelper.GetDisplayName<IoDevice>(d => d.Type),
                ReflectionHelper.GetDisplayName<IoDevice>(d => d.Options)
            };
        }

        protected override void SetCopyData(IoDevice item, string[] data) {
            item.Type = data[0];
            item.Options = data[1];
        }
    }
}
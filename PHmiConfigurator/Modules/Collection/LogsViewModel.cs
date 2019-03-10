using System.Windows;
using PHmiClient.Utils;
using PHmiConfigurator.Dialogs;
using PHmiModel.Entities;
using PHmiResources.Loc;

namespace PHmiConfigurator.Modules.Collection {
    public class LogsViewModel : CollectionViewModel<Log, Log.LogMetadata> {
        public LogsViewModel()
            : base(null) { }

        public override string Name {
            get { return Res.Logs; }
        }

        protected override IEditDialog<Log.LogMetadata> CreateAddDialog() {
            return new EditLog {Title = Res.AddLog, Owner = Window.GetWindow(View)};
        }

        protected override IEditDialog<Log.LogMetadata> CreateEditDialog() {
            return new EditLog {Title = Res.EditLog, Owner = Window.GetWindow(View)};
        }

        protected override string[] GetCopyData(Log item) {
            return new[] {
                item.TimeToStore
            };
        }

        protected override string[] GetCopyHeaders() {
            return new[] {
                ReflectionHelper.GetDisplayName<Log>(a => a.TimeToStore)
            };
        }

        protected override void SetCopyData(Log item, string[] data) {
            item.TimeToStore = data[0];
        }
    }
}
using System.Linq;
using PHmiResources.Loc;

namespace PHmiConfigurator.Modules {
    public class SettingsViewModel : ModuleViewModel {
        private PHmiModel.Entities.Settings _settings;

        public SettingsViewModel() : base(null) { }

        public override string Name {
            get { return Res.Settings; }
        }

        public override string Error {
            get { return Settings == null ? string.Empty : Settings.Error; }
        }

        public PHmiModel.Entities.Settings Settings {
            get { return _settings; }
            set {
                _settings = value;
                OnPropertyChanged(this, v => v.Settings);
            }
        }

        protected override void PostReloadAction() {
            Settings = Context.Get<PHmiModel.Entities.Settings>().Single();
        }
    }
}
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using PHmiClient.Controls.Input;
using PHmiClient.Utils;
using PHmiClient.Utils.ViewInterfaces;
using PHmiConfigurator.Utils;
using PHmiModel;
using PHmiModel.Entities;
using PHmiModel.Interfaces;
using PHmiResources;
using PHmiResources.Loc;
using PHmiTools.Utils;
using PHmiTools.ViewModels;

namespace PHmiConfigurator.Dialogs {
    public class BuildClientViewModel : ViewModelBase<IWindow>, IDataErrorInfo {
        private const string ProjectBuilder = "ProjectBuilder";

        private const string FolderName = "Folder";

        private const string NameSpaceName = "NameSpace";
        private readonly IActionHelper _actionHelper;
        private readonly DelegateCommand _buildCommand;
        private readonly DelegateCommand _cancelCommand;
        private readonly DelegateCommand _chooseFolderCommand;
        private readonly ICodeWriterFactory _codeWriterFactory;
        private readonly IModelContextFactory _contextFactory;
        private readonly IDialogHelper _dialogHelper;
        private readonly IResourceBuilderFactory _resourceBuilderFactory;
        private bool _busy;
        private string _folder;
        private string _nameSpace;

        public BuildClientViewModel() : this(
            new PHmiModelContextFactory(),
            new DialogHelper(),
            new ActionHelper(),
            new CodeWriterFactory(),
            new ResourceBuilderFactory()) { }

        public BuildClientViewModel(
            IModelContextFactory contextFactory,
            IDialogHelper dialogHelper,
            IActionHelper actionHelper,
            ICodeWriterFactory codeWriterFactory,
            IResourceBuilderFactory resourceBuilderFactory) {
            _contextFactory = contextFactory;
            _dialogHelper = dialogHelper;
            _actionHelper = actionHelper;
            _codeWriterFactory = codeWriterFactory;
            _resourceBuilderFactory = resourceBuilderFactory;
            _buildCommand = new DelegateCommand(BuildExecuted, BuildCanExecute);
            _cancelCommand = new DelegateCommand(CancelExecuted);
            _chooseFolderCommand = new DelegateCommand(ChooseFolderExecuted, ChooseFolderCanExecute);
            LoadFolder();
        }

        public ICommand CancelCommand {
            get { return _cancelCommand; }
        }

        public string ConnectionString { get; set; }

        public string Folder {
            get { return _folder; }
            set {
                _folder = value;
                OnPropertyChanged(this, m => m.Folder);
            }
        }

        public ICommand ChooseFolderCommand {
            get { return _chooseFolderCommand; }
        }

        public string CodeFile {
            get {
                const string pHmi = "PHmi.cs";
                return string.IsNullOrEmpty(Folder) ? pHmi : Path.Combine(Folder, pHmi);
            }
        }

        public string ResFile {
            get {
                const string res = "PHmiResources.resx";
                return string.IsNullOrEmpty(Folder) ? res : Path.Combine(Folder, res);
            }
        }

        [LocDisplayName("NameSpace", ResourceType = typeof(Res))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
        [RegularExpression(RegexPatterns.NameSpace, ErrorMessageResourceName = "DotNetNameSpaceMessage",
            ErrorMessageResourceType = typeof(Res))]
        public string NameSpace {
            get { return _nameSpace; }
            set {
                _nameSpace = value;
                OnPropertyChanged(this, m => m.NameSpace);
            }
        }

        public bool Busy {
            get { return _busy; }
            set {
                _busy = value;
                OnPropertyChanged(this, m => m.Busy);
            }
        }

        public ICommand BuildCommand {
            get { return _buildCommand; }
        }

        public string this[string columnName] {
            get { return this.GetError(columnName); }
        }

        public string Error {
            get { return this.GetError(); }
        }

        private void CancelExecuted(object obj) {
            View.DialogResult = false;
        }

        private void ChooseFolderExecuted(object obj) {
            var dialog = new FolderBrowserDialog {SelectedPath = Folder};
            IWin32Window owner = (View as Window).GetIWin32Window();
            if (dialog.ShowDialog(owner) != DialogResult.OK) return;
            Folder = dialog.SelectedPath;
            UpdateNamespace();
        }

        private bool ChooseFolderCanExecute(object obj) {
            return !Busy;
        }

        public event EventHandler NameSpaceUpdated;

        private void UpdateNamespace() {
            try {
                if (!File.Exists(CodeFile)) {
                    NameSpace = Path.GetFileName(Folder);
                    return;
                }

                using (FileStream file = File.Open(CodeFile, FileMode.Open)) {
                    var streamReader = new StreamReader(file, Encoding.UTF8);
                    var buffer = new char[10000];
                    int count = streamReader.ReadBlock(buffer, 0, buffer.Length);
                    var str = new string(buffer.Take(count).ToArray());
                    const string ns = "namespace ";
                    if (!str.Contains(ns))
                        return;
                    string afterNs = str.Split(new[] {ns}, 2, StringSplitOptions.None).Last();
                    NameSpace = new string(afterNs.TakeWhile(c => char.IsLetterOrDigit(c) || c == '.')
                        .ToArray());
                }

                EventHelper.Raise(ref NameSpaceUpdated, this, EventArgs.Empty);
            } catch (Exception exception) {
                _dialogHelper.Exception(exception);
            }
        }

        private bool BuildCanExecute(object obj) {
            return string.IsNullOrEmpty(Error) && !Busy;
        }

        private void BuildExecuted(object obj) {
            _actionHelper.Async(Build);
        }

        private void Build() {
            Busy = true;
            try {
                using (IModelContext context = _contextFactory.Create(ConnectionString, false)) {
                    using (ICodeWriter codeWriter = _codeWriterFactory.Create(CodeFile)) {
                        using (IResourceBuilder resourceBuilder =
                            _resourceBuilderFactory.CreateResXBuilder(ResFile)) {
                            Build(context, codeWriter, resourceBuilder);
                            resourceBuilder.Build();
                        }
                    }
                }

                SaveFolder();
                _actionHelper.Dispatch(() => { View.DialogResult = true; });
            } catch (Exception exception) {
                _dialogHelper.Exception(exception, View);
            } finally {
                Busy = false;
            }
        }

        private void Build(IModelContext context, ICodeWriter w, IResourceBuilder resBuilder) {
            w.Write("//------------------------------------------------------------------------------");
            w.Write("// <auto-generated>");
            w.Write("//     This code was generated by a tool.");
            w.Write("//");
            w.Write("//     Changes to this file may cause incorrect behavior and will be lost if");
            w.Write("//     the code is regenerated.");
            w.Write("// </auto-generated>");
            w.Write("//------------------------------------------------------------------------------");
            w.Write();
            string resourcesName = Path.GetFileNameWithoutExtension(Path.GetFileName(ResFile));
            Func<string, string> res = s => string.Format("{0}.{1}", resourcesName, resBuilder.Add(s));
            Func<double?, string> num = d =>
                d.HasValue ? d.Value.ToString(CultureInfo.InvariantCulture) : "null";
            using (w.Block("namespace {0}", NameSpace)) {
                var ioDevices = context.Get<IoDevice>().OrderBy(d => d.Name).ToArray();

                #region IoDevices

                foreach (IoDevice ioDevice in ioDevices) {
                    using (w.Block("public sealed partial class {0} : PHmiClient.Tags.IoDeviceBase",
                        ioDevice.Name)) {
                        var digitalTags = ioDevice.DigTags.OrderBy(t => t.Name).ToArray();
                        var numericTags = ioDevice.NumTags.OrderBy(t => t.Name).ToArray();
                        using (w.Block("public {0}() : base({1}, \"{0}\")", ioDevice.Name, ioDevice.Id)) {
                            foreach (DigTag t in digitalTags)
                                w.Write("{0} = AddDigitalTag({1}, \"{0}\", () => {2});",
                                    t.Name, t.Id, res(t.Description));
                            foreach (NumTag t in numericTags)
                                w.Write(
                                    "{0} = AddNumericTag({1}, \"{0}\", () => {2}, () => {3}, () => {4}, {5}, {6});",
                                    t.Name, t.Id, res(t.Description), res(t.Format),
                                    res(t.EngUnit), num(t.EngMinDb), num(t.EngMaxDb));
                        }

                        foreach (DigTag t in digitalTags) {
                            w.Write();
                            w.Write("public PHmiClient.Tags.IDigitalTag {0} {{ get; private set; }}", t.Name);
                        }

                        foreach (NumTag t in numericTags) {
                            w.Write();
                            w.Write("public PHmiClient.Tags.INumericTag {0} {{ get; private set; }}", t.Name);
                        }
                    }

                    w.Write();
                }

                #endregion

                var alarmCategories = context.Get<AlarmCategory>().OrderBy(c => c.Name).ToArray();

                #region AlarmCategories

                foreach (AlarmCategory category in alarmCategories) {
                    using (w.Block("public sealed partial class {0} : PHmiClient.Alarms.AlarmCategoryBase",
                        category.Name)) {
                        using (w.Block("public {0}() : base({1}, \"{0}\", () => {2})", category.Name,
                            category.Id, res(category.Description))) {
                            foreach (AlarmTag alarmTag in category.AlarmTags.ToArray())
                                w.Write("AddAlarmInfo({0}, () => {1}, () => {2});", alarmTag.Id,
                                    res(alarmTag.Location), res(alarmTag.Description));
                        }
                    }

                    w.Write();
                }

                #endregion

                var trendsCategories = context.Get<TrendCategory>().OrderBy(c => c.Name).ToArray();

                #region TrendsCategories

                foreach (TrendCategory category in trendsCategories) {
                    using (w.Block("public sealed partial class {0} : PHmiClient.Trends.TrendsCategoryBase",
                        category.Name)) {
                        var trendTags = category.TrendTags.OrderBy(t => t.Name).ToArray();
                        using (w.Block("public {0}() : base({1}, \"{0}\", {2})", category.Name, category.Id,
                            category.PeriodDb)) {
                            foreach (TrendTag t in trendTags)
                                w.Write(
                                    "{0} = AddTrendTag({1}, \"{0}\", () => {2}, () => {3}, () => {4}, {5}, {6});",
                                    t.Name, t.Id, res(t.Description), res(t.NumTag.Format),
                                    res(t.NumTag.EngUnit), num(t.NumTag.EngMinDb), num(t.NumTag.EngMaxDb));
                        }

                        foreach (TrendTag t in trendTags) {
                            w.Write();
                            w.Write("public PHmiClient.Trends.ITrendTag {0} {{ get; private set; }}", t.Name);
                        }
                    }

                    w.Write();
                }

                #endregion

                var logs = context.Get<Log>().OrderBy(l => l.Name).ToArray();

                Settings settings = context.Get<Settings>().Single();
                using (w.Block("public sealed partial class PHmi : PHmiClient.PHmiSystem.PHmiBase")) {
                    using (w.Block("public PHmi() : this(\"{0}\")", settings.Server)) { }

                    w.Write();
                    using (w.Block("public PHmi(string server) : this(server, \"{0}\")", settings.Guid)) { }

                    w.Write();
                    using (w.Block("public PHmi(string server, string guid) : base(server, guid)")) {
                        foreach (IoDevice ioDevice in ioDevices)
                            w.Write("{0} = AddIoDevice(new {0}());", ioDevice.Name);
                        foreach (AlarmCategory category in alarmCategories)
                            w.Write("{0} = AddAlarmCategory(new {0}());", category.Name);
                        foreach (TrendCategory category in trendsCategories)
                            w.Write("{0} = AddTrendsCategory(new {0}());", category.Name);
                        foreach (Log log in logs) w.Write("{0} = AddLog({1}, \"{0}\");", log.Name, log.Id);
                    }

                    foreach (IoDevice ioDevice in ioDevices) {
                        w.Write();
                        w.Write("public {0} {0} {{ get; private set; }}", ioDevice.Name);
                    }

                    foreach (AlarmCategory category in alarmCategories) {
                        w.Write();
                        w.Write("public {0} {0} {{ get; private set; }}", category.Name);
                    }

                    foreach (TrendCategory category in trendsCategories) {
                        w.Write();
                        w.Write("public {0} {0} {{ get; private set; }}", category.Name);
                    }

                    foreach (Log log in logs) {
                        w.Write();
                        w.Write("public PHmiClient.Logs.LogAbstract {0} {{ get; private set; }}", log.Name);
                    }
                }
            }
        }

        protected override void OnPropertyChanged(string property) {
            base.OnPropertyChanged(property);
            if (property == PropertyHelper.GetPropertyName(this, m => m.Folder)) {
                OnPropertyChanged(this, m => m.CodeFile);
                OnPropertyChanged(this, m => m.ResFile);
            }

            _actionHelper.Dispatch(() => {
                _buildCommand.RaiseCanExecuteChanged();
                _chooseFolderCommand.RaiseCanExecuteChanged();
            });
        }

        private void LoadFolder() {
            var settings = new PHmiClient.Utils.Configuration.Settings(ProjectBuilder);
            Folder = settings.GetString(FolderName);
            NameSpace = settings.GetString(NameSpaceName);
        }

        private void SaveFolder() {
            var settings = new PHmiClient.Utils.Configuration.Settings(ProjectBuilder);
            settings.SetString(FolderName, Folder);
            settings.SetString(NameSpaceName, NameSpace);
            settings.Save();
        }
    }
}
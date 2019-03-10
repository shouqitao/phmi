using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using PHmiClient.Controls.Input;

namespace PHmiTools.Dialogs {
    /// <summary>
    ///     Interaction logic for AboutDialog.xaml
    /// </summary>
    public partial class AboutDialog : Window {
        public AboutDialog() {
            InitializeComponent();
            OkCommand = new DelegateCommand(OkCommandExecuted);
            LicensesCommand = new DelegateCommand(LicensesCommandExecuted);
            DataContext = this;
        }

        #region OkCommand

        public ICommand OkCommand { get; }

        private void OkCommandExecuted(object obj) {
            DialogResult = true;
        }

        #endregion OkCommand

        #region LicensesCommand

        public ICommand LicensesCommand { get; }

        private void LicensesCommandExecuted(object obj) {
            try {
                string assemblyLocation = Assembly.GetAssembly(typeof(AboutDialog)).Location;
                string dirPath = Path.GetDirectoryName(assemblyLocation);
                if (dirPath == null)
                    return;
                string filePath = Path.Combine(dirPath, "LICENSES.txt");
                Process.Start(filePath);
            } catch (Exception exception) {
                ExceptionDialog.Show(exception, this);
            }
        }

        #endregion LicensesCommand

        #region Assembly Attribute Accessors

        public string AssemblyTitle {
            get {
                var attributes = Assembly.GetEntryAssembly()
                    .GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0) {
                    var titleAttribute = (AssemblyTitleAttribute) attributes[0];
                    if (titleAttribute.Title != "") return titleAttribute.Title;
                }

                return Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().CodeBase);
            }
        }

        public string AssemblyVersion {
            get { return Assembly.GetEntryAssembly().GetName().Version.ToString(); }
        }

        public string AssemblyDescription {
            get {
                var attributes = Assembly.GetEntryAssembly()
                    .GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0) return "";
                return ((AssemblyDescriptionAttribute) attributes[0]).Description;
            }
        }

        public string AssemblyProduct {
            get {
                var attributes = Assembly.GetEntryAssembly()
                    .GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0) return "";
                return ((AssemblyProductAttribute) attributes[0]).Product;
            }
        }

        public string AssemblyCopyright {
            get {
                var attributes = Assembly.GetEntryAssembly()
                    .GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0) return "";
                return ((AssemblyCopyrightAttribute) attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany {
            get {
                var attributes = Assembly.GetEntryAssembly()
                    .GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0) return "";
                return ((AssemblyCompanyAttribute) attributes[0]).Company;
            }
        }

        #endregion Assembly Attribute Accessors
    }
}
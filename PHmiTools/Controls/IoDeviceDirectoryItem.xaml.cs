using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using PHmiResources;

namespace PHmiTools.Controls {
    /// <summary>
    ///     Interaction logic for IoDeviceDirectoryItem.xaml
    /// </summary>
    public partial class IoDeviceDirectoryItem {
        private readonly string _path;

        public IoDeviceDirectoryItem(string path) {
            InitializeComponent();
            _path = path;
            ShowClosed();
            Expanded += IoDeviceDirectoryItemExpanded;
            Collapsed += IoDeviceDirectoryItemCollapsed;
            string fileName = Path.GetFileName(_path);
            ItemName = fileName == null
                ? string.Empty
                : fileName.Substring(PHmiConstants.PHmiIoDevicePrefix.Length);
            foreach (TreeViewItem item in GetItems(_path)) Items.Add(item);
            tb.Text = ItemName;
        }

        public string ItemName { get; }

        private void IoDeviceDirectoryItemCollapsed(object sender, RoutedEventArgs e) {
            if (ReferenceEquals(e.OriginalSource, this)) ShowClosed();
        }

        private void ShowClosed() {
            image.Source = new BitmapImage(ImagesUries.FolderClosedPng);
        }

        private void IoDeviceDirectoryItemExpanded(object sender, RoutedEventArgs e) {
            if (ReferenceEquals(e.OriginalSource, this)) ShowOpen();
        }

        private void ShowOpen() {
            image.Source = new BitmapImage(ImagesUries.FolderOpenPng);
        }

        public static TreeViewItem[] GetItems(string path) {
            var result = new List<TreeViewItem>();
            result.AddRange(GetValidPaths(Directory.GetDirectories(path))
                .Select(p => new IoDeviceDirectoryItem(p)));
            result.AddRange(GetValidPaths(
                    Directory.GetFiles(path))
                .Where(p => Path.GetExtension(p) == PHmiConstants.PHmiIoDeviceExt)
                .Select(p => new IoDeviceItem(p)));
            return result.ToArray();
        }

        private static IEnumerable<string> GetValidPaths(IEnumerable<string> paths) {
            return paths.Where(IsValidPath).OrderBy(p => p).ToArray();
        }

        private static bool IsValidPath(string path) {
            string fileName = Path.GetFileName(path);
            return fileName != null && fileName.StartsWith(PHmiConstants.PHmiIoDevicePrefix);
        }
    }
}
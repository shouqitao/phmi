using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PHmiClient.Controls.ListBoxWithColumns {
    public class HeaderGridSplitter : GridSplitter {
        public static readonly DependencyProperty ColumnDefinitionProperty =
            DependencyProperty.Register("ColumnDefinition", typeof(ColumnDefinition),
                typeof(HeaderGridSplitter), null);

        public HeaderGridSplitter() {
            MouseDoubleClick += HeaderGridSplitterMouseDoubleClick;
        }

        public ColumnDefinition ColumnDefinition {
            get { return (ColumnDefinition) GetValue(ColumnDefinitionProperty); }
            set { SetValue(ColumnDefinitionProperty, value); }
        }

        private void HeaderGridSplitterMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (ColumnDefinition != null)
                ColumnDefinition.Width = new GridLength();
        }
    }
}
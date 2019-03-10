﻿using System.Windows;
using System.Windows.Controls;

namespace PHmiConfigurator.Modules.Collection.Selectable {
    /// <summary>
    ///     Interaction logic for TrendTags.xaml
    /// </summary>
    public partial class TrendTags {
        public TrendTags() {
            InitializeComponent();
        }

        private void CbSelectorsSelectionChanged(object sender, SelectionChangedEventArgs e) {
            cbSelectors.SelectionChanged -= CbSelectorsSelectionChanged;
            if (cbSelectors.SelectedItem != null)
                cbSelectors.IsDropDownOpen = false;
        }

        private void ComboBoxLoaded(object sender, RoutedEventArgs e) {
            var comboBox = (ComboBox) sender;
            comboBox.Loaded -= ComboBoxLoaded;
            comboBox.IsDropDownOpen = true;
        }
    }
}
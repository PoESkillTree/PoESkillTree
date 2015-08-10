using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace POESKillTree.TreeGenerator.Views
{
    /// <summary>
    /// Interaction logic for AdvancedGeneratorTab.xaml
    /// </summary>
    public partial class AdvancedGeneratorTab
    {
        public AdvancedGeneratorTab()
        {
            InitializeComponent();
        }

        // If a row with validation errors that is currently edited gets removed,
        // AttrConstraintGrid.Items.Refresh() needs to be called because the validation
        // is not automatically updated on remove (Removing doesn't like to have a button for it, it seems).
        private bool _clicked;

        private void DeleteRowButton_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (_clicked)
            {
                AttrConstraintGrid.Items.Refresh();
                _clicked = false;
            }
        }

        private void DeleteRowButton_OnClick(object sender, RoutedEventArgs e)
        {
            _clicked = true;
        }
    }
}
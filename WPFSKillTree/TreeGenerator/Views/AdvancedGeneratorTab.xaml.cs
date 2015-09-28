using System;
using System.Windows;
using System.Windows.Controls;
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
                try
                {
                    AttrConstraintGrid.Items.Refresh();
                }
                catch (InvalidOperationException)
                {
                    // If the remove didn't cancel cell editing with validation errors, the Grid can't be refreshed.
                    // Some stuff is probably not working properly after it, but better than crashing.
                }
                _clicked = false;
            }
        }

        private void DeleteRowButton_OnClick(object sender, RoutedEventArgs e)
        {
            _clicked = true;
        }
        
        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var cell = sender as DataGridCell;
            if (cell != null && !cell.IsEditing && !cell.IsReadOnly)
            {
                if (!cell.IsFocused)
                {
                    cell.Focus();
                }
                // DataGrid.SelectionUnit must not be "FullRow"
                cell.IsSelected = true;
            }
        }
    }
}
using System.Windows.Controls;
using System.Windows.Input;
using POESKillTree.Utils;

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
        
        /// <summary>
        /// Forces the DataGridCell into Edit mode with a single click instead of the normal double click.
        /// </summary>
        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var cell = sender as DataGridCell;
            if (cell != null && !cell.IsEditing && !cell.IsReadOnly)
            {
                if (!cell.IsFocused)
                {
                    cell.Focus();
                }
                var dataGrid = cell.FindAnchestor<DataGrid>();
                if (dataGrid != null)
                {
                    if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
                    {
                        if (!cell.IsSelected)
                            cell.IsSelected = true;
                    }
                    else
                    {
                        var row = cell.FindAnchestor<DataGridRow>();
                        if (row != null && !row.IsSelected)
                        {
                            row.IsSelected = true;
                        }
                    }
                }
            }
        }
    }
}
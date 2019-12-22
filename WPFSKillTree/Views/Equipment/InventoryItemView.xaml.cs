using System.Windows;
using System.Windows.Input;
using PoESkillTree.ViewModels.Equipment;

namespace PoESkillTree.Views.Equipment
{
    /// <summary>
    /// Interaction logic for InventoryItemView.xaml
    /// </summary>
    public partial class InventoryItemView
    {
        public InventoryItemView()
        {
            InitializeComponent();
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (DataContext is InventoryItemViewModel vm)
            {
                vm.IsCurrent = true;
            }
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (DataContext is InventoryItemViewModel vm)
            {
                vm.IsCurrent = false;
            }
            base.OnMouseLeave(e);
        }

        protected override void OnPreviewDragEnter(DragEventArgs e)
        {
            if (DataContext is InventoryItemViewModel vm)
            {
                vm.IsCurrent = true;
            }
            base.OnDragEnter(e);
        }

        protected override void OnPreviewDragLeave(DragEventArgs e)
        {
            if (DataContext is InventoryItemViewModel vm)
            {
                vm.IsCurrent = false;
            }
            base.OnDragLeave(e);
        }
    }
}

using System.Windows;
using JetBrains.Annotations;
using PoESkillTree.Model.Items;

namespace PoESkillTree.ViewModels.Equipment
{
    /// <summary>
    /// View model for items in the stash.
    /// </summary>
    public class StashItemViewModel : DraggableItemViewModel
    {
        private Item _item;
        public sealed override Item Item
        {
            get { return _item; }
            set { SetProperty(ref _item, value); }
        }

        private bool _highlight;
        /// <summary>
        /// Gets or sets whether this view model should be displayed highlighted.
        /// </summary>
        // used in styles, Visual Studio/Resharper somehow doesn't recognize that
        [UsedImplicitly(ImplicitUseKindFlags.Access)]
        public bool Highlight
        {
            get { return _highlight; }
            set { SetProperty(ref _highlight, value); }
        }

        public override DragDropEffects DropOnInventoryEffect => DragDropEffects.Copy;

        public StashItemViewModel(Item item)
            => Item = item;
    }
}

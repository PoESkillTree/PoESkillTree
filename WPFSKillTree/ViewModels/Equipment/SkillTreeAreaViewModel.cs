using System;
using System.Collections.Generic;
using System.Windows;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.Utils;

namespace PoESkillTree.ViewModels.Equipment
{
    public sealed class SkillTreeAreaViewModel : Notifier, IDisposable
    {
        private readonly IReadOnlyDictionary<ushort, SkillNode> _skillNodes;
        private readonly IReadOnlyList<InventoryItemViewModel> _jewels;

        private bool _hasViewBox;
        private Rect _viewBox;

        public SkillTreeAreaViewModel(
            IReadOnlyDictionary<ushort, SkillNode> skillNodes, IReadOnlyList<InventoryItemViewModel> jewels)
        {
            _skillNodes = skillNodes;
            _jewels = jewels;
            foreach (var jewel in _jewels)
            {
                jewel.PropertyChanged += JewelOnPropertyChanged;
            }
        }

        public bool HasViewBox
        {
            get => _hasViewBox;
            private set => SetProperty(ref _hasViewBox, value);
        }

        public Rect ViewBox
        {
            get => _viewBox;
            private set => SetProperty(ref _viewBox, value);
        }

        private void JewelOnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InventoryItemViewModel.IsCurrent))
            {
                SetViewBox((InventoryItemViewModel) sender);
            }
        }

        private void SetViewBox(InventoryItemViewModel inventoryItem)
        {
            if (inventoryItem.IsCurrent)
            {
                HasViewBox = true;
                var position = _skillNodes[inventoryItem.Socket!.Value].Position;
                var topLeft = new Point(position.X - 1000, position.Y - 1000);
                ViewBox = new Rect(topLeft, new Size(2000, 2000));
            }
            else
            {
                HasViewBox = false;
                ViewBox = new Rect();
            }
        }

        public void Dispose()
        {
            foreach (var jewel in _jewels)
            {
                jewel.PropertyChanged -= JewelOnPropertyChanged;
            }
        }
    }
}
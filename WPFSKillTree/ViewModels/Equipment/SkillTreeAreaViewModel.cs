using System;
using System.Collections.Generic;
using System.Windows;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.Utils;

namespace PoESkillTree.ViewModels.Equipment
{
    public sealed class SkillTreeAreaViewModel : Notifier, IDisposable
    {
        private static readonly Rect DefaultViewBox = GetViewBox(15000, new Point());

        private readonly IReadOnlyDictionary<ushort, SkillNode> _skillNodes;
        private readonly IReadOnlyList<InventoryItemViewModel> _jewels;

        private Rect _viewBox = DefaultViewBox;

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
                var position = _skillNodes[inventoryItem.Socket!.Value].Position;
                ViewBox = GetViewBox(2500, position);
            }
            else
            {
                ViewBox = DefaultViewBox;
            }
        }

        private static Rect GetViewBox(double size, Point center) =>
            new Rect(new Point(center.X - size/2, center.Y - size/2), new Size(size, size));

        public void Dispose()
        {
            foreach (var jewel in _jewels)
            {
                jewel.PropertyChanged -= JewelOnPropertyChanged;
            }
        }
    }
}
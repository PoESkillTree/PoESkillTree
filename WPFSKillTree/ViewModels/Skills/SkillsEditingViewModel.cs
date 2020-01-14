using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.Skills;
using PoESkillTree.Model.Items;

namespace PoESkillTree.ViewModels.Skills
{
    public sealed class SkillsEditingViewModel : IDisposable
    {
        private static readonly IReadOnlyList<ItemSlot> SlotsWithSkills = new[]
        {
            ItemSlot.MainHand, ItemSlot.OffHand,
            ItemSlot.Helm, ItemSlot.BodyArmour, ItemSlot.Gloves, ItemSlot.Boots,
            ItemSlot.Belt, ItemSlot.Ring, ItemSlot.Ring2, ItemSlot.Amulet,
        };

        public IReadOnlyList<SkillsInSlotEditingViewModel> SkillsInSlots { get; }

        public SkillsEditingViewModel(
            SkillDefinitions skillDefinitions, ItemImageService itemImageService, ItemAttributes itemAttributes)
        {
            SkillsInSlots = SlotsWithSkills
                .Select(s => new SkillsInSlotEditingViewModel(skillDefinitions, itemImageService, itemAttributes, s))
                .ToList();
        }

        public void Dispose()
        {
            foreach (var slotViewModel in SkillsInSlots)
            {
                slotViewModel.Dispose();
            }
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Equipment;

namespace PoESkillTree.Computation.Builders.Equipment
{
    public class EquipmentBuilderCollection
        : FixedBuilderCollection<ItemSlot, IEquipmentBuilder>, IEquipmentBuilderCollection
    {
        private static readonly IReadOnlyList<ItemSlot> Keys = Enums.GetValues<ItemSlot>().ToList();

        public EquipmentBuilderCollection(IStatFactory statFactory)
            : base(Keys, s => new EquipmentBuilder(statFactory, s))
        {
        }
    }
}
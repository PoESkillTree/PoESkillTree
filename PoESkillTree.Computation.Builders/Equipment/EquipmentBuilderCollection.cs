using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Builders.Conditions;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Equipment;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Builders.Equipment
{
    public class EquipmentBuilderCollection : IEquipmentBuilderCollection
    {
        private static readonly IReadOnlyList<ItemSlot> ItemSlotValues =
            Enum.GetValues(typeof(ItemSlot)).Cast<ItemSlot>().ToList();

        private readonly IStatFactory _statFactory;

        private readonly IDictionary<ItemSlot, IEquipmentBuilder>
            _items = new Dictionary<ItemSlot, IEquipmentBuilder>();

        public EquipmentBuilderCollection(IStatFactory statFactory)
        {
            _statFactory = statFactory;
        }

        public IBuilderCollection<IEquipmentBuilder> Resolve(ResolveContext context) => this;

        public ValueBuilder Count(Func<IEquipmentBuilder, IConditionBuilder> predicate = null)
        {
            var conditions = Select(predicate);
            var valueBuilder = new ValueBuilderImpl(
                ps => Build(ps, conditions),
                c => (ps => Build(ps, Resolve(c, conditions))));
            return new ValueBuilder(valueBuilder);

            IEnumerable<IConditionBuilder> Resolve(ResolveContext context, IEnumerable<IConditionBuilder> cs) =>
                cs.Select(c => c.Resolve(context));

            IValue Build(BuildParameters parameters, IEnumerable<IConditionBuilder> cs)
            {
                var builtConditions = cs.Select(c => c.Build(parameters).value).ToList();
                return new FunctionalValue(
                    c => Calculate(c, builtConditions),
                    $"Count({string.Join(", ", builtConditions)})");
            }

            NodeValue? Calculate(IValueCalculationContext context, IEnumerable<IValue> values) =>
                values
                    .Select(v => v.Calculate(context))
                    .Select(v => new NodeValue(v.IsTrue() ? 1 : 0))
                    .Sum();
        }

        public IConditionBuilder Any(Func<IEquipmentBuilder, IConditionBuilder> predicate = null)
        {
            return Select(predicate).Aggregate((l, r) => l.Or(r));
        }

        private IReadOnlyList<IConditionBuilder> Select(Func<IEquipmentBuilder, IConditionBuilder> predicate)
        {
            predicate = predicate ?? (_ => ConstantConditionBuilder.True);
            return ItemSlotValues.Select(s => predicate(this[s])).ToList();
        }

        public IEquipmentBuilder this[ItemSlot slot] =>
            _items.GetOrAdd(slot, s => new EquipmentBuilder(_statFactory, s));
    }
}
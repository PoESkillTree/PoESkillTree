using System.Collections.Generic;
using System.Linq;
using Moq;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.Tests
{
    public static class SkillParserTestUtils
    {
        public static IValueCalculationContext MockValueCalculationContextForMainSkill(
            Skill skill, params (string stat, double? value)[] nodeValues)
            => MockValueCalculationContext(skill, true, false, nodeValues);

        public static IValueCalculationContext MockValueCalculationContextForActiveSkill(
            Skill skill, params (string stat, double? value)[] nodeValues)
            => MockValueCalculationContext(skill, false, true, nodeValues);

        public static IValueCalculationContext MockValueCalculationContextForInactiveSkill(
            Skill skill, params (string stat, double? value)[] nodeValues)
            => MockValueCalculationContext(skill, false, false, nodeValues);

        public static IValueCalculationContext MockValueCalculationContext(
            Skill skill, bool isMainSkill, bool isActiveSkill, params (string stat, double? value)[] nodeValues)
        {
            var contextMock = new Mock<IValueCalculationContext>();
            var isMainSkillStat = new Stat($"{skill.ItemSlot}.{skill.SocketIndex}.IsMainSkill");
            contextMock.Setup(c => c.GetValue(isMainSkillStat, NodeType.Total, PathDefinition.MainPath))
                .Returns((NodeValue?) isMainSkill);
            var activeSkillItemSlotStat = new Stat($"{skill.Id}.ActiveSkillItemSlot");
            var activeSkillItemSlot = isActiveSkill ? skill.ItemSlot : ItemSlot.Unequipable;
            contextMock.Setup(c => c.GetValue(activeSkillItemSlotStat, NodeType.Total, PathDefinition.MainPath))
                .Returns((NodeValue?) (double) activeSkillItemSlot);
            var activeSkillSocketIndexStat = new Stat($"{skill.Id}.ActiveSkillSocketIndex");
            var activeSkillSocketIndex = isActiveSkill ? skill.SocketIndex : -1;
            contextMock.Setup(c => c.GetValue(activeSkillSocketIndexStat, NodeType.Total, PathDefinition.MainPath))
                .Returns((NodeValue?) activeSkillSocketIndex);
            foreach (var (statIdentity, value) in nodeValues)
            {
                var stat = new Stat(statIdentity);
                contextMock.Setup(c => c.GetValue(stat, NodeType.Total, PathDefinition.MainPath))
                    .Returns((NodeValue?) value);
            }
            return contextMock.Object;
        }

        public static bool AnyModifierHasIdentity(IEnumerable<Modifier> modifiers, string identity)
            => modifiers.Any(m => m.Stats.Any(s => s.Identity == identity));

        public static IValue GetValueForIdentity(IEnumerable<Modifier> modifiers, string identity)
            => GetFirstModifierWithIdentity(modifiers, identity).Value;

        public static Modifier GetFirstModifierWithIdentity(IEnumerable<Modifier> modifiers, string identity)
            => modifiers.First(m => m.Stats.First().Identity == identity);
    }
}
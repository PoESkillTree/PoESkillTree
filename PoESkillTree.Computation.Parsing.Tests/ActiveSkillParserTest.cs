using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.Tests
{
    [TestFixture]
    public class ActiveSkillParserTest
    {
        [TestCase(true)]
        [TestCase(false)]
        public void FrenzyUsesOffHandIfNotEmpty(bool offHandHasItem)
        {
            var frenzyDefinition = CreateFrenzyDefinition();
            var skill = new Skill("Frenzy", 1, 0, ItemSlot.Belt, 0, null);
            var valueCalculationContext = MockValueCalculationContext(skill, true,
                ("OffHand.ItemTags", offHandHasItem ? (double?) 1 : null));
            var sut = CreateSut(frenzyDefinition);

            var result = sut.Parse(skill);

            var modifiers = result.Modifiers;
            var offHandMod = modifiers.First(m => m.Stats.First().Identity == "SkillUses.OffHand");
            var actual = offHandMod.Value.Calculate(valueCalculationContext);
            Assert.AreEqual(offHandHasItem, actual.IsTrue());
        }

        [TestCase(Tags.Default)]
        [TestCase(Tags.Ranged)]
        public void FrenzyHasMeleeKeywordIfNotRanged(Tags mainHandTags)
        {
            var frenzyDefinition = CreateFrenzyDefinition();
            var skill = new Skill("Frenzy", 1, 0, ItemSlot.Belt, 0, null);
            var valueCalculationContext = MockValueCalculationContext(skill, true,
                ("MainHand.ItemTags", mainHandTags.EncodeAsDouble()));
            var sut = CreateSut(frenzyDefinition);

            var result = sut.Parse(skill);

            var modifier = result.Modifiers.First(m => m.Stats.First().Identity == "MainSkill.Has.Melee");
            var actual = modifier.Value.Calculate(valueCalculationContext);
            Assert.AreEqual(!mainHandTags.HasFlag(Tags.Ranged), actual.IsTrue());
        }

        [TestCase(Tags.Default)]
        [TestCase(Tags.Ranged)]
        public void FrenzyHasProjectileKeywordIfRanged(Tags mainHandTags)
        {
            var frenzyDefinition = CreateFrenzyDefinition();
            var skill = new Skill("Frenzy", 1, 0, ItemSlot.Belt, 0, null);
            var valueCalculationContext = MockValueCalculationContext(skill, true,
                ("MainHand.ItemTags", mainHandTags.EncodeAsDouble()));
            var sut = CreateSut(frenzyDefinition);

            var result = sut.Parse(skill);

            var modifier = result.Modifiers.First(m => m.Stats.First().Identity == "MainSkill.Has.Projectile");
            var actual = modifier.Value.Calculate(valueCalculationContext);
            Assert.AreEqual(mainHandTags.HasFlag(Tags.Ranged), actual.IsTrue());
        }

        private static SkillDefinition CreateFrenzyDefinition()
        {
            var activeSkill = new ActiveSkillDefinition("Frenzy", 0, new string[0], new string[0],
                new[] { Keyword.Melee, Keyword.Projectile }, false, null, new ItemClass[0]);
            var level = new SkillLevelDefinition(null, null, null, null, null, null, 0, 0, 0, 0, 0,
                new UntranslatedStat[0], new UntranslatedStat[0], null);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return SkillDefinition.CreateActive("Frenzy", 0, "", null, activeSkill, levels);
        }

        private static ActiveSkillParser CreateSut(SkillDefinition skillDefinition)
        {
            var skillDefinitions = new SkillDefinitions(new[] { skillDefinition });
            var statFactory = new StatFactory();
            var builderFactories = new BuilderFactories(statFactory, new SkillDefinition[0]);
            var metaStatBuilders = new MetaStatBuilders(statFactory);
            return new ActiveSkillParser(skillDefinitions, builderFactories, metaStatBuilders);
        }

        private static IValueCalculationContext MockValueCalculationContext(
            Skill skill, bool isMainSkill, params (string stat, double? value)[] nodeValues)
        {
            var contextMock = new Mock<IValueCalculationContext>();
            var isMainSkillStat = new Stat($"{skill.ItemSlot}.{skill.SocketIndex}.IsMainSkill");
            contextMock.Setup(c => c.GetValue(isMainSkillStat, NodeType.Total, PathDefinition.MainPath))
                .Returns((NodeValue?) isMainSkill);
            foreach (var (statIdentity, value) in nodeValues)
            {
                var stat = new Stat(statIdentity);
                contextMock.Setup(c => c.GetValue(stat, NodeType.Total, PathDefinition.MainPath))
                    .Returns((NodeValue?) value);
            }
            return contextMock.Object;
        }
    }
}
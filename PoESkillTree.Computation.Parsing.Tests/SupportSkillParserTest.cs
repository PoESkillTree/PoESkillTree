using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Parsing.SkillParsers;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.Tests
{
    [TestFixture]
    public class SupportSkillParserTest
    {
        [Test]
        public void BlasphemyAddsAuraKeyword()
        {
            var (activeDefinition, activeSkill) = CreateEnfeebleDefinition();
            var (supportDefinition, supportSkill) = CreateBlasphemyDefinition();
            var sut = CreateSut(activeDefinition, supportDefinition);

            var result = sut.Parse(activeSkill, supportSkill);

            Assert.IsTrue(AnyModifierHasIdentity(result.Modifiers, "MainSkill.Has.Aura"));
        }

        [Test]
        public void BlasphemyOverridesBaseCost()
        {
            var (activeDefinition, activeSkill) = CreateEnfeebleDefinition();
            var (supportDefinition, supportSkill) = CreateBlasphemyDefinition();
            var sut = CreateSut(activeDefinition, supportDefinition);

            var result = sut.Parse(activeSkill, supportSkill);

            var modifier = GetFirstModifierWithIdentity(result.Modifiers, "Belt.1.Cost");
            Assert.AreEqual(Form.TotalOverride, modifier.Form);
            Assert.AreEqual(new NodeValue(42), modifier.Value.Calculate(null));
        }

        private static (SkillDefinition, Skill) CreateEnfeebleDefinition()
        {
            var activeSkill = new ActiveSkillDefinition("Enfeeble", 0, new[] { "curse" }, new string[0],
                new[] { Keyword.Curse }, true, null, new ItemClass[0]);
            var stats = new UntranslatedStat[0];
            var level = new SkillLevelDefinition(null, null, null, null, null, null, null, 0, 0, 0, 0,
                new UntranslatedStat[0], stats, null);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (SkillDefinition.CreateActive("Enfeeble", 0, "", null, activeSkill, levels),
                new Skill("Enfeeble", 1, 0, ItemSlot.Belt, 0, null));
        }

        private static (SkillDefinition, Skill) CreateBlasphemyDefinition()
        {
            var supportSkill = new SupportSkillDefinition(false, new string[0], new string[0], new string[0],
                new[] { Keyword.Aura });
            var stats = new UntranslatedStat[0];
            var level = new SkillLevelDefinition(null, null, null, null, null, 42, null, 0, 0, 0, 0,
                new UntranslatedStat[0], stats, null);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (SkillDefinition.CreateSupport("Blasphemy", 0, "", null, supportSkill, levels),
                new Skill("Blasphemy", 1, 0, ItemSlot.Belt, 1, null));
        }

        [Test]
        public void PhysicalToLightningConversionIsLocal()
        {
            var (activeDefinition, activeSkill) = CreateEnfeebleDefinition();
            var (supportDefinition, supportSkill) = CreatePhysicalToLightningDefinition();
            var sut = CreateSut(activeDefinition, supportDefinition);

            var result = sut.Parse(activeSkill, supportSkill);

            var modifiers = result.Modifiers;
            var expectedIdentity =
                "Physical.Damage.Attack.MainHand.Skill.ConvertTo(Lightning.Damage.Attack.MainHand.Skill)";
            Assert.IsTrue(AnyModifierHasIdentity(modifiers, expectedIdentity));
            var modifier = GetFirstModifierWithIdentity(modifiers, expectedIdentity);
            Assert.IsInstanceOf<ModifierSource.Local.Skill>(modifier.Source);
        }

        private static (SkillDefinition, Skill) CreatePhysicalToLightningDefinition()
        {
            var supportSkill = new SupportSkillDefinition(false, new string[0], new string[0], new string[0],
                new Keyword[0]);
            var stats = new[]
            {
                new UntranslatedStat("skill_physical_damage_%_to_convert_to_lightning", 50), 
            };
            var level = new SkillLevelDefinition(null, null, null, null, null, null, null, 0, 0, 0, 0,
                new UntranslatedStat[0], stats, null);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (SkillDefinition.CreateSupport("SupportPhysicalToLightning", 0, "", null, supportSkill, levels),
                new Skill("SupportPhysicalToLightning", 1, 0, ItemSlot.Belt, 1, null));
        }

        private static SupportSkillParser CreateSut(
            SkillDefinition activeSkillDefinition, SkillDefinition supportSkillDefinition,
            IParser<UntranslatedStatParserParameter> statParser = null)
        {
            var skillDefinitions = new SkillDefinitions(new[] { activeSkillDefinition, supportSkillDefinition });
            var statFactory = new StatFactory();
            var builderFactories = new BuilderFactories(statFactory, new SkillDefinitions(new SkillDefinition[0]));
            var metaStatBuilders = new MetaStatBuilders(statFactory);
            if (statParser is null)
            {
                statParser = Mock.Of<IParser<UntranslatedStatParserParameter>>(p =>
                    p.Parse(It.IsAny<UntranslatedStatParserParameter>()) == ParseResult.Success(new Modifier[0]));
            }
            return new SupportSkillParser(skillDefinitions, builderFactories, metaStatBuilders, _ => statParser);
        }

        private static bool AnyModifierHasIdentity(IEnumerable<Modifier> modifiers, string identity)
            => modifiers.Any(m => m.Stats.Any(s => s.Identity == identity));

        private static Modifier GetFirstModifierWithIdentity(IEnumerable<Modifier> modifiers, string identity)
            => modifiers.First(m => m.Stats.First().Identity == identity);
    }
}
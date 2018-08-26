using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Damage;
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
            var (definition, skill) = CreateFrenzyDefinition();
            var valueCalculationContext = MockValueCalculationContext(skill, true,
                ("OffHand.ItemTags", offHandHasItem ? (double?) 1 : null));
            var sut = CreateSut(definition);

            var result = sut.Parse(skill);

            var actual = GetValueForIdentity(result.Modifiers, "SkillUses.OffHand")
                .Calculate(valueCalculationContext);
            Assert.AreEqual(offHandHasItem, actual.IsTrue());
        }

        [TestCase(Tags.Default)]
        [TestCase(Tags.Ranged)]
        public void FrenzyHasMeleeKeywordIfNotRanged(Tags mainHandTags)
        {
            var (definition, skill) = CreateFrenzyDefinition();
            var valueCalculationContext = MockValueCalculationContext(skill, true,
                ("MainHand.ItemTags", mainHandTags.EncodeAsDouble()));
            var sut = CreateSut(definition);

            var result = sut.Parse(skill);

            var actual = GetValueForIdentity(result.Modifiers, "MainSkillPart.Has.Melee")
                .Calculate(valueCalculationContext);
            Assert.AreEqual(!mainHandTags.HasFlag(Tags.Ranged), actual.IsTrue());
        }

        [TestCase(Tags.Default)]
        [TestCase(Tags.Ranged)]
        public void FrenzyHasProjectileKeywordIfRanged(Tags mainHandTags)
        {
            var (definition, skill) = CreateFrenzyDefinition();
            var valueCalculationContext = MockValueCalculationContext(skill, true,
                ("MainHand.ItemTags", mainHandTags.EncodeAsDouble()));
            var sut = CreateSut(definition);

            var result = sut.Parse(skill);

            var actual = GetValueForIdentity(result.Modifiers, "MainSkillPart.Has.Projectile")
                .Calculate(valueCalculationContext);
            Assert.AreEqual(mainHandTags.HasFlag(Tags.Ranged), actual.IsTrue());
        }

        private static (SkillDefinition, Skill) CreateFrenzyDefinition()
        {
            var activeSkill = new ActiveSkillDefinition("Frenzy", 0, new[] { "attack" }, new string[0],
                new[] { Keyword.Melee, Keyword.Projectile }, false, null, new ItemClass[0]);
            var level = new SkillLevelDefinition(null, null, null, null, null, null, 0, 0, 0, 0, 0,
                new UntranslatedStat[0], new UntranslatedStat[0], null);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (SkillDefinition.CreateActive("Frenzy", 0, "", null, activeSkill, levels),
                new Skill("Frenzy", 1, 0, ItemSlot.Belt, 0, null));
        }

        [Test]
        public void FlameTotemHasSpellHitDamageSource()
        {
            var (definition, skill) = CreateFlameTotemDefinition();
            var valueCalculationContext = MockValueCalculationContext(skill, true);
            var sut = CreateSut(definition);

            var result = sut.Parse(skill);

            var actual = GetValueForIdentity(result.Modifiers, "SkillHitDamageSource")
                .Calculate(valueCalculationContext);
            Assert.AreEqual((double) DamageSource.Spell, actual.Single());
        }

        [Test]
        public void FlameTotemHasCorrectKeywords()
        {
            var (definition, skill) = CreateFlameTotemDefinition();
            var sut = CreateSut(definition);

            var result = sut.Parse(skill);

            var modifiers = result.Modifiers;
            Assert.IsTrue(AnyModifierHasIdentity(modifiers, "MainSkillPart.Has.Projectile"));
            Assert.IsTrue(AnyModifierHasIdentity(modifiers, "MainSkillPart.Damage.Spell.Has.Spell"));
            Assert.IsTrue(AnyModifierHasIdentity(modifiers, "MainSkillPart.Damage.Ailment.Has.Totem"));
        }

        private static (SkillDefinition, Skill) CreateFlameTotemDefinition()
        {
            var activeSkill = new ActiveSkillDefinition("Flame Totem", 0, new[] { "spell" }, new string[0],
                new[] { Keyword.Spell, Keyword.Projectile, Keyword.Totem }, false, null, new ItemClass[0]);
            var level = new SkillLevelDefinition(null, null, null, null, null, null, 0, 0, 0, 0, 0,
                new UntranslatedStat[0], new[] { new UntranslatedStat("spell_maximum_base_fire_damage", 10), }, null);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (SkillDefinition.CreateActive("FlameTotem", 0, "", null, activeSkill, levels),
                new Skill("FlameTotem", 1, 0, ItemSlot.Belt, 0, null));
        }

        [Test]
        public void ContagionHasNoHitDamageSource()
        {
            var (definition, skill) = CreateContagionDefinition();
            var sut = CreateSut(definition);

            var result = sut.Parse(skill);

            var modifiers = result.Modifiers;
            Assert.IsFalse(AnyModifierHasIdentity(modifiers, "SkillHitDamageSource"));
        }

        [Test]
        public void ContagionHasCorrectKeywords()
        {
            var (definition, skill) = CreateContagionDefinition();
            var sut = CreateSut(definition);

            var result = sut.Parse(skill);

            var modifiers = result.Modifiers;
            Assert.IsFalse(AnyModifierHasIdentity(modifiers, "MainSkillPart.Damage.Spell.Has.Spell"));
            Assert.IsFalse(AnyModifierHasIdentity(modifiers, "MainSkillPart.Damage.OverTime.Has.AreaOfEffect"));
            Assert.IsTrue(AnyModifierHasIdentity(modifiers, "MainSkillPart.Damage.OverTime.Has.Chaos"));
        }

        private static (SkillDefinition, Skill) CreateContagionDefinition()
        {
            var activeSkill = new ActiveSkillDefinition("Contagion", 0, new[] { "spell" }, new string[0],
                new[] { Keyword.Spell, Keyword.AreaOfEffect, Keyword.Chaos }, false, null, new ItemClass[0]);
            var stats = new[]
            {
                new UntranslatedStat("base_chaos_damage_to_deal_per_minute", 60),
            };
            var level = new SkillLevelDefinition(null, null, null, null, null, null, 0, 0, 0, 0, 0,
                new UntranslatedStat[0], stats, null);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (SkillDefinition.CreateActive("Contagion", 0, "", null, activeSkill, levels),
                new Skill("Contagion", 1, 0, ItemSlot.Belt, 0, null));
        }

        [Test]
        public void ShieldChargeHasCorrectKeywords()
        {
            var (definition, skill) = CreateShieldChargeDefinition();
            var sut = CreateSut(definition);

            var result = sut.Parse(skill);

            var modifiers = result.Modifiers;
            Assert.IsTrue(AnyModifierHasIdentity(modifiers, "MainSkillPart.Damage.Attack.Has.AreaOfEffect"));
        }

        private static (SkillDefinition, Skill) CreateShieldChargeDefinition()
        {
            var activeSkill = new ActiveSkillDefinition("ShieldCharge", 0, new[] { "attack" }, new string[0],
                new[] { Keyword.Attack, Keyword.AreaOfEffect }, false, null, new ItemClass[0]);
            var stats = new[]
            {
                new UntranslatedStat("is_area_damage", 1),
            };
            var level = new SkillLevelDefinition(null, null, null, null, null, null, 0, 0, 0, 0, 0,
                new UntranslatedStat[0], stats, null);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (SkillDefinition.CreateActive("ShieldCharge", 0, "", null, activeSkill, levels),
                new Skill("ShieldCharge", 1, 0, ItemSlot.Belt, 0, null));
        }

        [Test]
        public void CausticArrowHasCorrectKeywords()
        {
            var (definition, skill) = CreateCausticArrowDefinition();
            var sut = CreateSut(definition);

            var result = sut.Parse(skill);

            var modifiers = result.Modifiers;
            Assert.IsFalse(AnyModifierHasIdentity(modifiers, "MainSkillPart.Damage.Attack.Has.AreaOfEffect"));
            Assert.IsTrue(AnyModifierHasIdentity(modifiers, "MainSkillPart.Damage.OverTime.Has.AreaOfEffect"));
        }

        private static (SkillDefinition, Skill) CreateCausticArrowDefinition()
        {
            var activeSkill = new ActiveSkillDefinition("Caustic Arrow", 0, new[] { "attack" }, new string[0],
                new[] { Keyword.Attack, Keyword.AreaOfEffect }, false, null, new ItemClass[0]);
            var stats = new[]
            {
                new UntranslatedStat("base_chaos_damage_to_deal_per_minute", 60),
            };
            var level = new SkillLevelDefinition(null, null, null, null, null, null, 0, 0, 0, 0, 0,
                new UntranslatedStat[0], stats, null);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (SkillDefinition.CreateActive("PoisonArrow", 0, "", null, activeSkill, levels),
                new Skill("PoisonArrow", 1, 0, ItemSlot.Belt, 0, null));
        }

        [Test]
        public void AbyssalCryHasSecondaryHitDamageSource()
        {
            var (definition, skill) = CreateAbyssalCryDefinition();
            var valueCalculationContext = MockValueCalculationContext(skill, true);
            var sut = CreateSut(definition);

            var result = sut.Parse(skill);

            var actual = GetValueForIdentity(result.Modifiers, "SkillHitDamageSource")
                .Calculate(valueCalculationContext);
            Assert.AreEqual((double) DamageSource.Secondary, actual.Single());
        }

        private static (SkillDefinition, Skill) CreateAbyssalCryDefinition()
        {
            var activeSkill = new ActiveSkillDefinition("AbyssalCry", 0, new[] { "spell" }, new string[0],
                new[] { Keyword.Spell, Keyword.Projectile, Keyword.Totem }, false, null, new ItemClass[0]);
            var stats = new[]
            {
                new UntranslatedStat("display_skill_deals_secondary_damage", 1),
            };
            var level = new SkillLevelDefinition(null, null, null, null, null, null, 0, 0, 0, 0, 0,
                new UntranslatedStat[0], stats, null);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (SkillDefinition.CreateActive("AbyssalCry", 0, "", null, activeSkill, levels),
                new Skill("AbyssalCry", 1, 0, ItemSlot.Belt, 0, null));
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

        private static bool AnyModifierHasIdentity(IEnumerable<Modifier> modifiers, string identity)
            => modifiers.Any(m => m.Stats.Any(s => s.Identity == identity));

        private static IValue GetValueForIdentity(IEnumerable<Modifier> modifiers, string identity)
            => modifiers.First(m => m.Stats.First().Identity == identity).Value;
    }
}
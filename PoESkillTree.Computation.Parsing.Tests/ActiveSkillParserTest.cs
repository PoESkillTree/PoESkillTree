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
using static PoESkillTree.Computation.Common.Tests.Helper;

namespace PoESkillTree.Computation.Parsing.Tests
{
    [TestFixture]
    public class ActiveSkillParserTest
    {
        [TestCase(Tags.Shield)]
        [TestCase(Tags.Weapon)]
        public void FrenzyUsesOffHandIfWeapon(Tags offHandTags)
        {
            var (definition, skill) = CreateFrenzyDefinition();
            var valueCalculationContext = MockValueCalculationContext(skill, true,
                ("OffHand.ItemTags", offHandTags.EncodeAsDouble()));
            var sut = CreateSut(definition);

            var result = sut.Parse(skill);

            var actual = GetValueForIdentity(result.Modifiers, "SkillUses.OffHand")
                .Calculate(valueCalculationContext);
            Assert.AreEqual(offHandTags.HasFlag(Tags.Weapon), actual.IsTrue());
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
                new Skill("Frenzy", 1, 0, ItemSlot.Belt, 0, 0));
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

        [Test]
        public void FlameTotemHasCastTimeModifier()
        {
            var (definition, skill) = CreateFlameTotemDefinition();
            var expected = (NodeValue?) 1000D / definition.ActiveSkill.CastTime;
            var valueCalculationContext = MockValueCalculationContext(skill, true);
            var sut = CreateSut(definition);

            var result = sut.Parse(skill);

            var actual = GetValueForIdentity(result.Modifiers, "CastRate.Spell.Skill")
                .Calculate(valueCalculationContext);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void FlameTotemHasTotemLifeModifier()
        {
            var (definition, skill) = CreateFlameTotemDefinition();
            var valueCalculationContext = MockValueCalculationContext(skill, true);
            var sut = CreateSut(definition);

            var result = sut.Parse(skill);

            var lifeModifier = result.Modifiers.First(m => m.Stats.First().Identity == "Life");
            Assert.AreEqual(Entity.Totem, lifeModifier.Stats.First().Entity);
            Assert.AreEqual(Form.More, lifeModifier.Form);
            var actual = lifeModifier.Value.Calculate(valueCalculationContext);
            Assert.AreEqual(new NodeValue(62), actual);
        }

        [Test]
        public void FlameTotemSetsCriticalStrikeChance()
        {
            var (definition, skill) = CreateFlameTotemDefinition();
            var valueCalculationContext = MockValueCalculationContext(skill, true);
            var sut = CreateSut(definition);

            var result = sut.Parse(skill);

            var modifier = result.Modifiers.First(m => m.Stats.First().Identity == "CriticalStrike.Chance.Spell.Skill");
            Assert.AreEqual(Form.BaseSet, modifier.Form);
            var actual = modifier.Value.Calculate(valueCalculationContext);
            Assert.AreEqual(new NodeValue(5), actual);
        }

        [Test]
        public void FlameTotemHasCorrectRequirements()
        {
            var (definition, skill) = CreateFlameTotemDefinition();
            var valueCalculationContext = MockValueCalculationContext(skill, true);
            var sut = CreateSut(definition);

            var result = sut.Parse(skill);

            var modifiers = result.Modifiers;
            Assert.IsFalse(AnyModifierHasIdentity(modifiers, "Dexterity.Required"));
            var actualInt = GetValueForIdentity(modifiers, "Intelligence.Required").Calculate(valueCalculationContext);
            Assert.AreEqual(new NodeValue(68), actualInt);
            var actualStr = GetValueForIdentity(modifiers, "Strength.Required").Calculate(valueCalculationContext);
            Assert.AreEqual(new NodeValue(98), actualStr);
        }

        [Test]
        public void FlameTotemSetsSpellDamage()
        {
            var (definition, skill) = CreateFlameTotemDefinition();
            var valueCalculationContext = MockValueCalculationContext(skill, true);
            var sut = CreateSut(definition);

            var result = sut.Parse(skill);

            var modifiers = result.Modifiers;
            var actual = GetValueForIdentity(modifiers, "Fire.Damage.Spell.Skill").Calculate(valueCalculationContext);
            Assert.AreEqual(new NodeValue(5, 10), actual);
            Assert.IsFalse(AnyModifierHasIdentity(modifiers, "Fire.Damage.Spell.Ignite"));
        }

        [Test]
        public void FlameTotemStatsAreParsedCorrectly()
        {
            var (definition, skill) = CreateFlameTotemDefinition();
            var source = new ModifierSource.Local.Skill("Flame Totem");
            var parseResults = new[]
            {
                ParseResult.Success(new[] { MockModifier(new Stat("s1")) }),
                ParseResult.Success(new[] { MockModifier(new Stat("s2")) }),
            };
            var parseParameters = new[]
            {
                new UntranslatedStatParserParameter(source, new[]
                {
                    new UntranslatedStat("totem_life_+%", 10),
                }),
                new UntranslatedStatParserParameter(source, new[]
                {
                    new UntranslatedStat("number_of_additional_projectiles", 2),
                }),
            };
            var statParser = Mock.Of<IParser<UntranslatedStatParserParameter>>(p =>
                p.Parse(parseParameters[0]) == parseResults[0] &&
                p.Parse(parseParameters[1]) == parseResults[1]);
            var sut = CreateSut(definition, statParser);

            var result = sut.Parse(skill);

            var modifiers = result.Modifiers;
            Assert.IsTrue(AnyModifierHasIdentity(modifiers, "s1"));
            Assert.IsTrue(AnyModifierHasIdentity(modifiers, "s2"));
        }

        private static (SkillDefinition, Skill) CreateFlameTotemDefinition()
        {
            var activeSkill = new ActiveSkillDefinition("Flame Totem", 250, new[] { "spell" }, new string[0],
                new[] { Keyword.Spell, Keyword.Projectile, Keyword.Totem }, false, 1.62, new ItemClass[0]);
            var qualityStats = new[]
            {
                new UntranslatedStat("totem_life_+%", 1000),
            };
            var stats = new[]
            {
                new UntranslatedStat("spell_minimum_base_fire_damage", 5),
                new UntranslatedStat("spell_maximum_base_fire_damage", 10),
                new UntranslatedStat("number_of_additional_projectiles", 2),
            };
            var level = new SkillLevelDefinition(null, null, 5, null, null, null, null, 0, 0, 68, 98,
                qualityStats, stats, null);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (SkillDefinition.CreateActive("FlameTotem", 0, "", null, activeSkill, levels),
                new Skill("FlameTotem", 1, 10, ItemSlot.Belt, 0, 0));
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

        [Test]
        public void ContagionSetsDamageOverTime()
        {
            var (definition, skill) = CreateContagionDefinition();
            var valueCalculationContext = MockValueCalculationContext(skill, true);
            var sut = CreateSut(definition);

            var result = sut.Parse(skill);

            var modifiers = result.Modifiers;
            var actual = GetValueForIdentity(modifiers, "Chaos.Damage.OverTime.Skill")
                .Calculate(valueCalculationContext);
            Assert.AreEqual(new NodeValue(1), actual);
        }

        [Test]
        public void ContagionHasNoRequirementsWhenNotAGem()
        {
            var (definition, skill) = CreateContagionDefinition();
            var sut = CreateSut(definition);

            var result = sut.Parse(skill);

            var modifiers = result.Modifiers;
            Assert.IsFalse(AnyModifierHasIdentity(modifiers, "Level.Required"));
            Assert.IsFalse(AnyModifierHasIdentity(modifiers, "Intelligence.Required"));
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

        [Test]
        public void ShieldChargeDoesNotUseOffHand()
        {
            var (definition, skill) = CreateShieldChargeDefinition();
            var sut = CreateSut(definition);

            var result = sut.Parse(skill);

            var modifiers = result.Modifiers;
            Assert.IsFalse(AnyModifierHasIdentity(modifiers, "SkillUses.OffHand"));
        }

        [TestCase(Tags.Shield)]
        [TestCase(Tags.Weapon)]
        public void ShieldChargeUsesMainHandIfOffHandHasShield(Tags offHandTags)
        {
            var (definition, skill) = CreateShieldChargeDefinition();
            var valueCalculationContext = MockValueCalculationContext(skill, true,
                ("OffHand.ItemTags", offHandTags.EncodeAsDouble()));
            var sut = CreateSut(definition);

            var result = sut.Parse(skill);

            var actual = GetValueForIdentity(result.Modifiers, "SkillUses.MainHand")
                .Calculate(valueCalculationContext);
            Assert.AreEqual(offHandTags.HasFlag(Tags.Shield), actual.IsTrue());
        }

        private static (SkillDefinition, Skill) CreateShieldChargeDefinition()
        {
            var types = new[]
                { ActiveSkillType.Attack, ActiveSkillType.DoesNotUseOffHand, ActiveSkillType.RequiresShield };
            var activeSkill = new ActiveSkillDefinition("ShieldCharge", 0, types, new string[0],
                new[] { Keyword.Attack, Keyword.AreaOfEffect }, false, null, new ItemClass[0]);
            var stats = new[] { new UntranslatedStat("is_area_damage", 1), };
            var level = new SkillLevelDefinition(null, null, null, null, null, null, 0, 0, 0, 0, 0,
                new UntranslatedStat[0], stats, null);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (SkillDefinition.CreateActive("ShieldCharge", 0, "", null, activeSkill, levels),
                new Skill("ShieldCharge", 1, 0, ItemSlot.Belt, 0, null));
        }

        [TestCase(Tags.Shield)]
        [TestCase(Tags.Weapon)]
        public void DualStrikeUsesMainHandIfOffHandHasWeapon(Tags offHandTags)
        {
            var (definition, skill) = CreateDualStrikeDefinition();
            var valueCalculationContext = MockValueCalculationContext(skill, true,
                ("MainHand.ItemClass", (double) ItemClass.Claw),
                ("OffHand.ItemTags", offHandTags.EncodeAsDouble()));
            var sut = CreateSut(definition);

            var result = sut.Parse(skill);

            var actual = GetValueForIdentity(result.Modifiers, "SkillUses.MainHand")
                .Calculate(valueCalculationContext);
            Assert.AreEqual(offHandTags.HasFlag(Tags.Weapon), actual.IsTrue());
        }

        [TestCase(ItemClass.Claw)]
        [TestCase(ItemClass.Wand)]
        public void DualStrikeUsesMainHandIfWeaponRestrictionsAreSatisfied(ItemClass mainHandClass)
        {
            var (definition, skill) = CreateDualStrikeDefinition();
            var expected = definition.ActiveSkill.WeaponRestrictions.Contains(mainHandClass);
            var valueCalculationContext = MockValueCalculationContext(skill, true,
                ("MainHand.ItemClass", (double) mainHandClass),
                ("OffHand.ItemTags", Tags.Weapon.EncodeAsDouble()));
            var sut = CreateSut(definition);

            var result = sut.Parse(skill);

            var actual = GetValueForIdentity(result.Modifiers, "SkillUses.MainHand")
                .Calculate(valueCalculationContext);
            Assert.AreEqual(expected, actual.IsTrue());
        }

        [TestCase(ItemClass.Claw)]
        [TestCase(ItemClass.Wand)]
        public void DualStrikeUsesOffHandIfWeaponRestrictionsAreSatisfied(ItemClass mainHandClass)
        {
            var (definition, skill) = CreateDualStrikeDefinition();
            var expected = definition.ActiveSkill.WeaponRestrictions.Contains(mainHandClass);
            var valueCalculationContext = MockValueCalculationContext(skill, true,
                ("OffHand.ItemClass", (double) mainHandClass),
                ("OffHand.ItemTags", Tags.Weapon.EncodeAsDouble()));
            var sut = CreateSut(definition);

            var result = sut.Parse(skill);

            var actual = GetValueForIdentity(result.Modifiers, "SkillUses.OffHand")
                .Calculate(valueCalculationContext);
            Assert.AreEqual(expected, actual.IsTrue());
        }

        private static (SkillDefinition, Skill) CreateDualStrikeDefinition()
        {
            var types = new[] { ActiveSkillType.Attack, ActiveSkillType.RequiresDualWield };
            var weaponRestrictions = new[] { ItemClass.Claw, ItemClass.Dagger };
            var activeSkill = new ActiveSkillDefinition("DualStrike", 0, types, new string[0],
                new[] { Keyword.Attack }, false, null, weaponRestrictions);
            var level = new SkillLevelDefinition(null, null, null, null, null, null, 0, 0, 0, 0, 0,
                new UntranslatedStat[0], new UntranslatedStat[0], null);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (SkillDefinition.CreateActive("DualStrike", 0, "", null, activeSkill, levels),
                new Skill("DualStrike", 1, 0, ItemSlot.Belt, 0, null));
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

        [Test]
        public void AbyssalCryHasCooldown()
        {
            var (definition, skill) = CreateAbyssalCryDefinition();
            var valueCalculationContext = MockValueCalculationContext(skill, true);
            var sut = CreateSut(definition);

            var result = sut.Parse(skill);

            var modifier = result.Modifiers.First(m => m.Stats.First().Identity == "Cooldown");
            Assert.AreEqual(Form.BaseSet, modifier.Form);
            var actual = modifier.Value.Calculate(valueCalculationContext);
            Assert.AreEqual(new NodeValue(4000), actual);
        }

        private static (SkillDefinition, Skill) CreateAbyssalCryDefinition()
        {
            var activeSkill = new ActiveSkillDefinition("AbyssalCry", 0, new[] { "spell" }, new string[0],
                new[] { Keyword.Spell, Keyword.Projectile, Keyword.Totem }, false, null, new ItemClass[0]);
            var stats = new[]
            {
                new UntranslatedStat("display_skill_deals_secondary_damage", 1),
            };
            var level = new SkillLevelDefinition(null, null, null, null, null, null, 4000, 0, 0, 0, 0,
                new UntranslatedStat[0], stats, null);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (SkillDefinition.CreateActive("AbyssalCry", 0, "", null, activeSkill, levels),
                new Skill("AbyssalCry", 1, 0, ItemSlot.Belt, 0, null));
        }

        [Test]
        public void DoubleStrikeAddsToHitsPerCast()
        {
            var (definition, skill) = CreateDoubleStrikeDefinition();
            var sut = CreateSut(definition);

            var result = sut.Parse(skill);

            Assert.IsTrue(AnyModifierHasIdentity(result.Modifiers, "SkillNumberOfHitsPerCast"));
        }

        private static (SkillDefinition, Skill) CreateDoubleStrikeDefinition()
        {
            var activeSkill = new ActiveSkillDefinition("DoubleStrike", 0, new[] { "attack" }, new string[0],
                new[] { Keyword.Attack }, false, null, new ItemClass[0]);
            var stats = new[]
            {
                new UntranslatedStat("base_skill_number_of_additional_hits", 1),
            };
            var level = new SkillLevelDefinition(null, null, null, null, null, null, 4000, 0, 0, 0, 0,
                new UntranslatedStat[0], stats, null);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (SkillDefinition.CreateActive("DoubleStrike", 0, "", null, activeSkill, levels),
                new Skill("DoubleStrike", 1, 0, ItemSlot.Belt, 0, null));
        }

        [Test]
        public void CleaveSetsHitsWithBothWeaponsAtOnce()
        {
            var (definition, skill) = CreateCleaveDefinition();
            var sut = CreateSut(definition);

            var result = sut.Parse(skill);

            Assert.IsTrue(AnyModifierHasIdentity(result.Modifiers, "SkillDoubleHitsWhenDualWielding"));
        }

        private static (SkillDefinition, Skill) CreateCleaveDefinition()
        {
            var activeSkill = new ActiveSkillDefinition("Cleave", 0, new[] { "attack" }, new string[0],
                new[] { Keyword.Attack }, false, null, new ItemClass[0]);
            var stats = new[]
            {
                new UntranslatedStat("skill_double_hits_when_dual_wielding", 1),
            };
            var level = new SkillLevelDefinition(null, null, null, null, null, null, 4000, 0, 0, 0, 0,
                new UntranslatedStat[0], stats, null);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (SkillDefinition.CreateActive("Cleave", 0, "", null, activeSkill, levels),
                new Skill("Cleave", 1, 0, ItemSlot.Belt, 0, null));
        }

        private static ActiveSkillParser CreateSut(
            SkillDefinition skillDefinition, IParser<UntranslatedStatParserParameter> statParser = null)
        {
            var skillDefinitions = new SkillDefinitions(new[] { skillDefinition });
            var statFactory = new StatFactory();
            var builderFactories = new BuilderFactories(statFactory, new SkillDefinition[0]);
            var metaStatBuilders = new MetaStatBuilders(statFactory);
            if (statParser is null)
            {
                statParser = Mock.Of<IParser<UntranslatedStatParserParameter>>(p =>
                    p.Parse(It.IsAny<UntranslatedStatParserParameter>()) == ParseResult.Success(new Modifier[0]));
            }
            return new ActiveSkillParser(skillDefinitions, builderFactories, metaStatBuilders, statParser);
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
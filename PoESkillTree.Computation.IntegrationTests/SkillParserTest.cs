using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Parsing;
using PoESkillTree.Computation.Parsing.SkillParsers;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;
using PoESkillTree.Utils.Extensions;
using static PoESkillTree.Computation.IntegrationTests.ParsingTestUtils;

namespace PoESkillTree.Computation.IntegrationTests
{
    [TestFixture]
    public class SkillParserTest : CompositionRootTestBase
    {
        private SkillDefinitions _skillDefinitions;
        private IParser<Skill> _activeSkillParser;
        private IParser<SupportSkillParserParameter> _supportSkillParser;

        [SetUp]
        public async Task SetUpAsync()
        {
            _skillDefinitions = await CompositionRoot.SkillDefinitions.ConfigureAwait(false);
            _activeSkillParser = await CompositionRoot.ActiveSkillParser.ConfigureAwait(false);
            _supportSkillParser = await CompositionRoot.SupportSkillParser.ConfigureAwait(false);
        }

        [Test]
        public void ParseFrenzyReturnsCorrectResult()
        {
            var frenzy = new Skill("Frenzy", 20, 20, ItemSlot.Boots, 0, 0);
            var definition = _skillDefinitions.GetSkillById("Frenzy");
            var levelDefinition = definition.Levels[20];
            var local = new ModifierSource.Local.Skill("Frenzy");
            var global = new ModifierSource.Global(local);
            var gemSource = new ModifierSource.Local.Gem(ItemSlot.Boots, 0, "Frenzy");
            var valueCalculationContextMock = new Mock<IValueCalculationContext>();
            SetupIsActiveSkillInContext(valueCalculationContextMock, frenzy);
            var offHandTagsStat = new Stat("OffHand.ItemTags");
            valueCalculationContextMock.Setup(c => c.GetValue(offHandTagsStat, NodeType.Total, PathDefinition.MainPath))
                .Returns(new NodeValue(Tags.Weapon.EncodeAsDouble()));
            var mainHandTagsStat = new Stat("MainHand.ItemTags");
            valueCalculationContextMock
                .Setup(c => c.GetValue(mainHandTagsStat, NodeType.Total, PathDefinition.MainPath))
                .Returns(new NodeValue(Tags.Ranged.EncodeAsDouble()));
            var frenzyAmountStat = new Stat("Frenzy.Amount");
            valueCalculationContextMock
                .Setup(c => c.GetValue(frenzyAmountStat, NodeType.Total, PathDefinition.MainPath))
                .Returns(new NodeValue(3));
            var baseCostStat = new Stat("Boots.0.Cost");
            valueCalculationContextMock
                .Setup(c => c.GetValue(baseCostStat, NodeType.Total, PathDefinition.MainPath))
                .Returns((NodeValue?) levelDefinition.ManaCost);
            var isMainSkillStat = new Stat("Boots.0.IsMainSkill");
            var expectedModifiers =
                new (string stat, Form form, double? value, ModifierSource source, bool mainSkillOnly)[]
                {
                    ("SkillHitDamageSource", Form.TotalOverride, (int) DamageSource.Attack, global, true),
                    ("SkillUses.MainHand", Form.TotalOverride, 1, global, true),
                    ("SkillUses.OffHand", Form.TotalOverride, 1, global, true),
                    ("MainSkill.Id", Form.TotalOverride, definition.NumericId, global, true),
                    ("BaseCastTime.Spell.Skill", Form.BaseSet, definition.ActiveSkill.CastTime / 1000D, global, true),
                    ("BaseCastTime.Secondary.Skill", Form.BaseSet, definition.ActiveSkill.CastTime / 1000D, global,
                        true),
                    ("Frenzy.ActiveSkillItemSlot", Form.BaseSet, (double) frenzy.ItemSlot, global, false),
                    ("Frenzy.ActiveSkillSocketIndex", Form.BaseSet, frenzy.SocketIndex, global, false),
                    ("Frenzy.Instances", Form.BaseAdd, 1, global, false),
                    ("Skills[].Instances", Form.BaseAdd, 1, global, false),
                    ("Skills[Attack].Instances", Form.BaseAdd, 1, global, false),
                    ("Skills[Projectile].Instances", Form.BaseAdd, 1, global, false),
                    ("Skills[Melee].Instances", Form.BaseAdd, 1, global, false),
                    ("Skills[Bow].Instances", Form.BaseAdd, 1, global, false),
                    ("MainSkill.Has.Attack", Form.TotalOverride, 1, global, true),
                    ("MainSkill.Has.Projectile", Form.TotalOverride, 1, global, true),
                    ("MainSkill.Has.Melee", Form.TotalOverride, 1, global, true),
                    ("MainSkill.Has.Bow", Form.TotalOverride, 1, global, true),
                    ("MainSkillPart.Has.Attack", Form.TotalOverride, 1, global, true),
                    ("MainSkillPart.Has.Projectile", Form.TotalOverride, 1, global, true),
                    ("MainSkillPart.Has.Melee", Form.TotalOverride, null, global, true),
                    ("MainSkillPart.Has.Bow", Form.TotalOverride, 1, global, true),
                    ("MainSkillPart.CastRate.Has.Attack", Form.TotalOverride, 1, global, true),
                    ("MainSkillPart.CastRate.Has.Projectile", Form.TotalOverride, 1, global, true),
                    ("MainSkillPart.CastRate.Has.Melee", Form.TotalOverride, null, global, true),
                    ("MainSkillPart.CastRate.Has.Bow", Form.TotalOverride, 1, global, true),
                    ("MainSkillPart.Damage.Attack.Has.Attack", Form.TotalOverride, 1, global, true),
                    ("MainSkillPart.Damage.Attack.Has.Projectile", Form.TotalOverride, 1, global, true),
                    ("MainSkillPart.Damage.Attack.Has.Melee", Form.TotalOverride, null, global, true),
                    ("MainSkillPart.Damage.Attack.Has.Bow", Form.TotalOverride, 1, global, true),
                    ("MainSkillPart.Damage.Spell.Has.Projectile", Form.TotalOverride, 1, global, true),
                    ("MainSkillPart.Damage.Secondary.Has.Projectile", Form.TotalOverride, 1, global, true),
                    ("Boots.0.Type.attack", Form.TotalOverride, 1, global, false),
                    ("Boots.0.Type.projectile_attack", Form.TotalOverride, 1, global, false),
                    ("Boots.0.Type.mirage_archer_supportable", Form.TotalOverride, 1, global, false),
                    ("Boots.0.Type.projectile", Form.TotalOverride, 1, global, false),
                    ("Boots.0.Type.volley_supportable", Form.TotalOverride, 1, global, false),
                    ("Boots.0.Type.ranged_attack_totem_supportable", Form.TotalOverride, 1, global, false),
                    ("Boots.0.Type.trap_supportable", Form.TotalOverride, 1, global, false),
                    ("Boots.0.Type.remote_mine_supportable", Form.TotalOverride, 1, global, false),
                    ("Boots.0.Type.melee_single_target_initial_hit", Form.TotalOverride, 1, global, false),
                    ("Boots.0.Type.multistrike_supportable", Form.TotalOverride, 1, global, false),
                    ("Boots.0.Type.melee", Form.TotalOverride, 1, global, false),
                    ("DamageBaseAddEffectiveness", Form.TotalOverride, levelDefinition.DamageEffectiveness, global,
                        true),
                    ("DamageBaseSetEffectiveness", Form.TotalOverride, levelDefinition.DamageMultiplier, global, true),
                    ("Boots.0.Cost", Form.BaseSet, levelDefinition.ManaCost, global, false),
                    ("Mana.Cost", Form.BaseSet, levelDefinition.ManaCost, global, true),
                    ("Frenzy.Reservation", Form.BaseSet, null, global, false),
                    ("Life.Reservation", Form.BaseAdd, null, global, false),
                    ("EnergyShield.Reservation", Form.BaseAdd, null, global, false),
                    ("Mana.Reservation", Form.BaseAdd, null, global, false),
                    ("Level.Required", Form.BaseSet, levelDefinition.Requirements.Level, gemSource, false),
                    ("Dexterity.Required", Form.BaseSet, levelDefinition.Requirements.Dexterity, gemSource, false),
                    ("CastRate.Attack.MainHand.Skill", Form.Increase, levelDefinition.QualityStats[0].Value * 20 / 1000,
                        global, true),
                    ("CastRate.Attack.OffHand.Skill", Form.Increase, levelDefinition.QualityStats[0].Value * 20 / 1000,
                        global, true),
                    ("Physical.Damage.Attack.MainHand.Skill", Form.Increase, levelDefinition.Stats[1].Value * 3, global,
                        true),
                    ("Physical.Damage.Attack.OffHand.Skill", Form.Increase, levelDefinition.Stats[1].Value * 3, global,
                        true),
                    ("Physical.Damage.Spell.Skill", Form.Increase, levelDefinition.Stats[1].Value * 3, global,
                        true),
                    ("Physical.Damage.Secondary.Skill", Form.Increase, levelDefinition.Stats[1].Value * 3, global,
                        true),
                    ("Physical.Damage.OverTime.Skill", Form.Increase, levelDefinition.Stats[1].Value * 3, global,
                        true),
                    ("Physical.Damage.Attack.MainHand.Ignite", Form.Increase, levelDefinition.Stats[1].Value * 3,
                        global, true),
                    ("Physical.Damage.Attack.MainHand.Bleed", Form.Increase, levelDefinition.Stats[1].Value * 3, global,
                        true),
                    ("Physical.Damage.Attack.MainHand.Poison", Form.Increase, levelDefinition.Stats[1].Value * 3,
                        global, true),
                    ("Physical.Damage.Attack.OffHand.Ignite", Form.Increase, levelDefinition.Stats[1].Value * 3, global,
                        true),
                    ("Physical.Damage.Attack.OffHand.Bleed", Form.Increase, levelDefinition.Stats[1].Value * 3, global,
                        true),
                    ("Physical.Damage.Attack.OffHand.Poison", Form.Increase, levelDefinition.Stats[1].Value * 3, global,
                        true),
                    ("Physical.Damage.Spell.Ignite", Form.Increase, levelDefinition.Stats[1].Value * 3, global,
                        true),
                    ("Physical.Damage.Spell.Bleed", Form.Increase, levelDefinition.Stats[1].Value * 3, global,
                        true),
                    ("Physical.Damage.Spell.Poison", Form.Increase, levelDefinition.Stats[1].Value * 3, global,
                        true),
                    ("Physical.Damage.Secondary.Ignite", Form.Increase, levelDefinition.Stats[1].Value * 3, global,
                        true),
                    ("Physical.Damage.Secondary.Bleed", Form.Increase, levelDefinition.Stats[1].Value * 3, global,
                        true),
                    ("Physical.Damage.Secondary.Poison", Form.Increase, levelDefinition.Stats[1].Value * 3, global,
                        true),
                    ("CastRate.Attack.MainHand.Skill", Form.Increase, levelDefinition.Stats[1].Value * 3, global,
                        true),
                    ("CastRate.Attack.OffHand.Skill", Form.Increase, levelDefinition.Stats[1].Value * 3, global, true),
                    ("Range.Attack.MainHand.Skill", Form.BaseAdd, null, global, true),
                    ("Range.Attack.OffHand.Skill", Form.BaseAdd, null, global, true),
                }.Select(t => (t.stat, t.form, (NodeValue?) t.value, t.source, t.mainSkillOnly)).ToArray();

            var actual = _activeSkillParser.Parse(frenzy);

            AssertCorrectModifiers(valueCalculationContextMock, isMainSkillStat, expectedModifiers, actual);
        }

        [Test]
        public void ParseAddedColdDamageSupportReturnsCorrectResult()
        {
            var frenzy = new Skill("Frenzy", 20, 20, ItemSlot.Boots, 0, 0);
            var support = new Skill("SupportAddedColdDamage", 20, 20, ItemSlot.Boots, 1, 0);
            var definition = _skillDefinitions.GetSkillById(support.Id);
            var levelDefinition = definition.Levels[20];
            var local = new ModifierSource.Local.Skill("SupportAddedColdDamage");
            var global = new ModifierSource.Global(local);
            var gemSource =
                new ModifierSource.Local.Gem(support.ItemSlot, support.SocketIndex, "SupportAddedColdDamage");
            var valueCalculationContextMock = new Mock<IValueCalculationContext>();
            SetupIsActiveSkillInContext(valueCalculationContextMock, frenzy);
            var isMainSkillStat = new Stat("Boots.0.IsMainSkill");
            var addedDamageValue = new NodeValue(levelDefinition.Stats[0].Value, levelDefinition.Stats[1].Value);
            var expectedModifiers =
                new (string stat, Form form, double? value, ModifierSource source, bool mainSkillOnly)[]
                {
                    ("SupportAddedColdDamage.ActiveSkillItemSlot",
                        Form.BaseSet, (double) support.ItemSlot, global, false),
                    ("SupportAddedColdDamage.ActiveSkillSocketIndex",
                        Form.BaseSet, support.SocketIndex, global, false),
                    ("Mana.Cost", Form.More, levelDefinition.ManaMultiplier * 100 - 100, global, true),
                    ("Frenzy.Reservation", Form.More, levelDefinition.ManaMultiplier * 100 - 100, global, false),
                    ("Level.Required", Form.BaseSet, levelDefinition.Requirements.Level, gemSource, false),
                    ("Dexterity.Required", Form.BaseSet, levelDefinition.Requirements.Dexterity, gemSource, false),
                    ("Cold.Damage.Attack.MainHand.Skill", Form.Increase,
                        levelDefinition.QualityStats[0].Value * 20 / 1000, global, true),
                    ("Cold.Damage.Attack.OffHand.Skill", Form.Increase,
                        levelDefinition.QualityStats[0].Value * 20 / 1000, global, true),
                    ("Cold.Damage.Spell.Skill", Form.Increase, levelDefinition.QualityStats[0].Value * 20 / 1000,
                        global, true),
                    ("Cold.Damage.Secondary.Skill", Form.Increase, levelDefinition.QualityStats[0].Value * 20 / 1000,
                        global, true),
                    ("Cold.Damage.OverTime.Skill", Form.Increase, levelDefinition.QualityStats[0].Value * 20 / 1000,
                        global, true),
                    ("Cold.Damage.Attack.MainHand.Ignite", Form.Increase,
                        levelDefinition.QualityStats[0].Value * 20 / 1000, global, true),
                    ("Cold.Damage.Attack.MainHand.Bleed", Form.Increase,
                        levelDefinition.QualityStats[0].Value * 20 / 1000, global, true),
                    ("Cold.Damage.Attack.MainHand.Poison", Form.Increase,
                        levelDefinition.QualityStats[0].Value * 20 / 1000, global, true),
                    ("Cold.Damage.Attack.OffHand.Ignite", Form.Increase,
                        levelDefinition.QualityStats[0].Value * 20 / 1000, global, true),
                    ("Cold.Damage.Attack.OffHand.Bleed", Form.Increase,
                        levelDefinition.QualityStats[0].Value * 20 / 1000, global, true),
                    ("Cold.Damage.Attack.OffHand.Poison", Form.Increase,
                        levelDefinition.QualityStats[0].Value * 20 / 1000, global, true),
                    ("Cold.Damage.Spell.Ignite", Form.Increase, levelDefinition.QualityStats[0].Value * 20 / 1000,
                        global, true),
                    ("Cold.Damage.Spell.Bleed", Form.Increase, levelDefinition.QualityStats[0].Value * 20 / 1000,
                        global, true),
                    ("Cold.Damage.Spell.Poison", Form.Increase, levelDefinition.QualityStats[0].Value * 20 / 1000,
                        global, true),
                    ("Cold.Damage.Secondary.Ignite", Form.Increase, levelDefinition.QualityStats[0].Value * 20 / 1000,
                        global, true),
                    ("Cold.Damage.Secondary.Bleed", Form.Increase, levelDefinition.QualityStats[0].Value * 20 / 1000,
                        global, true),
                    ("Cold.Damage.Secondary.Poison", Form.Increase, levelDefinition.QualityStats[0].Value * 20 / 1000,
                        global, true),
                }.Select(t => (t.stat, t.form, (NodeValue?) t.value, t.source, t.mainSkillOnly)).ToArray();
            expectedModifiers = expectedModifiers.Append(
                    ("Cold.Damage.Attack.MainHand.Skill", Form.BaseAdd, addedDamageValue, global, true),
                    ("Cold.Damage.Attack.OffHand.Skill", Form.BaseAdd, addedDamageValue, global, true),
                    ("Cold.Damage.Spell.Skill", Form.BaseAdd, addedDamageValue, global, true),
                    ("Cold.Damage.Secondary.Skill", Form.BaseAdd, addedDamageValue, global, true))
                .ToArray();

            var actual = _supportSkillParser.Parse(frenzy, support);

            AssertCorrectModifiers(valueCalculationContextMock, isMainSkillStat, expectedModifiers, actual);
        }

        private static void SetupIsActiveSkillInContext(
            Mock<IValueCalculationContext> contextMock, Skill frenzy)
        {
            var activeSkillItemSlotStat = new Stat("Frenzy.ActiveSkillItemSlot");
            contextMock
                .Setup(c => c.GetValue(activeSkillItemSlotStat, NodeType.Total, PathDefinition.MainPath))
                .Returns(new NodeValue((double) frenzy.ItemSlot));
            var activeSkillSocketIndexStat = new Stat("Frenzy.ActiveSkillSocketIndex");
            contextMock
                .Setup(c => c.GetValue(activeSkillSocketIndexStat, NodeType.Total, PathDefinition.MainPath))
                .Returns(new NodeValue(frenzy.SocketIndex));
        }

        private static void AssertCorrectModifiers(
            Mock<IValueCalculationContext> contextMock,
            Stat isMainSkillStat,
            (string stat, Form form, NodeValue? value, ModifierSource source, bool mainSkillOnly)[] expectedModifiers,
            ParseResult result)
        {
            var (failedLines, remainingSubstrings, modifiers) = result;

            Assert.IsEmpty(failedLines);
            Assert.IsEmpty(remainingSubstrings);
            Assert.AreEqual(expectedModifiers.Length, modifiers.Count);
            for (var i = 0; i < modifiers.Count; i++)
            {
                var expected = expectedModifiers[i];
                var actual = modifiers[i];
                Assert.That(actual.Stats, Has.One.Items);
                Assert.AreEqual(expected.stat, actual.Stats[0].Identity);
                Assert.AreEqual(Entity.Character, actual.Stats[0].Entity);
                Assert.AreEqual(expected.form, actual.Form);
                Assert.AreEqual(expected.source, actual.Source);

                contextMock
                    .Setup(c => c.GetValue(isMainSkillStat, NodeType.Total, PathDefinition.MainPath))
                    .Returns((NodeValue?) true);
                var expectedValue = expected.value;
                var actualValue = actual.Value.Calculate(contextMock.Object);
                Assert.AreEqual(expectedValue, actualValue);

                contextMock
                    .Setup(c => c.GetValue(isMainSkillStat, NodeType.Total, PathDefinition.MainPath))
                    .Returns((NodeValue?) false);
                expectedValue = expected.mainSkillOnly ? null : expected.value;
                actualValue = actual.Value.Calculate(contextMock.Object);
                Assert.AreEqual(expectedValue, actualValue);
            }
        }

        [TestCaseSource(nameof(ReadParseableSkills))]
        public void SkillIsParsedSuccessfully(string skillId)
        {
            var actual = Parse(skillId);

            AssertIsParsedSuccessfully(actual, NotParseableStatLines.Value);
        }

        [TestCaseSource(nameof(ReadNotParseableSkills))]
        public void SkillIsParsedUnsuccessfully(string skillId)
        {
            var actual = Parse(skillId);

            AssertIsParsedUnsuccessfully(actual);
        }

        private ParseResult Parse(string skillId)
        {
            var definition = _skillDefinitions.GetSkillById(skillId);
            var level = definition.Levels.ContainsKey(20) ? 20 : 3;
            if (definition.IsSupport)
            {
                var activeSkill = new Skill("BloodRage", 20, 20, default, 0, 0);
                var supportSkill = new Skill(skillId, level, 20, default, 1, 0);
                return _supportSkillParser.Parse(activeSkill, supportSkill);
            }
            else
            {
                var skill = new Skill(skillId, level, 20, default, 0, 0);
                return _activeSkillParser.Parse(skill);
            }
        }

        private static IEnumerable<string> ReadParseableSkills()
            => ReadDataLines("ParseableSkills");

        private static IEnumerable<string> ReadNotParseableSkills()
            => ReadDataLines("NotParseableSkills");
    }
}
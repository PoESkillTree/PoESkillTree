using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Parsing;
using PoESkillTree.Computation.Parsing.SkillParsers;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;
using PoESkillTree.GameModel.StatTranslation;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.IntegrationTests.Parsing
{
    [TestFixture]
    public class SkillParserTest : CompositionRootTestBase
    {
        private static SkillDefinitions _skillDefinitions;
        private static IBuilderFactories _builderFactories;
        private static ICoreParser _coreParser;
        private static IMetaStatBuilders _metaStats;
        private static StatTranslationLoader _statTranslationLoader;

        [OneTimeSetUp]
        public static async Task OneTimeSetUpAsync()
        {
            _skillDefinitions = CompositionRoot.SkillDefinitions;
            _builderFactories = CompositionRoot.BuilderFactories;
            _coreParser = CompositionRoot.CoreParser;
            _metaStats = CompositionRoot.MetaStats;
            _statTranslationLoader = new StatTranslationLoader();
            await _statTranslationLoader.LoadAsync("stat_translations/skill").ConfigureAwait(false);
            await _statTranslationLoader.LoadAsync("stat_translations/support_gem").ConfigureAwait(false);
        }

        private static IParser<UntranslatedStatParserParameter> CreateStatParser(string translationFileName)
            => new UntranslatedStatParser(_statTranslationLoader[translationFileName], _coreParser);

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
                    ("Frenzy.ActiveSkillItemSlot", Form.BaseSet, (double) frenzy.ItemSlot, global, false),
                    ("Frenzy.ActiveSkillSocketIndex", Form.BaseSet, frenzy.SocketIndex, global, false),
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
                    ("Level.Required", Form.BaseSet, levelDefinition.RequiredLevel, gemSource, false),
                    ("Dexterity.Required", Form.BaseSet, levelDefinition.RequiredDexterity, gemSource, false),
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
            var parser = new ActiveSkillParser(_skillDefinitions, _builderFactories, _metaStats, CreateStatParser);

            var actual = parser.Parse(frenzy);
            AssertCorrectModifiers(valueCalculationContextMock, isMainSkillStat, expectedModifiers, actual);
        }

        [Test]
        public void ParseAddedFireDamageSupportReturnsCorrectResult()
        {
            var frenzy = new Skill("Frenzy", 20, 20, ItemSlot.Boots, 0, 0);
            var support = new Skill("SupportAddedColdDamage", 20, 20, ItemSlot.Boots, 1, 0);
            var definition = _skillDefinitions.GetSkillById(support.Id);
            var levelDefinition = definition.Levels[20];
            var local = new ModifierSource.Local.Skill("Added Cold Damage Support");
            var global = new ModifierSource.Global(local);
            var gemSource =
                new ModifierSource.Local.Gem(support.ItemSlot, support.SocketIndex, "Added Cold Damage Support");
            var valueCalculationContextMock = new Mock<IValueCalculationContext>();
            var activeSkillItemSlotStat = new Stat("Frenzy.ActiveSkillItemSlot");
            valueCalculationContextMock
                .Setup(c => c.GetValue(activeSkillItemSlotStat, NodeType.Total, PathDefinition.MainPath))
                .Returns(new NodeValue((double) frenzy.ItemSlot));
            var activeSkillSocketIndexStat = new Stat("Frenzy.ActiveSkillSocketIndex");
            valueCalculationContextMock
                .Setup(c => c.GetValue(activeSkillSocketIndexStat, NodeType.Total, PathDefinition.MainPath))
                .Returns(new NodeValue(frenzy.SocketIndex));
            var isMainSkillStat = new Stat("Boots.0.IsMainSkill");
            var addedDamageValue = new NodeValue(levelDefinition.Stats[0].Value, levelDefinition.Stats[1].Value);
            var expectedModifiers =
                new (string stat, Form form, double? value, ModifierSource source, bool mainSkillOnly)[]
                {
                    ("Mana.Cost", Form.More, levelDefinition.ManaMultiplier * 100 - 100, global, true),
                    ("Frenzy.Reservation", Form.More, levelDefinition.ManaMultiplier * 100 - 100, global, false),
                    ("Level.Required", Form.BaseSet, levelDefinition.RequiredLevel, gemSource, false),
                    ("Dexterity.Required", Form.BaseSet, levelDefinition.RequiredDexterity, gemSource, false),
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
            var parser = new SupportSkillParser(_skillDefinitions, _builderFactories, _metaStats, CreateStatParser);

            var actual = parser.Parse(frenzy, support);
            AssertCorrectModifiers(valueCalculationContextMock, isMainSkillStat, expectedModifiers, actual);
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
    }
}
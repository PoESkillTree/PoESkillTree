using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Parsing;
using PoESkillTree.Computation.Parsing.SkillParsers;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;
using PoESkillTree.GameModel.StatTranslation;

namespace PoESkillTree.Computation.IntegrationTests.Parsing
{
    [TestFixture]
    public class SkillParserTest
    {
        private static SkillDefinitions _skillDefinitions;
        private static Console.CompositionRoot _compositionRoot;
        private static StatTranslationLoader _statTranslationLoader;

        [OneTimeSetUp]
        public static async Task OneTimeSetUpAsync()
        {
            _skillDefinitions = await SkillJsonDeserializer.DeserializeAsync().ConfigureAwait(false);
            _compositionRoot = new Console.CompositionRoot();
            _statTranslationLoader = new StatTranslationLoader();
            await _statTranslationLoader.LoadAsync("stat_translations/skill").ConfigureAwait(false);
        }

        private static IParser<UntranslatedStatParserParameter> CreateStatParser(string translationFileName)
            => new UntranslatedStatParser(_statTranslationLoader[translationFileName], _compositionRoot.CoreParser);

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
            var isMainSkillStat = new Stat("Boots.0.IsMainSkill");
            var expectedModifiers =
                new (string stat, Form form, double? value, ModifierSource source, bool mainSkillOnly)[]
                {
                    ("SkillHitDamageSource", Form.TotalOverride, (int) DamageSource.Attack, global, true),
                    ("SkillUses.MainHand", Form.TotalOverride, 1, global, true),
                    ("SkillUses.OffHand", Form.TotalOverride, 1, global, true),
                    ("MainSkill.Id", Form.TotalOverride, definition.NumericId, global, true),
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
                    ("DamageBaseAddEffectiveness", Form.TotalOverride, levelDefinition.DamageEffectiveness, global,
                        true),
                    ("DamageBaseSetEffectiveness", Form.TotalOverride, levelDefinition.DamageMultiplier, global, true),
                    ("Mana.Cost", Form.BaseSet, levelDefinition.ManaCost, global, true),
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
                };
            var parser = new ActiveSkillParser(_skillDefinitions, _compositionRoot.BuilderFactories,
                _compositionRoot.MetaStats, CreateStatParser);

            var (failedLines, remainingSubstrings, modifiers) = parser.Parse(frenzy);

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

                valueCalculationContextMock
                    .Setup(c => c.GetValue(isMainSkillStat, NodeType.Total, PathDefinition.MainPath))
                    .Returns((NodeValue?) true);
                var expectedValue = (NodeValue?) expected.value;
                var actualValue = actual.Value.Calculate(valueCalculationContextMock.Object);
                Assert.AreEqual(expectedValue, actualValue);

                valueCalculationContextMock
                    .Setup(c => c.GetValue(isMainSkillStat, NodeType.Total, PathDefinition.MainPath))
                    .Returns((NodeValue?) false);
                expectedValue = expected.mainSkillOnly ? null : (NodeValue?) expected.value;
                actualValue = actual.Value.Calculate(valueCalculationContextMock.Object);
                Assert.AreEqual(expectedValue, actualValue);
            }
        }
    }
}
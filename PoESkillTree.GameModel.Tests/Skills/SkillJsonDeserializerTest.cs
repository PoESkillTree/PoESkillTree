using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using PoESkillTree.GameModel.Items;

namespace PoESkillTree.GameModel.Skills
{
    [TestFixture]
    public class SkillJsonDeserializerTest
    {
        [Test]
        public void DeserializeReturnsCorrectResultForFrenzy()
        {
            var gemJson = JObject.Parse(TestUtils.ReadDataFile("frenzyGem.json"));
            var gemTooltipJson = JObject.Parse(TestUtils.ReadDataFile("frenzyGemTooltip.json"));

            var definitions = SkillJsonDeserializer.Deserialize(gemJson, gemTooltipJson);

            Assert.That(definitions.Skills, Has.One.Items);
            var definition = definitions.GetSkillById("Frenzy");
            Assert.AreEqual("Frenzy", definition.Id);
            Assert.AreEqual(0, definition.NumericId);
            Assert.IsFalse(definition.IsSupport);
            Assert.IsNull(definition.SupportSkill);

            var baseItem = definition.BaseItem;
            Assert.IsNotNull(baseItem);
            Assert.AreEqual("Frenzy", baseItem.DisplayName);
            Assert.AreEqual("Metadata/Items/Gems/SkillGemFrenzy", baseItem.MetadataId);
            Assert.AreEqual(ReleaseState.Released, baseItem.ReleaseState);
            Assert.AreEqual(new[] { "dexterity", "active_skill", "attack", "melee", "bow" }, baseItem.GemTags);

            var activeSkill = definition.ActiveSkill;
            Assert.AreEqual("Frenzy", activeSkill.DisplayName);
            Assert.That(activeSkill.ActiveSkillTypes, Has.Exactly(11).Items);
            Assert.IsEmpty(activeSkill.MinionActiveSkillTypes);
            Assert.AreEqual(new[] { Keyword.Attack, Keyword.Projectile, Keyword.Melee, Keyword.Bow },
                activeSkill.Keywords);
            Assert.IsFalse(activeSkill.ProvidesBuff);
            Assert.IsNull(activeSkill.TotemLifeMultiplier);
            Assert.IsEmpty(activeSkill.WeaponRestrictions);

            Assert.That(definition.Levels, Has.Exactly(2).Items);
            var level1 = definition.Levels[1];
            Assert.AreEqual(1.1, level1.DamageEffectiveness);
            Assert.AreEqual(1.1, level1.DamageMultiplier);
            Assert.IsNull(level1.CriticalStrikeChance);
            Assert.IsNull(level1.AttackSpeedMultiplier);
            Assert.AreEqual(10, level1.ManaCost);
            Assert.IsNull(level1.ManaMultiplier);
            Assert.IsNull(level1.ManaCostOverride);
            Assert.IsNull(level1.Cooldown);
            Assert.AreEqual(16, level1.Requirements.Level);
            Assert.AreEqual(41, level1.Requirements.Dexterity);
            Assert.AreEqual(0, level1.Requirements.Intelligence);
            Assert.AreEqual(new[] { new UntranslatedStat("attack_speed_+%", 500) }, level1.QualityStats);
            Assert.AreEqual(new UntranslatedStat("physical_damage_+%_per_frenzy_charge", 5), level1.Stats[0]);
            var level20 = definition.Levels[20];
            Assert.AreEqual(1.37, level20.DamageEffectiveness);
            Assert.AreEqual(10, level20.ManaCost);
            Assert.AreEqual(155, level20.Requirements.Dexterity);
            Assert.AreEqual(0, level20.Requirements.Intelligence);
            Assert.AreEqual(new[] { new UntranslatedStat("attack_speed_+%", 500) }, level20.QualityStats);
            Assert.AreEqual(new UntranslatedStat("physical_damage_+%_per_frenzy_charge", 5), level20.Stats[0]);

            var tooltip = level20.Tooltip;
            Assert.AreEqual("Frenzy", tooltip.Name);
            Assert.AreEqual(new[]
            {
                new TranslatedStat("Attack, Melee, Bow"),
                new TranslatedStat("Level: {0} (Max)", 20),
                new TranslatedStat("Mana Cost: {0}", 10),
                new TranslatedStat("Cast Time: {0} sec", 1),
                new TranslatedStat("Damage Effectiveness: {0}%", 137),
            }, tooltip.Properties);
            Assert.AreEqual(new[] { new TranslatedStat("Requires Level {0}, {1} Dex", 70, 155) }, tooltip.Requirements);
            Assert.That(tooltip.Description, Has.One.StartsWith("Performs an attack that gives"));
            Assert.AreEqual(new[] { new TranslatedStat("{0}% increased Attack Speed", 0.5) }, tooltip.QualityStats);
            Assert.AreEqual(new[]
            {
                new TranslatedStat("Deals {0}% of Base Attack Damage", 136.6),
                new TranslatedStat("{0}% increased Physical Damage per Frenzy Charge", 5),
                new TranslatedStat("{0}% increased Attack Speed per Frenzy Charge", 5),
            }, tooltip.Stats);
        }

        [Test]
        public void DeserializeReturnsCorrectResultForFlameTotem()
        {
            var definitions = DeserializeAll();

            var definition = definitions.GetSkillById("FlameTotem");
            var activeSkill = definition.ActiveSkill;
            Assert.AreEqual(250, activeSkill.CastTime);
            Assert.AreEqual(1.62, activeSkill.TotemLifeMultiplier);
            var level20 = definition.Levels[20];
            Assert.AreEqual(5, level20.CriticalStrikeChance);
        }

        [Test]
        public void DeserializeReturnsCorrectResultForBladeFlurry()
        {
            var definitions = DeserializeAll();

            var definition = definitions.GetSkillById("ChargedAttack");
            Assert.AreEqual(new[] { "No Release", "Release at 6 Stages" }, definition.PartNames);
            var activeSkill = definition.ActiveSkill;
            Assert.AreEqual(new[] { activeSkill.Keywords, activeSkill.Keywords }, activeSkill.KeywordsPerPart);
            Assert.AreEqual(new[]
                    { ItemClass.Claw, ItemClass.Dagger, ItemClass.OneHandSword, ItemClass.ThrustingOneHandSword },
                activeSkill.WeaponRestrictions);
            var level20 = definition.Levels[20];
            Assert.AreEqual(new[]
            {
                new UntranslatedStat("active_skill_attack_speed_+%_final", 60),
                new UntranslatedStat("is_area_damage", 1),
                new UntranslatedStat("skill_can_add_multiple_charges_per_action", 1),
            }, level20.Stats);
            Assert.AreEqual(new[]
            {
                new UntranslatedStat("charged_attack_damage_per_stack_+%_final", 20),
                new UntranslatedStat("maximum_stages", 6),
            }, level20.AdditionalStatsPerPart[0]);
            Assert.AreEqual(new[]
            {
                new UntranslatedStat("base_skill_number_of_additional_hits", 1),
                new UntranslatedStat("hit_ailment_damage_+%_final", 80),
            }, level20.AdditionalStatsPerPart[1]);
            Assert.AreEqual(60, level20.AttackSpeedMultiplier);
        }

        [Test]
        public void DeserializeReturnsCorrectResultForClarity()
        {
            var definitions = DeserializeAll();

            var definition = definitions.GetSkillById("Clarity");
            Assert.IsTrue(definition.ActiveSkill.ProvidesBuff);
            var level20 = definition.Levels[20];
            Assert.AreEqual(1200, level20.Cooldown);
            Assert.AreEqual(new[]
            {
                new UntranslatedStat("base_skill_area_of_effect_+%", 1000),
            }, level20.QualityStats);
            Assert.AreEqual(new[]
            {
                new UntranslatedStat("active_skill_base_radius_+", 19),
                new UntranslatedStat("base_deal_no_damage", 1),
            }, level20.Stats);
            Assert.IsEmpty(level20.QualityBuffStats);
            Assert.AreEqual(new[]
            {
                new BuffStat(new UntranslatedStat("base_mana_regeneration_rate_per_minute", 1031),
                    new[] { Entity.Character, Entity.Minion, Entity.Totem }),
            }, level20.BuffStats);
        }

        [Test]
        public void DeserializeReturnsCorrectResultForBurningDamageSupport()
        {
            var definitions = DeserializeAll();

            var definition = definitions.GetSkillById("SupportIncreasedBurningDamage");
            Assert.IsTrue(definition.IsSupport);
            Assert.IsNull(definition.ActiveSkill);

            var supportSkill = definition.SupportSkill;
            Assert.IsFalse(supportSkill.SupportsGemsOnly);
            Assert.AreEqual(new[] { "hits", "attack", "applies_burning" }, supportSkill.AllowedActiveSkillTypes);
            Assert.IsEmpty(supportSkill.ExcludedActiveSkillTypes);
            Assert.IsEmpty(supportSkill.AddedActiveSkillTypes);

            var level20 = definition.Levels[20];
            Assert.IsNull(level20.ManaCost);
            Assert.AreEqual(1.2, level20.ManaMultiplier);
            Assert.IsNull(level20.ManaCostOverride);
        }

        [Test]
        public void DeserializeReturnsCorrectResultForBlasphemySupport()
        {
            var definitions = DeserializeAll();

            var definition = definitions.GetSkillById("SupportBlasphemy");
            Assert.IsTrue(definition.IsSupport);
            Assert.IsNull(definition.ActiveSkill);

            var supportSkill = definition.SupportSkill;
            Assert.AreEqual(
                new[] { "mana_cost_is_reservation", "mana_cost_is_percentage", "unknown_30", "aura", "aura_debuff" },
                supportSkill.AddedActiveSkillTypes);
            Assert.AreEqual(new[] { Keyword.Aura }, supportSkill.AddedKeywords);

            var level20 = definition.Levels[20];
            Assert.AreEqual(35, level20.ManaCostOverride);
            Assert.IsEmpty(level20.QualityStats);
            Assert.AreEqual(new[] { "curse_effect_+%" }, level20.QualityPassiveStats.Select(s => s.StatId));
        }

        [Test]
        public void DeserializeIgnoresSkillsWithUnreleasedBaseItems()
        {
            var definitions = DeserializeAll();

            var releaseStates = definitions.Skills
                .Where(d => d.BaseItem != null)
                .Select(d => d.BaseItem.ReleaseState);
            CollectionAssert.DoesNotContain(releaseStates, ReleaseState.Unreleased);
        }

        private static SkillDefinitions DeserializeAll()
        {
            /* Skills in gems.min.json and gem_tooltips.min.json: (from game version 3.3.0)
             ['RainOfArrows', 'Clarity', 'SupportIncreasedBurningDamage', 'VaalPowerSiphon', 'ChargedAttack',
              'FreezeMine', 'ShadeForm', 'ElementalHit', 'VaalRighteousFire', 'EnchantmentOfIreWhenHit4',
              'SupportItemQuantity', 'SupportPierce', 'VaalFireTrap', 'NewPhaseRun', 'IntimidatingCry',
              'SupportAdditionalProjectilesUnique', 'SupportMinionLife', 'ThrownShield', 'BirdAspect', 'FlameTotem',
              'SupportBlasphemy']
             */
            var gemJson = JObject.Parse(TestUtils.ReadDataFile("gems.min.json"));
            var gemTooltipJson = JObject.Parse(TestUtils.ReadDataFile("gem_tooltips.min.json"));
            return SkillJsonDeserializer.Deserialize(gemJson, gemTooltipJson);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.GameModel.Skills
{
    public class SkillDefinitionExtensions
    {
        private static readonly Entity[] AuraEntities = { Entity.Character, Entity.Minion, Entity.Totem };

        private readonly SkillDefinitionExtension _emptyExtension =
            new SkillDefinitionExtension(new SkillPartDefinitionExtension(),
                new Dictionary<string, IEnumerable<Entity>>());

        private readonly Dictionary<string, SkillDefinitionExtension> _extensions =
            new Dictionary<string, SkillDefinitionExtension>();

        public SkillDefinitionExtension GetExtensionForSkill(string skillId)
            => _extensions.TryGetValue(skillId, out var result) ? result : _emptyExtension;

        public SkillDefinitionExtensions()
        {
            var skillDotIsAreaDamageExtension = new SkillPartDefinitionExtension(
                AddStat("skill_dot_is_area_damage", 1));
            var removeShowAverageDamageExtension = new SkillPartDefinitionExtension(
                RemoveStat("base_skill_show_average_damage_instead_of_dps"));

            Add("Barrage",
                ("Single Projectile", new SkillPartDefinitionExtension()),
                ("All Projectiles", new SkillPartDefinitionExtension()));
            Add("BearTrap", EnemyBuff("bear_trap_damage_taken_+%_from_traps_and_mines"));
            Add("ChargedDash", removeShowAverageDamageExtension);
            Add("ChargedAttack", removeShowAverageDamageExtension,
                ("No Release", new SkillPartDefinitionExtension(
                    AddStat("maximum_stages", 6))),
                ("Release at 6 Stages", new SkillPartDefinitionExtension(
                    RemoveStat("charged_attack_damage_per_stack_+%_final"),
                    AddStats(
                        // For releasing
                        ("base_skill_number_of_additional_hits", 1),
                        // Average stage multiplier, slightly smaller than the perfect 85
                        ("hit_ailment_damage_+%_final", 80)))));
            Add("Clarity", Aura("base_mana_regeneration_rate_per_minute"));
            Add("ColdSnap", skillDotIsAreaDamageExtension);
            Add("VaalColdSnap", skillDotIsAreaDamageExtension);
            Add("Desecrate", skillDotIsAreaDamageExtension);
            Add("FireTrap", skillDotIsAreaDamageExtension);
            Add("FlameDash", skillDotIsAreaDamageExtension);
            Add("FlickerStrike", removeShowAverageDamageExtension);
            Add("IceSpear",
                ("First Form", new SkillPartDefinitionExtension(
                    AddStat("always_pierce", 1))),
                ("Second Form", new SkillPartDefinitionExtension(
                    ReplaceStat("ice_spear_second_form_critical_strike_chance_+%", "critical_strike_chance_+%"))));
            Add("InfernalBlow",
                ("Attack", new SkillPartDefinitionExtension()),
                ("Corpse Explosion", new SkillPartDefinitionExtension(
                    AddStats(
                        ("display_skill_deals_secondary_damage", 1),
                        ("base_skill_show_average_damage_instead_of_dps", 1)))),
                ("6 Charge Explosion", new SkillPartDefinitionExtension(
                    RemoveStat("corpse_explosion_monster_life_%"),
                    AddStats(
                        ("display_skill_deals_secondary_damage", 1),
                        ("base_skill_show_average_damage_instead_of_dps", 1)))));
            Add("PoisonArrow", skillDotIsAreaDamageExtension);
            Add("RainOfSpores", skillDotIsAreaDamageExtension);
            Add("RighteousFire", skillDotIsAreaDamageExtension);
            Add("VaalRighteousFire", skillDotIsAreaDamageExtension);
            Add("FrostBoltNova", skillDotIsAreaDamageExtension);
            Add("WildStrike",
                ("Fire", new SkillPartDefinitionExtension(
                    AddStat("skill_physical_damage_%_to_convert_to_fire", 100))),
                ("Fire Explosion", new SkillPartDefinitionExtension(
                    AddStats(
                        ("skill_physical_damage_%_to_convert_to_fire", 100),
                        ("cast_rate_is_melee", 1),
                        ("is_area_damage", 1)),
                    removedKeywords: new[] { Keyword.Melee })),
                ("Cold", new SkillPartDefinitionExtension(
                    AddStat("skill_physical_damage_%_to_convert_to_cold", 100))),
                ("Cold Wave", new SkillPartDefinitionExtension(
                    AddStats(
                        ("skill_physical_damage_%_to_convert_to_cold", 100),
                        ("cast_rate_is_melee", 1),
                        ("base_is_projectile", 1)),
                    removedKeywords: new[] { Keyword.Melee })),
                ("Lightning", new SkillPartDefinitionExtension(
                    AddStat("skill_physical_damage_%_to_convert_to_lightning", 100))),
                ("Lightning Bolt", new SkillPartDefinitionExtension(
                    AddStats(
                        ("skill_physical_damage_%_to_convert_to_lightning", 100),
                        ("cast_rate_is_melee", 1)),
                    removedKeywords: new[] { Keyword.Melee })));
        }

        private void Add(string skillId, params (string name, SkillPartDefinitionExtension extension)[] parts)
            => Add(skillId, new SkillPartDefinitionExtension(), parts);

        private void Add(string skillId, SkillPartDefinitionExtension commonExtension,
            params (string name, SkillPartDefinitionExtension extension)[] parts)
            => Add(skillId, commonExtension, new Dictionary<string, IEnumerable<Entity>>(), parts);

        private void Add(string skillId, IReadOnlyDictionary<string, IEnumerable<Entity>> buffStats,
            params (string name, SkillPartDefinitionExtension extension)[] parts)
            => Add(skillId, new SkillPartDefinitionExtension(), buffStats, parts);

        private void Add(string skillId, SkillPartDefinitionExtension commonExtension,
            IReadOnlyDictionary<string, IEnumerable<Entity>> buffStats,
            params (string name, SkillPartDefinitionExtension extension)[] parts)
            => _extensions[skillId] = new SkillDefinitionExtension(commonExtension, buffStats, parts);

        private static IEnumerable<string> RemoveStat(string statId)
            => new[] { statId };

        private static IEnumerable<UntranslatedStat> AddStat(string statId, int value)
            => AddStats((statId, value));

        private static IEnumerable<UntranslatedStat> AddStats(params (string statId, int value)[] stats)
            => stats.Select(t => new UntranslatedStat(t.statId, t.value));

        private static Func<IEnumerable<UntranslatedStat>, IEnumerable<UntranslatedStat>> ReplaceStat(
            string oldStatId, string newStatId)
        {
            return stats => stats.Select(Replace);

            UntranslatedStat Replace(UntranslatedStat stat)
                => stat.StatId == oldStatId ? new UntranslatedStat(newStatId, stat.Value) : stat;
        }

        private static IReadOnlyDictionary<string, IEnumerable<Entity>> SelfBuff(params string[] statIds)
            => Buff(new[] { Entity.Character }, statIds);

        private static IReadOnlyDictionary<string, IEnumerable<Entity>> EnemyBuff(params string[] statIds)
            => Buff(new[] { Entity.Enemy }, statIds);

        private static IReadOnlyDictionary<string, IEnumerable<Entity>> Aura(params string[] statIds)
            => Buff(AuraEntities, statIds);

        private static IReadOnlyDictionary<string, IEnumerable<Entity>> Buff(
            IEnumerable<Entity> affectedEntities, params string[] statIds)
            => Buff(statIds.Select(s => (s, affectedEntities)).ToArray());

        private static IReadOnlyDictionary<string, IEnumerable<Entity>> Buff(
            params (string statId, IEnumerable<Entity> affectedEntities)[] stats)
            => stats.ToDictionary(t => t.statId, t => t.affectedEntities);
    }
}
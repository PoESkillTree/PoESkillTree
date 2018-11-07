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
                addedStats: Stats(("skill_dot_is_area_damage", 1)));
            var removeShowAverageDamageExtension = new SkillPartDefinitionExtension(
                removedStats: new[] { "base_skill_show_average_damage_instead_of_dps" });
            Add("Barrage",
                ("Single Projectile", new SkillPartDefinitionExtension()),
                ("All Projectiles", new SkillPartDefinitionExtension()));
            Add("BearTrap", BuffStats(("bear_trap_damage_taken_+%_from_traps_and_mines", Entity.Character)));
            Add("ChargedDash", removeShowAverageDamageExtension);
            Add("ChargedAttack", removeShowAverageDamageExtension,
                ("No Release", new SkillPartDefinitionExtension(
                    addedStats: Stats(("maximum_stages", 6)))),
                ("Release at 6 Stages", new SkillPartDefinitionExtension(
                    removedStats: new[] { "charged_attack_damage_per_stack_+%_final" },
                    addedStats: Stats(
                        // For releasing
                        ("base_skill_number_of_additional_hits", 1),
                        // Average stage multiplier, slightly smaller than the perfect 85
                        ("damage_hits_ailments_more", 80)))));
            Add("Clarity", BuffStats(("base_mana_regeneration_rate_per_minute", AuraEntities)));
            Add("ColdSnap", skillDotIsAreaDamageExtension);
            Add("VaalColdSnap", skillDotIsAreaDamageExtension);
            Add("Desecrate", skillDotIsAreaDamageExtension);
            Add("FireTrap", skillDotIsAreaDamageExtension);
            Add("FlameDash", skillDotIsAreaDamageExtension);
            Add("FlickerStrike", removeShowAverageDamageExtension);
            Add("PoisonArrow", skillDotIsAreaDamageExtension);
            Add("RainOfSpores", skillDotIsAreaDamageExtension);
            Add("RighteousFire", skillDotIsAreaDamageExtension);
            Add("VaalRighteousFire", skillDotIsAreaDamageExtension);
            Add("FrostBoltNova", skillDotIsAreaDamageExtension);
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

        private static IEnumerable<UntranslatedStat> Stats(params (string statId, int value)[] stats)
            => stats.Select(t => new UntranslatedStat(t.statId, t.value));

        private static IReadOnlyDictionary<string, IEnumerable<Entity>> BuffStats(
            params (string statId, Entity affectedEntity)[] stats)
            => stats.ToDictionary(t => t.statId, t => (IEnumerable<Entity>) new[] { t.affectedEntity });

        private static IReadOnlyDictionary<string, IEnumerable<Entity>> BuffStats(
            params (string statId, IEnumerable<Entity> affectedEntities)[] stats)
            => stats.ToDictionary(t => t.statId, t => t.affectedEntities);
    }
}
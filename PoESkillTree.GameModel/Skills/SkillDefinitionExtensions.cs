using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Utils;
using PoESkillTree.Utils.Extensions;

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
            var corpseExplodingSpellParts = new[]
            {
                ("Spell", new SkillPartDefinitionExtension()),
                ("Corpse Explosion", new SkillPartDefinitionExtension(
                    AddStat("display_skill_deals_secondary_damage", 1)))
            };
            var secondaryProjectileMeleeAttackParts = new[]
            {
                ("Melee Attack", new SkillPartDefinitionExtension()),
                ("Projectiles", new SkillPartDefinitionExtension(
                    AddStats(("cast_rate_is_melee", 1), ("base_is_projectile", 1)),
                    removedKeywords: new[] { Keyword.Melee }))
            };
            var secondaryExplosionProjectileParts = new[]
            {
                ("Projectile", new SkillPartDefinitionExtension()),
                ("Explosion", new SkillPartDefinitionExtension(
                    AddStat("is_area_damage", 1)))
            };

            Add("AbyssalCry", EnemyBuff("base_movement_velocity_+%",
                "abyssal_cry_movement_velocity_+%_per_one_hundred_nearby_enemies"));
            Add("AncestorTotemSlam", // Ancestral Warchief
                SelfBuff("slam_ancestor_totem_grant_owner_melee_damage_+%_final"));
            Add("VaalAncestralWarchief", SelfBuff("slam_ancestor_totem_grant_owner_melee_damage_+%_final"));
            Add("Barrage",
                ("Single Projectile", new SkillPartDefinitionExtension()),
                ("All Projectiles", new SkillPartDefinitionExtension()));
            Add("BearTrap", EnemyBuff("bear_trap_damage_taken_+%_from_traps_and_mines"));
            Add("BladeVortex", new SkillPartDefinitionExtension(
                RemoveStat("base_skill_show_average_damage_instead_of_dps"),
                AddStat("hit_rate_ms", 600),
                ReplaceStat("maximum_number_of_spinning_blades", "maximum_stages")));
            Add("VaalBladeVortex", new SkillPartDefinitionExtension(
                ReplaceStat("base_blade_vortex_hit_rate_ms", "hit_rate_ms")));
            Add("BlastRain",
                ("Single Explosion", new SkillPartDefinitionExtension()),
                ("All 4 Explosions", new SkillPartDefinitionExtension(
                    AddStat("base_skill_number_of_additional_hits", 3))));
            Add("Bodyswap",
                ("Self Explosion", new SkillPartDefinitionExtension()),
                ("Corpse Explosion", new SkillPartDefinitionExtension(
                    AddStat("display_skill_deals_secondary_damage", 1))));
            Add("ChargedDash", removeShowAverageDamageExtension);
            Add("ChargedAttack", removeShowAverageDamageExtension, // Blade Flurry
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
            Add("ClusterBurst", secondaryExplosionProjectileParts); // Kinetic Blast
            Add("ColdSnap", skillDotIsAreaDamageExtension);
            Add("Convocation", Buff(Entity.Minion, "base_life_regeneration_rate_per_minute"));
            Add("VaalColdSnap", skillDotIsAreaDamageExtension);
            Add("CorpseEruption", corpseExplodingSpellParts); // Cremation
            Add("DarkPact",
                ("Cast on Self", new SkillPartDefinitionExtension(
                    ReplaceStat("skeletal_chains_aoe_%_health_dealt_as_chaos_damage",
                            "spell_base_chaos_damage_%_maximum_life")
                        .AndThen(ReplaceStat("skeletal_chains_no_minions_damage_+%_final",
                            "hit_ailment_damage_+%_final")))),
                ("Cast on Skeleton", new SkillPartDefinitionExtension()));
            Add("Desecrate", skillDotIsAreaDamageExtension);
            Add("DetonateDead", corpseExplodingSpellParts);
            Add("DoubleSlash", // Lacerate
                ("Single Slash", new SkillPartDefinitionExtension()),
                ("Both Slashes", new SkillPartDefinitionExtension(
                    AddStat("base_skill_number_of_additional_hits", 1))));
            Add("VaalDetonateDead", corpseExplodingSpellParts);
            var earthquakeParts = new[]
            {
                ("Initial Hit", new SkillPartDefinitionExtension()),
                ("Aftershock", new SkillPartDefinitionExtension(
                    AddStat("base_skill_show_average_damage_instead_of_dps", 1),
                    ReplaceStat("quake_slam_fully_charged_explosion_damage_+%_final", "damage_+%_final")))
            };
            Add("Earthquake", earthquakeParts);
            Add("VaalEarthquake", earthquakeParts);
            Add("ElementalHit",
                ("Fire", new SkillPartDefinitionExtension()),
                ("Cold", new SkillPartDefinitionExtension()),
                ("Lightning", new SkillPartDefinitionExtension()));
            Add("EnduringCry", SelfBuff("base_life_regeneration_rate_per_minute"));
            Add("ExpandingFireCone", // Incinerate
                ("Channeling", new SkillPartDefinitionExtension(
                    ReplaceStat("expanding_fire_cone_maximum_number_of_stages", "maximum_stages"))),
                ("Release", new SkillPartDefinitionExtension(
                    AddStat("base_skill_show_average_damage_instead_of_dps", 1),
                    ReplaceStat("expanding_fire_cone_final_wave_always_ignite", "always_ignite")
                        .AndThen(ReplaceStat("expanding_fire_cone_maximum_number_of_stages", "maximum_stages", 0)))));
            Add("ExplosiveArrow", new SkillPartDefinitionExtension(
                    AddStat("maximum_stages", 5)),
                ("Attack", new SkillPartDefinitionExtension()),
                ("Explosion", new SkillPartDefinitionExtension(
                    AddStats(
                        ("base_skill_show_average_damage_instead_of_dps", 1),
                        ("display_skill_deals_secondary_damage", 1)))));
            Add("Fireball", secondaryExplosionProjectileParts);
            Add("VaalFireball", secondaryExplosionProjectileParts);
            Add("FireBeam", new SkillPartDefinitionExtension( // Scorching Ray
                    ReplaceStat("fire_beam_enemy_fire_resistance_%_per_stack", "base_fire_damage_resistance_%")),
                EnemyBuff("base_fire_damage_resistance_%"));
            Add("FireTrap", skillDotIsAreaDamageExtension);
            Add("Flameblast", new SkillPartDefinitionExtension(AddStat("maximum_stages", 9)));
            Add("FlameDash", skillDotIsAreaDamageExtension);
            Add("FlickerStrike", removeShowAverageDamageExtension);
            Add("FrostBlades", secondaryProjectileMeleeAttackParts);
            Add("FrostBomb",
                EnemyBuff("base_cold_damage_resistance_%", "life_regeneration_rate_+%",
                    "energy_shield_regeneration_rate_+%", "energy_shield_recharge_rate_+%"));
            Add("FrostBoltNova", skillDotIsAreaDamageExtension); // Vortex
            Add("IceCrash",
                ("First Hit", new SkillPartDefinitionExtension()),
                ("Second Hit", new SkillPartDefinitionExtension(
                    ReplaceStat("ice_crash_second_hit_damage_+%_final", "damage_+%_final"))),
                ("Third Hit", new SkillPartDefinitionExtension(
                    ReplaceStat("ice_crash_third_hit_damage_+%_final", "damage_+%_final"))));
            Add("IceShot",
                ("Projectile", new SkillPartDefinitionExtension()),
                ("Cone", new SkillPartDefinitionExtension(
                    AddStat("is_area_damage", 1))));
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
            Add("LightningStrike", secondaryProjectileMeleeAttackParts);
            Add("VaalLightningStrike", secondaryProjectileMeleeAttackParts);
            Add("MoltenShell", SelfBuff("base_physical_damage_reduction_rating"));
            Add("VaalMoltenShell", SelfBuff("base_physical_damage_reduction_rating"));
            Add("MoltenStrike",
                ("Melee Attack", new SkillPartDefinitionExtension()),
                ("Projectiles", new SkillPartDefinitionExtension(
                    AddStats(("cast_rate_is_melee", 1), ("base_is_projectile", 1), ("is_area_damage", 1)),
                    removedKeywords: new[] { Keyword.Melee })));
            Add("PoisonArrow", skillDotIsAreaDamageExtension); // Caustic Arrow
            Add("Reave", new SkillPartDefinitionExtension(
                AddStat("maximum_stages", 8)));
            Add("VaalReave", new SkillPartDefinitionExtension(
                AddStat("maximum_stages", 8)));
            Add("RainOfSpores", skillDotIsAreaDamageExtension); // Toxic Rain
            Add("RallyingCry", SelfBuff("inspiring_cry_damage_+%_per_one_hundred_nearby_enemies", "damage_+%",
                "base_mana_regeneration_rate_per_minute"));
            Add("RejuvenationTotem", Aura("base_mana_regeneration_rate_per_minute"));
            Add("RighteousFire", skillDotIsAreaDamageExtension,
                SelfBuff("righteous_fire_spell_damage_+%_final"));
            Add("VaalRighteousFire", new SkillPartDefinitionExtension(
                    AddStat("skill_dot_is_area_damage", 1),
                    VaalRighteousFireReplaceStats),
                SelfBuff("righteous_fire_spell_damage_+%_final"));
            Add("ScourgeArrow", new SkillPartDefinitionExtension(
                    ReplaceStat("virulent_arrow_maximum_number_of_stacks", "maximum_stages")),
                ("Primary Projectile", new SkillPartDefinitionExtension(
                    AddStat("always_pierce", 1))),
                ("Thorn Arrows", new SkillPartDefinitionExtension(
                    ReplaceStat("virulent_arrow_pod_projectile_damage_+%_final", "damage_+%_final"))));
            Add("ShockNova",
                ("Ring", new SkillPartDefinitionExtension(
                    ReplaceStat("newshocknova_first_ring_damage_+%_final", "damage_+%_final"))),
                ("Nova", new SkillPartDefinitionExtension()));
            Add("ShrapnelShot",
                ("Projectile", new SkillPartDefinitionExtension(
                    AddStat("always_pierce", 1))),
                ("Cone", new SkillPartDefinitionExtension(
                    AddStat("is_area_damage", 1))));
            Add("Smite", Aura("base_chance_to_shock_%_from_skill", "minimum_added_lightning_damage_from_skill",
                "maximum_added_lightning_damage_from_skill"));
            Add("StaticStrike", new SkillPartDefinitionExtension(
                    AddStat("maximum_stages", 3)),
                ("Melee Attack", new SkillPartDefinitionExtension()),
                ("Beams", new SkillPartDefinitionExtension(
                    ReplaceStat("static_strike_base_zap_frequency_ms", "hit_rate_ms"))));
            Add("StormBurst",
                ("Projectile", new SkillPartDefinitionExtension(
                    RemoveStat("base_skill_show_average_damage_instead_of_dps"),
                    AddStat("always_pierce", 1))),
                ("Explosion", new SkillPartDefinitionExtension(
                    AddStat("is_area_damage", 1))));
            Add("Sunder",
                ("Initial Hit", new SkillPartDefinitionExtension()),
                ("Shockwave", new SkillPartDefinitionExtension(
                    ReplaceStat("shockwave_slam_explosion_damage_+%_final", "damage_+%_final"))));
            Add("TempestShield", SelfBuff("shield_block_%"));
            Add("ThrownShield", // Spectral Shield Throw
                ("Primary Projectile", new SkillPartDefinitionExtension()),
                ("Shards", new SkillPartDefinitionExtension()));
            Add("ThrownWeapon", new SkillPartDefinitionExtension( // Spectral Throw
                AddStat("always_pierce", 1)));
            Add("TotemMelee", // Ancestral Protector
                SelfBuff("melee_ancestor_totem_grant_owner_attack_speed_+%_final"));
            Add("VolatileDead", corpseExplodingSpellParts);
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
            Add("Wither", new SkillPartDefinitionExtension(
                RemoveStat("chaos_damage_taken_+%")));

            Add("SupportCastOnDeath", new SkillPartDefinitionExtension(
                ReplaceStat("area_of_effect_+%_while_dead", "base_skill_area_of_effect_+%")
                    .AndThen(ReplaceStat("cast_on_death_damage_+%_final_while_dead", "damage_+%_final"))));
            Add("SupportGemFrenzyPowerOnTrapTrigger", new SkillPartDefinitionExtension(
                ReplaceStat("trap_critical_strike_multiplier_+_per_power_charge",
                    "critical_strike_multiplier_+_per_power_charge")));
            Add("SupportRangedAttackTotem", new SkillPartDefinitionExtension(
                ReplaceStat("support_attack_totem_attack_speed_+%_final", "active_skill_attack_speed_+%_final")));
            Add("SupportSpellTotem", new SkillPartDefinitionExtension(
                ReplaceStat("support_spell_totem_cast_speed_+%_final", "active_skill_cast_speed_+%_final")));
            Add("SupportCastWhileChannelling", new SkillPartDefinitionExtension(
                ReplaceStat("cast_while_channelling_time_ms", "hit_rate_ms")
                    .AndThen(ReplaceStat("support_cast_while_channelling_triggered_skill_damage_+%_final",
                        "damage_+%_final"))));
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
            => ReplaceStat(oldStatId, newStatId, Funcs.Identity);

        private static Func<IEnumerable<UntranslatedStat>, IEnumerable<UntranslatedStat>> ReplaceStat(
            string oldStatId, string newStatId, int newValue)
            => ReplaceStat(oldStatId, newStatId, _ => newValue);

        private static Func<IEnumerable<UntranslatedStat>, IEnumerable<UntranslatedStat>> ReplaceStat(
            string oldStatId, string newStatId, Func<int, int> replaceValue)
        {
            return stats => stats.Select(Replace);

            UntranslatedStat Replace(UntranslatedStat stat)
                => stat.StatId == oldStatId ? new UntranslatedStat(newStatId, replaceValue(stat.Value)) : stat;
        }

        /// <summary>
        /// Replaces the pool sacrifice and sacrifice as damage stats of Vaal Righteous Fire with the burn stats used
        /// by normal Righteous Fire.
        /// </summary>
        private static IEnumerable<UntranslatedStat> VaalRighteousFireReplaceStats(IEnumerable<UntranslatedStat> stats)
        {
            var enumeratedStats = stats.ToList();

            var poolToLoseOnUse =
                enumeratedStats.FirstOrDefault(s => s.StatId == "vaal_righteous_fire_life_and_es_%_to_lose_on_use");
            if (poolToLoseOnUse is null)
                return enumeratedStats;

            var sacrificedPoolDamagePerSecond =
                enumeratedStats.First(s => s.StatId == "vaal_righteous_fire_life_and_es_%_as_damage_per_second");

            // (x / 100) * (y / 100) * 100 * 60 = x * y * 0.6 [combining the percentages, converting seconds to minutes]
            // No loss of precision with current values: x is always 30 -> poolDamagePerMinute = 18 * y
            var poolDamagePerMinute =
                (int) Math.Round(poolToLoseOnUse.Value * sacrificedPoolDamagePerSecond.Value * 0.6);
            return enumeratedStats.Append(
                new UntranslatedStat("base_righteous_fire_%_of_max_life_to_deal_to_nearby_per_minute",
                    poolDamagePerMinute),
                new UntranslatedStat("base_righteous_fire_%_of_max_energy_shield_to_deal_to_nearby_per_minute",
                    poolDamagePerMinute));
        }

        private static IReadOnlyDictionary<string, IEnumerable<Entity>> SelfBuff(params string[] statIds)
            => Buff(Entity.Character, statIds);

        private static IReadOnlyDictionary<string, IEnumerable<Entity>> EnemyBuff(params string[] statIds)
            => Buff(Entity.Enemy, statIds);

        private static IReadOnlyDictionary<string, IEnumerable<Entity>> Aura(params string[] statIds)
            => Buff(AuraEntities, statIds);

        private static IReadOnlyDictionary<string, IEnumerable<Entity>> Buff(
            Entity affectedEntity, params string[] statIds)
            => Buff(new[] { affectedEntity }, statIds);

        private static IReadOnlyDictionary<string, IEnumerable<Entity>> Buff(
            IEnumerable<Entity> affectedEntities, params string[] statIds)
            => Buff(statIds.Select(s => (s, affectedEntities)).ToArray());

        private static IReadOnlyDictionary<string, IEnumerable<Entity>> Buff(
            params (string statId, IEnumerable<Entity> affectedEntities)[] stats)
            => stats.ToDictionary(t => t.statId, t => t.affectedEntities);
    }
}
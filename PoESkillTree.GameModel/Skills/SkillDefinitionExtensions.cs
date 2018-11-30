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
                new Dictionary<string, IEnumerable<Entity>>(), new string[0]);

        private readonly IReadOnlyDictionary<string, SkillDefinitionExtension> _extensions;

        public SkillDefinitionExtension GetExtensionForSkill(string skillId)
            => _extensions.TryGetValue(skillId, out var result) ? result : _emptyExtension;

        public SkillDefinitionExtensions()
        {
            _extensions = CreateCollection().ToDictionary();
        }

        private static SkillDefinitionExtensionCollection CreateCollection() => new SkillDefinitionExtensionCollection
        {
            {
                "AbyssalCry",
                EnemyBuff("base_movement_velocity_+%",
                    "abyssal_cry_movement_velocity_+%_per_one_hundred_nearby_enemies")
            },
            {
                "AncestorTotemSlam", // Ancestral Warchief
                new SkillPartDefinitionExtension(
                    ReplaceStat("slam_ancestor_totem_grant_owner_melee_damage_+%_final", "melee_damage_+%_final")),
                SelfBuff("slam_ancestor_totem_grant_owner_melee_damage_+%_final")
            },
            {
                "VaalAncestralWarchief",
                new SkillPartDefinitionExtension(
                    ReplaceStat("slam_ancestor_totem_grant_owner_melee_damage_+%_final", "melee_damage_+%_final")),
                SelfBuff("slam_ancestor_totem_grant_owner_melee_damage_+%_final")
            },
            {
                "Anger",
                SelfBuff("spell_minimum_added_fire_damage", "spell_maximum_added_fire_damage",
                    "attack_minimum_added_fire_damage", "attack_maximum_added_fire_damage")
            },
            {
                "ArcticArmour",
                SelfBuff("new_arctic_armour_physical_damage_taken_when_hit_+%_final",
                    "new_arctic_armour_fire_damage_taken_when_hit_+%_final")
            },
            {
                "AssassinsMark",
                new SkillPartDefinitionExtension(
                    ReplaceStat("chance_to_grant_power_charge_on_death_%", "add_power_charge_on_kill_%_chance")
                        .AndThen(ReplaceStat("life_granted_when_killed", "base_life_gained_on_enemy_death"))
                        .AndThen(ReplaceStat("mana_granted_when_killed", "base_mana_gained_on_enemy_death"))),
                Buff(("base_self_critical_strike_multiplier_-%", new[] { Entity.Enemy }),
                    ("additional_chance_to_take_critical_strike_%", new[] { Entity.Enemy }),
                    ("add_power_charge_on_kill_%_chance", AuraEntities),
                    ("base_life_gained_on_enemy_death", AuraEntities),
                    ("base_mana_gained_on_enemy_death", AuraEntities))
            },
            {
                "Barrage",
                ("Single Projectile", new SkillPartDefinitionExtension()),
                ("All Projectiles", new SkillPartDefinitionExtension())
            },
            {
                "BearTrap",
                new SkillPartDefinitionExtension(
                    ReplaceStat("bear_trap_damage_taken_+%_from_traps_and_mines",
                        "damage_taken_from_traps_and_mines_+%")),
                EnemyBuff("bear_trap_damage_taken_+%_from_traps_and_mines")
            },
            {
                "BladeVortex",
                new SkillPartDefinitionExtension(
                    RemoveStat("base_skill_show_average_damage_instead_of_dps"),
                    AddStat("hit_rate_ms", 600),
                    ReplaceStat("maximum_number_of_spinning_blades", "maximum_stages"))
            },
            {
                "VaalBladeVortex",
                new SkillPartDefinitionExtension(ReplaceStat("base_blade_vortex_hit_rate_ms", "hit_rate_ms"))
            },
            {
                "BlastRain",
                ("Single Explosion", new SkillPartDefinitionExtension()),
                ("All 4 Explosions", new SkillPartDefinitionExtension(
                    AddStat("base_skill_number_of_additional_hits", 3)))
            },
            {
                "BloodRage",
                SelfBuff("life_leech_from_physical_attack_damage_permyriad",
                    "base_physical_damage_%_of_maximum_life_to_deal_per_minute",
                    "base_physical_damage_%_of_maximum_energy_shield_to_deal_per_minute",
                    "add_frenzy_charge_on_kill_%_chance",
                    "attack_speed_+%")
            },
            {
                "Bodyswap",
                ("Self Explosion", new SkillPartDefinitionExtension()),
                ("Corpse Explosion", new SkillPartDefinitionExtension(
                    AddStat("display_skill_deals_secondary_damage", 1)))
            },
            { "ChargedDash", RemoveShowAverageDamageExtension },
            {
                "ChargedAttack", // Blade Flurry
                RemoveShowAverageDamageExtension,
                ("No Release", new SkillPartDefinitionExtension(
                    AddStat("maximum_stages", 6))),
                ("Release at 6 Stages", new SkillPartDefinitionExtension(
                    RemoveStat("charged_attack_damage_per_stack_+%_final"),
                    AddStats(
                        // For releasing
                        ("base_skill_number_of_additional_hits", 1),
                        // Average stage multiplier, slightly smaller than the perfect 85
                        ("hit_ailment_damage_+%_final", 80))))
            },
            { "Clarity", Aura("base_mana_regeneration_rate_per_minute") },
            { "VaalClarity", Aura("no_mana_cost") },
            { "ClusterBurst", SecondaryExplosionProjectileParts }, // Kinetic Blast
            {
                "ColdImpurity", // Vaal Impurity of Ice
                new SkillPartDefinitionExtension(
                    ReplaceStat("base_immune_to_freeze", "base_avoid_freeze_%", 100)
                        .AndThen(ReplaceStat("base_immune_to_chill", "base_avoid_chill_%", 100))),
                Buff(("cold_damage_taken_+%", AuraEntities),
                    ("base_avoid_freeze_%", AuraEntities),
                    ("base_avoid_chill_%", AuraEntities),
                    ("hits_ignore_my_cold_resistance", new[] { Entity.Enemy })),
                Passive("aura_effect_+%")
            },
            {
                "ColdResistAura", // Purity of Ice
                Aura("base_cold_damage_resistance_%", "base_maximum_cold_damage_resistance_%")
            },
            { "ColdSnap", SkillDotIsAreaDamageExtension },
            {
                "Conductivity",
                new SkillPartDefinitionExtension(
                    ReplaceStat("base_self_shock_duration_-%", "shock_duration_+%")
                        .AndThen(ReplaceStat("chance_to_be_shocked_%", "base_chance_to_shock_%"))),
                Buff(("base_lightning_damage_resistance_%", new[] { Entity.Enemy }),
                    ("shock_duration_+%", AuraEntities),
                    ("base_chance_to_shock_%", AuraEntities))
            },
            { "Convocation", Buff(Entity.Minion, "base_life_regeneration_rate_per_minute") },
            { "VaalColdSnap", SkillDotIsAreaDamageExtension },
            { "CorpseEruption", CorpseExplodingSpellParts }, // Cremation
            {
                "DarkPact",
                ("Cast on Self", new SkillPartDefinitionExtension(
                    ReplaceStat("skeletal_chains_aoe_%_health_dealt_as_chaos_damage",
                            "spell_base_chaos_damage_%_maximum_life")
                        .AndThen(ReplaceStat("skeletal_chains_no_minions_damage_+%_final",
                            "hit_ailment_damage_+%_final")))),
                ("Cast on Skeleton", new SkillPartDefinitionExtension())
            },
            { "Desecrate", SkillDotIsAreaDamageExtension },
            {
                "Despair",
                new SkillPartDefinitionExtension(
                    ReplaceStat("minimum_added_chaos_damage_taken", "global_minimum_added_chaos_damage")
                        .AndThen(ReplaceStat("maximum_added_chaos_damage_taken", "global_maximum_added_chaos_damage"))),
                Buff(("degen_effect_+%", new[] { Entity.Enemy }),
                    ("base_chaos_damage_resistance_%", new[] { Entity.Enemy }),
                    ("global_minimum_added_chaos_damage", AuraEntities),
                    ("global_maximum_added_chaos_damage", AuraEntities))
            },
            {
                "Determination",
                new SkillPartDefinitionExtension(
                    ReplaceStat("determination_aura_armour_+%_final", "armour_+%_final")),
                Aura("armour_+%_final")
            },
            { "DetonateDead", CorpseExplodingSpellParts },
            { "Discipline", Aura("energy_shield_recharge_rate_+%", "base_maximum_energy_shield") },
            { "VaalDiscipline", Aura("energy_shield_recharge_not_delayed_by_damage") },
            {
                "DoubleSlash", // Lacerate
                ("Single Slash", new SkillPartDefinitionExtension()),
                ("Both Slashes", new SkillPartDefinitionExtension(
                    AddStat("base_skill_number_of_additional_hits", 1)))
            },
            { "VaalDetonateDead", CorpseExplodingSpellParts },
            { "Earthquake", EarthquakeParts },
            { "VaalEarthquake", EarthquakeParts },
            {
                "ElementalHit",
                ("Fire", new SkillPartDefinitionExtension()),
                ("Cold", new SkillPartDefinitionExtension()),
                ("Lightning", new SkillPartDefinitionExtension())
            },
            { "ElementalWeakness", EnemyBuff("base_resist_all_elements_%") },
            { "EnduringCry", SelfBuff("base_life_regeneration_rate_per_minute") },
            {
                "Enfeeble",
                new SkillPartDefinitionExtension(
                    ReplaceStat("enfeeble_damage_+%_final", "damage_+%_final")),
                EnemyBuff("critical_strike_chance_+%", "accuracy_rating_+%", "damage_+%_final",
                    "base_critical_strike_multiplier_+")
            },
            {
                "ExpandingFireCone", // Incinerate
                ("Channeling", new SkillPartDefinitionExtension(
                    RemoveStat("expanding_fire_cone_final_wave_always_ignite"),
                    ReplaceStat("expanding_fire_cone_maximum_number_of_stages", "maximum_stages"))),
                ("Release", new SkillPartDefinitionExtension(
                    AddStat("base_skill_show_average_damage_instead_of_dps", 1),
                    ReplaceStat("expanding_fire_cone_final_wave_always_ignite", "always_ignite")
                        .AndThen(ReplaceStat("expanding_fire_cone_maximum_number_of_stages", "maximum_stages", 0))))
            },
            {
                "ExplosiveArrow",
                new SkillPartDefinitionExtension(
                    AddStat("maximum_stages", 5)),
                ("Attack", new SkillPartDefinitionExtension()),
                ("Explosion", new SkillPartDefinitionExtension(
                    AddStats(
                        ("base_skill_show_average_damage_instead_of_dps", 1),
                        ("display_skill_deals_secondary_damage", 1))))
            },
            { "Fireball", SecondaryExplosionProjectileParts },
            { "VaalFireball", SecondaryExplosionProjectileParts },
            {
                "FireBeam", // Scorching Ray
                new SkillPartDefinitionExtension(
                    ReplaceStat("fire_beam_enemy_fire_resistance_%_per_stack", "base_fire_damage_resistance_%")),
                EnemyBuff("base_fire_damage_resistance_%")
            },
            {
                "FireImpurity", // Vaal Impurity of Fire
                new SkillPartDefinitionExtension(
                    ReplaceStat("base_immune_to_ignite", "base_avoid_ignite_%", 100)),
                Buff(("fire_damage_taken_+%", AuraEntities),
                    ("base_avoid_ignite_%", AuraEntities),
                    ("hits_ignore_my_fire_resistance", new[] { Entity.Enemy })),
                Passive("aura_effect_+%")
            },
            {
                "FireResistAura", // Purity of Fire
                Aura("base_fire_damage_resistance_%", "base_maximum_fire_damage_resistance_%")
            },
            { "FireTrap", SkillDotIsAreaDamageExtension },
            { "Flameblast", new SkillPartDefinitionExtension(AddStat("maximum_stages", 9)) },
            { "FlameDash", SkillDotIsAreaDamageExtension },
            {
                "Flammability",
                new SkillPartDefinitionExtension(
                    ReplaceStat("base_self_ignite_duration_-%", "ignite_duration_+%")
                        .AndThen(ReplaceStat("chance_to_be_ignited_%", "base_chance_to_ignite_%"))),
                Buff(("base_fire_damage_resistance_%", new[] { Entity.Enemy }),
                    ("ignite_duration_+%", AuraEntities),
                    ("base_chance_to_ignite_%", AuraEntities))
            },
            { "FlickerStrike", RemoveShowAverageDamageExtension },
            {
                "Frostbite",
                new SkillPartDefinitionExtension(
                    ReplaceStat("base_self_freeze_duration_-%", "freeze_duration_+%")
                        .AndThen(ReplaceStat("chance_to_be_frozen_%", "base_chance_to_freeze_%"))),
                Buff(("base_cold_damage_resistance_%", new[] { Entity.Enemy }),
                    ("freeze_duration_+%", AuraEntities),
                    ("base_chance_to_freeze_%", AuraEntities))
            },
            { "FrostBlades", SecondaryProjectileMeleeAttackParts },
            {
                "FrostBomb",
                EnemyBuff("base_cold_damage_resistance_%", "life_regeneration_rate_+%",
                    "energy_shield_regeneration_rate_+%", "energy_shield_recharge_rate_+%")
            },
            { "FrostBoltNova", SkillDotIsAreaDamageExtension }, // Vortex
            { "Grace", Aura("base_evasion_rating") },
            { "VaalGrace", Aura("base_chance_to_dodge_%", "base_chance_to_dodge_spells_%") },
            {
                "Haste",
                Aura("attack_speed_+%", "cast_speed_+%_granted_from_skill",
                    "base_movement_velocity_+%")
            },
            {
                "VaalHaste",
                Aura("attack_speed_+%", "cast_speed_+%_granted_from_skill", "base_movement_velocity_+%")
            },
            { "Hatred", Aura("physical_damage_%_to_add_as_cold") },
            {
                "HeraldOfAgony",
                SelfBuff("skill_buff_grants_chance_to_poison_%", "herald_of_agony_poison_damage_+%_final",
                    "herald_of_agony_add_stack_on_poison")
            },
            {
                "HeraldOfAsh",
                SelfBuff("physical_damage_%_to_add_as_fire", "herald_of_ash_fire_damage_+%",
                    "herald_of_ash_spell_fire_damage_+%_final", "herald_of_ash_burning_damage_+%_final")
            },
            {
                "HeraldOfIce",
                SelfBuff("herald_of_ice_cold_damage_+%",
                    "spell_minimum_added_cold_damage", "spell_maximum_added_cold_damage",
                    "attack_minimum_added_cold_damage", "attack_maximum_added_cold_damage")
            },
            {
                "HeraldOfPurity",
                SelfBuff("herald_of_light_spell_minimum_added_physical_damage",
                    "herald_of_light_spell_maximum_added_physical_damage",
                    "herald_of_light_attack_minimum_added_physical_damage",
                    "herald_of_light_attack_maximum_added_physical_damage")
            },
            {
                "HeraldOfThunder",
                SelfBuff("herald_of_thunder_lightning_damage_+%",
                    "spell_minimum_added_lightning_damage", "spell_maximum_added_lightning_damage",
                    "attack_minimum_added_lightning_damage", "attack_maximum_added_lightning_damage")
            },
            {
                "IceCrash",
                ("First Hit", new SkillPartDefinitionExtension(
                    RemoveStats("ice_crash_second_hit_damage_+%_final", "ice_crash_third_hit_damage_+%_final"))),
                ("Second Hit", new SkillPartDefinitionExtension(
                    RemoveStat("ice_crash_third_hit_damage_+%_final"),
                    ReplaceStat("ice_crash_second_hit_damage_+%_final", "damage_+%_final"))),
                ("Third Hit", new SkillPartDefinitionExtension(
                    RemoveStat("ice_crash_second_hit_damage_+%_final"),
                    ReplaceStat("ice_crash_third_hit_damage_+%_final", "damage_+%_final")))
            },
            {
                "IceShot",
                ("Projectile", new SkillPartDefinitionExtension()),
                ("Cone", new SkillPartDefinitionExtension(
                    AddStat("is_area_damage", 1)))
            },
            {
                "IceSpear",
                ("First Form", new SkillPartDefinitionExtension(
                    RemoveStat("ice_spear_second_form_critical_strike_chance_+%"),
                    AddStat("always_pierce", 1))),
                ("Second Form", new SkillPartDefinitionExtension(
                    ReplaceStat("ice_spear_second_form_critical_strike_chance_+%", "critical_strike_chance_+%")))
            },
            {
                "InfernalBlow",
                ("Attack", new SkillPartDefinitionExtension()),
                ("Corpse Explosion", new SkillPartDefinitionExtension(
                    AddStats(
                        ("display_skill_deals_secondary_damage", 1),
                        ("base_skill_show_average_damage_instead_of_dps", 1)))),
                ("6 Charge Explosion", new SkillPartDefinitionExtension(
                    RemoveStat("corpse_explosion_monster_life_%"),
                    AddStats(
                        ("display_skill_deals_secondary_damage", 1),
                        ("base_skill_show_average_damage_instead_of_dps", 1))))
            },
            {
                "LightningImpurity", // Vaal Impurity of Lightning
                new SkillPartDefinitionExtension(
                    ReplaceStat("base_immune_to_shock", "base_avoid_shock_%", 100)),
                Buff(("lightning_damage_taken_+%", AuraEntities),
                    ("base_avoid_shock_%", AuraEntities),
                    ("hits_ignore_my_lightning_resistance", new[] { Entity.Enemy })),
                Passive("aura_effect_+%")
            },
            {
                "LightningResistAura", // Purity of Lightning
                Aura("base_lightning_damage_resistance_%", "base_maximum_lightning_damage_resistance_%")
            },
            { "LightningStrike", SecondaryProjectileMeleeAttackParts },
            { "VaalLightningStrike", SecondaryProjectileMeleeAttackParts },
            { "MoltenShell", SelfBuff("base_physical_damage_reduction_rating") },
            { "VaalMoltenShell", SelfBuff("base_physical_damage_reduction_rating") },
            {
                "MoltenStrike",
                ("Melee Attack", new SkillPartDefinitionExtension()),
                ("Projectiles", new SkillPartDefinitionExtension(
                    AddStats(("cast_rate_is_melee", 1), ("base_is_projectile", 1), ("is_area_damage", 1)),
                    removedKeywords: new[] { Keyword.Melee }))
            },
            {
                "ProjectileWeakness",
                new SkillPartDefinitionExtension(
                    ReplaceStat("projectiles_always_pierce_you", "always_pierce")
                        .AndThen(ReplaceStat("chance_to_be_knocked_back_%", "base_global_chance_to_knockback_%"))),
                Buff(("projectile_damage_taken_+%", new[] { Entity.Enemy }),
                    ("always_pierce", AuraEntities),
                    ("base_global_chance_to_knockback_%", AuraEntities))
            },
            {
                "PoachersMark",
                new SkillPartDefinitionExtension(
                    ReplaceStat("evasion_rating_+%_final_from_poachers_mark", "evasion_rating_+%")
                        .AndThen(ReplaceStat("life_granted_when_hit_by_attacks", "life_gain_per_target"))
                        .AndThen(ReplaceStat("mana_granted_when_hit_by_attacks", "mana_gain_per_target"))
                        .AndThen(ReplaceStat("chance_to_grant_frenzy_charge_on_death_%",
                            "add_frenzy_charge_on_kill_%_chance"))),
                Buff(("monster_slain_flask_charges_granted_+%", new[] { Entity.Enemy }),
                    ("evasion_rating_+%", new[] { Entity.Enemy }),
                    ("life_gain_per_target", AuraEntities),
                    ("mana_gain_per_target", AuraEntities),
                    ("add_frenzy_charge_on_kill_%_chance", AuraEntities))
            },
            { "PoisonArrow", SkillDotIsAreaDamageExtension }, // Caustic Arrow
            {
                "Punishment",
                new SkillPartDefinitionExtension(
                    ReplaceStat("newpunishment_attack_speed_+%", "attack_speed_+%")
                        .AndThen(ReplaceStat("newpunishment_melee_damage_+%_final", "melee_damage_+%_final"))),
                SelfBuff("attack_speed_+%", "melee_damage_+%_final")
            },
            { "Purity", Aura("base_resist_all_elements_%") }, // Purity of Elements
            { "Reave", new SkillPartDefinitionExtension(AddStat("maximum_stages", 8)) },
            { "VaalReave", new SkillPartDefinitionExtension(AddStat("maximum_stages", 8)) },
            { "RainOfSpores", SkillDotIsAreaDamageExtension }, // Toxic Rain
            {
                "RallyingCry",
                SelfBuff("inspiring_cry_damage_+%_per_one_hundred_nearby_enemies", "damage_+%",
                    "base_mana_regeneration_rate_per_minute")
            },
            { "RejuvenationTotem", Aura("base_mana_regeneration_rate_per_minute") },
            { "RighteousFire", SkillDotIsAreaDamageExtension, SelfBuff("righteous_fire_spell_damage_+%_final") },
            {
                "VaalRighteousFire",
                new SkillPartDefinitionExtension(
                    AddStat("skill_dot_is_area_damage", 1),
                    VaalRighteousFireReplaceStats),
                SelfBuff("righteous_fire_spell_damage_+%_final")
            },
            {
                "ScourgeArrow",
                new SkillPartDefinitionExtension(
                    ReplaceStat("virulent_arrow_maximum_number_of_stacks", "maximum_stages")),
                ("Primary Projectile", new SkillPartDefinitionExtension(
                    RemoveStat("virulent_arrow_pod_projectile_damage_+%_final"),
                    AddStat("always_pierce", 1))),
                ("Thorn Arrows", new SkillPartDefinitionExtension(
                    ReplaceStat("virulent_arrow_pod_projectile_damage_+%_final", "damage_+%_final")))
            },
            {
                "ShockNova",
                ("Ring", new SkillPartDefinitionExtension(
                    ReplaceStat("newshocknova_first_ring_damage_+%_final", "damage_+%_final"))),
                ("Nova", new SkillPartDefinitionExtension(
                    RemoveStat("newshocknova_first_ring_damage_+%_final")))
            },
            {
                "ShrapnelShot",
                ("Projectile", new SkillPartDefinitionExtension(
                    AddStat("always_pierce", 1))),
                ("Cone", new SkillPartDefinitionExtension(
                    AddStat("is_area_damage", 1)))
            },
            {
                "Smite",
                new SkillPartDefinitionExtension(
                    ReplaceStat("base_chance_to_shock_%_from_skill", "base_chance_to_shock_%")
                        .AndThen(ReplaceStat("minimum_added_lightning_damage_from_skill",
                            "global_minimum_added_lightning_damage"))
                        .AndThen(ReplaceStat("maximum_added_lightning_damage_from_skill",
                            "global_maximum_added_lightning_damage"))),
                Aura("base_chance_to_shock_%",
                    "global_minimum_added_lightning_damage", "global_maximum_added_lightning_damage")
            },
            { "SmokeMine", SelfBuff("base_movement_velocity_+%") },
            {
                "StaticStrike",
                new SkillPartDefinitionExtension(
                    AddStat("maximum_stages", 3)),
                ("Melee Attack", new SkillPartDefinitionExtension(
                    RemoveStat("static_strike_base_zap_frequency_ms"))),
                ("Beams", new SkillPartDefinitionExtension(
                    ReplaceStat("static_strike_base_zap_frequency_ms", "hit_rate_ms")))
            },
            {
                "StormBurst",
                ("Projectile", new SkillPartDefinitionExtension(
                    RemoveStat("base_skill_show_average_damage_instead_of_dps"),
                    AddStat("always_pierce", 1))),
                ("Explosion", new SkillPartDefinitionExtension(
                    AddStat("is_area_damage", 1)))
            },
            {
                "SummonChaosGolem",
                new SkillPartDefinitionExtension(
                    ReplaceStat("chaos_golem_grants_additional_physical_damage_reduction_%",
                        "base_additional_physical_damage_reduction_%")),
                SelfBuff("base_additional_physical_damage_reduction_%")
            },
            {
                "SummonFireGolem",
                new SkillPartDefinitionExtension(
                    ReplaceStat("fire_golem_grants_damage_+%", "damage_+%")),
                SelfBuff("damage_+%")
            },
            {
                "SummonIceGolem",
                new SkillPartDefinitionExtension(
                    ReplaceStat("ice_golem_grants_critical_strike_chance_+%", "critical_strike_chance_+%")
                        .AndThen(ReplaceStat("ice_golem_grants_accuracy_+%", "accuracy_rating_+%"))),
                SelfBuff("critical_strike_chance_+%", "accuracy_rating_+%")
            },
            {
                "SummonLightningGolem",
                new SkillPartDefinitionExtension(
                    ReplaceStat("lightning_golem_grants_attack_and_cast_speed_+%", "attack_and_cast_speed_+%")),
                SelfBuff("attack_and_cast_speed_+%")
            },
            {
                "SummonRockGolem", // Summon Stone Golem
                new SkillPartDefinitionExtension(
                    ReplaceStat("stone_golem_grants_base_life_regeneration_rate_per_minute",
                        "base_life_regeneration_rate_per_minute")),
                SelfBuff("base_life_regeneration_rate_per_minute")
            },
            {
                "Sunder",
                ("Initial Hit", new SkillPartDefinitionExtension(
                    RemoveStat("shockwave_slam_explosion_damage_+%_final"))),
                ("Shockwave", new SkillPartDefinitionExtension(
                    ReplaceStat("shockwave_slam_explosion_damage_+%_final", "damage_+%_final")))
            },
            { "TempestShield", SelfBuff("shield_block_%") },
            {
                "TemporalChains",
                EnemyBuff("buff_time_passed_-%", "temporal_chains_action_speed_+%_final"),
                Passive("curse_effect_+%_vs_players")
            },
            {
                "ThrownShield", // Spectral Shield Throw
                ("Primary Projectile", new SkillPartDefinitionExtension()),
                ("Shards", new SkillPartDefinitionExtension())
            },
            { "ThrownWeapon", new SkillPartDefinitionExtension(AddStat("always_pierce", 1)) }, // Spectral Throw
            {
                "TotemMelee", // Ancestral Protector
                new SkillPartDefinitionExtension(
                    ReplaceStat("melee_ancestor_totem_grant_owner_attack_speed_+%_final",
                        "active_skill_attack_speed_+%_final")),
                SelfBuff("melee_ancestor_totem_grant_owner_attack_speed_+%_final")
            },
            { "Vitality", Aura("life_regeneration_rate_per_minute_%") },
            { "VolatileDead", CorpseExplodingSpellParts },
            {
                "Vulnerability",
                new SkillPartDefinitionExtension(
                    ReplaceStat("receive_bleeding_chance_%_when_hit_by_attack", "bleed_on_hit_with_attacks_%")
                        .AndThen(ReplaceStat("chance_to_be_maimed_when_hit_%", "maim_on_hit_%"))),
                Buff(("base_physical_damage_over_time_taken_+%", new[] { Entity.Enemy }),
                    ("physical_damage_taken_+%", new[] { Entity.Enemy }),
                    ("bleed_on_hit_with_attacks_%", AuraEntities),
                    ("maim_on_hit_%", AuraEntities))
            },
            {
                "WarlordsMark",
                new SkillPartDefinitionExtension(
                    ReplaceStat("life_leech_on_any_damage_when_hit_permyriad", "life_leech_from_any_damage_permyriad")
                        .AndThen(ReplaceStat("mana_leech_on_any_damage_when_hit_permyriad",
                            "mana_leech_from_any_damage_permyriad"))
                        .AndThen(ReplaceStat("chance_to_grant_endurance_charge_on_death_%",
                            "endurance_charge_on_kill_%"))),
                Buff(("chance_to_be_stunned_%", new[] { Entity.Enemy }),
                    ("base_stun_recovery_+%", new[] { Entity.Enemy }),
                    ("life_leech_from_any_damage_permyriad", AuraEntities),
                    ("mana_leech_from_any_damage_permyriad", AuraEntities),
                    ("endurance_charge_on_kill_%", AuraEntities))
            },
            {
                "WildStrike",
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
                    removedKeywords: new[] { Keyword.Melee }))
            },
            { "Wither", new SkillPartDefinitionExtension(RemoveStat("chaos_damage_taken_+%")) },
            {
                "Wrath",
                new SkillPartDefinitionExtension(
                    ReplaceStat("wrath_aura_spell_lightning_damage_+%_final", "spell_lightning_damage_+%_final")),
                SelfBuff("attack_minimum_added_lightning_damage", "attack_maximum_added_lightning_damage",
                    "spell_lightning_damage_+%_final")
            },

            {
                // The Arcane Surge buff always has added stats. Modify the ones granted by the support so the resulting
                // values end up being the same as with just the (unmodified) support and no stats added outside of it.
                // For BaseAdd and Increase that is just a reduction. For More, the multiplier has to be adjusted.
                // Because of rounding to int, the result is not precise.
                "SupportArcaneSurge",
                new SkillPartDefinitionExtension(
                    ReplaceStat("support_arcane_surge_spell_damage_+%_final", v => (int) Math.Round((v - 10) / 1.1))
                        .AndThen(ReplaceStat("support_arcane_surge_cast_speed_+%", v => v - 10))
                        .AndThen(ReplaceStat("support_arcane_surge_mana_regeneration_rate_per_minute_%", v => v - 30))),
                Passive("support_arcane_surge_spell_damage_+%_final", "support_arcane_surge_cast_speed_+%",
                    "support_arcane_surge_mana_regeneration_rate_per_minute_%")
            },
            { "SupportBlasphemy", Passive("curse_effect_+%") },
            {
                "SupportCastOnDeath",
                new SkillPartDefinitionExtension(
                    ReplaceStat("area_of_effect_+%_while_dead", "base_skill_area_of_effect_+%")
                        .AndThen(ReplaceStat("cast_on_death_damage_+%_final_while_dead", "damage_+%_final")))
            },
            {
                "SupportCastWhileChannelling",
                new SkillPartDefinitionExtension(
                    ReplaceStat("cast_while_channelling_time_ms", "hit_rate_ms")
                        .AndThen(ReplaceStat("support_cast_while_channelling_triggered_skill_damage_+%_final",
                            "damage_+%_final")))
            },
            {
                "SupportGemFrenzyPowerOnTrapTrigger", // Charged Traps
                new SkillPartDefinitionExtension(
                    ReplaceStat("trap_critical_strike_multiplier_+_per_power_charge",
                        "critical_strike_multiplier_+_per_power_charge"))
            },
            { "SupportGenerosity", Passive("aura_cannot_affect_self", "non_curse_aura_effect_+%") },
            {
                "SupportOnslaughtOnSlayingShockedEnemy", // Innervate
                Passive("support_innervate_minimum_added_lightning_damage",
                    "support_innervate_maximum_added_lightning_damage")
            },
            {
                "SupportRangedAttackTotem",
                new SkillPartDefinitionExtension(
                    ReplaceStat("support_attack_totem_attack_speed_+%_final", "active_skill_attack_speed_+%_final"))
            },
            {
                "SupportSpellTotem",
                new SkillPartDefinitionExtension(
                    ReplaceStat("support_spell_totem_cast_speed_+%_final", "active_skill_cast_speed_+%_final"))
            },
        };

        private static SkillPartDefinitionExtension SkillDotIsAreaDamageExtension
            => new SkillPartDefinitionExtension(AddStat("skill_dot_is_area_damage", 1));

        private static SkillPartDefinitionExtension RemoveShowAverageDamageExtension
            => new SkillPartDefinitionExtension(RemoveStat("base_skill_show_average_damage_instead_of_dps"));

        private static (string name, SkillPartDefinitionExtension extension)[] CorpseExplodingSpellParts
            => new[]
            {
                ("Spell", new SkillPartDefinitionExtension()),
                ("Corpse Explosion", new SkillPartDefinitionExtension(
                    AddStat("display_skill_deals_secondary_damage", 1)))
            };

        private static (string name, SkillPartDefinitionExtension extension)[] SecondaryProjectileMeleeAttackParts
            => new[]
            {
                ("Melee Attack", new SkillPartDefinitionExtension()),
                ("Projectiles", new SkillPartDefinitionExtension(
                    AddStats(("cast_rate_is_melee", 1), ("base_is_projectile", 1)),
                    removedKeywords: new[] { Keyword.Melee }))
            };

        private static (string name, SkillPartDefinitionExtension extension)[] SecondaryExplosionProjectileParts
            => new[]
            {
                ("Projectile", new SkillPartDefinitionExtension()),
                ("Explosion", new SkillPartDefinitionExtension(AddStat("is_area_damage", 1)))
            };

        private static (string name, SkillPartDefinitionExtension extension)[] EarthquakeParts
            => new[]
            {
                ("Initial Hit", new SkillPartDefinitionExtension(
                    RemoveStat("quake_slam_fully_charged_explosion_damage_+%_final"))),
                ("Aftershock", new SkillPartDefinitionExtension(
                    AddStat("base_skill_show_average_damage_instead_of_dps", 1),
                    ReplaceStat("quake_slam_fully_charged_explosion_damage_+%_final", "damage_+%_final")))
            };

        private static IEnumerable<string> RemoveStat(string statId)
            => new[] { statId };

        private static IEnumerable<string> RemoveStats(params string[] statIds)
            => statIds;

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
            string statId, Func<int, int> replaceValue)
            => ReplaceStat(statId, statId, replaceValue);

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

        private static IEnumerable<string> Passive(params string[] statIds)
            => statIds;
    }
}
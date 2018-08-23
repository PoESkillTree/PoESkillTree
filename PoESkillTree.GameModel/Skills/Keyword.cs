using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.GameModel.Skills
{
    public enum Keyword
    {
        /// <summary>
        /// Equivalent to the ActiveSkillType "attack".
        /// </summary>
        Attack,

        /// <summary>
        /// Equivalent to the ActiveSkillType "spell".
        /// </summary>
        Spell,

        /// <summary>
        /// Equivalent to the union of the ActiveSkillTypes "projectile" and "explicit_deals_projectile_damage"
        /// (skills have this keyword if they have at least one of the two types).
        /// </summary>
        Projectile,

        /// <summary>
        /// Equivalent to the ActiveSkillType "aoe".
        /// </summary>
        AreaOfEffect,

        /// <summary>
        /// Equivalent to the ActiveSkillType "melee".
        /// </summary>
        Melee,

        /// <summary>
        /// Equivalent to the ActiveSkillType "totem".
        /// </summary>
        Totem,

        /// <summary>
        /// Equivalent to the ActiveSkillType "curse".
        /// </summary>
        Curse,

        /// <summary>
        /// Equivalent to the ActiveSkillType "trap".
        /// </summary>
        Trap,

        /// <summary>
        /// Equivalent to the ActiveSkillType "movement".
        /// </summary>
        Movement,

        /// <summary>
        /// Equivalent to the ActiveSkillType "mine".
        /// </summary>
        Mine,

        /// <summary>
        /// Equivalent to the ActiveSkillType "vaal".
        /// </summary>
        Vaal,

        /// <summary>
        /// Equivalent to the ActiveSkillType "aura".
        /// </summary>
        Aura,

        /// <summary>
        /// Equivalent to the ActiveSkillType "golem".
        /// </summary>
        Golem,

        /// <summary>
        /// Equivalent to the ActiveSkillType "minion".
        /// </summary>
        Minion,

        /// <summary>
        /// Equivalent to the gem tag "Warcry".
        /// </summary>
        Warcry,

        /// <summary>
        /// Equivalent to the gem tag.
        /// </summary>
        Herald,

        /// <summary>
        /// Has no equivalent gem tag or ActiveSkillType.
        /// </summary>
        Offering,

        /// <summary>
        /// Equivalent to the ActiveSkillType "trigger_attack"
        /// </summary>
        CounterAttack,

        /// <summary>
        /// Never set.
        /// </summary>
        Physical,

        /// <summary>
        /// Equivalent to the ActiveSkillType.
        /// </summary>
        Lightning,

        /// <summary>
        /// Equivalent to the ActiveSkillType.
        /// </summary>
        Cold,

        /// <summary>
        /// Equivalent to the ActiveSkillType.
        /// </summary>
        Fire,

        /// <summary>
        /// Equivalent to the ActiveSkillType.
        /// </summary>
        Chaos,
    }

    public static class KeywordExtensions
    {
        private delegate bool KeywordApplies(
            string skillDisplayName, IEnumerable<string> activeSkillTypes, IEnumerable<string> gemTags);

        private static readonly IReadOnlyDictionary<Keyword, KeywordApplies> Conditions =
            new Dictionary<Keyword, KeywordApplies>
            {
                { Keyword.Attack, (_, types, __) => types.Contains("attack") },
                { Keyword.Spell, (_, types, __) => types.Contains("spell") },
                {
                    Keyword.Projectile,
                    (_, types, __) => types.Intersect(new[] { "projectile", "explicit_deals_projectile_damage" }).Any()
                },
                { Keyword.AreaOfEffect, (_, types, __) => types.Contains("aoe") },
                { Keyword.Melee, (_, types, __) => types.Contains("melee") },
                { Keyword.Totem, (_, types, __) => types.Contains("totem") },
                { Keyword.Curse, (_, types, __) => types.Contains("curse") },
                { Keyword.Trap, (_, types, __) => types.Contains("trap") },
                { Keyword.Movement, (_, types, __) => types.Contains("movement") },
                { Keyword.Mine, (_, types, __) => types.Contains("mine") },
                { Keyword.Vaal, (_, types, __) => types.Contains("vaal") },
                { Keyword.Aura, (_, types, __) => types.Contains("aura") },
                { Keyword.Golem, (_, types, __) => types.Contains("golem") },
                { Keyword.Minion, (_, types, __) => types.Contains("minion") },
                { Keyword.Warcry, (_, __, tags) => tags.Contains("warcry") },
                { Keyword.Herald, (_, __, tags) => tags.Contains("herald") },
                { Keyword.Offering, (name, _, __) => name.EndsWith("Offering") },
                { Keyword.CounterAttack, (_, types, __) => types.Contains("trigger_attack") },
                { Keyword.Physical, (_, ___, __) => false },
                { Keyword.Lightning, (_, types, __) => types.Contains("lightning") },
                { Keyword.Cold, (_, types, __) => types.Contains("cold") },
                { Keyword.Fire, (_, types, __) => types.Contains("fire") },
                { Keyword.Chaos, (_, types, __) => types.Contains("chaos") },
            };

        public static bool IsOnSkill(this Keyword @this,
            string skillDisplayName, IEnumerable<string> activeSkillTypes, IEnumerable<string> gemTags)
            => Conditions[@this](skillDisplayName, activeSkillTypes, gemTags);
    }
}
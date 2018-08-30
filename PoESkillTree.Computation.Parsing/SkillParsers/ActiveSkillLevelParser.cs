using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    public class ActiveSkillLevelParser
    {
        private readonly IBuilderFactories _builderFactories;
        private readonly IMetaStatBuilders _metaStatBuilders;
        private readonly IModifierBuilder _modifierBuilder = new ModifierBuilder();

        public ActiveSkillLevelParser(IBuilderFactories builderFactories, IMetaStatBuilders metaStatBuilders)
            => (_builderFactories, _metaStatBuilders) = (builderFactories, metaStatBuilders);

        public PartialSkillParseResult Parse(SkillPreParseResult preParseResult)
        {
            var modifiers = new List<Modifier>();
            var level = preParseResult.LevelDefinition;

            void AddModifier(IStatBuilder stat, Form form, double value)
            {
                var intermediateModifier = _modifierBuilder
                    .WithStat(stat)
                    .WithForm(_builderFactories.FormBuilders.From(form))
                    .WithValue(_builderFactories.ValueBuilders.Create(value))
                    .WithCondition(preParseResult.IsMainSkill.IsSet).Build();
                modifiers.AddRange(intermediateModifier.Build(preParseResult.GlobalSource, Entity.Character));
            }

            if (level.DamageEffectiveness is double effectiveness)
            {
                AddModifier(_metaStatBuilders.DamageBaseAddEffectiveness, Form.TotalOverride, effectiveness);
            }
            if (level.DamageMultiplier is double multiplier)
            {
                AddModifier(_metaStatBuilders.DamageBaseSetEffectiveness, Form.TotalOverride, multiplier);
            }
            if (level.CriticalStrikeChance is double crit &&
                preParseResult.HitDamageSource is DamageSource hitDamageSource)
            {
                AddModifier(_builderFactories.ActionBuilders.CriticalStrike.Chance.With(hitDamageSource),
                    Form.BaseSet, crit);
            }
            if (level.ManaCost is int cost)
            {
                AddModifier(_builderFactories.StatBuilders.Pool.From(Pool.Mana).Cost, Form.BaseSet, cost);
            }
            if (level.Cooldown is int cooldown)
            {
                AddModifier(_builderFactories.StatBuilders.Cooldown, Form.BaseSet, cooldown);
            }

            return new PartialSkillParseResult(modifiers, new UntranslatedStat[0]);
        }
    }
}
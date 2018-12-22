using EnumsNET;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    public class ActiveSkillLevelParser : IPartialSkillParser
    {
        private readonly IBuilderFactories _builderFactories;
        private readonly IMetaStatBuilders _metaStatBuilders;

        private SkillModifierCollection _modifiers;
        private SkillPreParseResult _preParseResult;

        public ActiveSkillLevelParser(IBuilderFactories builderFactories, IMetaStatBuilders metaStatBuilders)
            => (_builderFactories, _metaStatBuilders) = (builderFactories, metaStatBuilders);

        public PartialSkillParseResult Parse(Skill mainSkill, Skill parsedSkill, SkillPreParseResult preParseResult)
        {
            _modifiers = new SkillModifierCollection(_builderFactories,
                preParseResult.IsMainSkill.IsSet, preParseResult.LocalSource);
            _preParseResult = preParseResult;
            var level = preParseResult.LevelDefinition;

            if (level.DamageEffectiveness is double effectiveness)
            {
                _modifiers.AddGlobalForMainSkill(_metaStatBuilders.DamageBaseAddEffectiveness,
                    Form.TotalOverride, effectiveness);
            }
            if (level.DamageMultiplier is double multiplier)
            {
                _modifiers.AddGlobalForMainSkill(_metaStatBuilders.DamageBaseSetEffectiveness,
                    Form.TotalOverride, multiplier);
            }
            if (level.CriticalStrikeChance is double crit)
            {
                _modifiers.AddGlobalForMainSkill(_builderFactories.ActionBuilders.CriticalStrike.Chance.WithHits,
                    Form.BaseSet, crit);
            }
            if (level.ManaCost is int cost)
            {
                var costStat = _metaStatBuilders.SkillBaseCost(parsedSkill.ItemSlot, parsedSkill.SocketIndex);
                _modifiers.AddGlobal(costStat, Form.BaseSet, cost);
                _modifiers.AddGlobalForMainSkill(_builderFactories.StatBuilders.Pool.From(Pool.Mana).Cost,
                    Form.BaseSet, costStat.Value);
                ParseReservation(mainSkill, costStat);
            }
            if (level.Cooldown is int cooldown)
            {
                _modifiers.AddGlobalForMainSkill(_builderFactories.StatBuilders.Cooldown, Form.BaseSet, cooldown);
            }

            var result = new PartialSkillParseResult(_modifiers, new UntranslatedStat[0]);
            _modifiers = null;
            _preParseResult = preParseResult;
            return result;
        }

        private void ParseReservation(Skill skill, IStatBuilder costStat)
        {
            var isReservation = _metaStatBuilders
                .SkillHasType(skill.ItemSlot, skill.SocketIndex, ActiveSkillType.ManaCostIsReservation).IsSet;
            var isReservationAndActive = isReservation.And(_preParseResult.IsActiveSkill);
            var isPercentage = _metaStatBuilders
                .SkillHasType(skill.ItemSlot, skill.SocketIndex, ActiveSkillType.ManaCostIsPercentage).IsSet;
            var skillBuilder = _builderFactories.SkillBuilders.FromId(_preParseResult.SkillDefinition.Id);

            _modifiers.AddGlobal(skillBuilder.Reservation, Form.BaseSet, costStat.Value, isReservationAndActive);

            foreach (var pool in Enums.GetValues<Pool>())
            {
                var poolBuilder = _builderFactories.StatBuilders.Pool.From(pool);
                var value = skillBuilder.Reservation.Value;
                value = _builderFactories.ValueBuilders
                    .If(isPercentage).Then(value.AsPercentage * poolBuilder.Value)
                    .Else(value);
                _modifiers.AddGlobal(poolBuilder.Reservation, Form.BaseAdd, value,
                    skillBuilder.ReservationPool.Value.Eq((double) pool).And(isReservationAndActive));
            }
        }
    }
}
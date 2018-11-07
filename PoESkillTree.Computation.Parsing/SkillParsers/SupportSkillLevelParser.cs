using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    public class SupportSkillLevelParser : IPartialSkillParser
    {
        private readonly IBuilderFactories _builderFactories;
        private readonly IMetaStatBuilders _metaStatBuilders;
        private readonly IModifierBuilder _modifierBuilder = new ModifierBuilder();

        public SupportSkillLevelParser(IBuilderFactories builderFactories, IMetaStatBuilders metaStatBuilders)
            => (_builderFactories, _metaStatBuilders) = (builderFactories, metaStatBuilders);

        public PartialSkillParseResult Parse(Skill mainSkill, Skill parsedSkill, SkillPreParseResult preParseResult)
        {
            var modifiers = new List<Modifier>();
            var level = preParseResult.LevelDefinition;

            void AddModifier(IStatBuilder stat, Form form, double value, IConditionBuilder condition = null)
            {
                var builder = _modifierBuilder
                    .WithStat(stat)
                    .WithForm(_builderFactories.FormBuilders.From(form))
                    .WithValue(_builderFactories.ValueBuilders.Create(value));
                if (condition != null)
                    builder = builder.WithCondition(condition);
                var intermediateModifier = builder.Build();
                modifiers.AddRange(intermediateModifier.Build(preParseResult.GlobalSource, Entity.Character));
            }

            if (level.ManaMultiplier is double multiplier)
            {
                var moreMultiplier = multiplier * 100 - 100;
                AddModifier(_builderFactories.StatBuilders.Pool.From(Pool.Mana).Cost, Form.More, moreMultiplier,
                    preParseResult.IsMainSkill.IsSet);
                AddModifier(_builderFactories.SkillBuilders.FromId(mainSkill.Id).Reservation,
                    Form.More, moreMultiplier, preParseResult.IsActiveSkill);
            }

            if (level.ManaCostOverride is int manaCostOverride)
            {
                AddModifier(_metaStatBuilders.SkillBaseCost(mainSkill.ItemSlot, mainSkill.SocketIndex),
                    Form.TotalOverride, manaCostOverride);
            }

            return new PartialSkillParseResult(modifiers, new UntranslatedStat[0]);
        }
    }
}
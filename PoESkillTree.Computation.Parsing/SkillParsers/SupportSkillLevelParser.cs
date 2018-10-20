using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
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

        public PartialSkillParseResult Parse(Skill skill, SkillPreParseResult preParseResult)
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

            if (level.ManaMultiplier is double multiplier)
            {
                AddModifier(_builderFactories.StatBuilders.Pool.From(Pool.Mana).Cost, Form.More,
                    multiplier * 100 - 100);
            }

            return new PartialSkillParseResult(modifiers, new UntranslatedStat[0]);
        }
    }
}
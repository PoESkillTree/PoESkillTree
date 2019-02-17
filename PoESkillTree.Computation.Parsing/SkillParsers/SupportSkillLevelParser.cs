using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    /// <summary>
    /// Partial parser of <see cref="SupportSkillParser"/> that parses level-dependent modifiers.
    /// </summary>
    public class SupportSkillLevelParser : IPartialSkillParser
    {
        private readonly IBuilderFactories _builderFactories;

        public SupportSkillLevelParser(IBuilderFactories builderFactories)
            => _builderFactories = builderFactories;

        private IMetaStatBuilders MetaStats => _builderFactories.MetaStatBuilders;

        public PartialSkillParseResult Parse(Skill mainSkill, Skill parsedSkill, SkillPreParseResult preParseResult)
        {
            var level = preParseResult.LevelDefinition;
            var modifiers = new ModifierCollection(_builderFactories, preParseResult.LocalSource);

            if (level.ManaMultiplier is double multiplier)
            {
                var moreMultiplier = multiplier * 100 - 100;
                modifiers.AddGlobal(_builderFactories.StatBuilders.Pool.From(Pool.Mana).Cost,
                    Form.More, moreMultiplier, preParseResult.IsMainSkill);
                modifiers.AddGlobal(_builderFactories.SkillBuilders.FromId(mainSkill.Id).Reservation,
                    Form.More, moreMultiplier, preParseResult.IsActiveSkill);
            }

            if (level.ManaCostOverride is int manaCostOverride)
            {
                modifiers.AddGlobal(MetaStats.SkillBaseCost(mainSkill.ItemSlot, mainSkill.SocketIndex),
                    Form.TotalOverride, manaCostOverride);
            }

            return new PartialSkillParseResult(modifiers.Modifiers, new UntranslatedStat[0]);
        }
    }
}
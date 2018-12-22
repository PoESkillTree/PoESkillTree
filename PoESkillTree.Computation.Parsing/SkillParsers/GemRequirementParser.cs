using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    public class GemRequirementParser : IPartialSkillParser
    {
        private readonly IBuilderFactories _builderFactories;

        public GemRequirementParser(IBuilderFactories builderFactories)
            => _builderFactories = builderFactories;

        public PartialSkillParseResult Parse(Skill mainSkill, Skill parsedSkill, SkillPreParseResult preParseResult)
        {
            if (!parsedSkill.GemGroup.HasValue)
                return new PartialSkillParseResult(new Modifier[0], new UntranslatedStat[0]);

            var modifiers = new ModifierCollection(_builderFactories, preParseResult.GemSource);
            var level = preParseResult.LevelDefinition;
            var requirementStats = _builderFactories.StatBuilders.Requirements;

            modifiers.AddLocal(requirementStats.Level, Form.BaseSet, level.Requirements.Level);
            if (level.Requirements.Dexterity > 0)
            {
                modifiers.AddLocal(requirementStats.Dexterity, Form.BaseSet, level.Requirements.Dexterity);
            }
            if (level.Requirements.Intelligence > 0)
            {
                modifiers.AddLocal(requirementStats.Intelligence, Form.BaseSet, level.Requirements.Intelligence);
            }
            if (level.Requirements.Strength > 0)
            {
                modifiers.AddLocal(requirementStats.Strength, Form.BaseSet, level.Requirements.Strength);
            }

            return new PartialSkillParseResult(modifiers, new UntranslatedStat[0]);
        }
    }
}
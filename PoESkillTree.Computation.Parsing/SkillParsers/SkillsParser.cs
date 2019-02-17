using System.Collections.Generic;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    /// <summary>
    /// Parses a group of skills at once. E.g. all equipped skills or all skills for one ItemSlot.
    /// </summary>
    public class SkillsParser : IParser<IReadOnlyCollection<Skill>>
    {
        private readonly SkillDefinitions _skillDefinitions;
        private readonly SupportabilityTester _supportabilityTester;
        private readonly IParser<Skill> _activeSkillParser;
        private readonly IParser<SupportSkillParserParameter> _supportSkillParser;

        public SkillsParser(
            SkillDefinitions skillDefinitions,
            IParser<Skill> activeSkillParser, IParser<SupportSkillParserParameter> supportSkillParser)
        {
            _skillDefinitions = skillDefinitions;
            _supportabilityTester = new SupportabilityTester(skillDefinitions);
            _activeSkillParser = activeSkillParser;
            _supportSkillParser = supportSkillParser;
        }

        public ParseResult Parse(IReadOnlyCollection<Skill> skills)
        {
            var activeSkills = new List<Skill>();
            var supportSkills = new List<Skill>(skills.Count);
            foreach (var skill in skills)
            {
                if (_skillDefinitions.GetSkillById(skill.Id).IsSupport)
                    supportSkills.Add(skill);
                else
                    activeSkills.Add(skill);
            }

            var parseResults = new List<ParseResult>(activeSkills.Count * supportSkills.Count);
            foreach (var activeSkill in activeSkills)
            {
                parseResults.Add(_activeSkillParser.Parse(activeSkill));
                var supportingSkills = _supportabilityTester.SelectSupportingSkills(activeSkill, supportSkills);
                foreach (var supportingSkill in supportingSkills)
                {
                    parseResults.Add(_supportSkillParser.Parse(activeSkill, supportingSkill));
                }
            }

            return ParseResult.Aggregate(parseResults);
        }
    }
}
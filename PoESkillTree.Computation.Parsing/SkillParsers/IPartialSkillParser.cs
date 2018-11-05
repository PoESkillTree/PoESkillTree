using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    public interface IPartialSkillParser
    {
        /// <param name="mainSkill">The supported active skill</param>
        /// <param name="parsedSkill">The skill currently being parsed. If this is an active skill, it is the same
        /// as <paramref name="mainSkill"/>. Otherwise this is a support skill.</param>
        /// <param name="preParseResult"></param>
        PartialSkillParseResult Parse(Skill mainSkill, Skill parsedSkill, SkillPreParseResult preParseResult);
    }
}
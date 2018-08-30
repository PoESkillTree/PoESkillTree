namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    public interface IPartialSkillParser
    {
        PartialSkillParseResult Parse(Skill skill, SkillPreParseResult preParseResult);
    }
}
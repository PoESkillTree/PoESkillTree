namespace PoESkillTree.Computation.Parsing.Referencing
{
    public interface IRegexGroupFactory
    {
        string CreateValueGroup(string groupPrefix, string innerRegex);

        string CreateReferenceGroup(string groupPrefix, string referenceName, int matcherIndex, string innerRegex);

        string CombineGroupPrefixes(string left, string right);
    }
}
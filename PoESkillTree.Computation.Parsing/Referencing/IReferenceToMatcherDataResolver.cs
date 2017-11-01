using PoESkillTree.Computation.Parsing.Data;

namespace PoESkillTree.Computation.Parsing.Referencing
{
    public interface IReferenceToMatcherDataResolver
    {
        bool TryGetReferencedMatcherData(
            string referenceName, int matcherIndex, out ReferencedMatcherData matcherData);

        bool TryGetMatcherData(string referenceName, int matcherIndex, out MatcherData matcherData);
    }
}
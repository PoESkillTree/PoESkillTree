using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.Builders.Matching
{
    public interface IMatchContexts
    {
        IMatchContext<IReferenceConverter> References { get; }

        IMatchContext<ValueBuilder> Values { get; }
    }
}
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.Builders.Matching
{
    /// <summary>
    /// Interface holding the two contextual collections: objects of other matcher collections referenced in the regex
    /// and numerical values matched in the regex.
    /// </summary>
    public interface IMatchContexts
    {
        /// <summary>
        /// Gets the collection holding the objects from other matcher collections that were referenced in the
        /// matcher's regex.
        /// </summary>
        IMatchContext<IReferenceConverter> References { get; }

        /// <summary>
        /// Gets the collection holding the numerical values referenced in the matcher's regex.
        /// </summary>
        IMatchContext<ValueBuilder> Values { get; }
    }
}
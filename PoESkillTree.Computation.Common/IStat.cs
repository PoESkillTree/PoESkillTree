using System;

namespace PoESkillTree.Computation.Common
{
    public interface IStat : IEquatable<IStat>
    {
        // Each IStat represents one calculation subgraph

        // Returns true if object is an IStat representing the same calculation subgraph.
        bool Equals(object obj);

        // Returns a string naming the represented calculation subgraph.
        string ToString();

        // If there is only one IStat subclass:
        // The object determining equality can be passed to its constructor and can be used for ToString()

        // If there are multiple:
        // A property like "object Identity { get; }" is required to have different subclasses that can have instances
        // representing the same subgraph.
        // Whether this is required depends on how identification of special stats is implemented.
    }
}
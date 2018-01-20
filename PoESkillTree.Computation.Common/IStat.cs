using System;
using JetBrains.Annotations;

namespace PoESkillTree.Computation.Common
{
    public interface IStat : IEquatable<IStat>
    {
        // Each IStat represents one calculation subgraph

        // Returns true if object is an IStat representing the same calculation subgraph.
        bool Equals(object obj);

        // Returns a string naming the represented calculation subgraph.
        string ToString();

        // To avoid endless recursion, these can be null if the stat subgraph shouldn't reference them.
        // They should only be null if the stat itself already represents a minimum/maximum subgraph.
        [CanBeNull]
        IStat Minimum { get; }
        [CanBeNull]
        IStat Maximum { get; }

        // If there is only one IStat subclass:
        // The object determining equality can be passed to its constructor and can be used for ToString()

        // If there are multiple and instances of different subclasses can be equal:
        // A property like "object Identity { get; }" is required to have different subclasses that can have instances
        // representing the same subgraph.
        // Whether this is required depends on how identification of special stats is implemented.
    }
}
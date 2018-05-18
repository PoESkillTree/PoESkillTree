using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace PoESkillTree.Computation.Common
{
    // Each IStat represents one calculation subgraph
    // Object.Equals() and IEquatable.Equals() return true if the parameter is an IStat representing the same
    // calculation subgraph.
    public interface IStat : IEquatable<IStat>
    {
        // Returns a string naming the represented calculation subgraph.
        string ToString();

        // To avoid endless recursion, these can be null if the stat subgraph shouldn't reference them.
        // They should only be null if the stat itself already represents a minimum/maximum subgraph.
        [CanBeNull]
        IStat Minimum { get; }

        [CanBeNull]
        IStat Maximum { get; }

        // True if the existence/usage of this stat should be explicitly announced to clients
        bool IsRegisteredExplicitly { get; }

        // The type of this stat's values. Can be double, int or bool (0 or 1).
        // The value range is determined by Minimum and Maximum (which have the same DataType).
        Type DataType { get; }

        IEnumerable<Behavior> Behaviors { get; }

        // If there is only one IStat subclass:
        // The object determining equality can be passed to its constructor and can be used for ToString()

        // If there are multiple and instances of different subclasses can be equal:
        // A property like "object Identity { get; }" is required to have different subclasses that can have instances
        // representing the same subgraph.
        // Whether this is required depends on how identification of special stats is implemented.
    }
}
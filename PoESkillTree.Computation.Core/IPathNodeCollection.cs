using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    // Collection representing the nodes in a stat subgraph of a specific NodeType.
    public interface IPathNodeCollection
    {
        IReadOnlyList<PathNodeCollectionItem> Items { get; }

        event EventHandler ItemsChanged;
    }


    public class PathNodeCollectionItem
    {
        public ICalculationNode Node { get; }
        
        // Modifier source:
        // - One PathNodeCollectionItem with Global in most cases
        // - Local should only be items, which are then distinguished by their slot
        // - The object only holds information that is the same for the sources of all modifiers applying to the path
        //   E.g. the Global source will generally not hold additional information, and item local sources will contain
        //   the item's name.
        public IModifierSource Source { get; }
        
        // Conversion/gain path: The stats on the conversion path, in order beginning with the original stat.
        // The type is not final, a string representation might be enough.
        public IReadOnlyList<IStat> ConversionPath { get; }
    }
}
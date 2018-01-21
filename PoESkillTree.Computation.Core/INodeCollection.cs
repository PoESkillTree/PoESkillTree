using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public interface INodeCollection<out T> where T: NodeCollectionItem
    {
        IReadOnlyList<T> Items { get; }

        event EventHandler ItemsChanged;
    }


    public class NodeCollectionItem
    {
        public NodeCollectionItem(ICalculationNode node)
        {
            Node = node;
        }

        public ICalculationNode Node { get; }
    }


    public class PathNodeCollectionItem : NodeCollectionItem
    {
        public PathNodeCollectionItem(
            ICalculationNode node, IModifierSource source, IReadOnlyList<IStat> conversionPath)
            : base(node)
        {
            Source = source;
            ConversionPath = conversionPath;
        }
        
        // Modifier source:
        // - One PathNodeCollectionItem with Global in most cases
        // - Local should only be items, which are then distinguished by their slot
        // - The object only holds information that is the same for the sources of all modifiers applying to the path
        //   E.g. the Global source will generally not hold additional information, and item local sources will contain
        //   the item's name.
        public IModifierSource Source { get; }
        
        // Conversion/gain path: The stats on the conversion path, in order beginning with the original stat.
        // For unconverted paths this only contains the target stat.
        // The type is not final, a string representation might be enough.
        public IReadOnlyList<IStat> ConversionPath { get; }
    }


    public class FormNodeCollectionItem : NodeCollectionItem
    {
        public FormNodeCollectionItem(ICalculationNode node, IModifierSource source, object notes)
            : base(node)
        {
            Source = source;
            Notes = notes;
        }

        // IModifierSource of the modifier that lead to the creation of the node
        public IModifierSource Source { get; }

        // Notes about the modifiers application, e.g. conditions, whether it is provided by a buff/aura, ...
        // Actual type is not yet determined.
        public object Notes { get; }
    }
}
using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public interface INodeCollection
    {
        IReadOnlyList<NodeCollectionItem> Items { get; }

        event EventHandler ItemsChanged;
    }

    public interface INodeCollection<out T> : INodeCollection where T: NodeCollectionItem
    {
        new IReadOnlyList<T> Items { get; }
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
        public FormNodeCollectionItem(ICalculationNode node, Modifier modifier)
            : base(node)
        {
            Modifier = modifier;
        }

        // The modifier that lead to the creation of the node
        public Modifier Modifier { get; }
    }
}
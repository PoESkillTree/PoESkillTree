using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    // Collection representing the modifier nodes of a stat for a specific form.
    public interface IFormNodeCollection
    {
        IReadOnlyList<FormNodeCollectionItem> Items { get; }

        event EventHandler ItemsChanged;
    }


    public class FormNodeCollectionItem
    {
        public FormNodeCollectionItem(ICalculationNode node, IModifierSource source, object notes)
        {
            Node = node;
            Source = source;
            Notes = notes;
        }

        public ICalculationNode Node { get; }

        // IModifierSource of the modifier that lead to the creation of the node
        public IModifierSource Source { get; }

        // Notes about the modifiers application, e.g. conditions, whether it is provided by a buff/aura, ...
        // Actual type is not yet determined.
        public object Notes { get; }
    }
}
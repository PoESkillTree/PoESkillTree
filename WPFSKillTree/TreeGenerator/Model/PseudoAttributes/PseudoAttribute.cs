using System;
using System.Collections.Generic;

namespace PoESkillTree.TreeGenerator.Model.PseudoAttributes
{
    /// <summary>
    /// Data class describing a PseudoAttribute as a collection of <see cref="Attribute"/>s.
    /// </summary>
    public class PseudoAttribute
    {
        /// <summary>
        /// Gets the name of the PseudoAttribute.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the list of Attributes this PseudoAttribute contains.
        /// </summary>
        public List<Attribute> Attributes { get; }

        /// <summary>
        /// Gets the name of the group this PseudoAttribute belongs to.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // Used in group and sort descriptions.
        public string Group { get; }

        /// <summary>
        /// Creates a new PseudoAttribute with the given name and group
        /// and an empty list of Attributes.
        /// </summary>
        /// <param name="name">Name (not null)</param>
        /// <param name="group">Group (not null)</param>
        internal PseudoAttribute(string name, string group)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Group = @group ?? throw new ArgumentNullException(nameof(@group));
            Attributes = new List<Attribute>();
        }

        public override string ToString() => Name;
    }
}
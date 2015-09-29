using System;

namespace POESKillTree.TreeGenerator.Model.PseudoAttributes
{
    /// <summary>
    /// Data class describing a conditioned Attribute.
    /// At least one condition must be true or there must not be
    /// any conditions for <see cref="ICondition.Eval"/> to return true.
    /// (see <see cref="OrComposition"/>)
    /// </summary>
    public class Attribute : OrComposition
    {
        /// <summary>
        /// Gets the name this attribute. 
        /// Represents the name of an attribute of nodes in the skill tree
        /// (numbers replaced by '#').
        /// </summary>
        /// <remarks>
        /// Parts of the skill node attribute may be replaced by '{number}'.
        /// For testing if a skill node attribute matches this attribute, these
        /// wildcards can match any string. The wildcards can be referenced
        /// in the conditions: '{number}' parts in conditions are replaced by
        /// all matches of skill node attributes.
        /// </remarks>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the multiplier by which values of skill node attributes should
        /// be multiplied before being adding into the <see cref="PseudoAttribute"/>
        /// containing this attribute.
        /// </summary>
        public float ConversionMultiplier { get; internal set; }

        /// <summary>
        /// Creates a new Attribute with the given name, a ConversionMultiplier
        /// of 1 and no conditions.
        /// </summary>
        /// <param name="name">Name referencing skill tree attributes (not null)</param>
        internal Attribute(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            Name = name;
            ConversionMultiplier = 1;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
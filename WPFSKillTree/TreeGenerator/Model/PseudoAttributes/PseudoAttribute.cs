using System;
using System.Collections.Generic;

namespace POESKillTree.TreeGenerator.Model.PseudoAttributes
{
    public class PseudoAttribute
    {
        public string Name { get; private set; }

        public List<Attribute> Attributes { get; private set; }

        public string Group { get; private set; }

        internal PseudoAttribute(string name, string group)
        {
            if (name == null) throw new ArgumentNullException("name");
            Name = name;
            Group = group;
            Attributes = new List<Attribute>();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
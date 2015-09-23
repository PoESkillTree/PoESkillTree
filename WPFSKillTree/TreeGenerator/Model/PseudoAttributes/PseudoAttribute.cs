using System;
using System.Collections.Generic;

namespace POESKillTree.TreeGenerator.Model.PseudoAttributes
{
    public class PseudoAttribute
    {
        public string Name { get; private set; }

        public List<Attribute> Attributes { get; set; }

        public string Group { get; set; }

        public PseudoAttribute(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            Name = name;
            Attributes = new List<Attribute>();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
using System;

namespace POESKillTree.TreeGenerator.Model.PseudoAttributes
{
    public class Attribute : OrComposition
    {
        public string Name { get; private set; }

        public float ConversionMultiplier { get; set; }

        public Attribute(string name)
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
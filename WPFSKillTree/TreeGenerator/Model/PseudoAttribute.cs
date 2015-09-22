using System.Collections.Generic;

namespace POESKillTree.TreeGenerator.Model
{
    public class PseudoAttribute : Attribute
    {
        public List<Attribute> Attributes { get; set; }

        public bool Hidden { get; set; }

        public string Group { get; set; }

        public PseudoAttribute(string name)
            : base(name)
        {
            Attributes = new List<Attribute>();
        }
    }
}
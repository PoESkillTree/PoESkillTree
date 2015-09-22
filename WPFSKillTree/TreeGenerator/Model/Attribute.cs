using POESKillTree.TreeGenerator.Model.Conditions;

namespace POESKillTree.TreeGenerator.Model
{
    public class Attribute : OrComposition
    {
        public string Name { get; private set; }

        public int Id { get; set; }

        public double ConversionRate { get; set; }

        public Attribute(string name)
        {
            Name = name;
            ConversionRate = 1;
        }
    }
}
using POESKillTree.TreeGenerator.Model.Conditions;

namespace POESKillTree.TreeGenerator.Model
{
    public class Attribute : OrComposition
    {
        public string Name { get; set; }

        public int Id { get; set; }

        public double ConversionRate { get; set; }

        public Attribute()
        {
            ConversionRate = 1;
        }
    }
}
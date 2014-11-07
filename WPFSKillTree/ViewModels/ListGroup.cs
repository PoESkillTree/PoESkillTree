using System.Collections.Generic;

namespace POESKillTree.ViewModels
{
    public class ListGroup
    {
        public string Name { get; set; }
        public Dictionary<string, List<float>> Properties { get; set; }

        public ListGroup(string name, Dictionary<string, List<float>> props)
        {
            Name = name;
            Properties = props;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

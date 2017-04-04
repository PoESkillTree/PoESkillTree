using System.Collections.Generic;

namespace POESKillTree.ViewModels
{
    using CSharpGlobalCode.GlobalCode_ExperimentalCode;
    public class ListGroup
    {
        public string Name { get; set; }

#if (PoESkillTree_UseSmallDec_ForAttributes)
        public Dictionary<string, List<SmallDec>> Properties { get; set; }
        public ListGroup(string name, Dictionary<string, List<SmallDec>> props)
#else
        public Dictionary<string, List<float>> Properties { get; set; }
        public ListGroup(string name, Dictionary<string, List<float>> props)
#endif
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

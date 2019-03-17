using System.Collections.Generic;

namespace PoESkillTree.SkillTreeFiles
{
    public class SkillNodeGroup
    {
        public List<SkillNode> Nodes = new List<SkillNode>(); // "n": [-28194677,769796679,-1093139159]
        public Dictionary<int, bool> OcpOrb = new Dictionary<int, bool>(); //  "oo": {"1": true},
        public Vector2D Position; // "x": 1105.14,"y": -5295.31,
    }
}
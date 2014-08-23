using System;
using System.Collections.Generic;

namespace POESKillTree.SkillTreeFiles
{
    public class SkillNode
    {
        public static float[] skillsPerOrbit = {1, 6, 12, 12, 12};
        public static float[] orbitRadii = {0, 81.5f, 163, 326, 489};
        public Dictionary<string, List<float>> Attributes;
        public HashSet<int> Connections = new HashSet<int>();
        public bool Mastery;
        public List<SkillNode> Neighbor = new List<SkillNode>();
        public SkillNodeGroup SkillNodeGroup;
        public int a; // "a": 3,
        public string[] attributes; // "sd": ["8% increased Block Recovery"],
        public int da; // "da": 0,
        public int g; // "g": 1,
        public int ia; //"ia": 0,
        public string icon; // icon "icon": "Art/2DArt/SkillIcons/passives/tempint.png",
        public UInt16 id; // "id": -28194677,
        public bool ks; //"ks": false,
        public List<int> linkID = new List<int>(); // "out": []
        public bool m; //"m": false
        public string name; //"dn": "Block Recovery",
        public bool not; // not": false,
        public int orbit; //  "o": 1,
        public int orbitIndex; // "oidx": 3,
        public int sa; //s "sa": 0,
        public bool skilled = false;
        public int? spc;

        public Vector2D Position
        {
            get
            {
                if (SkillNodeGroup == null) return new Vector2D();
                double d = orbitRadii[orbit];
                double b = (2 * Math.PI * orbitIndex / skillsPerOrbit[orbit]);
                return (SkillNodeGroup.Position - new Vector2D(d * Math.Sin(-b), d * Math.Cos(-b)));
            }
        }

        public double Arc
        {
            get { return (2 * Math.PI * orbitIndex / skillsPerOrbit[orbit]); }
        }
    }
}
using System;
using System.Collections.Generic;

namespace POESKillTree.SkillTreeFiles
{
    public class SkillNode
    {
        public static float[] SkillsPerOrbit = {1, 6, 12, 12, 12};
        public static float[] OrbitRadii = {0, 81.5f, 163, 326, 489};
        public Dictionary<string, List<float>> Attributes;
        public HashSet<int> Connections = new HashSet<int>();
        public List<SkillNode> Neighbor = new List<SkillNode>();
        public SkillNodeGroup SkillNodeGroup;
        public int A; // "a": 3,
        public string[] attributes; // "sd": ["8% increased Block Recovery"],
        public int Da; // "da": 0,
        public int G; // "g": 1,
        public int Ia; //"ia": 0,
        public string Icon; // icon "icon": "Art/2DArt/SkillIcons/passives/tempint.png",
        public UInt16 Id; // "id": -28194677,
        public bool IsKeyStone; //"ks": false,
        public bool IsNotable; // not": false,
        public bool IsMastery; // m: false,
        public bool IsJewelSocket; //comes from name
        public List<int> LinkId = new List<int>(); // "out": []
        public string Name; //"dn": "Block Recovery",
        public int Orbit; //  "o": 1,
        public int OrbitIndex; // "oidx": 3,
        public int Sa; //s "sa": 0,
        public bool IsSkilled = false;
        public int? Spc;

        public Vector2D Position
        {
            get
            {
                if (SkillNodeGroup == null) return new Vector2D();
                double d = OrbitRadii[Orbit];
                double b = (2*Math.PI*OrbitIndex/SkillsPerOrbit[Orbit]);
                return (SkillNodeGroup.Position - new Vector2D(d*Math.Sin(-b), d*Math.Cos(-b)));
            }
        }

        public double Arc
        {
            get { return (2*Math.PI*OrbitIndex/SkillsPerOrbit[Orbit]); }
        }

        public string IconKey
        {
            get
            {
                //TODO: Change The tree bools to an enum instead
                int iconWidth;
                if (IsMastery)
                {
                    iconWidth = SkillIcons.MasteryIconWidth;
                }
                else if(IsKeyStone)
                {
                    iconWidth = SkillIcons.KeystoneIconWidth;
                }
                else if (IsNotable)
                {
                    iconWidth = SkillIcons.NotableIconWidth;
                }
                else
                {
                    iconWidth = SkillIcons.NormalIconWidth;
                }
                return Icon + "_" + iconWidth;
            }
        }
    }
}
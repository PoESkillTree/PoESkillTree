using System;
using System.Collections.Generic;
using PoESkillTree.GameModel.PassiveTree;

namespace POESKillTree.SkillTreeFiles
{
    public class SkillNode
    {
        public static float[] SkillsPerOrbit = {1, 6, 12, 12, 40};
        public static float[] OrbitRadii = {0, 81.5f, 163, 326, 489};
        public Dictionary<string, IReadOnlyList<float>> Attributes;
        public List<SkillNode> Neighbor = new List<SkillNode>();
        // The subset of neighbors to which connections should be drawn.
        public readonly List<SkillNode> VisibleNeighbors = new List<SkillNode>();
        public SkillNodeGroup SkillNodeGroup;
        public int A; // "a": 3,
        public string[] attributes; // "sd": ["8% increased Block Recovery"],
        public int Da; // "da": 0,
        public int G; // "g": 1,
        public int Ia; //"ia": 0,
        public string Icon; // icon "icon": "Art/2DArt/SkillIcons/passives/tempint.png",
        public ushort Id; // "id": -28194677,
        public PassiveNodeType Type; // "ks", "not", "m", "isJewelSocket"
        public List<ushort> LinkId = new List<ushort>(); // "out": []
        public string Name; //"dn": "Block Recovery",
        public int Orbit; //  "o": 1,
        public int OrbitIndex; // "oidx": 3,
        public int Sa; //s "sa": 0,
        public int? Spc;
        public bool IsMultipleChoice; //"isMultipleChoice": false
        public bool IsMultipleChoiceOption; //"isMultipleChoiceOption": false
        public int passivePointsGranted; //"passivePointsGranted": 1
        public string ascendancyName; //"ascendancyName": "Raider"
        public bool IsAscendancyStart; //"isAscendancyStart": false
        public string[] reminderText;
        public bool IsRootNode;

        public Vector2D Position
        {
            get
            {
                if (SkillNodeGroup == null) return new Vector2D();
                double d = OrbitRadii[Orbit];
                return (SkillNodeGroup.Position - new Vector2D(d * Math.Sin(-Arc), d * Math.Cos(-Arc)));
            }
        }
        public double Arc => GetOrbitAngle(OrbitIndex, (int) SkillsPerOrbit[Orbit]);

        public string IconKey
        {
            get
            {
                string iconPrefix;
                switch (Type)
                {
                    case PassiveNodeType.JewelSocket:
                    case PassiveNodeType.Normal:
                        iconPrefix = "normal";
                        break;
                    case PassiveNodeType.Notable:
                        iconPrefix = "notable";
                        break;
                    case PassiveNodeType.Keystone:
                        iconPrefix = "keystone";
                        break;
                    case PassiveNodeType.Mastery:
                        iconPrefix = "mastery";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return iconPrefix + "_" + Icon;
            }
        }

        private static double GetOrbitAngle(int orbitIndex, int maxNodePositions)
            => 2 * Math.PI * orbitIndex / maxNodePositions;
    }
}
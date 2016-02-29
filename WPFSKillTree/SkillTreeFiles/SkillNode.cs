using System;
using System.Collections.Generic;

namespace POESKillTree.SkillTreeFiles
{
    public class SkillNode
    {
        public static float[] SkillsPerOrbit = {1, 6, 12, 12, 40};
        public static float[] OrbitRadii = {0, 81.5f, 163, 326, 489};
        public Dictionary<string, List<float>> Attributes;
        public HashSet<int> Connections = new HashSet<int>();
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
        public UInt16 Id; // "id": -28194677,
        public bool IsKeyStone; //"ks": false,
        public bool IsNotable; // not": false,
        public bool IsMastery; // m: false,
        public bool IsJewelSocket; //"isJewelSocket": false
        public List<int> LinkId = new List<int>(); // "out": []
        public string Name; //"dn": "Block Recovery",
        public int Orbit; //  "o": 1,
        public int OrbitIndex; // "oidx": 3,
        public int Sa; //s "sa": 0,
        public bool IsSkilled = false;
        public int? Spc;
        public bool IsMultipleChoice; //"isMultipleChoice": false
        public bool IsMultipleChoiceOption; //"isMultipleChoiceOption": false
        public int passivePointsGranted; //"passivePointsGranted": 1
        public string ascendancyName; //"ascendancyName": "Raider"
        public bool IsAscendancyStart; //"isAscendancyStart": false
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

        public double Arc
        {
            get { return GetOrbitAngle(OrbitIndex, (int) SkillsPerOrbit[Orbit]); }
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
        double GetOrbitAngle(int orbit_index, int max_node_positions )
        {
            // An orbit with 40 node placements has specific angles for certain orbit indices.
            /*if( max_node_positions == 40 )
            {
                switch( orbit_index )
                {
                case  0: return GetOrbitAngle(  0, 12 );
                case  1: return GetOrbitAngle(  0, 12 ) + 1 * 10.0f;
                case  2: return GetOrbitAngle(  0, 12 ) + 2 * 10.0f;
                case  3: return GetOrbitAngle(  1, 12 );
                case  4: return GetOrbitAngle(  1, 12 ) + 1 * 10.0f;
                case  5: return GetOrbitAngle(  1, 12 ) + 1 * 15.0f;
                case  6: return GetOrbitAngle(  1, 12 ) + 2 * 10.0f;
                case  7: return GetOrbitAngle(  2, 12 );
                case  8: return GetOrbitAngle(  2, 12 ) + 1 * 10.0f;
                case  9: return GetOrbitAngle(  2, 12 ) + 2 * 10.0f;
                case 10: return GetOrbitAngle(  3, 12 );
                case 11: return GetOrbitAngle(  3, 12 ) + 1 * 10.0f;
                case 12: return GetOrbitAngle(  3, 12 ) + 2 * 10.0f;
                case 13: return GetOrbitAngle(  4, 12 );
                case 14: return GetOrbitAngle(  4, 12 ) + 1 * 10.0f;
                case 15: return GetOrbitAngle(  4, 12 ) + 1 * 15.0f;
                case 16: return GetOrbitAngle(  4, 12 ) + 2 * 10.0f;
                case 17: return GetOrbitAngle(  5, 12 );
                case 18: return GetOrbitAngle(  5, 12 ) + 1 * 10.0f;
                case 19: return GetOrbitAngle(  5, 12 ) + 2 * 10.0f;
                case 20: return GetOrbitAngle(  6, 12 );
                case 21: return GetOrbitAngle(  6, 12 ) + 1 * 10.0f;
                case 22: return GetOrbitAngle(  6, 12 ) + 2 * 10.0f;
                case 23: return GetOrbitAngle(  7, 12 );
                case 24: return GetOrbitAngle(  7, 12 ) + 1 * 10.0f;
                case 25: return GetOrbitAngle(  7, 12 ) + 1 * 15.0f;
                case 26: return GetOrbitAngle(  7, 12 ) + 2 * 10.0f;
                case 27: return GetOrbitAngle(  8, 12 );
                case 28: return GetOrbitAngle(  8, 12 ) + 1 * 10.0f;
                case 29: return GetOrbitAngle(  8, 12 ) + 2 * 10.0f;
                case 30: return GetOrbitAngle(  9, 12 );
                case 31: return GetOrbitAngle(  9, 12 ) + 1 * 10.0f;
                case 32: return GetOrbitAngle(  9, 12 ) + 2 * 10.0f;
                case 33: return GetOrbitAngle( 10, 12 );
                case 34: return GetOrbitAngle( 10, 12 ) + 1 * 10.0f;
                case 35: return GetOrbitAngle( 10, 12 ) + 1 * 15.0f;
                case 36: return GetOrbitAngle( 10, 12 ) + 2 * 10.0f;
                case 37: return GetOrbitAngle( 11, 12 );
                case 38: return GetOrbitAngle( 11, 12 ) + 1 * 10.0f;
                case 39: return GetOrbitAngle( 11, 12 ) + 2 * 10.0f;
                }
            }*/

            return 2 * Math.PI * orbit_index / max_node_positions;
        }
    }
}
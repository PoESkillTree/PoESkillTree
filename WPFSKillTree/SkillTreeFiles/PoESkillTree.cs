using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using PoESkillTree.Engine.GameModel.PassiveTree;

namespace PoESkillTree.SkillTreeFiles
{
    public class BaseCharacterData
    {
        [JsonProperty("base_str")]
        public int BaseStrength { get; set; }

        [JsonProperty("base_dex")]
        public int BaseDexterity { get; set; }

        [JsonProperty("base_int")]
        public int BaseIntelligence { get; set; }
    }

    public class SkillNodeGroup
    {
        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }

        [JsonIgnore]
        private Vector2D? _position = null;
        [JsonIgnore]
        public Vector2D Position
        {
            get
            {
                if (!_position.HasValue)
                {
                    _position = new Vector2D(X, Y);
                }

                return _position.Value;
            }
            set => _position = value;
        }

        [JsonProperty("oo")]
        public Dictionary<int, bool> OccupiedOrbits { get; set; }

        [JsonProperty("n")]
        public List<ushort> NodeIds { get; set; }

        [JsonIgnore]
        public List<SkillNode> Nodes { get; set; } = new List<SkillNode>();
    }

    public class SkillNode
    {
        [JsonProperty("id")]
        public ushort Id { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("dn")]
        public string Name { get; set; }

        [JsonProperty("ascendancyName")]
        public string AscendancyName { get; set; }

        [JsonIgnore]
        public bool IsRootNode { get; set; } = false;

        [JsonIgnore]
        public bool IsAscendancyNode => !string.IsNullOrWhiteSpace(AscendancyName);

        [JsonIgnore]
        public bool IsSmall => !IsKeystone && !IsNotable && !IsMastery && !IsJewelSocket;

        [JsonProperty("not")]
        public bool IsNotable { get; set; }

        [JsonProperty("ks")]
        public bool IsKeystone { get; set; }

        [JsonProperty("m")]
        public bool IsMastery { get; set; }

        [JsonProperty("isJewelSocket")]
        public bool IsJewelSocket { get; set; }

        [JsonProperty("isMultipleChoice")]
        public bool IsMultipleChoice { get; set; }

        [JsonProperty("isMultipleChoiceOption")]
        public bool IsMultipleChoiceOption { get; set; }

        [JsonProperty("isAscendancyStart")]
        public bool IsAscendancyStart { get; set; }

        [JsonProperty("passivePointsGranted")]
        public int PassivePointsGranted { get; set; }

        [JsonProperty("sa")]
        public int StrengthGranted { get; set; }

        [JsonProperty("da")]
        public int DexterityGranted { get; set; }

        [JsonProperty("ia")]
        public int IntelligenceGranted { get; set; }

        [JsonProperty("spc")]
        public int[] Characters { get; set; }

        [JsonIgnore]
        public int? Character => Characters.Length > 0 ? Characters[0] : (int?)null;

        [JsonProperty("sd")]
        public string[] StatDefinitions { get; set; }

        [JsonIgnore]
        public Dictionary<string, IReadOnlyList<float>> Attributes { get; set; } = new Dictionary<string, IReadOnlyList<float>>();

        [JsonProperty("reminderText")]
        public string[] ReminderText { get; set; }

        [JsonProperty("g")]
        public int GroupId { get; set; }

        [JsonIgnore]
        public SkillNodeGroup Group { get; set; }

        [JsonProperty("o")]
        public int OrbitRadiiIndex { get; set; }

        [JsonProperty("oidx")]
        public int SkillsPreOrbitIndex { get; set; }

        [JsonProperty("out")]
        public List<ushort> NodeIdsOut { get; set; }

        [JsonProperty("in")]
        public List<ushort> NodesIdsIn { get; set; }

        [JsonIgnore]
        // The subset of neighbors to which connections should be drawn.
        public List<SkillNode> VisibleNeighbors { get; set; } = new List<SkillNode>();

        [JsonIgnore]
        public List<SkillNode> Neighbor { get; set; } = new List<SkillNode>();

        [JsonIgnore]
        public Vector2D Position => Group == null ? new Vector2D(0, 0) : Group.Position - new Vector2D(OrbitRadii[OrbitRadiiIndex] * Math.Sin(-Arc), OrbitRadii[OrbitRadiiIndex] * Math.Cos(-Arc));

        [JsonIgnore]
        public double Arc => 2 * Math.PI * SkillsPreOrbitIndex / SkillsPerOrbit[OrbitRadiiIndex];

        [JsonIgnore]
        private PassiveNodeType? _type = null;

        [JsonIgnore]
        public PassiveNodeType Type
        {
            get
            {
                if (!_type.HasValue)
                {
                    if (IsKeystone && !IsNotable && !IsJewelSocket && !IsMastery)
                    {
                        _type = PassiveNodeType.Keystone;
                    }
                    else if (!IsKeystone && IsNotable && !IsJewelSocket && !IsMastery)
                    {
                        Type = PassiveNodeType.Notable;
                    }
                    else if (!IsKeystone && !IsNotable && IsJewelSocket && !IsMastery)
                    {
                        Type = PassiveNodeType.JewelSocket;
                    }
                    else if (!IsKeystone && !IsNotable && !IsJewelSocket && IsMastery)
                    {
                        Type = PassiveNodeType.Mastery;
                    }
                    else if (!IsKeystone && !IsNotable && !IsJewelSocket && !IsMastery)
                    {
                        Type = PassiveNodeType.Small;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Invalid node type for node {Name}");
                    }
                }

                return _type.Value;
            }
            set => _type = value;
        }

        [JsonIgnore]
        public string IconKey
        {
            get
            {
                string iconPrefix;
                switch (Type)
                {
                    case PassiveNodeType.JewelSocket:
                    case PassiveNodeType.Small:
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

        [JsonIgnore]
        public static int[] SkillsPerOrbit = { 1, 6, 12, 12, 40 };

        [JsonIgnore]
        public static float[] OrbitRadii = { 0, 81.5f, 163, 326, 489 };
    }

    public class BaseConstants
    {
        [JsonProperty("classes")]
        public Dictionary<string, int> Classes { get; set; }

        [JsonProperty("characterAttributes")]
        public Dictionary<string, int> CharacterAttributes { get; set; }

        [JsonProperty("PSSCentreInnerRadius")]
        public int PSSCentreInnerRadius { get; set; }

        [JsonProperty("skillsPerOrbit")]
        public int[] SkillsPerOrbit { get; set; }

        [JsonProperty("orbitRadii")]
        public float[] OrbitRadii { get; set; }
    }

    public class Art2D
    {
        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }

        [JsonProperty("w")]
        public int Width { get; set; }

        [JsonProperty("h")]
        public int Height { get; set; }
    }

    public class SkillSprite
    {
        [JsonProperty("filename")]
        public string FileName { get; set; }

        [JsonProperty("coords")]
        public Dictionary<string, Art2D> Coords { get; set; }
    }

    public class OldSkillSprite
    {
        [JsonProperty("filename")]
        public string FileName { get; set; }

        [JsonProperty("coords")]
        public Dictionary<string, Art2D> Coords { get; set; }

        [JsonProperty("notableCoords")]
        public Dictionary<string, Art2D> NotableCoords { get; set; }

        [JsonProperty("keystoneCoords")]
        public Dictionary<string, Art2D> KeystoneCoords { get; set; }
    }

    public class ExtraImage
    {
        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }
    }

    public class PoESkillTree
    {
        [JsonProperty("characterData")]
        public Dictionary<int, BaseCharacterData> CharacterData { get; set; }

        [JsonProperty("groups")]
        public Dictionary<int, SkillNodeGroup> Groups { get; set; }

        [JsonProperty("root")]
        public SkillNode Root { get; set; }

        [JsonProperty("nodes")]
        public Dictionary<ushort, SkillNode> Nodes { get; set; }

        [JsonProperty("assets")]
        public Dictionary<string, Dictionary<string, string>> Assets { get; set; }

        [JsonProperty("constants")]
        public BaseConstants Constants { get; set; }

        [JsonIgnore]
        private string _imageRoot = null;

        [JsonProperty("imageRoot")]
        public string ImageRoot
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_imageRoot))
                {
                    _imageRoot = @"/image/";
                }

                return _imageRoot;
            }
            set => _imageRoot = value.Replace("//", "/");
        }

        [JsonProperty("skillSprites")]
        public Dictionary<string, List<SkillSprite>> SkillSprites { get; set; }

        [JsonProperty("extraImages")]
        public Dictionary<int, ExtraImage> ExtraImages { get; set; }

        [JsonProperty("min_x")]
        public int min_x { get; set; }

        [JsonProperty("min_y")]
        public int min_y { get; set; }

        [JsonProperty("max_x")]
        public int max_x { get; set; }

        [JsonProperty("max_y")]
        public int max_y { get; set; }

        [JsonProperty("imageZoomLevels")]
        public double[] ImageZoomLevels { get; set; }
    }

    public class PoESkillTreeOptions
    {
        [JsonProperty("ascClasses")]
        public Dictionary<int, CharacterToAscendancyOption> CharacterToAscendancy { get; set; }

        [JsonProperty("zoomLevels")]
        public double[] ZoomLevels { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("startClass")]
        public int StartClass { get; set; }

        [JsonProperty("fullScreen")]
        public bool FullScreen { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("realm")]
        public string Realm { get; set; }

        [JsonProperty("build")]
        public object Build { get; set; } //Model.Builds.GGGBuild?

        [JsonProperty("circles")]
        public Dictionary<string, List<CircleOption>> Circles { get; set; }

    }

    public class CircleOption
    {
        [JsonProperty("level")]
        public double ZoomLevel { get; set; }

        [JsonProperty("width")]
        public double Width { get; set; }
    }

    public class CharacterToAscendancyOption
    {
        [JsonProperty("name")]
        public string CharacterName { get; set; }

        [JsonProperty("classes")]
        public Dictionary<int, AscendancyClassOption> AscendancyClasses { get; set; }
    }

    public class AscendancyClassOption
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("flavourText")]
        public string FlavourText { get; set; }

        [JsonProperty("flavourTextRect")]
        public string FlavourTextRect { get; set; }

        [JsonProperty("flavourTextColour")]
        public string FlavourTextColour { get; set; }
    }
}
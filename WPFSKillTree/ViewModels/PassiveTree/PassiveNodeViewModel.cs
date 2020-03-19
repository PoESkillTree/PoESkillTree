using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.PassiveTree;
using PoESkillTree.Engine.GameModel.PassiveTree.Base;
using PoESkillTree.Engine.Utils.Extensions;
using PoESkillTree.SkillTreeFiles;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PoESkillTree.ViewModels.PassiveTree
{
    public class PassiveNodeViewModel
    {
        private readonly JsonPassiveNode JsonPassiveNode;
        public readonly PassiveNodeGroupViewModel? PassiveNodeGroup;

        public PassiveNodeViewModel() : this(new JsonPassiveNode()) { }
        public PassiveNodeViewModel(JsonPassiveNode jsonPassiveNode, PassiveNodeGroupViewModel? group = null)
        {
            JsonPassiveNode = jsonPassiveNode;
            PassiveNodeGroup = group;
            InitializeAttributes();
        }
        public PassiveNodeDefinition PassiveNodeDefinition => PassiveNodeDefinition.Convert(JsonPassiveNode);
        public CharacterClass? StartingCharacterClass { get => JsonPassiveNode.StartingCharacterClass; set => JsonPassiveNode.StartingCharacterClass = value; }
        public string[]? Recipe { get => JsonPassiveNode.Recipe; set => JsonPassiveNode.Recipe = value; }
        public JsonExpansionJewelSocket? ExpansionJewelSocket { get => JsonPassiveNode.ExpansionJewelSocket; set => JsonPassiveNode.ExpansionJewelSocket = value; }
        public HashSet<ushort> OutPassiveNodeIds => JsonPassiveNode.OutPassiveNodeIds;
        public HashSet<ushort> InPassiveNodeIds => JsonPassiveNode.InPassiveNodeIds;
        public bool IsSkilled { get => JsonPassiveNode.IsSkilled; set => JsonPassiveNode.IsSkilled = value; }
        public float[] SkillsPerOrbit { get => JsonPassiveNode.SkillsPerOrbit; set => JsonPassiveNode.SkillsPerOrbit = value; }
        public float[] OrbitRadii { get => JsonPassiveNode.OrbitRadii; set => JsonPassiveNode.OrbitRadii = value; }
        public int OrbitRadiiIndex { get => JsonPassiveNode.OrbitRadiiIndex; set => JsonPassiveNode.OrbitRadiiIndex = value; }
        public int SkillsPerOrbitIndex { get => JsonPassiveNode.SkillsPerOrbitIndex; set => JsonPassiveNode.SkillsPerOrbitIndex = value; }
        public bool IsSmall => JsonPassiveNode.IsSmall;
        public bool IsAscendancyNode => JsonPassiveNode.IsAscendancyNode;
        public bool IsRootNode => JsonPassiveNode.IsRootNode;
        public PassiveNodeType PassiveNodeType { get => JsonPassiveNode.PassiveNodeType; set => JsonPassiveNode.PassiveNodeType = value; }
        public string[] ReminderText { get => JsonPassiveNode.ReminderText; set => JsonPassiveNode.ReminderText = value; }
        public string[] StatDescriptions { get => JsonPassiveNode.StatDescriptions; set => JsonPassiveNode.StatDescriptions = value; }
        public int PassivePointsGranted { get => JsonPassiveNode.PassivePointsGranted; set => JsonPassiveNode.PassivePointsGranted = value; }
        public ushort Id { get => JsonPassiveNode.Id; set => JsonPassiveNode.Id = value; }
        public ushort Skill { get => JsonPassiveNode.Skill; set => JsonPassiveNode.Skill = value; }
        public string Icon { get => JsonPassiveNode.Icon; set => JsonPassiveNode.Icon = value; }
        public string Name { get => JsonPassiveNode.Name; set => JsonPassiveNode.Name = value; }
        public string? AscendancyName { get => JsonPassiveNode.AscendancyName; set => JsonPassiveNode.AscendancyName = value; }
        public bool IsNotable { get => JsonPassiveNode.IsNotable; set => JsonPassiveNode.IsNotable = value; }
        public bool IsKeystone { get => JsonPassiveNode.IsKeystone; set => JsonPassiveNode.IsKeystone = value; }
        public bool IsMastery { get => JsonPassiveNode.IsMastery; set => JsonPassiveNode.IsMastery = value; }
        public bool IsJewelSocket { get => JsonPassiveNode.IsJewelSocket; set => JsonPassiveNode.IsJewelSocket = value; }
        public bool IsBlighted { get => JsonPassiveNode.IsBlighted; set => JsonPassiveNode.IsBlighted = value; }
        public bool IsProxy { get => JsonPassiveNode.IsProxy; set => JsonPassiveNode.IsProxy = value; }
        public bool IsAscendancyStart { get => JsonPassiveNode.IsAscendancyStart; set => JsonPassiveNode.IsAscendancyStart = value; }
        public bool IsMultipleChoice { get => JsonPassiveNode.IsMultipleChoice; set => JsonPassiveNode.IsMultipleChoice = value; }
        public bool IsMultipleChoiceOption { get => JsonPassiveNode.IsMultipleChoiceOption; set => JsonPassiveNode.IsMultipleChoiceOption = value; }
        public int Strength { get => JsonPassiveNode.Strength; set => JsonPassiveNode.Strength = value; }
        public int Dexterity { get => JsonPassiveNode.Dexterity; set => JsonPassiveNode.Dexterity = value; }
        public int Intelligence { get => JsonPassiveNode.Intelligence; set => JsonPassiveNode.Intelligence = value; }
        public double Arc => JsonPassiveNode.Arc;

        public string IconKey => $"{IconKeyPrefix}_{Icon}";
        private string IconKeyPrefix => PassiveNodeType switch
        {
            PassiveNodeType.Keystone => $"keystone",
            PassiveNodeType.Notable => $"notable",
            PassiveNodeType.Mastery => $"mastery",
            _ => $"normal"
        };
        public Dictionary<string, IReadOnlyList<float>> Attributes { get; } = new Dictionary<string, IReadOnlyList<float>>();
        public Dictionary<ushort, PassiveNodeViewModel> NeighborPassiveNodes { get; } = new Dictionary<ushort, PassiveNodeViewModel>();
        public Dictionary<ushort, PassiveNodeViewModel> VisibleNeighborPassiveNodes { get; } = new Dictionary<ushort, PassiveNodeViewModel>();

        public float ZoomLevel { get => JsonPassiveNode.ZoomLevel; set { JsonPassiveNode.ZoomLevel = value; ClearPositionCache(); } }
        private Vector2D? _position = null;
        public Vector2D Position
        {
            get
            {
                if (_position?.X != JsonPassiveNode.Position.X || _position?.Y != JsonPassiveNode.Position.Y)
                {
                    _position = new Vector2D(JsonPassiveNode.Position.X, JsonPassiveNode.Position.Y);
                }
                return _position.Value;
            }
        }

        private bool? _isScionAscendancyNotable = null;
        public bool IsAscendantClassStartNode
        {
            get
            {
                if (!_isScionAscendancyNotable.HasValue)
                {
                    _isScionAscendancyNotable = false;
                    if (PassiveNodeType == PassiveNodeType.Notable)
                    {
                        /// <summary>
                        /// Nodes with an attribute matching this regex are one of the "Path of the ..." nodes connection Scion
                        /// Ascendant with other classes.
                        /// </summary>
                        var regexString = new Regex(@"Can Allocate Passives from the .* starting point");
                        foreach (var attibute in StatDescriptions)
                        {
                            if (regexString.IsMatch(attibute))
                            {
                                _isScionAscendancyNotable = true;
                                break;
                            }
                        }
                    }
                }

                return _isScionAscendancyNotable.Value;
            }
        }

        public void ClearPositionCache()
        {
            _position = null;
            JsonPassiveNode.ClearPositionCache();
        }

        private void InitializeAttributes()
        {
            if (PassiveNodeType == PassiveNodeType.JewelSocket || PassiveNodeType == PassiveNodeType.ExpansionJewelSocket)
            {
                StatDescriptions = new[] { "+1 Jewel Socket" };
            }

            var regexAttrib = new Regex("[0-9]*\\.?[0-9]+");
            foreach (string s in StatDescriptions)
            {
                var values = new List<float>();

                foreach (var m in regexAttrib.Matches(s).WhereNotNull())
                {
                    if (m.Value == "")
                        values.Add(float.NaN);
                    else
                        values.Add(float.Parse(m.Value, CultureInfo.InvariantCulture));
                }
                string cs = (regexAttrib.Replace(s, "#"));
                Attributes[cs] = values;
            }
        }
    }
}

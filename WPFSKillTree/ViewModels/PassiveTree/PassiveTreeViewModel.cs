using EnumsNET;
using Newtonsoft.Json;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.PassiveTree.Base;
using PoESkillTree.SkillTreeFiles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PoESkillTree.ViewModels.PassiveTree
{
    public class PassiveTreeViewModel
    {
        private readonly JsonPassiveTree JsonPassiveTree;

        public PassiveTreeViewModel(string treeJson, string? optionsJson = null)
            : this(JsonConvert.DeserializeObject<JsonPassiveTree>(treeJson), !string.IsNullOrWhiteSpace(optionsJson) ? JsonConvert.DeserializeObject<JsonPassiveTreeOptions>(optionsJson) : null) { }
        public PassiveTreeViewModel(JsonPassiveTree? jsonPassiveTree, JsonPassiveTreeOptions? options = null)
        {
            JsonPassiveTree = jsonPassiveTree ?? throw new ArgumentNullException(nameof(JsonPassiveTree));
            Root = new PassiveNodeViewModel(JsonPassiveTree.Root);

            InitializeJsonPassiveTreeOptions(options);
            InitializePassiveNodeGroups();
            InitializePassiveNodes();
            InitializePassiveNodeNeighbors();
            FixAscendancyPassiveNodeGroups();
        }

        private void InitializeJsonPassiveTreeOptions(JsonPassiveTreeOptions? options)
        {
            if (!(options is null))
            {
                foreach (var pair in options.CharacterToAscendancy)
                {
                    foreach (var other in CharacterClasses)
                    {
                        if (pair.Value.CharacterName == other.Name && !other.AscendancyClasses.Any())
                        {
                            other.AscendancyClasses.AddRange(pair.Value.AscendancyClasses.Values);
                            break;
                        }
                    }
                }
            }
        }

        public Uri ImageUri => JsonPassiveTree.ImageUri;
        public Uri WebCDN => JsonPassiveTree.WebCDN;
        public RectangleF Bounds => JsonPassiveTree.Bounds;
        public float MaxImageZoomLevel => JsonPassiveTree.MaxImageZoomLevel;
        public int MaxImageZoomLevelIndex => JsonPassiveTree.MaxImageZoomLevelIndex;
        public List<ushort> JewelSocketPassiveNodeIds => JsonPassiveTree.JewelSocketPassiveNodeIds;
        public Dictionary<CharacterClass, JsonPassiveTreeExtraImage> ExtraImages => JsonPassiveTree.ExtraImages;
        public Dictionary<string, List<JsonPassiveTreeSkillSprite>> SkillSprites => JsonPassiveTree.SkillSprites;
        public float[] ImageZoomLevels { get => JsonPassiveTree.ImageZoomLevels; }
        public JsonPassiveTreeConstants Constants => JsonPassiveTree.Constants;
        public Dictionary<string, Dictionary<string, string>> Assets => JsonPassiveTree.Assets;
        public float MaxY { get => JsonPassiveTree.MaxY; }
        public float MaxX { get => JsonPassiveTree.MaxX; }
        public float MinY { get => JsonPassiveTree.MinY; }
        public float MinX { get => JsonPassiveTree.MinX; }
        public List<JsonPassiveTreeCharacterClass> CharacterClasses => JsonPassiveTree.CharacterClasses;
        public Uri SpriteSheetUri => JsonPassiveTree.SpriteSheetUri;
        public string ImageRoot { get => JsonPassiveTree.ImageRoot; }
        public bool LargeGroupUsesHalfImage { get => JsonPassiveTree.UIArtOptions.LargeGroupUsesHalfImage; }

        public PassiveNodeViewModel Root { get; }
        public Dictionary<ushort, PassiveNodeViewModel> PassiveNodes { get; } = new Dictionary<ushort, PassiveNodeViewModel>();
        public Dictionary<ushort, PassiveNodeViewModel> AscendancyStartPassiveNodes { get; } = new Dictionary<ushort, PassiveNodeViewModel>();
        public Dictionary<ushort, PassiveNodeGroupViewModel> PassiveNodeGroups { get; } = new Dictionary<ushort, PassiveNodeGroupViewModel>();

        private void InitializePassiveNodeGroups()
        {
            foreach (var (id, group) in JsonPassiveTree.PassiveNodeGroups)
            {
                PassiveNodeGroups[id] = new PassiveNodeGroupViewModel(id, group);
            }
        }

        private void InitializePassiveNodes()
        {
            foreach (var (id, node) in JsonPassiveTree.PassiveNodes)
            {
                if (!(node.PassiveNodeGroupId is null) && PassiveNodeGroups.ContainsKey(node.PassiveNodeGroupId.Value))
                {
                    var group = PassiveNodeGroups[node.PassiveNodeGroupId.Value];
                    PassiveNodes[id] = new PassiveNodeViewModel(node, group);
                    group.PassiveNodes[id] = PassiveNodes[id];

                    if (PassiveNodes[id].IsAscendancyStart)
                    {
                        AscendancyStartPassiveNodes[id] = PassiveNodes[id];
                    }
                }
            }
        }

        private void InitializePassiveNodeNeighbors()
        {
            foreach (var (_, n1) in PassiveNodes)
            {
                foreach (var id in n1.OutPassiveNodeIds)
                {
                    if (!PassiveNodes.ContainsKey(id))
                    {
                        continue;
                    }

                    var n2 = PassiveNodes[id];
                    if (n2.IsAscendantClassStartNode && n1.IsRootNode)
                    {
                        n2.NeighborPassiveNodes[n1.Id] = n1;
                    }
                    else if (n1.IsAscendantClassStartNode && n2.IsRootNode)
                    {
                        n1.NeighborPassiveNodes[n2.Id] = n2;
                    }
                    else
                    {
                        n1.NeighborPassiveNodes[n2.Id] = n2;
                        n2.NeighborPassiveNodes[n1.Id] = n1;
                    }

                    if (n1.IsAscendancyNode == n2.IsAscendancyNode)
                    {
                        n1.VisibleNeighborPassiveNodes[n2.Id] = n2;
                        n2.VisibleNeighborPassiveNodes[n1.Id] = n1;
                    }
                }
            }
        }


        public void FixAscendancyPassiveNodeGroups()
        {
            var padding = 1.20f;
            var validNodes = PassiveNodes
                .Where(x => !(x.Value.IsAscendancyNode || x.Value.IsRootNode || x.Value.PassiveNodeGroup is null || x.Value.IsProxy || x.Value.PassiveNodeGroup.IsProxy))
                .Select(x => x.Value);
            var minX = validNodes.Min(x => x.Position.X) * padding;
            var maxX = validNodes.Max(x => x.Position.X) * padding;
            var minY = validNodes.Min(x => x.Position.Y) * padding;
            var maxY = validNodes.Max(x => x.Position.Y) * padding;

            foreach (var (_, node) in AscendancyStartPassiveNodes)
            {
                if (node.PassiveNodeGroup is null) continue;

                PassiveNodeViewModel? FindStartNode(IEnumerable<ushort> ids)
                {
                    foreach (var oid in ids)
                        if (PassiveNodes.ContainsKey(oid) && PassiveNodes[oid].StartingCharacterClass.HasValue)
                            return PassiveNodes[oid];
                    return null;
                }

                var start = FindStartNode(node.OutPassiveNodeIds) ?? FindStartNode(node.InPassiveNodeIds);
                if (start is null || !start.StartingCharacterClass.HasValue || start.PassiveNodeGroup is null) continue;

                var name = Enums.GetName(start.StartingCharacterClass.Value);
                var offset = CharacterClasses.First(c => c.Name == name).AscendancyClasses.Select((a, index) => a.Name == node.AscendancyName ? index - 1 : -2).Max();
                if (offset == -2) continue;

                var startGroup = PassiveNodeGroups[start.PassiveNodeGroup.Id];
                var centerThreshold = 100;
                var offsetDistance = 550;
                var basePosition = new Vector2D(0, 0);

                if (startGroup.X > -centerThreshold && startGroup.X < centerThreshold && startGroup.Y > -centerThreshold && startGroup.Y < centerThreshold)
                {
                    // Scion
                    basePosition = new Vector2D(minX + Math.Sign(minX) * offset * offsetDistance, maxY);
                }
                else if (startGroup.X > -centerThreshold && startGroup.X < centerThreshold)
                {
                    // Witch, Duelist
                    basePosition = new Vector2D(startGroup.X + Math.Sign(startGroup.X) * offset * offsetDistance, startGroup.Y + Math.Sign(startGroup.Y) > 0 ? maxY : minY);
                }
                else
                {
                    // Templar, Marauder, Ranger, Shadow 
                    basePosition = new Vector2D(startGroup.X < 0 ? minX : maxX, startGroup.Y + Math.Sign(startGroup.Y) * offset * offsetDistance);
                }

                RepositionAscendancyAt(node, basePosition);
            }
        }

        public void RepositionAscendancyAt(PassiveNodeViewModel node, Vector2D position)
        {
            if (node.PassiveNodeGroup is null) return;
            
            position = position / node.PassiveNodeGroup.ZoomLevel;

            var completed = new HashSet<ushort>() { node.PassiveNodeGroup.Id };
            foreach (var (_, other) in PassiveNodes)
            {
                if (other.AscendancyName != node.AscendancyName) continue;
                if (other.PassiveNodeGroup is null) continue;
                if (completed.Contains(other.PassiveNodeGroup.Id)) continue;

                var diff = node.PassiveNodeGroup.Position - other.PassiveNodeGroup.Position;
                other.PassiveNodeGroup.Position = position - (diff / other.PassiveNodeGroup.ZoomLevel);

                completed.Add(other.PassiveNodeGroup.Id);
            }

            node.PassiveNodeGroup.Position = position;
        }
    }
}

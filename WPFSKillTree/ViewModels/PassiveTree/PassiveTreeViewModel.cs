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

        public PassiveTreeViewModel(string json) : this(JsonConvert.DeserializeObject<JsonPassiveTree>(json)) { }

        public PassiveTreeViewModel(JsonPassiveTree? jsonPassiveTree)
        {
            JsonPassiveTree = jsonPassiveTree ?? throw new ArgumentNullException(nameof(JsonPassiveTree));
            Root = new PassiveNodeViewModel(JsonPassiveTree.Root);

            InitializePassiveNodeGroups();
            InitializePassiveNodes();
            InitializePassiveNodeNeighbors();
            FixAscendancyPassiveNodeGroups();
        }

        public Uri ImageUri => JsonPassiveTree.ImageUri;
        public Uri WebCDN => JsonPassiveTree.WebCDN;
        public RectangleF Bounds => JsonPassiveTree.Bounds;
        public float MaxImageZoomLevel => JsonPassiveTree.MaxImageZoomLevel;
        public int MaxImageZoomLevelIndex => JsonPassiveTree.MaxImageZoomLevelIndex;
        public List<ushort> JewelSocketPassiveNodeIds => JsonPassiveTree.JewelSocketPassiveNodeIds;
        public Dictionary<CharacterClass, JsonPassiveTreeExtraImage> ExtraImages => JsonPassiveTree.ExtraImages;
        public Dictionary<string, List<JsonPassiveTreeSkillSprite>> SkillSprites => JsonPassiveTree.SkillSprites;
        public float[] ImageZoomLevels { get => JsonPassiveTree.ImageZoomLevels; set => JsonPassiveTree.ImageZoomLevels = value; }
        public JsonPassiveTreeConstants Constants => JsonPassiveTree.Constants;
        public Dictionary<string, Dictionary<string, string>> Assets => JsonPassiveTree.Assets;
        public float MaxY { get => JsonPassiveTree.MaxY; set => JsonPassiveTree.MaxY = value; }
        public float MaxX { get => JsonPassiveTree.MaxX; set => JsonPassiveTree.MaxX = value; }
        public float MinY { get => JsonPassiveTree.MinY; set => JsonPassiveTree.MinY = value; }
        public float MinX { get => JsonPassiveTree.MinX; set => JsonPassiveTree.MinX = value; }
        public List<JsonPassiveTreeCharacterClass> CharacterClasses => JsonPassiveTree.CharacterClasses;
        public Uri SpriteSheetUri => JsonPassiveTree.SpriteSheetUri;
        public string ImageRoot { get => JsonPassiveTree.ImageRoot; set => JsonPassiveTree.ImageRoot = value; }

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

                var centerThreshold = 100;
                var offsetDistance = 1450;
                var basePosition = new Vector2D(0, 0);
                var startGroup = PassiveNodeGroups[start.PassiveNodeGroup.Id];

                if ((startGroup.X > -centerThreshold && startGroup.X < centerThreshold) && (startGroup.Y > -centerThreshold && startGroup.Y < centerThreshold))
                {
                    // Scion
                    basePosition = new Vector2D(MinX * .65f, MaxY * .95f);
                }
                else if (startGroup.X > -centerThreshold && startGroup.X < centerThreshold)
                {
                    // Witch, Duelist
                    basePosition = new Vector2D(startGroup.X / startGroup.ZoomLevel + (Math.Sign(startGroup.X) * offset * offsetDistance), (startGroup.Y / startGroup.ZoomLevel) + Math.Sign(startGroup.Y) > 0 ? MaxY + 1.05f : MinY);
                }
                else
                {
                    // Templar, Marauder, Ranger, Shadow 
                    basePosition = new Vector2D(startGroup.X < 0 ? MinX * .80f : MaxX, (startGroup.Y / startGroup.ZoomLevel) + (Math.Sign(startGroup.Y) * offset * offsetDistance));
                }

                RepositionAscendancyAt(node, basePosition);
            }
        }

        public void RepositionAscendancyAt(PassiveNodeViewModel node, Vector2D position)
        {
            if (node.PassiveNodeGroup is null) return;

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

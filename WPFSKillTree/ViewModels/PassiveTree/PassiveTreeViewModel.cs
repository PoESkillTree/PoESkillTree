using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.PassiveTree.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;

namespace PoESkillTree.ViewModels.PassiveTree
{
    public class PassiveTreeViewModel
    {
        private readonly JsonPassiveTree JsonPassiveTree;

        public PassiveTreeViewModel(JsonPassiveTree jsonPassiveTree)
        {
            JsonPassiveTree = jsonPassiveTree;
            Root = new PassiveNodeViewModel(JsonPassiveTree.Root);

            InitializePassiveNodeGroups();
            InitializePassiveNodes();
            InitializePassiveNodeNeighbors();
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

        public Dictionary<ushort, PassiveNodeViewModel> PassiveNodes { get; } = new Dictionary<ushort, PassiveNodeViewModel>();
        public PassiveNodeViewModel Root { get; }
        public Dictionary<ushort, PassiveNodeGroupViewModel> PassiveNodeGroups { get; } = new Dictionary<ushort, PassiveNodeGroupViewModel>();
                
        public void InitializePassiveNodeGroups()
        {
            foreach (var (id, group) in JsonPassiveTree.PassiveNodeGroups)
            {
                PassiveNodeGroups[id] = new PassiveNodeGroupViewModel(group);
            }
        }

        public void InitializePassiveNodes()
        {
            foreach (var (id, node) in JsonPassiveTree.PassiveNodes)
            {
                if (!(node.PassiveNodeGroupId is null) && PassiveNodeGroups.ContainsKey(node.PassiveNodeGroupId.Value))
                {
                    var group = PassiveNodeGroups[node.PassiveNodeGroupId.Value];
                    PassiveNodes[id] = new PassiveNodeViewModel(node, group);
                    group.PassiveNodes[id] = PassiveNodes[id];
                }
                else
                {
                    PassiveNodes[id] = new PassiveNodeViewModel(node);
                }
            }
        }

        public void InitializePassiveNodeNeighbors()
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
                    if (n2.IsScionAscendancyNotable && n1.IsRootNode)
                    {
                        n2.NeighborPassiveNodes[n1.Id] = n1;
                    }
                    else if (n1.IsScionAscendancyNotable && n2.IsRootNode)
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
    }
}

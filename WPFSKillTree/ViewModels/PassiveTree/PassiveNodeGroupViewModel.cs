using PoESkillTree.Engine.GameModel.PassiveTree.Base;
using PoESkillTree.SkillTreeFiles;
using System.Collections.Generic;

namespace PoESkillTree.ViewModels.PassiveTree
{
    public class PassiveNodeGroupViewModel
    {
        private readonly JsonPassiveNodeGroup JsonPassiveNodeGroup;

        public PassiveNodeGroupViewModel(JsonPassiveNodeGroup jsonPassiveNodeGroup)
        {
            JsonPassiveNodeGroup = jsonPassiveNodeGroup;
        }

        public List<ushort> OccupiedOrbits => JsonPassiveNodeGroup.OccupiedOrbits;
        public HashSet<ushort> PassiveNodeIds => JsonPassiveNodeGroup.PassiveNodeIds;
        public bool? IsProxy { get => JsonPassiveNodeGroup.IsProxy; set => JsonPassiveNodeGroup.IsProxy = value; }
        public Dictionary<ushort, PassiveNodeViewModel> PassiveNodes { get; } = new Dictionary<ushort, PassiveNodeViewModel>();

        public float X { get => JsonPassiveNodeGroup.OriginalX; set { JsonPassiveNodeGroup.OriginalX = value; ClearPositionCache(); } }
        public float Y { get => JsonPassiveNodeGroup.OriginalY; set { JsonPassiveNodeGroup.OriginalY = value; ClearPositionCache(); } }
        public float ZoomLevel { get => JsonPassiveNodeGroup.ZoomLevel; set { JsonPassiveNodeGroup.ZoomLevel = value; ClearPositionCache(); } }
        private Vector2D? _position = null;
        public Vector2D Position
        {
            get
            {
                if (_position?.X != JsonPassiveNodeGroup.Position.X || _position?.Y != JsonPassiveNodeGroup.Position.Y)
                {
                    _position = new Vector2D(JsonPassiveNodeGroup.Position.X, JsonPassiveNodeGroup.Position.Y);
                }

                return _position.Value;
            }
            set
            {
                X = (float)value.X;
                Y = (float)value.Y;
                ClearPositionCache();
            }
        }
        
        public void ClearPositionCache()
        {
            _position = null;
            JsonPassiveNodeGroup.ClearPositionCache();
            foreach (var (_, node) in PassiveNodes)
            {
                node.ClearPositionCache();
            }
        }
    }
}

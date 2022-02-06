using PoESkillTree.Engine.GameModel.PassiveTree.Base;
using PoESkillTree.SkillTreeFiles;
using System.Collections.Generic;

namespace PoESkillTree.ViewModels.PassiveTree
{
    public class PassiveNodeGroupViewModel
    {
        private readonly JsonPassiveNodeGroup JsonPassiveNodeGroup;

        public PassiveNodeGroupViewModel(ushort id, JsonPassiveNodeGroup jsonPassiveNodeGroup)
        {
            Id = id;
            JsonPassiveNodeGroup = jsonPassiveNodeGroup;
        }

        public ushort Id { get; private set; } = 0;
        public List<ushort> OccupiedOrbits => JsonPassiveNodeGroup.OccupiedOrbits;
        public ushort BackgroundOverride => JsonPassiveNodeGroup.BackgroundOverride;
        public HashSet<ushort> PassiveNodeIds => JsonPassiveNodeGroup.PassiveNodeIds;
        public bool IsProxy { get => JsonPassiveNodeGroup.IsProxy.HasValue && JsonPassiveNodeGroup.IsProxy.Value; }
        public Dictionary<ushort, PassiveNodeViewModel> PassiveNodes { get; } = new Dictionary<ushort, PassiveNodeViewModel>();

        public float X { get => JsonPassiveNodeGroup.OriginalX; private set { JsonPassiveNodeGroup.OriginalX = value; } }
        public float Y { get => JsonPassiveNodeGroup.OriginalY; private set { JsonPassiveNodeGroup.OriginalY = value; } }
        public float ZoomLevel { get => JsonPassiveNodeGroup.ZoomLevel; }
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

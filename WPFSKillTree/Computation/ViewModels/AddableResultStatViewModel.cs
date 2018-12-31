using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel;
using POESKillTree.Utils;

namespace POESKillTree.Computation.ViewModels
{
    public class AddableResultStatViewModel : Notifier
    {
        private Entity _entity = Entity.Character;
        private string _identity;
        private NodeType _nodeType = NodeType.Total;

        public Entity Entity
        {
            get => _entity;
            set => SetProperty(ref _entity, value);
        }

        public string Identity
        {
            get => _identity;
            set => SetProperty(ref _identity, value);
        }

        public NodeType NodeType
        {
            get => _nodeType;
            set => SetProperty(ref _nodeType, value);
        }
    }
}
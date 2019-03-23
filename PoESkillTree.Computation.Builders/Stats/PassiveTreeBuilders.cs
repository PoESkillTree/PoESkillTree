using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel.PassiveTree;

namespace PoESkillTree.Computation.Builders.Stats
{
    public class PassiveTreeBuilders : StatBuildersBase, IPassiveTreeBuilders
    {
        private readonly PassiveTreeDefinition _tree;

        public PassiveTreeBuilders(IStatFactory statFactory, PassiveTreeDefinition tree) : base(statFactory)
        {
            _tree = tree;
        }

        public IStatBuilder NodeSkilled(ushort nodeId) => FromIdentity($"{nodeId}.Skilled", typeof(bool));
    }
}
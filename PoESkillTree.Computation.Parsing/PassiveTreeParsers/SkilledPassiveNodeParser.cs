using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.GameModel.PassiveTree;

namespace PoESkillTree.Computation.Parsing.PassiveTreeParsers
{
    public class SkilledPassiveNodeParser : IParser<ushort>
    {
        private readonly PassiveTreeDefinition _passiveTreeDefinition;
        private readonly IBuilderFactories _builderFactories;

        public SkilledPassiveNodeParser(
            PassiveTreeDefinition passiveTreeDefinition, IBuilderFactories builderFactories)
            => (_passiveTreeDefinition, _builderFactories) = (passiveTreeDefinition, builderFactories);

        public ParseResult Parse(ushort nodeId)
        {
            var nodeDefinition = _passiveTreeDefinition.GetNodeById(nodeId);
            var localSource = new ModifierSource.Local.Tree(nodeDefinition.Name);
            var modifiers = new ModifierCollection(_builderFactories, localSource);
            modifiers.AddGlobal(_builderFactories.StatBuilders.PassiveNodeSkilled(nodeId), Form.TotalOverride, 1);
            return ParseResult.Success(modifiers.ToList());
        }
    }
}
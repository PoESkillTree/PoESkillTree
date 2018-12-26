using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.PassiveTree;

namespace PoESkillTree.Computation.Parsing.PassiveTreeParsers
{
    /// <summary>
    /// Parser for passive tree nodes. Adding parsed modifiers to a calculator does nothing on its own.
    /// Use <see cref="SkilledPassiveNodeParser"/> to activate nodes that are skilled. Nodes, keystones in particular,
    /// can also be activated from items and skills. Because of that, the whole passive tree has to be parsed and
    /// added to the calculator initially.
    /// </summary>
    public class PassiveNodeParser : IParser<ushort>
    {
        private readonly PassiveTreeDefinition _passiveTreeDefinition;
        private readonly IBuilderFactories _builderFactories;
        private readonly ICoreParser _coreParser;

        public PassiveNodeParser(
            PassiveTreeDefinition passiveTreeDefinition, IBuilderFactories builderFactories, ICoreParser coreParser)
            => (_passiveTreeDefinition, _builderFactories, _coreParser) =
                (passiveTreeDefinition, builderFactories, coreParser);

        public ParseResult Parse(ushort nodeId)
        {
            var nodeDefinition = _passiveTreeDefinition.GetNodeById(nodeId);
            var localSource = new ModifierSource.Local.Tree(nodeDefinition.Name);
            var globalSource = new ModifierSource.Global(localSource);
            var skilledCondition = _builderFactories.StatBuilders.PassiveNodeSkilled(nodeId).IsSet;

            var results = nodeDefinition.Modifiers
                .Select(s => Parse(s, globalSource))
                .Select(r => r.ApplyCondition(skilledCondition.Build))
                .ToList();
            return ParseResult.Aggregate(results);
        }

        private ParseResult Parse(string modifierLine, ModifierSource modifierSource)
            => _coreParser.Parse(modifierLine, modifierSource, Entity.Character);
    }
}
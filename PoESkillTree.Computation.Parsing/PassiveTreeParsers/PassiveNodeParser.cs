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
            var isSkilledStat = _builderFactories.StatBuilders.PassiveNodeSkilled(nodeId);
            var isSkilled = isSkilledStat.IsSet;

            var results = nodeDefinition.Modifiers
                .Select(s => Parse(s, globalSource))
                .Select(r => r.ApplyCondition(isSkilled.Build))
                .ToList();
            
            var modifiers = new ModifierCollection(_builderFactories, localSource);
            modifiers.AddGlobal(isSkilledStat, Form.BaseSet, false);
            if (nodeDefinition.CostsPassivePoint)
            {
                var passivePointStat = nodeDefinition.IsAscendancyNode
                    ? _builderFactories.StatBuilders.AscendancyPassivePoints
                    : _builderFactories.StatBuilders.PassivePoints;
                modifiers.AddGlobal(passivePointStat, Form.BaseAdd, 1, isSkilled);
            }
            if (nodeDefinition.PassivePointsGranted > 0)
            {
                modifiers.AddGlobal(_builderFactories.StatBuilders.PassivePoints.Maximum,
                    Form.BaseAdd, nodeDefinition.PassivePointsGranted, isSkilled);
            }
            results.Add(ParseResult.Success(modifiers.ToList()));

            return ParseResult.Aggregate(results);
        }

        private ParseResult Parse(string modifierLine, ModifierSource modifierSource)
            => _coreParser.Parse(modifierLine, modifierSource, Entity.Character);
    }
}
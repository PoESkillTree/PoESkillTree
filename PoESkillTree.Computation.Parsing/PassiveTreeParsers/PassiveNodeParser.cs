using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Modifiers;
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
            var localSource = new ModifierSource.Local.PassiveNode(nodeId, nodeDefinition.Name);
            var globalSource = new ModifierSource.Global(localSource);
            var isSkilledStat = _builderFactories.PassiveTreeBuilders.NodeSkilled(nodeId);
            var isSkilled = isSkilledStat.IsSet;

            var results = new List<ParseResult>(nodeDefinition.Modifiers.Count + 1);
            foreach (var modifier in nodeDefinition.Modifiers)
            {
                var result = ModifierLocalityTester.AffectsPassiveNodeProperty(modifier)
                    ? Parse(modifier + " (AsPassiveNodeProperty)", localSource)
                    : Parse(modifier, globalSource).ApplyCondition(isSkilled.Build);
                results.Add(result);
            }
            
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

            var attributes = _builderFactories.StatBuilders.Attribute;
            SetupProperty(modifiers, attributes.Strength, isSkilled);
            SetupProperty(modifiers, attributes.Dexterity, isSkilled);
            SetupProperty(modifiers, attributes.Intelligence, isSkilled);

            results.Add(ParseResult.Success(modifiers.Modifiers));

            return ParseResult.Aggregate(results);
        }

        private static void SetupProperty(ModifierCollection modifiers, IStatBuilder stat, IConditionBuilder condition)
            => modifiers.AddGlobal(stat, Form.BaseSet, stat.AsPassiveNodeProperty.Value, condition);

        private ParseResult Parse(string modifierLine, ModifierSource modifierSource)
            => _coreParser.Parse(modifierLine, modifierSource, Entity.Character);
    }
}
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.PassiveTree;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Parsing.JewelParsers
{
    public class JewelInSkillTreeParser : IParser<JewelInSkillTreeParserParameter>
    {
        private readonly PassiveTreeDefinition _tree;
        private readonly TransformationJewelParser _transformationParser;
        private readonly ICoreParser _coreParser;

        public JewelInSkillTreeParser(
            PassiveTreeDefinition tree, IBuilderFactories builderFactories, ICoreParser coreParser)
        {
            _tree = tree;
            _transformationParser = new TransformationJewelParser(
                i => builderFactories.PassiveTreeBuilders.NodeSkilled(i).IsSet);
            _coreParser = coreParser;
        }

        public ParseResult Parse(JewelInSkillTreeParserParameter parameter)
        {
            var (item, radius, nodeId) = parameter;
            if (!item.IsEnabled)
                return ParseResult.Empty;

            var localSource = new ModifierSource.Local.Jewel(radius, nodeId, item.Name);
            var globalSource = new ModifierSource.Global(localSource);
            var nodesInRadius = _tree.GetNodesInRadius(nodeId, radius).ToList();

            var results = new List<ParseResult>(item.Modifiers.Count);
            foreach (var modifier in item.Modifiers)
            {
                results.Add(ParseModifier(modifier, globalSource, nodesInRadius));
            }

            return ParseResult.Aggregate(results);
        }

        private ParseResult ParseModifier(
            string modifier, ModifierSource modifierSource, IEnumerable<PassiveNodeDefinition> nodesInRadius)
            => _transformationParser.IsTransformationJewelModifier(modifier)
                ? ParseTransformationModifier(modifier, modifierSource, nodesInRadius)
                : _coreParser.Parse(modifier, modifierSource, Entity.Character);

        private ParseResult ParseTransformationModifier(string modifier, ModifierSource modifierSource,
            IEnumerable<PassiveNodeDefinition> nodesInRadius)
        {
            var transformedNodeModifiers = _transformationParser.ApplyTransformation(modifier, nodesInRadius).ToList();
            var results = new List<ParseResult>(transformedNodeModifiers.Count);
            foreach (var transformedModifier in transformedNodeModifiers)
            {
                var parseResult = _coreParser.Parse(transformedModifier.Modifier, modifierSource, Entity.Character)
                    .ApplyCondition(transformedModifier.Condition.Build)
                    .ApplyMultiplier(_ => transformedModifier.ValueMultiplier);
                results.Add(parseResult);
            }
            return ParseResult.Aggregate(results);
        }
    }

    public class JewelInSkillTreeParserParameter : ValueObject
    {
        public JewelInSkillTreeParserParameter(Item item, JewelRadius jewelRadius, ushort passiveNodeId)
        {
            Item = item;
            JewelRadius = jewelRadius;
            PassiveNodeId = passiveNodeId;
        }

        public Item Item { get; }
        public JewelRadius JewelRadius { get; }
        public ushort PassiveNodeId { get; }

        public void Deconstruct(out Item item, out JewelRadius jewelRadius, out ushort passiveNodeId)
            => (item, jewelRadius, passiveNodeId) = (Item, JewelRadius, PassiveNodeId);

        protected override object ToTuple()
            => (Item, JewelRadius, PassiveNodeId);
    }
}
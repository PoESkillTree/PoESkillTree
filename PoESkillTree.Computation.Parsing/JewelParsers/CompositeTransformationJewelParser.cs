using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.GameModel.PassiveTree;

namespace PoESkillTree.Computation.Parsing.JewelParsers
{
    public class CompositeTransformationJewelParser : ITransformationJewelParser
    {
        private readonly IReadOnlyList<ITransformationJewelParser> _components;

        public CompositeTransformationJewelParser(params ITransformationJewelParser[] components)
        {
            _components = components;
        }

        public static ITransformationJewelParser Create(Func<ushort, IValueBuilder> createEffectivenessForNode)
        {
            var components = TransformationJewelParserData.CreateAll()
                .Select(d => new TransformationJewelParser(createEffectivenessForNode, d))
                .Cast<ITransformationJewelParser>()
                .ToArray();
            return new CompositeTransformationJewelParser(components);
        }

        public bool IsTransformationJewelModifier(string jewelModifier)
            => _components.Any(c => c.IsTransformationJewelModifier(jewelModifier));

        public IEnumerable<TransformedNodeModifier> ApplyTransformation(
            string jewelModifier, IEnumerable<PassiveNodeDefinition> nodesInRadius)
            => _components.FirstOrDefault(c => c.IsTransformationJewelModifier(jewelModifier))
                   ?.ApplyTransformation(jewelModifier, nodesInRadius)
               ?? new TransformedNodeModifier[0];
    }
}
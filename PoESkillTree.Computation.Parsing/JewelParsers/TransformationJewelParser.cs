using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.GameModel.PassiveTree;

namespace PoESkillTree.Computation.Parsing.JewelParsers
{
    public class TransformationJewelParser : ITransformationJewelParser
    {
        private readonly Func<ushort, IValueBuilder> _createEffectivenessForNode;
        private readonly TransformationJewelParserData _jewelData;

        public TransformationJewelParser(
            Func<ushort, IValueBuilder> createEffectivenessForNode, TransformationJewelParserData jewelData)
        {
            _createEffectivenessForNode = createEffectivenessForNode;
            _jewelData = jewelData;
        }

        public bool IsTransformationJewelModifier(string jewelModifier)
            => _jewelData.JewelModifierRegex.IsMatch(StringNormalizer.MergeWhiteSpace(jewelModifier));

        public IEnumerable<TransformedNodeModifier> ApplyTransformation(
            string jewelModifier, IEnumerable<PassiveNodeDefinition> nodesInRadius)
        {
            jewelModifier = StringNormalizer.MergeWhiteSpace(jewelModifier);
            var jewelMatch = _jewelData.JewelModifierRegex.Match(jewelModifier);
            if (!jewelMatch.Success)
                yield break;
            
            var valueMultiplier = _jewelData.GetValueMultiplier(jewelMatch);
            var nodeRegexes = _jewelData.GetNodeModifierRegexes(jewelMatch).ToList();
            foreach (var node in nodesInRadius)
            {
                var effectiveness = _createEffectivenessForNode(node.Id);
                var transformedModifiers = node.Modifiers
                    .Select(StringNormalizer.MergeWhiteSpace)
                    .SelectMany(m => TransformNodeModifier(m, nodeRegexes, valueMultiplier, effectiveness));
                foreach (var transformedNodeModifier in transformedModifiers)
                {
                    yield return transformedNodeModifier;
                }
            }
        }

        private IEnumerable<TransformedNodeModifier> TransformNodeModifier(
            string nodeModifier,
            IEnumerable<(Regex regex, string replacement)> nodeRegexes,
            double valueMultiplier,
            IValueBuilder effectiveness)
        {
            var (nodeRegex, replacement) = nodeRegexes.FirstOrDefault(t => t.regex.IsMatch(nodeModifier));
            if (nodeRegex is null)
                yield break;
            
            if (_jewelData.CancelOutOriginalModifier)
                yield return new TransformedNodeModifier(nodeModifier,
                    effectiveness.Multiply(effectiveness.Create(-1)));

            var newModifier = nodeRegex.Replace(nodeModifier, replacement);
            yield return new TransformedNodeModifier(newModifier,
                effectiveness.Multiply(effectiveness.Create(valueMultiplier)));
        }
    }
}
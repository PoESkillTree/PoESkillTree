using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.GameModel.PassiveTree;

namespace PoESkillTree.Computation.Parsing.JewelParsers
{
    public class TransformationJewelParser : ITransformationJewelParser
    {
        private readonly Func<ushort, IConditionBuilder> _createIsSkilledConditionForNode;
        private readonly TransformationJewelParserData _jewelData;

        public TransformationJewelParser(
            Func<ushort, IConditionBuilder> createIsSkilledConditionForNode, TransformationJewelParserData jewelData)
        {
            _createIsSkilledConditionForNode = createIsSkilledConditionForNode;
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
                var condition = _createIsSkilledConditionForNode(node.Id);
                var transformedModifiers = node.Modifiers
                    .Select(StringNormalizer.MergeWhiteSpace)
                    .SelectMany(m => TransformNodeModifier(m, nodeRegexes, valueMultiplier, condition));
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
            IConditionBuilder condition)
        {
            var (nodeRegex, replacement) = nodeRegexes.FirstOrDefault(t => t.regex.IsMatch(nodeModifier));
            if (nodeRegex is null)
                yield break;
            
            if (_jewelData.CancelOutOriginalModifier)
                yield return new TransformedNodeModifier(nodeModifier, condition, new Constant(-1));

            var newModifier = nodeRegex.Replace(nodeModifier, replacement);
            yield return new TransformedNodeModifier(newModifier, condition, new Constant(valueMultiplier));
        }
    }
}
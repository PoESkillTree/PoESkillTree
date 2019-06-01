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
        private static readonly Regex JewelRegex = new Regex(
            @"increases and reductions to (?<source>\w+) damage in radius are transformed to apply to (?<target>\w+) damage",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        private readonly Func<ushort, IConditionBuilder> _createIsSkilledConditionForNode;

        public TransformationJewelParser(Func<ushort, IConditionBuilder> createIsSkilledConditionForNode)
        {
            _createIsSkilledConditionForNode = createIsSkilledConditionForNode;
        }

        public bool IsTransformationJewelModifier(string jewelModifier)
            => JewelRegex.IsMatch(StringNormalizer.MergeWhiteSpace(jewelModifier));

        public IEnumerable<TransformedNodeModifier> ApplyTransformation(
            string jewelModifier, IEnumerable<PassiveNodeDefinition> nodesInRadius)
        {
            jewelModifier = StringNormalizer.MergeWhiteSpace(jewelModifier);
            var jewelMatch = JewelRegex.Match(jewelModifier);
            if (!jewelMatch.Success)
                yield break;

            var nodeRegex = new Regex(
                $@"((increased|reduced).*) {jewelMatch.Groups["source"]} (.*damage)",
                RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            foreach (var node in nodesInRadius)
            {
                var condition = _createIsSkilledConditionForNode(node.Id);
                foreach (var nodeModifier in node.Modifiers.Select(StringNormalizer.MergeWhiteSpace))
                {
                    var nodeMatch = nodeRegex.Match(nodeModifier);
                    if (nodeMatch.Success)
                    {
                        yield return new TransformedNodeModifier(nodeModifier, condition, new Constant(-1));
                        var newModifier =
                            nodeRegex.Replace(nodeModifier, $"$1 {jewelMatch.Groups["target"]} $3");
                        yield return new TransformedNodeModifier(newModifier, condition, new Constant(1));
                    }
                }
            }
        }
    }
}
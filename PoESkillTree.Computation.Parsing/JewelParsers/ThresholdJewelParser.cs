using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MoreLinq;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.PassiveTree;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Parsing.JewelParsers
{
    public class ThresholdJewelParser
    {
        private const string AttributeGroup = "(strength|dexterity|intelligence)";

        private static readonly Regex SingleThresholdRegex =
            new Regex($@"^with(?: at least)? (\d+) {AttributeGroup} in radius, (.*)$",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private static readonly Regex DoubleThresholdRegex =
            new Regex($@"^with (\d+) total {AttributeGroup} and {AttributeGroup} in radius, (.*)$",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private static readonly Regex SingleAttributeRegex =
            new Regex($@"^\+(\d+) to {AttributeGroup}$", RegexOptions.IgnoreCase);

        private static readonly Regex DoubleAttributeRegex =
            new Regex($@"^\+(\d+) to {AttributeGroup} and {AttributeGroup}$", RegexOptions.IgnoreCase);

        private readonly PassiveTreeDefinition _treeDefinition;

        public ThresholdJewelParser(PassiveTreeDefinition treeDefinition)
            => _treeDefinition = treeDefinition;

        public bool ReplaceThresholdIfMet(
            JewelInSkillTreeParserParameter parameter, string modifier, out string modifierWithoutThreshold)
        {
            if (!TryMatch(modifier, out var captures, SingleThresholdRegex, DoubleThresholdRegex))
            {
                modifierWithoutThreshold = modifier;
                return true;
            }
            modifierWithoutThreshold = captures.Last();

            var attributes = CountAttributesInRange(parameter);
            var actual = captures.Skip(2).SkipLast(1)
                .Select(a => a.ToLowerInvariant())
                .Select(attributes.GetValueOrDefault)
                .Sum();
            var requirement = int.Parse(captures[1]);
            return actual >= requirement;
        }

        private Dictionary<string, int> CountAttributesInRange(JewelInSkillTreeParserParameter parameter)
        {
            var attributes = new Dictionary<string, int>();
            foreach (var modifier in GetNodesInRadius(parameter).SelectMany(d => d.Modifiers))
            {
                if (!TryMatch(modifier, out var captures, SingleAttributeRegex, DoubleAttributeRegex))
                    continue;

                var value = int.Parse(captures[1]);
                var capturedAttributes = captures.Skip(2).Select(a => a.ToLowerInvariant());
                foreach (var attribute in capturedAttributes)
                {
                    attributes[attribute] = attributes.GetValueOrDefault(attribute) + value;
                }
            }
            return attributes;
        }

        private IEnumerable<PassiveNodeDefinition> GetNodesInRadius(JewelInSkillTreeParserParameter parameter)
            => _treeDefinition.GetNodesInRadius(parameter.PassiveNodeId, parameter.JewelRadius.GetRadius());

        private static bool TryMatch(string input, out IReadOnlyList<string> captures, params Regex[] regexes)
        {
            foreach (var regex in regexes)
            {
                if (TryMatch(regex, input, out captures))
                    return true;
            }
            captures = null;
            return false;
        }

        private static bool TryMatch(Regex regex, string input, out IReadOnlyList<string> captures)
        {
            var match = regex.Match(input);
            captures = match.Groups.Cast<Capture>().Select(c => c.Value).ToList();
            return match.Success;
        }
    }
}
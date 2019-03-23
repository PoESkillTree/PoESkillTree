using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.PassiveTree;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Parsing.JewelParsers
{
    public class JewelInSkillTreeParser : IParser<JewelInSkillTreeParserParameter>
    {
        private readonly ICoreParser _coreParser;
        private readonly ThresholdJewelParser _thresholdParser;

        public JewelInSkillTreeParser(PassiveTreeDefinition treeDefinition, ICoreParser coreParser)
        {
            _coreParser = coreParser;
            _thresholdParser = new ThresholdJewelParser(treeDefinition);
        }

        public ParseResult Parse(JewelInSkillTreeParserParameter parameter)
        {
            var (item, radius, nodeId) = parameter;
            if (!item.IsEnabled)
                return ParseResult.Empty;

            var localSource = new ModifierSource.Local.Jewel(radius, nodeId, item.Name);
            var globalSource = new ModifierSource.Global(localSource);

            var results = new List<ParseResult>(item.Modifiers.Count);
            foreach (var modifier in item.Modifiers)
            {
                if (_thresholdParser.ReplaceThresholdIfMet(parameter, modifier, out var replacedModifier))
                {
                    results.Add(_coreParser.Parse(replacedModifier, globalSource, Entity.Character));
                }
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
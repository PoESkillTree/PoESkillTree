using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Parsing.ItemParsers;
using PoESkillTree.GameModel;

namespace PoESkillTree.Computation.Parsing.JewelParsing
{
    public class JewelInItemParser : IParser<ItemParserParameter>
    {
        private readonly ICoreParser _coreParser;

        public JewelInItemParser(ICoreParser coreParser)
            => _coreParser = coreParser;

        public ParseResult Parse(ItemParserParameter parameter)
        {
            var (item, slot) = parameter;
            if (!item.IsEnabled)
                return ParseResult.Empty;

            var localSource = new ModifierSource.Local.Item(slot, item.Name);
            var globalSource = new ModifierSource.Global(localSource);

            var results = new List<ParseResult>(item.Modifiers.Count);
            foreach (var modifier in item.Modifiers)
            {
                results.Add(_coreParser.Parse(modifier, globalSource, Entity.Character));
            }
            return ParseResult.Aggregate(results);
        }
    }
}
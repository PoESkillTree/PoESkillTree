using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Parsing.ItemParsers;
using PoESkillTree.Computation.Parsing.JewelParsing;
using PoESkillTree.Computation.Parsing.PassiveTreeParsers;
using PoESkillTree.Computation.Parsing.SkillParsers;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.PassiveTree;
using PoESkillTree.GameModel.Skills;
using PoESkillTree.GameModel.StatTranslation;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Parsing
{
    public class Parser<TStep> : IParser
    {
        private readonly ICoreParser _coreParser;
        private readonly IParser<ushort> _passiveNodeParser;
        private readonly IParser<ushort> _skilledPassiveNodeParser;
        private readonly IParser<ItemParserParameter> _itemParser;
        private readonly IParser<ItemParserParameter> _itemJewelParser;
        private readonly IParser<JewelInSkillTreeParserParameter> _treeJewelParser;
        private readonly IParser<IReadOnlyList<Skill>> _skillsParser;
        private readonly IParser<Skill> _activeSkillParser;
        private readonly IParser<SupportSkillParserParameter> _supportSkillParser;

        private readonly StatTranslators _statTranslators;
        private readonly IEnumerable<IGivenStats> _givenStats;

        private readonly ConcurrentDictionary<IReadOnlyList<string>, IParser<UntranslatedStatParserParameter>>
            _untranslatedStatParsers =
                new ConcurrentDictionary<IReadOnlyList<string>, IParser<UntranslatedStatParserParameter>>();

        public static async Task<IParser> CreateAsync(
            GameData gameData, Task<IBuilderFactories> builderFactoriesTask, Task<IParsingData<TStep>> parsingDataTask)
        {
            var passiveTreeTask = gameData.PassiveTree;
            var baseItemsTask = gameData.BaseItems;
            var skillsTask = gameData.Skills;
            var statTranslatorsTask = gameData.StatTranslators;
            return new Parser<TStep>(
                await passiveTreeTask.ConfigureAwait(false),
                await baseItemsTask.ConfigureAwait(false),
                await skillsTask.ConfigureAwait(false),
                await statTranslatorsTask.ConfigureAwait(false),
                await builderFactoriesTask.ConfigureAwait(false),
                await parsingDataTask.ConfigureAwait(false));
        }

        private Parser(
            PassiveTreeDefinition passiveTree, BaseItemDefinitions baseItems, SkillDefinitions skills,
            StatTranslators statTranslators, IBuilderFactories builderFactories, IParsingData<TStep> parsingData)
        {
            _statTranslators = statTranslators;
            _coreParser = new CoreParser<TStep>(parsingData, builderFactories);
            _givenStats = parsingData.GivenStats;

            _passiveNodeParser = Caching(new PassiveNodeParser(passiveTree, builderFactories, _coreParser));
            _skilledPassiveNodeParser = Caching(new SkilledPassiveNodeParser(passiveTree, builderFactories));
            _itemParser = Caching(new ItemParser(baseItems, builderFactories, _coreParser,
                statTranslators[StatTranslationFileNames.Main]));
            _itemJewelParser = Caching(new JewelInItemParser(_coreParser));
            _treeJewelParser = Caching(new JewelInSkillTreeParser(_coreParser));
            _activeSkillParser =
                Caching(new ActiveSkillParser(skills, builderFactories, GetOrAddUntranslatedStatParser));
            _supportSkillParser =
                Caching(new SupportSkillParser(skills, builderFactories, GetOrAddUntranslatedStatParser));
            _skillsParser = new SkillsParser(skills, _activeSkillParser, _supportSkillParser);
        }

        private IParser<UntranslatedStatParserParameter> GetOrAddUntranslatedStatParser(
            IReadOnlyList<string> translationFileNames)
            => _untranslatedStatParsers.GetOrAdd(translationFileNames, CreateUntranslatedStatParser);

        private IParser<UntranslatedStatParserParameter> CreateUntranslatedStatParser(
            IReadOnlyList<string> translationFileNames)
        {
            var translators = translationFileNames
                .Append(StatTranslationFileNames.Custom)
                .Select(s => _statTranslators[s])
                .ToList();
            var composite = new CompositeStatTranslator(translators);
            return Caching(new UntranslatedStatParser(composite, _coreParser));
        }

        private static IParser<T> Caching<T>(IParser<T> parser)
            => new CachingParser<T>(parser);

        public ParseResult ParseRawModifier(
            string modifierLine, ModifierSource modifierSource, Entity modifierSourceEntity)
            => _coreParser.Parse(modifierLine, modifierSource, modifierSourceEntity);

        public ParseResult ParsePassiveNode(ushort nodeId)
            => _passiveNodeParser.Parse(nodeId);

        public ParseResult ParseSkilledPassiveNode(ushort nodeId)
            => _skilledPassiveNodeParser.Parse(nodeId);

        public ParseResult ParseItem(Item item, ItemSlot itemSlot)
            => _itemParser.Parse(new ItemParserParameter(item, itemSlot));

        public ParseResult ParseJewelSocketedInItem(Item item, ItemSlot itemSlot)
            => _itemJewelParser.Parse(new ItemParserParameter(item, itemSlot));

        public ParseResult ParseJewelSocketedInSkillTree(Item item, JewelRadius jewelRadius, ushort nodeId)
            => _treeJewelParser.Parse(new JewelInSkillTreeParserParameter(item, jewelRadius, nodeId));

        public ParseResult ParseSkills(IReadOnlyList<Skill> skills)
            => _skillsParser.Parse(new SequenceEquatableListView<Skill>(skills));

        public ParseResult ParseActiveSkill(Skill activeSkill)
            => _activeSkillParser.Parse(activeSkill);

        public ParseResult ParseSupportSkill(Skill activeSkill, Skill supportSkill)
            => _supportSkillParser.Parse(activeSkill, supportSkill);

        public IReadOnlyList<Modifier> ParseGivenModifiers()
            => GivenStatsParser.Parse(_coreParser, _givenStats);

        public IEnumerable<Func<IReadOnlyList<Modifier>>> CreateGivenModifierParseDelegates()
            => _givenStats.Select<IGivenStats, Func<IReadOnlyList<Modifier>>>(
                g => () => GivenStatsParser.Parse(_coreParser, g));
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoESkillTree.Computation.Builders;
using PoESkillTree.Computation.Builders.Resolving;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data;
using PoESkillTree.Computation.Data.GivenStats;
using PoESkillTree.Computation.Data.Steps;
using PoESkillTree.Computation.Parsing;
using PoESkillTree.Computation.Parsing.SkillParsers;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.PassiveTree;
using PoESkillTree.GameModel.Skills;
using PoESkillTree.GameModel.StatTranslation;

namespace PoESkillTree.Computation.Console
{
    public class CompositionRoot
    {
        private readonly Lazy<GameData> _gameData;
        private readonly Lazy<IStatFactory> _statFactory;
        private readonly Lazy<Task<IBuilderFactories>> _builderFactories;
        private readonly Lazy<Task<IParsingData<ParsingStep>>> _parsingData;
        private readonly Lazy<Task<ICoreParser>> _coreParser;
        private readonly Lazy<IMetaStatBuilders> _metaStats;
        private readonly Lazy<Task<IEnumerable<IGivenStats>>> _givenStats;
        private readonly Lazy<Task<IParser<Skill>>> _activeSkillParser;
        private readonly Lazy<Task<IParser<SupportSkillParserParameter>>> _supportSkillParser;

        public CompositionRoot()
        {
            _gameData = new Lazy<GameData>(() => new GameData(PassiveTreeDefinition.CreateKeystoneDefinitions()));
            _statFactory = new Lazy<IStatFactory>(() => new StatFactory());
            _builderFactories = new Lazy<Task<IBuilderFactories>>(CreateBuilderFactoriesAsync);
            _parsingData = new Lazy<Task<IParsingData<ParsingStep>>>(CreateParsingDataAsync);
            _coreParser = new Lazy<Task<ICoreParser>>(CreateCoreParserAsync);
            _metaStats = new Lazy<IMetaStatBuilders>(() => new MetaStatBuilders(_statFactory.Value));
            _givenStats = new Lazy<Task<IEnumerable<IGivenStats>>>(CreateGivenStatsAsync);
            _activeSkillParser = new Lazy<Task<IParser<Skill>>>(CreateActiveSkillParserAsync);
            _supportSkillParser = new Lazy<Task<IParser<SupportSkillParserParameter>>>(CreateSupportSkillParserAsync);
        }

        private async Task<IBuilderFactories> CreateBuilderFactoriesAsync()
        {
            var skillDefinitions = await _gameData.Value.Skills.ConfigureAwait(false);
            return new BuilderFactories(_statFactory.Value, skillDefinitions);
        }

        private async Task<IParsingData<ParsingStep>> CreateParsingDataAsync()
        {
            var skillDefinitions = await _gameData.Value.Skills.ConfigureAwait(false);
            var passiveNodeDefinitions = await _gameData.Value.PassiveTree.ConfigureAwait(false);
            var builderFactories = await _builderFactories.Value.ConfigureAwait(false);
            return new ParsingData(builderFactories, new MatchContexts(_statFactory.Value), skillDefinitions.Skills,
                passiveNodeDefinitions.Nodes);
        }

        private async Task<ICoreParser> CreateCoreParserAsync()
        {
            var builderFactories = await _builderFactories.Value.ConfigureAwait(false);
            var parsingData = await _parsingData.Value.ConfigureAwait(false);
            return new CoreParser<ParsingStep>(parsingData, builderFactories);
        }

        private async Task<IEnumerable<IGivenStats>> CreateGivenStatsAsync()
        {
            var characterTask = CharacterBaseStats.CreateAsync();
            var monsterTask = MonsterBaseStats.CreateAsync();
            var builderFactoriesTask = _builderFactories.Value;
            return new GivenStatsCollection(await builderFactoriesTask.ConfigureAwait(false), _metaStats.Value,
                await characterTask.ConfigureAwait(false), await monsterTask.ConfigureAwait(false));
        }

        private async Task<IParser<Skill>> CreateActiveSkillParserAsync()
        {
            var (skillDefinitions, builderFactories, metaStats, createStatParser) =
                await CreateSkillParserParametersAsync().ConfigureAwait(false);
            return new ActiveSkillParser(skillDefinitions, builderFactories, metaStats, createStatParser);
        }

        private async Task<IParser<SupportSkillParserParameter>> CreateSupportSkillParserAsync()
        {
            var (skillDefinitions, builderFactories, metaStats, createStatParser) =
                await CreateSkillParserParametersAsync().ConfigureAwait(false);
            return new SupportSkillParser(skillDefinitions, builderFactories, metaStats, createStatParser);
        }

        private async Task<(SkillDefinitions, IBuilderFactories, IMetaStatBuilders, UntranslatedStatParserFactory)>
            CreateSkillParserParametersAsync()
        {
            var statTranslationLoaderTask = _gameData.Value.StatTranslators;
            var skillDefinitionsTask = _gameData.Value.Skills;
            var builderFactoriesTask = _builderFactories.Value;
            var metaStats = _metaStats.Value;
            var coreParserTask = _coreParser.Value;

            await Task.WhenAll(statTranslationLoaderTask, skillDefinitionsTask, builderFactoriesTask, coreParserTask)
                .ConfigureAwait(false);

            return (skillDefinitionsTask.Result, builderFactoriesTask.Result, metaStats, CreateStatParser);

            IParser<UntranslatedStatParserParameter> CreateStatParser(string translationFileName)
            {
                var composite = new CompositeStatTranslator(
                    statTranslationLoaderTask.Result[translationFileName],
                    statTranslationLoaderTask.Result[StatTranslationLoader.CustomFileName]);
                return new UntranslatedStatParser(composite, coreParserTask.Result);
            }
        }

        public Task<SkillDefinitions> SkillDefinitions => _gameData.Value.Skills;
        public Task<BaseItemDefinitions> BaseItemDefinitions => _gameData.Value.BaseItems;
        public Task<IBuilderFactories> BuilderFactories => _builderFactories.Value;
        public Task<IParsingData<ParsingStep>> ParsingData => _parsingData.Value;
        public Task<ICoreParser> CoreParser => _coreParser.Value;
        public IMetaStatBuilders MetaStats => _metaStats.Value;
        public Task<IEnumerable<IGivenStats>> GivenStats => _givenStats.Value;
        public Task<IParser<Skill>> ActiveSkillParser => _activeSkillParser.Value;
        public Task<IParser<SupportSkillParserParameter>> SupportSkillParser => _supportSkillParser.Value;
    }
}
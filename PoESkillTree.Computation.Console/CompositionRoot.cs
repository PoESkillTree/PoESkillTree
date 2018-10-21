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
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Console
{
    /// <summary>
    /// Composition root for <see cref="Program"/> and integration tests. If <see cref="GivenStats"/> is not
    /// required, <see cref="AsyncCompositionRoot"/> is faster.
    /// </summary>
    public class CompositionRoot
    {
        private readonly AsyncCompositionRoot _asyncRoot;

        private CompositionRoot(AsyncCompositionRoot asyncRoot)
            => _asyncRoot = asyncRoot;

        public static async Task<CompositionRoot> CreateAsync()
        {
            var asyncRoot = new AsyncCompositionRoot();
            await asyncRoot.InitializeAsync().ConfigureAwait(false);
            return new CompositionRoot(asyncRoot);
        }

        public SkillDefinitions SkillDefinitions => _asyncRoot.SkillDefinitions.Result;
        public IBuilderFactories BuilderFactories => _asyncRoot.BuilderFactories.Result;
        public IParsingData<ParsingStep> ParsingData => _asyncRoot.ParsingData.Result;
        public ICoreParser CoreParser => _asyncRoot.CoreParser.Result;
        public IMetaStatBuilders MetaStats => _asyncRoot.MetaStats;
        public IEnumerable<IGivenStats> GivenStats => _asyncRoot.GivenStats.Result;
    }

    public class AsyncCompositionRoot
    {
        private readonly Lazy<IStatFactory> _statFactory;
        private readonly Lazy<Task<SkillDefinitions>> _skillDefinitions;
        private readonly Lazy<Task<IBuilderFactories>> _builderFactories;
        private readonly Lazy<Task<IParsingData<ParsingStep>>> _parsingData;
        private readonly Lazy<Task<ICoreParser>> _coreParser;
        private readonly Lazy<IMetaStatBuilders> _metaStats;
        private readonly Lazy<Task<IEnumerable<IGivenStats>>> _givenStats;

        public AsyncCompositionRoot()
        {
            _statFactory = new Lazy<IStatFactory>(() => new StatFactory());
            _skillDefinitions = new Lazy<Task<SkillDefinitions>>(
                async () => await SkillJsonDeserializer.DeserializeAsync().ConfigureAwait(false));
            _builderFactories = new Lazy<Task<IBuilderFactories>>(CreateBuilderFactoriesAsync);
            _parsingData = new Lazy<Task<IParsingData<ParsingStep>>>(CreateParsingDataAsync);
            _coreParser = new Lazy<Task<ICoreParser>>(CreateCoreParserAsync);
            _metaStats = new Lazy<IMetaStatBuilders>(() => new MetaStatBuilders(_statFactory.Value));
            _givenStats = new Lazy<Task<IEnumerable<IGivenStats>>>(CreateGivenStatsAsync);
        }

        private async Task<IBuilderFactories> CreateBuilderFactoriesAsync()
        {
            var skillDefinitions = await _skillDefinitions.Value.ConfigureAwait(false);
            return new BuilderFactories(_statFactory.Value, skillDefinitions);
        }

        private async Task<IParsingData<ParsingStep>> CreateParsingDataAsync()
        {
            var skillDefinitions = await _skillDefinitions.Value.ConfigureAwait(false);
            var builderFactories = await _builderFactories.Value.ConfigureAwait(false);
            return new ParsingData(builderFactories, new MatchContexts(_statFactory.Value), skillDefinitions.Skills);
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

        public Task InitializeAsync()
            => Task.WhenAll(BuilderFactories, ParsingData, CoreParser, GivenStats);

        public Task<SkillDefinitions> SkillDefinitions => _skillDefinitions.Value;
        public Task<IBuilderFactories> BuilderFactories => _builderFactories.Value;
        public Task<IParsingData<ParsingStep>> ParsingData => _parsingData.Value;
        public Task<ICoreParser> CoreParser => _coreParser.Value;
        public IMetaStatBuilders MetaStats => _metaStats.Value;
        public Task<IEnumerable<IGivenStats>> GivenStats => _givenStats.Value;
    }
}
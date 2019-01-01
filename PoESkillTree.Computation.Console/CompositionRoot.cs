using System;
using System.Threading.Tasks;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Steps;
using PoESkillTree.Computation.Parsing;
using PoESkillTree.Computation.Parsing.SkillParsers;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.PassiveTree;
using PoESkillTree.GameModel.Skills;
using PoESkillTree.GameModel.StatTranslation;

namespace PoESkillTree.Computation.Console
{
    public class CompositionRoot
    {
        private readonly Lazy<GameData> _gameData;
        private readonly Lazy<Task<IBuilderFactories>> _builderFactories;
        private readonly Lazy<Task<IParsingData<ParsingStep>>> _parsingData;
        private readonly Lazy<Task<ICoreParser>> _coreParser;
        private readonly Lazy<Task<IParser<Skill>>> _activeSkillParser;
        private readonly Lazy<Task<IParser<SupportSkillParserParameter>>> _supportSkillParser;

        public CompositionRoot()
        {
            _gameData = new Lazy<GameData>(
                () => new GameData(PassiveTreeDefinition.CreateKeystoneDefinitions()));
            _builderFactories = new Lazy<Task<IBuilderFactories>>(
                () => Builders.BuilderFactories.CreateAsync(_gameData.Value));
            _parsingData = new Lazy<Task<IParsingData<ParsingStep>>>(
                () => Data.ParsingData.CreateAsync(_gameData.Value, _builderFactories.Value));
            _coreParser = new Lazy<Task<ICoreParser>>(CreateCoreParserAsync);
            _activeSkillParser = new Lazy<Task<IParser<Skill>>>(CreateActiveSkillParserAsync);
            _supportSkillParser = new Lazy<Task<IParser<SupportSkillParserParameter>>>(CreateSupportSkillParserAsync);
        }

        private async Task<ICoreParser> CreateCoreParserAsync()
        {
            var builderFactories = await _builderFactories.Value.ConfigureAwait(false);
            var parsingData = await _parsingData.Value.ConfigureAwait(false);
            return new CoreParser<ParsingStep>(parsingData, builderFactories);
        }

        private async Task<IParser<Skill>> CreateActiveSkillParserAsync()
        {
            var (skillDefinitions, builderFactories, createStatParser) =
                await CreateSkillParserParametersAsync().ConfigureAwait(false);
            return new ActiveSkillParser(skillDefinitions, builderFactories, createStatParser);
        }

        private async Task<IParser<SupportSkillParserParameter>> CreateSupportSkillParserAsync()
        {
            var (skillDefinitions, builderFactories, createStatParser) =
                await CreateSkillParserParametersAsync().ConfigureAwait(false);
            return new SupportSkillParser(skillDefinitions, builderFactories, createStatParser);
        }

        private async Task<(SkillDefinitions, IBuilderFactories, UntranslatedStatParserFactory)>
            CreateSkillParserParametersAsync()
        {
            var statTranslationLoaderTask = _gameData.Value.StatTranslators;
            var skillDefinitionsTask = _gameData.Value.Skills;
            var builderFactoriesTask = _builderFactories.Value;
            var coreParserTask = _coreParser.Value;

            await Task.WhenAll(statTranslationLoaderTask, skillDefinitionsTask, builderFactoriesTask, coreParserTask)
                .ConfigureAwait(false);

            return (skillDefinitionsTask.Result, builderFactoriesTask.Result, CreateStatParser);

            IParser<UntranslatedStatParserParameter> CreateStatParser(string translationFileName)
            {
                var composite = new CompositeStatTranslator(
                    statTranslationLoaderTask.Result[translationFileName],
                    statTranslationLoaderTask.Result[StatTranslationFileNames.Custom]);
                return new UntranslatedStatParser(composite, coreParserTask.Result);
            }
        }

        public GameData GameData => _gameData.Value;
        public Task<IBuilderFactories> BuilderFactories => _builderFactories.Value;
        public Task<IParsingData<ParsingStep>> ParsingData => _parsingData.Value;
        public Task<ICoreParser> CoreParser => _coreParser.Value;
        public Task<IParser<Skill>> ActiveSkillParser => _activeSkillParser.Value;
        public Task<IParser<SupportSkillParserParameter>> SupportSkillParser => _supportSkillParser.Value;
    }
}
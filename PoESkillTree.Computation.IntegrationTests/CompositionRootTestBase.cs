using System;
using System.Threading.Tasks;
using NUnit.Framework;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Steps;
using PoESkillTree.Computation.Parsing;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.PassiveTree;

namespace PoESkillTree.Computation.IntegrationTests
{
    public abstract class CompositionRootTestBase
    {
        private static Lazy<GameData> _gameData;
        private static Lazy<Task<IBuilderFactories>> _builderFactories;
        private static Lazy<Task<IParsingData<ParsingStep>>> _parsingData;
        private static Lazy<Task<IParser>> _parser;

        protected static GameData GameData => _gameData.Value;
        protected static Task<IBuilderFactories> BuilderFactoriesTask => _builderFactories.Value;
        protected static Task<IParsingData<ParsingStep>> ParsingDataTask => _parsingData.Value;
        protected static Task<IParser> ParserTask => _parser.Value;

        [OneTimeSetUp]
        public static void CreateCompositionRoot()
        {
            if (_gameData is null)
            {
                _gameData = new Lazy<GameData>(
                    () => new GameData(PassiveTreeDefinition.CreateKeystoneDefinitions()));
                _builderFactories = new Lazy<Task<IBuilderFactories>>(
                    () => Builders.BuilderFactories.CreateAsync(_gameData.Value));
                _parsingData = new Lazy<Task<IParsingData<ParsingStep>>>(
                    () => Data.ParsingData.CreateAsync(_gameData.Value, _builderFactories.Value));
                _parser = new Lazy<Task<IParser>>(
                    () => Parser<ParsingStep>.CreateAsync(_gameData.Value, _builderFactories.Value,
                        _parsingData.Value));
            }
        }
    }
}
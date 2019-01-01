using System;
using System.Threading.Tasks;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Data;
using PoESkillTree.Computation.Data.Steps;
using PoESkillTree.Computation.Parsing;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.PassiveTree;

namespace PoESkillTree.Computation.Console
{
    public class CompositionRoot
    {
        private readonly Lazy<GameData> _gameData;
        private readonly Lazy<Task<IBuilderFactories>> _builderFactories;
        private readonly Lazy<Task<IParser>> _parser;

        public CompositionRoot()
        {
            _gameData = new Lazy<GameData>(
                () => new GameData(PassiveTreeDefinition.CreateKeystoneDefinitions(), true));
            _builderFactories = new Lazy<Task<IBuilderFactories>>(
                () => Builders.BuilderFactories.CreateAsync(_gameData.Value));
            _parser = new Lazy<Task<IParser>>(
                () => Parser<ParsingStep>.CreateAsync(_gameData.Value, _builderFactories.Value,
                    ParsingData.CreateAsync(_gameData.Value, _builderFactories.Value)));
        }

        public GameData GameData => _gameData.Value;
        public Task<IBuilderFactories> BuilderFactories => _builderFactories.Value;
        public Task<IParser> Parser => _parser.Value;
    }
}
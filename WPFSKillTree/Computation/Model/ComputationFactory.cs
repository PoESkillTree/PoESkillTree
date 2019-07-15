using System;
using System.Threading.Tasks;
using PoESkillTree.Engine.Computation.Builders;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Core;
using PoESkillTree.Engine.Computation.Data;
using PoESkillTree.Engine.Computation.Data.Steps;
using PoESkillTree.Engine.Computation.Parsing;
using PoESkillTree.Engine.GameModel;

namespace PoESkillTree.Computation.Model
{
    public class ComputationFactory
    {
        private readonly Lazy<Task<IBuilderFactories>> _builderFactories;
        private readonly Lazy<Task<IParser>> _parser;

        public ComputationFactory(GameData gameData)
        {
            _builderFactories = new Lazy<Task<IBuilderFactories>>(
                () => BuilderFactories.CreateAsync(gameData));
            _parser = new Lazy<Task<IParser>>(
                () => Parser<ParsingStep>.CreateAsync(gameData, _builderFactories.Value,
                    ParsingData.CreateAsync(gameData, _builderFactories.Value)));
        }

        public ICalculator CreateCalculator()
            => Calculator.Create();

        public Task<IBuilderFactories> CreateBuilderFactoriesAsync()
            => _builderFactories.Value;

        public Task<IParser> CreateParserAsync()
            => _parser.Value;
    }
}
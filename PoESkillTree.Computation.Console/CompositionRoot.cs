using System;
using PoESkillTree.Computation.Builders;
using PoESkillTree.Computation.Builders.Resolving;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data;
using PoESkillTree.Computation.Data.Steps;
using PoESkillTree.Computation.Parsing;

namespace PoESkillTree.Computation.Console
{
    /// <summary>
    /// Composition root for <see cref="Program"/> and integration tests.
    /// </summary>
    public class CompositionRoot
    {
        private readonly Lazy<IStatFactory> _statFactory = new Lazy<IStatFactory>(() => new StatFactory());
        private readonly Lazy<IParsingData<ParsingStep>> _parsingData;
        private readonly Lazy<IParser> _parser;

        public CompositionRoot()
        {
            var builderFactories = new Lazy<IBuilderFactories>(
                () => new BuilderFactories(_statFactory.Value, SkillDefinitions.Skills));
            _parsingData = new Lazy<IParsingData<ParsingStep>>(
                () => new ParsingData(builderFactories.Value, new MatchContexts(_statFactory.Value),
                    SkillDefinitions.SkillNames));
            _parser = new Lazy<IParser>(
                () => new Parser<ParsingStep>(_parsingData.Value, builderFactories.Value));
        }

        public IParsingData<ParsingStep> ParsingData => _parsingData.Value;
        public IParser Parser => _parser.Value;
    }
}
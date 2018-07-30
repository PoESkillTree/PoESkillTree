using System;
using System.Collections.Generic;
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

namespace PoESkillTree.Computation.Console
{
    /// <summary>
    /// Composition root for <see cref="Program"/> and integration tests.
    /// </summary>
    public class CompositionRoot
    {
        private readonly Lazy<IBuilderFactories> _builderFactories;
        private readonly Lazy<IParsingData<ParsingStep>> _parsingData;
        private readonly Lazy<IParser> _parser;
        private readonly Lazy<IMetaStatBuilders> _metaStats;
        private readonly Lazy<IEnumerable<IGivenStats>> _givenStats;

        public CompositionRoot()
        {
            var statFactory = new Lazy<IStatFactory>(() => new StatFactory());
            _builderFactories = new Lazy<IBuilderFactories>(
                () => new BuilderFactories(statFactory.Value, SkillDefinitions.Skills));
            _parsingData = new Lazy<IParsingData<ParsingStep>>(
                () => new ParsingData(_builderFactories.Value, new MatchContexts(statFactory.Value),
                    SkillDefinitions.SkillNames));
            _parser = new Lazy<IParser>(
                () => new Parser<ParsingStep>(_parsingData.Value, _builderFactories.Value));
            _metaStats = new Lazy<IMetaStatBuilders>(
                () => new MetaStatBuilders(statFactory.Value));
            _givenStats = new Lazy<IEnumerable<IGivenStats>>(
                () => new GivenStatsCollection(_builderFactories.Value, _metaStats.Value));
        }

        public IBuilderFactories BuilderFactories => _builderFactories.Value;
        public IParsingData<ParsingStep> ParsingData => _parsingData.Value;
        public IParser Parser => _parser.Value;
        public IMetaStatBuilders MetaStats => _metaStats.Value;
        public IEnumerable<IGivenStats> GivenStats => _givenStats.Value;
    }
}
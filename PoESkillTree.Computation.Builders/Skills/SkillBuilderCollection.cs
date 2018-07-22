using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using PoESkillTree.Computation.Builders.Actions;
using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Skills
{
    public class SkillBuilderCollection : ISkillBuilderCollection
    {
        private readonly IStatFactory _statFactory;
        private readonly ICoreBuilder<IEnumerable<Keyword>> _coreBuilder;

        public SkillBuilderCollection(IStatFactory statFactory, IEnumerable<IKeywordBuilder> keywords)
            : this(statFactory, new KeywordsCoreBuilder(keywords))
        {
        }

        private SkillBuilderCollection(IStatFactory statFactory, ICoreBuilder<IEnumerable<Keyword>> coreBuilder)
        {
            _statFactory = statFactory;
            _coreBuilder = coreBuilder;
        }

        public ISkillBuilderCollection Resolve(ResolveContext context) =>
            new SkillBuilderCollection(_statFactory, _coreBuilder.Resolve(context));

        public IActionBuilder Cast =>
            new ActionBuilder(_statFactory, CoreBuilder.UnaryOperation(_coreBuilder,
                ks => $"{KeywordsToString(ks)}.Cast"), new ModifierSourceEntityBuilder());

        public IStatBuilder CombinedInstances =>
            new StatBuilder(_statFactory, new CoreStatBuilderFromCoreBuilder<IEnumerable<Keyword>>(_coreBuilder,
                (e, ks) => _statFactory.FromIdentity($"{KeywordsToString(ks)}.Instances", e,
                    typeof(int))));

        private static string KeywordsToString(IEnumerable<Keyword> keywords) =>
            $"Skills[{keywords.ToDelimitedString(", ")}]";

        private class KeywordsCoreBuilder : ICoreBuilder<IEnumerable<Keyword>>
        {
            private readonly IEnumerable<IKeywordBuilder> _keywords;

            public KeywordsCoreBuilder(IEnumerable<IKeywordBuilder> keywords) =>
                _keywords = keywords;

            public ICoreBuilder<IEnumerable<Keyword>> Resolve(ResolveContext context) =>
                new KeywordsCoreBuilder(_keywords.Select(b => b.Resolve(context)));

            public IEnumerable<Keyword> Build() =>
                _keywords.Select(b => b.Build());
        }
    }
}
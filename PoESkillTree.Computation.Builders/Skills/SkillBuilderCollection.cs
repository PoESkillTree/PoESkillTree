using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MoreLinq;
using PoESkillTree.Computation.Builders.Actions;
using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel.Skills;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Builders.Skills
{
    public class SkillBuilderCollection : ISkillBuilderCollection
    {
        private readonly IStatFactory _statFactory;
        private readonly ICoreBuilder<IEnumerable<Keyword>> _coreBuilder;
        private readonly IEnumerable<SkillDefinition> _skills;

        public SkillBuilderCollection(
            IStatFactory statFactory, IEnumerable<IKeywordBuilder> keywords,
            IEnumerable<SkillDefinition> skills)
            : this(statFactory, new KeywordsCoreBuilder(keywords), skills)
        {
        }

        private SkillBuilderCollection(
            IStatFactory statFactory, ICoreBuilder<IEnumerable<Keyword>> coreBuilder,
            IEnumerable<SkillDefinition> skills)
        {
            _statFactory = statFactory;
            _coreBuilder = coreBuilder;
            _skills = skills;
        }

        public ISkillBuilderCollection Resolve(ResolveContext context) =>
            new SkillBuilderCollection(_statFactory, _coreBuilder.Resolve(context), _skills);

        public IActionBuilder Cast =>
            new ActionBuilder(_statFactory, CoreBuilder.UnaryOperation(_coreBuilder,
                ks => $"{KeywordsToString(ks)}.Cast"), new ModifierSourceEntityBuilder());

        public IStatBuilder CombinedInstances =>
            new StatBuilder(_statFactory, new CoreStatBuilderFromCoreBuilder<IEnumerable<Keyword>>(_coreBuilder,
                (e, ks) => _statFactory.FromIdentity($"{KeywordsToString(ks)}.Instances", e,
                    typeof(int))));

        public IStatBuilder Reservation =>
            new StatBuilder(_statFactory, new CoreStatBuilderFromCoreBuilder<IEnumerable<Keyword>>(_coreBuilder,
                (_, e, ks) => SelectSkillStats(ks, e, typeof(int))));

        public IStatBuilder ReservationPool =>
            new StatBuilder(_statFactory, new CoreStatBuilderFromCoreBuilder<IEnumerable<Keyword>>(_coreBuilder,
                (_, e, ks) => SelectSkillStats(ks, e, typeof(Pool))));

        private IEnumerable<IStat> SelectSkillStats(
            IEnumerable<Keyword> keywords, Entity entity, Type dataType,
            [CallerMemberName] string identitySuffix = null)
        {
            var keywordList = keywords.ToList();
            return from skill in _skills
                   where !skill.IsSupport && skill.ActiveSkill.Keywords.ContainsAll(keywordList)
                   let identity = $"{skill.Id}.{identitySuffix}"
                   select _statFactory.FromIdentity(identity, entity, dataType);
        }

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
using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Utils;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Common.Parsing;

namespace PoESkillTree.Computation.Builders.Stats
{
    public class CompositeCoreStatBuilder : ICoreStatBuilder
    {
        private readonly IReadOnlyList<ICoreStatBuilder> _items;

        public CompositeCoreStatBuilder(params ICoreStatBuilder[] items) =>
            _items = items;

        private CompositeCoreStatBuilder Select(Func<ICoreStatBuilder, ICoreStatBuilder> selector) =>
            new CompositeCoreStatBuilder(_items.Select(selector).ToArray());

        public ICoreStatBuilder Resolve(ResolveContext context) =>
            Select(i => i.Resolve(context));

        public ICoreStatBuilder WithEntity(IEntityBuilder entityBuilder) =>
            Select(i => i.WithEntity(entityBuilder));

        public ICoreStatBuilder WithStatConverter(Func<IStat, IStat> statConverter) =>
            Select(i => i.WithStatConverter(statConverter));

        public IValue BuildValue(Entity modifierSourceEntity) =>
            throw new ParseException("Can only access the value of stat builders that represent a single stat");

        public IReadOnlyList<StatBuilderResult> Build(
            ModifierSource originalModifierSource, Entity modifierSourceEntity)
        {
            var seed = new StatBuilderResult(new IStat[0], originalModifierSource, Funcs.Identity);
            return new[] { _items.Aggregate(seed, Aggregate) }; // TODO

            StatBuilderResult Aggregate(StatBuilderResult previous, ICoreStatBuilder item)
            {
                var built = item.Build(previous.ModifierSource, modifierSourceEntity).Single(); // TODO
                var stats = previous.Stats.Concat(built.Stats).ToList();
                var source = built.ModifierSource;
                IValueBuilder ConvertValue(IValueBuilder v) => built.ValueConverter(previous.ValueConverter(v));
                return new StatBuilderResult(stats, source, ConvertValue);
            }
        }
    }
}
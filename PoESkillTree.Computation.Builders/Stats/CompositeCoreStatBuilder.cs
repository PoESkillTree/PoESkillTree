using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Utils;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Values;

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

        public ICoreStatBuilder CombineWith(ICoreStatBuilder other) =>
            new CompositeCoreStatBuilder(_items.Append(other).ToArray());

        public IValue BuildValue(Entity modifierSourceEntity) =>
            throw new InvalidOperationException(
                "Can only access the value of IStatBuilders that represent a single stat");

        public StatBuilderResult Build(ModifierSource originalModifierSource, Entity modifierSourceEntity)
        {
            var seed = new StatBuilderResult(new IStat[0], originalModifierSource, Funcs.Identity);
            return _items.Aggregate(seed, Aggregate);

            StatBuilderResult Aggregate(StatBuilderResult previous, ICoreStatBuilder item)
            {
                var built = item.Build(previous.ModifierSource, modifierSourceEntity);
                var stats = previous.Stats.Concat(built.Stats).ToList();
                var source = built.ModifierSource;
                IValueBuilder ConvertValue(IValueBuilder v) => built.ValueConverter(previous.ValueConverter(v));
                return new StatBuilderResult(stats, source, ConvertValue);
            }
        }
    }
}
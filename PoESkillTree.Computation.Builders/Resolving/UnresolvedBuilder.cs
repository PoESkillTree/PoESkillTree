using System;
using System.Collections.Generic;
using PoESkillTree.Common.Utils;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Equipment;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Common.Parsing;
using PoESkillTree.GameModel.Items;

namespace PoESkillTree.Computation.Builders.Resolving
{
    public class UnresolvedBuilder<TResolve, TBuild> : IResolvable<TResolve>
    {
        protected string Description { get; }
        protected Func<ResolveContext, TResolve> Resolver { get; }

        public UnresolvedBuilder(string description, Func<ResolveContext, TResolve> resolver)
        {
            Description = description;
            Resolver = resolver;
        }

        public TResolve Resolve(ResolveContext context) =>
            Resolver(context);

        public TBuild Build() => 
            throw new UnresolvedException(Description);

        public override string ToString() => Description;
    }

    public class UnresolvedException : ParseException
    {
        public UnresolvedException(string message) 
            : base("Builder must be resolved before being built, " + message)
        {
        }
    }

    internal class UnresolvedItemSlotBuilder : UnresolvedBuilder<IItemSlotBuilder, ItemSlot>, IItemSlotBuilder
    {
        public UnresolvedItemSlotBuilder(string description, Func<ResolveContext, IItemSlotBuilder> resolver) 
            : base(description, resolver)
        {
        }
    }

    internal class UnresolvedKeywordBuilder : UnresolvedBuilder<IKeywordBuilder, Keyword>, IKeywordBuilder
    {
        public UnresolvedKeywordBuilder(string description, Func<ResolveContext, IKeywordBuilder> resolver) 
            : base(description, resolver)
        {
        }
    }

    public class UnresolvedValueBuilder : ValueBuilderImpl
    {
        private readonly string _description;

        public UnresolvedValueBuilder(string description, Func<ResolveContext, IValueBuilder> resolver)
            : base(_ => throw new UnresolvedException(description), resolver)
        {
            _description = description;
        }

        public override string ToString() => _description;
    }

    internal class UnresolvedCoreBuilder<TResult>
        : UnresolvedBuilder<ICoreBuilder<TResult>, TResult>, ICoreBuilder<TResult>
    {
        public UnresolvedCoreBuilder(string description, Func<ResolveContext, ICoreBuilder<TResult>> resolver)
            : base(description, resolver)
        {
        }
    }

    internal class UnresolvedCoreStatBuilder
        : UnresolvedBuilder<ICoreStatBuilder, IEnumerable<StatBuilderResult>>, ICoreStatBuilder
    {
        public UnresolvedCoreStatBuilder(string description, Func<ResolveContext, ICoreStatBuilder> resolver)
            : base(description, resolver)
        {
        }

        public ICoreStatBuilder WithEntity(IEntityBuilder entityBuilder) =>
            new UnresolvedCoreStatBuilder(Description, Resolver.AndThen(b => b.WithEntity(entityBuilder)));

        public IEnumerable<StatBuilderResult> Build(BuildParameters parameters) => Build();
    }
}
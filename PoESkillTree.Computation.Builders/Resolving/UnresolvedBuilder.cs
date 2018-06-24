using System;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common.Builders.Equipment;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Common.Parsing;

namespace PoESkillTree.Computation.Builders.Resolving
{
    public class UnresolvedBuilder<TResolve, TBuild> : IResolvable<TResolve>
    {
        private readonly string _description;
        private readonly Func<ResolveContext, TResolve> _resolver;

        public UnresolvedBuilder(string description, Func<ResolveContext, TResolve> resolver)
        {
            _description = description;
            _resolver = resolver;
        }

        public TResolve Resolve(ResolveContext context) =>
            _resolver(context);

        public TBuild Build() => 
            throw new UnresolvedException(_description);

        public override string ToString() => _description;
    }

    public class UnresolvedException : ParseException
    {
        public UnresolvedException(string message) 
            : base("Builder must be resolved before being built, " + message)
        {
        }
    }

    public class UnresolvedItemSlotBuilder : UnresolvedBuilder<IItemSlotBuilder, ItemSlot>, IItemSlotBuilder
    {
        public UnresolvedItemSlotBuilder(string description, Func<ResolveContext, IItemSlotBuilder> resolver) 
            : base(description, resolver)
        {
        }
    }

    public class UnresolvedKeywordBuilder : UnresolvedBuilder<IKeywordBuilder, Keyword>, IKeywordBuilder
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
}
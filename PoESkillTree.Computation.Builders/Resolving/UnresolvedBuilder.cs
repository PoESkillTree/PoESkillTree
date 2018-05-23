using System;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Common.Builders.Equipment;
using PoESkillTree.Computation.Common.Builders.Resolving;
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
            throw new ParseException("Builder must be resolved before being built, " + this);

        public override string ToString() => _description;
    }


    public class UnresolvedItemSlotBuilder : UnresolvedBuilder<IItemSlotBuilder, ItemSlot>, IItemSlotBuilder
    {
        public UnresolvedItemSlotBuilder(string description, Func<ResolveContext, IItemSlotBuilder> resolver) 
            : base(description, resolver)
        {
        }
    }
}
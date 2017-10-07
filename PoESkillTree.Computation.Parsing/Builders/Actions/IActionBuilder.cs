using System;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.Builders.Actions
{
    // Default source is Self, default target is AnyTarget
    public interface IActionBuilder : IResolvable<IActionBuilder>
    {
        IEntityBuilder Source { get; }

        IEntityBuilder Target { get; }

        IConditionBuilder On(IKeywordBuilder withKeyword);

        // how often action happened recently (always user entered)
        ValueBuilder CountRecently { get; }
    }


    public interface IActionBuilder<out TSource, out TTarget> : IActionBuilder
        where TSource : IEntityBuilder
        where TTarget : IEntityBuilder
    {
        // changes source
        IActionBuilder<TNewSource, TTarget> By<TNewSource>(TNewSource source)
            where TNewSource : IEntityBuilder;
        // changes target
        IActionBuilder<TSource, TNewTarget> Against<TNewTarget>(TNewTarget target)
            where TNewTarget : IEntityBuilder;

        // swaps source and target
        IActionBuilder<TTarget, TSource> Taken { get; }

        IConditionBuilder On(Func<TTarget, IConditionBuilder> targetPredicate = null,
            Func<TSource, IConditionBuilder> sourcePredicate = null);

        // seconds for all actions need to be specified by the user
        IConditionBuilder InPastXSeconds(IValueBuilder seconds,
            Func<TTarget, IConditionBuilder> targetPredicate = null,
            Func<TSource, IConditionBuilder> sourcePredicate = null);

        // shortcut for InPastXSeconds(4)
        IConditionBuilder Recently(Func<TTarget, IConditionBuilder> targetPredicate = null,
            Func<TSource, IConditionBuilder> sourcePredicate = null);
    }


    // ActionProvider with default source and target
    public interface ISelfToAnyActionBuilder : IActionBuilder<ISelfBuilder, IEntityBuilder>
    {
        
    }
}
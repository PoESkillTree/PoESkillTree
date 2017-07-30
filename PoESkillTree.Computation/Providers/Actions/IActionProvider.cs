using System;
using PoESkillTree.Computation.Providers.Conditions;
using PoESkillTree.Computation.Providers.Skills;
using PoESkillTree.Computation.Providers.Entities;
using PoESkillTree.Computation.Providers.Values;

namespace PoESkillTree.Computation.Providers.Actions
{
    // Default source is Self, default target is AnyTarget
    public interface IActionProvider
    {
        IConditionProvider On(IKeywordProvider withKeyword);

        // how often action happened recently (always user entered)
        ValueProvider CountRecently { get; }
    }


    public interface IActionProvider<out TSource, out TTarget> : IActionProvider
        where TSource : IEntityProvider
        where TTarget : IEntityProvider
    {
        // changes source
        IActionProvider<TNewSource, TTarget> By<TNewSource>(TNewSource source)
            where TNewSource : IEntityProvider;
        // changes target
        IActionProvider<TSource, TNewTarget> Against<TNewTarget>(TNewTarget target)
            where TNewTarget : IEntityProvider;

        // swaps source and target
        IActionProvider<TTarget, TSource> Taken { get; }

        IConditionProvider On(Func<TTarget, IConditionProvider> targetPredicate = null,
            Func<TSource, IConditionProvider> sourcePredicate = null);

        // seconds for all actions need to be specified by the user
        IConditionProvider InPastXSeconds(ValueProvider seconds,
            Func<TTarget, IConditionProvider> targetPredicate = null,
            Func<TSource, IConditionProvider> sourcePredicate = null);

        // shortcut for InPastXSeconds(4)
        IConditionProvider Recently(Func<TTarget, IConditionProvider> targetPredicate = null,
            Func<TSource, IConditionProvider> sourcePredicate = null);
    }


    // ActionProvider with default source and target
    public interface ISelfToAnyActionProvider : IActionProvider<ISelfProvider, IEntityProvider>
    {
        
    }
}
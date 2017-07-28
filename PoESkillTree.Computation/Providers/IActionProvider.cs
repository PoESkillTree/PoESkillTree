using System;

namespace PoESkillTree.Computation.Providers
{
    using IDefaultActionProvider = IActionProvider<ISelfProvider, ITargetProvider>;

    // Default source is Self, default target is AnyTarget
    public interface IActionProvider
    {
        IConditionProvider On(IKeywordProvider withKeyword);

        // how often action happened recently (always user entered)
        ValueProvider CountRecently { get; }
    }


    public interface IActionProvider<out TSource, out TTarget> : IActionProvider
        where TSource : ITargetProvider
        where TTarget : ITargetProvider
    {
        // changes source
        IActionProvider<TNewSource, TTarget> By<TNewSource>(TNewSource source)
            where TNewSource : ITargetProvider;
        // changes target
        IActionProvider<TSource, TNewTarget> Against<TNewTarget>(TNewTarget target)
            where TNewTarget : ITargetProvider;

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


    public static class ActionProviders
    {
        public static readonly IDefaultActionProvider Kill;
        public static readonly IDefaultActionProvider Block;
        public static readonly IDefaultActionProvider Hit;
        public static readonly IDefaultActionProvider SavageHit;
        public static readonly IDefaultActionProvider CriticalStrike;
        public static readonly IDefaultActionProvider NonCriticalStrike;
        public static readonly IDefaultActionProvider Shatter;
        public static readonly IDefaultActionProvider ConsumeCorpse;

        public static IDefaultActionProvider SpendMana(ValueProvider amount)
        {
            throw new NotImplementedException();
        }
    }
}
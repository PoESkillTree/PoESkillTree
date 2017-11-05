using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Console.Builders
{
    public static class BuilderFactory
    {
        #region IValueBuilder

        public static IValueBuilder CreateValue<T>(
            [CanBeNull] T operand, 
            Func<T, string> stringRepresentation)
            where T : class, IResolvable<T>
        {
            return Create<IValueBuilder, T>(CreateValue, operand, stringRepresentation);
        }

        public static IValueBuilder CreateValue<T1, T2>(
            [CanBeNull] T1 operand1, [CanBeNull] T2 operand2,
            Func<T1, T2, string> stringRepresentation)
            where T1 : class, IResolvable<T1>
            where T2 : class, IResolvable<T2>
        {
            return Create<IValueBuilder, T1, T2>(CreateValue, operand1, operand2,
                stringRepresentation);
        }

        public static IValueBuilder CreateValue(string stringRepresentation)
        {
            return Create<IValueBuilder>(CreateValue, stringRepresentation);
        }

        private static IValueBuilder CreateValue(string stringRepresentation,
            Resolver<IValueBuilder> resolver)
        {
            return new ValueBuilderStub(stringRepresentation, resolver);
        }

        #endregion

        #region IConditionBuilder

        public static IConditionBuilder CreateCondition<T>(
            [CanBeNull] T operand, 
            Func<T, string> stringRepresentation)
            where T : class, IResolvable<T>
        {
            return Create<IConditionBuilder, T>(CreateCondition, operand, stringRepresentation);
        }

        public static IConditionBuilder CreateCondition<T1, T2>(
            [CanBeNull] T1 operand1, [CanBeNull] T2 operand2,
            Func<T1, T2, string> stringRepresentation)
            where T1 : class, IResolvable<T1>
            where T2 : class, IResolvable<T2>
        {
            return Create<IConditionBuilder, T1, T2>(CreateCondition, operand1, operand2,
                stringRepresentation);
        }

        public static IConditionBuilder CreateCondition<T1, T2, T3>(
            [CanBeNull] T1 operand1, [CanBeNull] T2 operand2, [CanBeNull] T3 operand3,
            Func<T1, T2, T3, string> stringRepresentation)
            where T1 : class, IResolvable<T1>
            where T2 : class, IResolvable<T2>
            where T3 : class, IResolvable<T3>
        {
            return Create<IConditionBuilder, T1, T2, T3>(CreateCondition,
                operand1, operand2, operand3, stringRepresentation);
        }

        public static IConditionBuilder CreateCondition<T1, T2, T3, T4>(
            [CanBeNull] T1 operand1, 
            [CanBeNull] T2 operand2, 
            [CanBeNull] T3 operand3, 
            [CanBeNull] T4 operand4,
            Func<T1, T2, T3, T4, string> stringRepresentation)
            where T1 : class, IResolvable<T1>
            where T2 : class, IResolvable<T2>
            where T3 : class, IResolvable<T3>
            where T4 : class, IResolvable<T4>
        {
            return Create<IConditionBuilder, T1, T2, T3, T4>(CreateCondition,
                operand1, operand2, operand3, operand4, stringRepresentation);
        }

        public static IConditionBuilder CreateCondition<T>(
            [ItemCanBeNull] IEnumerable<T> operands,
            Func<IEnumerable<T>, string> stringRepresentation)
            where T : class, IResolvable<T>
        {
            return Create<IConditionBuilder, T>(CreateCondition, operands, stringRepresentation);
        }

        public static IConditionBuilder CreateCondition(string stringRepresentation)
        {
            return Create<IConditionBuilder>(CreateCondition, stringRepresentation);
        }

        private static IConditionBuilder CreateCondition(string stringRepresentation,
            Resolver<IConditionBuilder> resolver)
        {
            return new ConditionBuilderStub(stringRepresentation, resolver);
        }

        #endregion

        #region IStatBuilder

        public static IStatBuilder CreateStat<T>(
            [CanBeNull] T operand,
            Func<T, string> stringRepresentation)
            where T : class, IResolvable<T>
        {
            return Create<IStatBuilder, T>(CreateStat, operand, stringRepresentation);
        }

        public static IStatBuilder CreateStat<T1, T2>(
            [CanBeNull] T1 operand1, [CanBeNull] T2 operand2,
            Func<T1, T2, string> stringRepresentation)
            where T1 : class, IResolvable<T1>
            where T2 : class, IResolvable<T2>
        {
            return Create<IStatBuilder, T1, T2>(CreateStat, operand1, operand2,
                stringRepresentation);
        }

        public static IStatBuilder CreateStat<T>(
            [ItemCanBeNull] IEnumerable<T> operands,
            Func<IEnumerable<T>, string> stringRepresentation)
            where T : class, IResolvable<T>
        {
            return Create<IStatBuilder, T>(CreateStat, operands, stringRepresentation);
        }

        public static IStatBuilder CreateStat(string stringRepresentation)
        {
            return Create<IStatBuilder>(CreateStat, stringRepresentation);
        }

        private static IStatBuilder CreateStat(string stringRepresentation,
            Resolver<IStatBuilder> resolver)
        {
            return new StatBuilderStub(stringRepresentation, resolver);
        }

        #endregion

        #region IFlagStatBuilder

        public static IFlagStatBuilder CreateFlagStat<T>(
            [CanBeNull] T operand,
            Func<T, string> stringRepresentation)
            where T : class, IResolvable<T>
        {
            return (IFlagStatBuilder) Create<IStatBuilder, T>(CreateFlagStat, operand,
                stringRepresentation);
        }

        public static IFlagStatBuilder CreateFlagStat<T1, T2>(
            [CanBeNull] T1 operand1, [CanBeNull] T2 operand2,
            Func<T1, T2, string> stringRepresentation)
            where T1 : class, IResolvable<T1>
            where T2 : class, IResolvable<T2>
        {
            return (IFlagStatBuilder) Create<IStatBuilder, T1, T2>(CreateFlagStat, operand1,
                operand2, stringRepresentation);
        }

        public static IFlagStatBuilder CreateFlagStat<T1, T2, T3>(
            [CanBeNull] T1 operand1, [CanBeNull] T2 operand2, [CanBeNull] T3 operand3,
            Func<T1, T2, T3, string> stringRepresentation)
            where T1 : class, IResolvable<T1>
            where T2 : class, IResolvable<T2>
            where T3 : class, IResolvable<T3>
        {
            return (IFlagStatBuilder) Create<IStatBuilder, T1, T2, T3>(CreateFlagStat,
                operand1, operand2, operand3, stringRepresentation);
        }

        public static IFlagStatBuilder CreateFlagStat<T1, T2>(
            [CanBeNull] T1 operand1, [ItemCanBeNull] IEnumerable<T2> operand2,
            Func<T1, IEnumerable<T2>, string> stringRepresentation)
            where T1 : class, IResolvable<T1>
            where T2 : class, IResolvable<T2>
        {
            return (IFlagStatBuilder) Create<IStatBuilder, T1, T2>(CreateFlagStat, operand1,
                operand2, stringRepresentation);
        }

        private static IFlagStatBuilder CreateFlagStat(string stringRepresentation,
            Resolver<IStatBuilder> resolver)
        {
            return new FlagStatBuilderStub(stringRepresentation, resolver);
        }

        #endregion

        public static TOut Create<TOut, T>(
            Func<string, Resolver<TOut>, TOut> constructor,
            [CanBeNull] T operand,
            Func<T, string> stringRepresentation)
            where T : class, IResolvable<T>
        {
            TOut Resolve(ResolveContext context) =>
                Create(constructor, stringRepresentation(operand?.Resolve(context)));

            return Create(constructor, stringRepresentation(operand), Resolve);
        }

        public static T Create<T>(
            Func<string, Resolver<T>, T> constructor,
            [CanBeNull] T operand,
            Func<T, string> stringRepresentation)
            where T : class, IResolvable<T>
        {
            T Resolve(ResolveContext context) =>
                Create(constructor, stringRepresentation(operand?.Resolve(context)));

            return Create(constructor, stringRepresentation(operand), Resolve);
        }

        public static TOut Create<TOut, T1, T2>(
            Func<string, Resolver<TOut>, TOut> constructor,
            [CanBeNull] T1 operand1,
            [CanBeNull] T2 operand2,
            Func<T1, T2, string> stringRepresentation)
            where T1 : class, IResolvable<T1>
            where T2 : class, IResolvable<T2>
        {
            TOut Resolve(ResolveContext context) =>
                Create(constructor, stringRepresentation(
                    operand1?.Resolve(context),
                    operand2?.Resolve(context)));

            return Create(constructor, stringRepresentation(operand1, operand2), Resolve);
        }

        public static T1 Create<T1, T2>(
            Func<string, Resolver<T1>, T1> constructor,
            [CanBeNull] T1 operand1,
            [CanBeNull] T2 operand2,
            Func<T1, T2, string> stringRepresentation)
            where T1 : class, IResolvable<T1>
            where T2 : class, IResolvable<T2>
        {
            T1 Resolve(ResolveContext context) =>
                Create(constructor, stringRepresentation(
                    operand1?.Resolve(context),
                    operand2?.Resolve(context)));

            return Create(constructor, stringRepresentation(operand1, operand2), Resolve);
        }

        private static TOut Create<TOut, T1, T2, T3>(
            Func<string, Resolver<TOut>, TOut> constructor,
            [CanBeNull] T1 operand1,
            [CanBeNull] T2 operand2,
            [CanBeNull] T3 operand3,
            Func<T1, T2, T3, string> stringRepresentation)
            where T1 : class, IResolvable<T1>
            where T2 : class, IResolvable<T2>
            where T3 : class, IResolvable<T3>
        {
            TOut Resolve(ResolveContext context) =>
                Create(constructor, stringRepresentation(
                    operand1?.Resolve(context),
                    operand2?.Resolve(context),
                    operand3?.Resolve(context)));

            return Create(constructor, stringRepresentation(operand1, operand2, operand3), Resolve);
        }

        private static TOut Create<TOut, T1, T2, T3, T4>(
            Func<string, Resolver<TOut>, TOut> constructor,
            [CanBeNull] T1 operand1,
            [CanBeNull] T2 operand2,
            [CanBeNull] T3 operand3,
            [CanBeNull] T4 operand4,
            Func<T1, T2, T3, T4, string> stringRepresentation)
            where T1 : class, IResolvable<T1>
            where T2 : class, IResolvable<T2>
            where T3 : class, IResolvable<T3>
            where T4 : class, IResolvable<T4>
        {
            TOut Resolve(ResolveContext context) =>
                Create(constructor, stringRepresentation(
                    operand1?.Resolve(context),
                    operand2?.Resolve(context),
                    operand3?.Resolve(context),
                    operand4?.Resolve(context)));

            return Create(constructor, 
                stringRepresentation(operand1, operand2, operand3, operand4), Resolve);
        }

        public static TOut Create<TOut, T>(
            Func<string, Resolver<TOut>, TOut> constructor,
            [ItemCanBeNull] IEnumerable<T> operands,
            Func<IEnumerable<T>, string> stringRepresentation)
            where T : class, IResolvable<T>
        {
            var os = operands.ToList();

            TOut Resolve(ResolveContext context) =>
                Create(constructor, stringRepresentation(
                    os.Select(o => o?.Resolve(context))));

            return Create(constructor, stringRepresentation(os), Resolve);
        }

        private static TOut Create<TOut, T1, T2>(
            Func<string, Resolver<TOut>, TOut> constructor,
            [CanBeNull] T1 operand1,
            [ItemCanBeNull] IEnumerable<T2> operands,
            Func<T1, IEnumerable<T2>, string> stringRepresentation)
            where T1 : class, IResolvable<T1>
            where T2 : class, IResolvable<T2>
        {
            var os = operands.ToList();

            TOut Resolve(ResolveContext context) =>
                Create(constructor, stringRepresentation(
                    operand1?.Resolve(context),
                    os.Select(o => o?.Resolve(context))));

            return Create(constructor, stringRepresentation(operand1, os), Resolve);
        }

        public static T1 Create<T1, T2>(
            Func<string, Resolver<T1>, T1> constructor,
            [CanBeNull] T1 operand1,
            [ItemCanBeNull] IEnumerable<T2> operands,
            Func<T1, IEnumerable<T2>, string> stringRepresentation)
            where T1 : class, IResolvable<T1>
            where T2 : class, IResolvable<T2>
        {
            var os = operands.ToList();

            T1 Resolve(ResolveContext context) =>
                Create(constructor, stringRepresentation(
                    operand1?.Resolve(context),
                    os.Select(o => o?.Resolve(context))));

            return Create(constructor, stringRepresentation(operand1, os), Resolve);
        }

        public static T1 Create<T1, T2, T3>(
            Func<string, Resolver<T1>, T1> constructor,
            [CanBeNull] T1 operand1,
            [CanBeNull] T2 operand2,
            [ItemCanBeNull] IEnumerable<T3> operands,
            Func<T1, T2, IEnumerable<T3>, string> stringRepresentation)
            where T1 : class, IResolvable<T1>
            where T2 : class, IResolvable<T2>
            where T3 : class, IResolvable<T3>
        {
            var os = operands.ToList();

            T1 Resolve(ResolveContext context) =>
                Create(constructor, stringRepresentation(
                    operand1?.Resolve(context),
                    operand2?.Resolve(context),
                    os.Select(o => o?.Resolve(context))));

            return Create(constructor, stringRepresentation(operand1, operand2, os), Resolve);
        }

        private static T Create<T>(
            Func<string, Resolver<T>, T> constructor,
            string stringRepresentation)
        {
            return constructor(stringRepresentation, (current, _) => current);
        }

        private static T Create<T>(
            Func<string, Resolver<T>, T> constructor,
            string stringRepresentation,
            Func<ResolveContext, T> resolver)
        {
            return constructor(stringRepresentation, (_, context) => resolver(context));
        }
    }
}
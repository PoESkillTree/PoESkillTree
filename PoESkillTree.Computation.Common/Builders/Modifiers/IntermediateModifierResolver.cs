using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Parsing;

namespace PoESkillTree.Computation.Common.Builders.Modifiers
{
    /// <inheritdoc />
    public class IntermediateModifierResolver : IIntermediateModifierResolver
    {
        private readonly IModifierBuilder _builder;

        /// <param name="builder">An empty <see cref="IModifierBuilder"/> instance to build new 
        /// <see cref="IIntermediateModifier"/>s from.</param>
        public IntermediateModifierResolver(IModifierBuilder builder)
        {
            _builder = builder;
        }

        public IIntermediateModifier Resolve(IIntermediateModifier unresolved, ResolveContext context)
        {
            var entries = unresolved.Entries;
            return _builder
                .WithValues(Resolve(entries, e => e.Value, context))
                .WithForms(Resolve(entries, e => e.Form, context))
                .WithStats(Resolve(entries, e => e.Stat, context))
                .WithConditions(Resolve(entries, e => e.Condition, context))
                .WithValueConverter(v => unresolved.ValueConverter(v)?.Resolve(context))
                .WithStatConverter(s => unresolved.StatConverter(s)?.Resolve(context))
                .Build();
        }

        private static IReadOnlyList<T> Resolve<T>(
            IReadOnlyList<IntermediateModifierEntry> entries, Func<IntermediateModifierEntry, T> selector,
            ResolveContext context)
            where T: class, IResolvable<T>
        {
            var newEntries = new List<T>(entries.Count);
            foreach (var entry in entries)
            {
                newEntries.Add(selector(entry)?.Resolve(context));
            }
            return newEntries;
        }

        public IStatBuilder ResolveToReferencedBuilder(IIntermediateModifier unresolved, ResolveContext context)
        {
            if (unresolved.Entries.Count != 1)
                throw new ParseException(
                    $"Referenced matchers must have exactly one IntermediateModifierEntry, {unresolved.Entries.Count} given ({unresolved})");

            var entry = unresolved.Entries.Single();
            if (entry.Value != null)
                throw new ParseException($"Referenced matchers may not have values ({entry})");
            if (entry.Form != null)
                throw new ParseException($"Referenced matchers may not have forms ({entry})");
            if (entry.Stat == null)
                throw new ParseException($"Referenced matchers must have stats ({entry})");
            var stat = unresolved.StatConverter(entry.Stat);
            if (entry.Condition != null)
            {
                stat = stat.WithCondition(entry.Condition);
            }

            return stat.Resolve(context);
        }
    }
}
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
            return _builder
                .WithValues(unresolved.Entries.Select(e => e.Value?.Resolve(context)))
                .WithForms(unresolved.Entries.Select(e => e.Form?.Resolve(context)))
                .WithStats(unresolved.Entries.Select(e => e.Stat?.Resolve(context)))
                .WithConditions(unresolved.Entries.Select(e => e.Condition?.Resolve(context)))
                .WithValueConverter(v => unresolved.ValueConverter(v)?.Resolve(context))
                .WithStatConverter(s => unresolved.StatConverter(s)?.Resolve(context))
                .Build();
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
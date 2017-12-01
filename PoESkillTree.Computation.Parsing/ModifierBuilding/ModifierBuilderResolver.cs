using System.Linq;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.ModifierBuilding
{
    public interface IModifierBuilderResolver
    {
        IModifierBuilder Resolve(IModifierBuilder unresolvedBuilder, ResolveContext context);
        IStatBuilder ResolveToReferencedBuilder(IModifierBuilder unresolvedBuilder, ResolveContext context);
    }

    // TODO tests
    public class ModifierBuilderResolver : IModifierBuilderResolver
    {
        private readonly IModifierBuilder _builder;

        public ModifierBuilderResolver(IModifierBuilder builder)
        {
            _builder = builder;
        }

        public IModifierBuilder Resolve(IModifierBuilder unresolvedBuilder, ResolveContext context)
        {
            var oldResult = unresolvedBuilder.Build();
            return _builder
                .WithValues(oldResult.Entries.Select(e => e.Value?.Resolve(context)))
                .WithForms(oldResult.Entries.Select(e => e.Form?.Resolve(context)))
                .WithStats(oldResult.Entries.Select(e => e.Stat?.Resolve(context)))
                .WithConditions(oldResult.Entries.Select(e => e.Condition?.Resolve(context)))
                .WithValueConverter(v => oldResult.ValueConverter(v)?.Resolve(context))
                .WithStatConverter(s => oldResult.StatConverter(s)?.Resolve(context));
        }

        public IStatBuilder ResolveToReferencedBuilder(IModifierBuilder unresolvedBuilder, ResolveContext context)
        {
            var result = Resolve(unresolvedBuilder, context).Build();

            if (result.Entries.Count != 1)
                throw new ParseException(
                    $"Referenced matchers must have exactly one ModifierResultEntry, {result.Entries.Count} given ({result})");

            var entry = result.Entries.Single();
            if (entry.Value != null)
                throw new ParseException($"Referenced matchers may not have values ({entry})");
            if (entry.Form != null)
                throw new ParseException($"Referenced matchers may not have forms ({entry})");
            if (entry.Stat == null)
                throw new ParseException($"Referenced matchers must have stats ({entry})");
            var stat = result.StatConverter(entry.Stat);
            if (entry.Condition != null)
            {
                stat = stat.WithCondition(entry.Condition);
            }

            return stat;
        }
    }
}
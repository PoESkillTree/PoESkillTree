using System.Linq;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.ModifierBuilding
{
    public interface IModifierResultResolver
    {
        IModifierResult Resolve(IModifierResult unresolvedResult, ResolveContext context);
        IStatBuilder ResolveToReferencedBuilder(IModifierResult unresolvedResult, ResolveContext context);
    }

    public class ModifierResultResolver : IModifierResultResolver
    {
        private readonly IModifierBuilder _builder;

        public ModifierResultResolver(IModifierBuilder builder)
        {
            _builder = builder;
        }

        public IModifierResult Resolve(IModifierResult unresolvedResult, ResolveContext context)
        {
            return _builder
                .WithValues(unresolvedResult.Entries.Select(e => e.Value?.Resolve(context)))
                .WithForms(unresolvedResult.Entries.Select(e => e.Form?.Resolve(context)))
                .WithStats(unresolvedResult.Entries.Select(e => e.Stat?.Resolve(context)))
                .WithConditions(unresolvedResult.Entries.Select(e => e.Condition?.Resolve(context)))
                .WithValueConverter(v => unresolvedResult.ValueConverter(v)?.Resolve(context))
                .WithStatConverter(s => unresolvedResult.StatConverter(s)?.Resolve(context))
                .Build();
        }

        public IStatBuilder ResolveToReferencedBuilder(IModifierResult unresolvedResult, ResolveContext context)
        {
            if (unresolvedResult.Entries.Count != 1)
                throw new ParseException(
                    $"Referenced matchers must have exactly one ModifierResultEntry, {unresolvedResult.Entries.Count} given ({unresolvedResult})");

            var entry = unresolvedResult.Entries.Single();
            if (entry.Value != null)
                throw new ParseException($"Referenced matchers may not have values ({entry})");
            if (entry.Form != null)
                throw new ParseException($"Referenced matchers may not have forms ({entry})");
            if (entry.Stat == null)
                throw new ParseException($"Referenced matchers must have stats ({entry})");
            var stat = unresolvedResult.StatConverter(entry.Stat);
            if (entry.Condition != null)
            {
                stat = stat.WithCondition(entry.Condition);
            }

            return stat.Resolve(context);
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Parsing.Tests
{
    public static class ParserTestUtils
    {
        public static readonly ParseResult EmptyParseResult = ParseResult.Success(new Modifier[0]);

        public static bool AnyModifierHasIdentity(IEnumerable<Modifier> modifiers, string identity)
            => modifiers.Any(m => m.Stats.Any(s => s.Identity == identity));

        public static IValue GetValueForIdentity(IEnumerable<Modifier> modifiers, string identity)
            => GetFirstModifierWithIdentity(modifiers, identity).Value;

        public static Modifier GetFirstModifierWithIdentity(IEnumerable<Modifier> modifiers, string identity)
            => modifiers.First(m => m.Stats.First().Identity == identity);

        public static IEnumerable<IValue> GetValuesForIdentity(IEnumerable<Modifier> modifiers, string identity)
            => GetModifiersWithIdentity(modifiers, identity).Select(m => m.Value);

        public static IEnumerable<Modifier> GetModifiersWithIdentity(IEnumerable<Modifier> modifiers, string identity)
            => modifiers.Where(m => m.Stats.First().Identity == identity);

        public static IEnumerable<NodeValue?> Calculate(
            this IEnumerable<IValue> @this, IValueCalculationContext context)
            => @this.Select(v => v.Calculate(context));
    }
}
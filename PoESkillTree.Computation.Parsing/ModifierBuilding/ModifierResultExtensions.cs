using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.Computation.Parsing.ModifierBuilding
{
    public static class ModifierResultExtensions
    {
        public static IModifierResult Merge(IModifierResult left, IModifierResult right)
        {
            return null;
        }

        public static IModifierResult Aggregate(this IEnumerable<IModifierResult> results)
        {
            return results.Aggregate((IModifierResult) new EmptyModifierResult(), Merge);
        }

        public static IReadOnlyList<Modifier> Build(this IModifierResult result)
        {
            return (from entry in result.Entries
                    let stat = result.StatConverter(entry.Stat)
                    let value = result.ValueConverter(entry.Value)
                    select new Modifier(stat, entry.Form, value, entry.Condition))
                .ToList();
        }
    }
}
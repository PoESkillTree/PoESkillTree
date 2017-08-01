using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Providers.Actions;

namespace PoESkillTree.Computation.Data
{
    public class ActionMatchers : IReferencedMatchers<IActionProvider>
    {
        private IActionProviderFactory Action { get; }

        public ActionMatchers(IActionProviderFactory actionProviderFactory)
        {
            Action = actionProviderFactory;

            Matchers = CreateCollection().ToList();
        }

        public IReadOnlyList<(string regex, IActionProvider match)> Matchers { get; }

        private MatcherCollection<IActionProvider> CreateCollection() =>
            new MatcherCollection<IActionProvider>
            {
                { "kill(ed)?", Action.Kill },
                { "block(ed)?", Action.Block },
                { "hit", Action.Hit },
                { "critical strike", Action.CriticalStrike },
                { "non-critical strike", Action.NonCriticalStrike },
            };
    }
}
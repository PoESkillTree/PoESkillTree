using System.Collections.Generic;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Providers.Actions;

namespace PoESkillTree.Computation.Data
{
    public class ActionMatchers : ReferencedMatchersBase<IActionProvider>
    {
        private IActionProviderFactory Action { get; }

        public ActionMatchers(IActionProviderFactory actionProviderFactory)
        {
            Action = actionProviderFactory;
        }

        protected override IEnumerable<ReferencedMatcherData<IActionProvider>> CreateCollection() =>
            new ReferencedMatcherCollection<IActionProvider>
            {
                { "kill(ed)?", Action.Kill },
                { "block(ed)?", Action.Block },
                { "hit", Action.Hit },
                { "critical strike", Action.CriticalStrike },
                { "non-critical strike", Action.NonCriticalStrike },
            };
    }
}
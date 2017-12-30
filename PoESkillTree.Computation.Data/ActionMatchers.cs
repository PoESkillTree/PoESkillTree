using System.Collections.Generic;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Parsing.Builders.Actions;
using PoESkillTree.Computation.Parsing.Data;

namespace PoESkillTree.Computation.Data
{
    /// <summary>
    /// <see cref="IReferencedMatchers"/> implementation for <see cref="IActionBuilder"/>s.
    /// </summary>
    public class ActionMatchers : ReferencedMatchersBase<IActionBuilder>
    {
        private IActionBuilders Action { get; }

        public ActionMatchers(IActionBuilders actionBuilders)
        {
            Action = actionBuilders;
        }

        protected override IEnumerable<ReferencedMatcherData> CreateCollection() =>
            new ReferencedMatcherCollection<IActionBuilder>
            {
                { "kill(ed)?", Action.Kill },
                { "block(ed)?", Action.Block },
                { "hit", Action.Hit },
                { "critical strike", Action.CriticalStrike },
                { "non-critical strike", Action.NonCriticalStrike },
            };
    }
}
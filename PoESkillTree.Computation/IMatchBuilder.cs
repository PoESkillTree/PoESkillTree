using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Providers.Conditions;
using PoESkillTree.Computation.Providers.Forms;
using PoESkillTree.Computation.Providers.Stats;
using PoESkillTree.Computation.Providers.Values;

namespace PoESkillTree.Computation
{
    public interface IMatchBuilder
    {
        // All With methods return new IMatchBuilder instances

        IMatchBuilder WithForm(IFormProvider form);

        IMatchBuilder WithForms(IEnumerable<IFormProvider> forms);

        IMatchBuilder WithStat(IStatProvider stat);

        IMatchBuilder WithStats(IEnumerable<IStatProvider> stats);

        IMatchBuilder WithStatConverter(Func<IStatProvider, IStatProvider> converter);

        IMatchBuilder WithValue(ValueProvider value);

        IMatchBuilder WithValues(IEnumerable<ValueProvider> values);

        IMatchBuilder WithValueConverter(ValueFunc converter);

        IMatchBuilder WithCondition(IConditionProvider condition);

        IMatchBuilder WithConditions(IEnumerable<IConditionProvider> conditions);
    }
}
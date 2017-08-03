using System;
using PoESkillTree.Computation.Providers.Conditions;
using PoESkillTree.Computation.Providers.Forms;
using PoESkillTree.Computation.Providers.Stats;
using PoESkillTree.Computation.Providers.Values;

namespace PoESkillTree.Computation
{
    public interface IMatchBuilder
    {
        IMatchBuilder WithForm(IFormProvider form);

        IMatchBuilder WithStats(params IStatProvider[] stats);

        IMatchBuilder WithStatConverter(Func<IStatProvider, IStatProvider> converter);

        IMatchBuilder WithValues(params ValueProvider[] values);

        IMatchBuilder WithValueConverter(ValueFunc converter);

        IMatchBuilder WithCondition(IConditionProvider condition);
    }
}
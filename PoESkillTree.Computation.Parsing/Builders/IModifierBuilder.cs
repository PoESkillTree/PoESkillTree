using System;
using System.Collections.Generic;
using PoESkillTree.Common.Utils;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Forms;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.Builders
{
    public interface IModifierBuilder : IFactory<IModifier>
    {
        // All With methods return new IModifierBuilder instances

        IModifierBuilder WithForm(IFormBuilder form);

        IModifierBuilder WithForms(IEnumerable<IFormBuilder> forms);

        IModifierBuilder WithStat(IStatBuilder stat);

        IModifierBuilder WithStats(IEnumerable<IStatBuilder> stats);

        IModifierBuilder WithStatConverter(Func<IStatBuilder, IStatBuilder> converter);

        IModifierBuilder WithValue(ValueBuilder value);

        IModifierBuilder WithValues(IEnumerable<ValueBuilder> values);

        IModifierBuilder WithValueConverter(ValueFunc converter);

        IModifierBuilder WithCondition(IConditionBuilder condition);

        IModifierBuilder WithConditions(IEnumerable<IConditionBuilder> conditions);
    }
}
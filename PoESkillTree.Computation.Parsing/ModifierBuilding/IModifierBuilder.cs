using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Forms;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.ModifierBuilding
{
    public interface IModifierBuilder
    {
        // All With methods return new IModifierBuilder instances

        IModifierBuilder WithForm(IFormBuilder form);

        IModifierBuilder WithForms(IEnumerable<IFormBuilder> forms);

        IModifierBuilder WithStat(IStatBuilder stat);

        IModifierBuilder WithStats(IEnumerable<IStatBuilder> stats);

        IModifierBuilder WithStatConverter(Func<IStatBuilder, IStatBuilder> converter);

        IModifierBuilder WithValue(IValueBuilder value);

        IModifierBuilder WithValues(IEnumerable<IValueBuilder> values);

        IModifierBuilder WithValueConverter(Func<IValueBuilder, IValueBuilder> converter);

        IModifierBuilder WithCondition(IConditionBuilder condition);

        IModifierBuilder WithConditions(IEnumerable<IConditionBuilder> conditions);

        IModifierResult Build();
    }
}
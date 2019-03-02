using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Forms;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Common.Builders.Modifiers
{
    /// <summary>
    /// Fluent interface that constructs an <see cref="IIntermediateModifier"/> instance from builders and
    /// converters.
    /// </summary>
    /// <remarks>
    /// All methods are pure and implementations must be immutable, meaning all methods must return new instances 
    /// instead of mutating the current instance.
    /// <para>Each type of builder may only be set once. I.e. starting with an empty builder,
    /// only one method of WithX and WithXs may be called and that method may only be called once for each X.
    /// </para>
    /// <para>All calls to methods with an <see cref="IEnumerable{T}"/> parameter must be made with enumerables of the
    /// same size.
    /// </para>
    /// </remarks>
    public interface IModifierBuilder
    {
        IModifierBuilder WithForm(IFormBuilder form);

        IModifierBuilder WithForms(IReadOnlyList<IFormBuilder> forms);

        IModifierBuilder WithStat(IStatBuilder stat);

        IModifierBuilder WithStats(IReadOnlyList<IStatBuilder> stats);

        IModifierBuilder WithStatConverter(StatConverter converter);

        IModifierBuilder WithValue(IValueBuilder value);

        IModifierBuilder WithValues(IReadOnlyList<IValueBuilder> values);

        IModifierBuilder WithValueConverter(ValueConverter converter);

        IModifierBuilder WithCondition(IConditionBuilder condition);

        IModifierBuilder WithConditions(IReadOnlyList<IConditionBuilder> conditions);

        /// <summary>
        /// Builds this instance to an <see cref="IIntermediateModifier"/>.
        /// </summary>
        IIntermediateModifier Build();
    }
}
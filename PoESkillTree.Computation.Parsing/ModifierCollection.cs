using System.Collections;
using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.GameModel;

namespace PoESkillTree.Computation.Parsing
{
    /// <summary>
    /// Collection and factory for creating <see cref="Modifier"/>s
    /// </summary>
    public class ModifierCollection : IEnumerable<Modifier>
    {
        private readonly IBuilderFactories _builderFactories;
        private readonly ModifierSource.Local _localModifierSource;
        private readonly ModifierSource.Global _globalModifierSource;
        private readonly Entity _modifierSourceEntity;
        private readonly IModifierBuilder _modifierBuilder = new ModifierBuilder();
        private readonly List<Modifier> _modifiers = new List<Modifier>();

        public ModifierCollection(
            IBuilderFactories builderFactories, ModifierSource.Local localModifierSource,
            Entity modifierSourceEntity = Entity.Character)
        {
            (_builderFactories, _localModifierSource, _modifierSourceEntity) =
                (builderFactories, localModifierSource, modifierSourceEntity);
            _globalModifierSource = new ModifierSource.Global(_localModifierSource);
        }

        public IEnumerator<Modifier> GetEnumerator() => _modifiers.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void AddLocal(IStatBuilder stat, Form form, double value, IConditionBuilder condition = null)
            => AddLocal(stat, form, _builderFactories.ValueBuilders.Create(value), condition);

        public void AddLocal(IStatBuilder stat, Form form, IValueBuilder value, IConditionBuilder condition = null)
            => Add(stat, form, value, condition, _localModifierSource);

        public void AddGlobal(IStatBuilder stat, Form form, double value, IConditionBuilder condition = null)
            => AddGlobal(stat, form, _builderFactories.ValueBuilders.Create(value), condition);

        public void AddGlobal(IStatBuilder stat, Form form, bool value, IConditionBuilder condition = null)
            => AddGlobal(stat, form, _builderFactories.ValueBuilders.Create(value), condition);

        public void AddGlobal(IStatBuilder stat, Form form, IValueBuilder value, IConditionBuilder condition = null)
            => Add(stat, form, value, condition, _globalModifierSource);

        private void Add(
            IStatBuilder stat, Form form, IValueBuilder value, IConditionBuilder condition,
            ModifierSource modifierSource)
        {
            var builder = _modifierBuilder
                .WithStat(stat)
                .WithForm(_builderFactories.FormBuilders.From(form))
                .WithValue(value);
            if (condition != null)
                builder = builder.WithCondition(condition);
            var intermediateModifier = builder.Build();
            _modifiers.AddRange(intermediateModifier.Build(modifierSource, _modifierSourceEntity));
        }
    }
}
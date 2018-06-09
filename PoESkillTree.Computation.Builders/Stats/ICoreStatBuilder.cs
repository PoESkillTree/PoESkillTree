using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Computation.Builders.Stats
{
    public interface ICoreStatBuilder : IResolvable<ICoreStatBuilder>
    {
        ICoreStatBuilder WithEntity(IEntityBuilder entityBuilder);

        ICoreStatBuilder WithStatConverter(Func<IStat, IStat> statConverter);

        ICoreStatBuilder CombineWith(ICoreStatBuilder other);

        IValue BuildValue(Entity modifierSourceEntity);

        StatBuilderResult Build(ModifierSource originalModifierSource, Entity modifierSourceEntity);
    }

    public class StatBuilderResult
    {
        public StatBuilderResult(
            IReadOnlyList<IStat> stats, ModifierSource modifierSource, ValueConverter valueConverter)
        {
            Stats = stats;
            ModifierSource = modifierSource;
            ValueConverter = valueConverter;
        }

        public IReadOnlyList<IStat> Stats { get; }
        public ModifierSource ModifierSource { get; }
        public ValueConverter ValueConverter { get; }
    }
}
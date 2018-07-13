using System;
using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Buffs;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Stats;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class BuffBuilderStub : EffectBuilderStub, IBuffBuilder
    {
        public BuffBuilderStub(string stringRepresentation, Resolver<IEffectBuilder> resolver)
            : base(stringRepresentation, resolver)
        {
        }

        public IFlagStatBuilder NotAsBuffOn(IEntityBuilder target) =>
            CreateFlagStat(This, target, (o1, o2) => $"Apply {o1} to {o2} (not as buff)");

        public IStatBuilder Effect => CreateStat(This, o => $"Effect of {o}");

        public IActionBuilder Action =>
            Create<IActionBuilder, IEffectBuilder>(ActionBuilderStub.BySelf, this, b => $"{b} application");

        public string Build() => throw new NotImplementedException();
    }
}
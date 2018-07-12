using System;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Buffs;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Skills;
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


    public class BuffBuilderCollectionStub : BuilderCollectionStub<IBuffBuilder>,
        IBuffBuilderCollection
    {
        public BuffBuilderCollectionStub(
            string stringRepresentation, Resolver<IBuilderCollection> resolver)
            : base(new BuffBuilderStub("Buff", (current, _) => current), stringRepresentation, resolver)
        {
        }

        private IBuilderCollection This => this;

        private static IBuilderCollection Construct(
            string stringRepresentation, Resolver<IBuilderCollection> resolver)
        {
            return new BuffBuilderCollectionStub(stringRepresentation, resolver);
        }

        public IStatBuilder Effect => CreateStat(This, o => $"Effect of {o}");

        public IStatBuilder AddStat(IStatBuilder stat) =>
            CreateStat(This, stat, (o1, o2) => $"{o2} added to buffs {o1}");

        public IBuffBuilderCollection With(IKeywordBuilder keyword) =>
            (IBuffBuilderCollection) Create(
                Construct, This, keyword,
                (o1, o2) => $"{o1}.Where(has keyword {o2}]");

        public IBuffBuilderCollection Without(IKeywordBuilder keyword) =>
            (IBuffBuilderCollection) Create(
                Construct, This, keyword,
                (o1, o2) => $"{o1}.Where(does not have keyword {o2}]");
    }


    public class BuffBuildersStub : IBuffBuilders
    {
        private static IBuffBuilder Create(string stringRepresentation) =>
            new BuffBuilderStub(stringRepresentation, (current, _) => current);

        public IBuffBuilder Fortify => Create("Fortify");
        public IBuffBuilder Maim => Create("Maim");
        public IBuffBuilder Intimidate => Create("Intimidate");
        public IBuffBuilder Taunt => Create("Taunt");
        public IBuffBuilder Blind => Create("Blind");
        public IBuffBuilder Onslaught => Create("Onslaught");
        public IBuffBuilder UnholyMight => Create("UnholyMight");
        public IBuffBuilder Phasing => Create("Phasing");

        public IConfluxBuffBuilders Conflux => new ConfluxBuffBuilders();

        public IStatBuilder Temporary(IStatBuilder gainedStat) =>
            CreateFlagStat(gainedStat, o => $"Every x seconds, gain {o} for y seconds");

        public IStatBuilder Temporary<T>(IBuffBuilder buff, T condition) where T : struct, Enum =>
            CreateFlagStat((IEffectBuilder) buff,
                o => $"Every x seconds, gain {o} for y seconds " +
                     $"(as part of the rotation {typeof(T)} when {condition})");

        public IStatBuilder Aura(IStatBuilder gainedStat, params IEntityBuilder[] affectedEntites) =>
            CreateStat(gainedStat, affectedEntites, (o1, os) => $"{o1} as Aura affecting [{string.Join(", ", os)}]");

        public IBuffBuilderCollection Buffs(IEntityBuilder source = null, params IEntityBuilder[] targets) =>
            (IBuffBuilderCollection) Create<IBuilderCollection, IEntityBuilder, IEntityBuilder>(
                (s, r) => new BuffBuilderCollectionStub(s, r),
                source, targets,
                (s, ts) => $"All buffs by {s} against {string.Join(" or ", ts)}");

        public IStatBuilder CurseLimit => CreateStat("CurseLimit");


        private class ConfluxBuffBuilders : IConfluxBuffBuilders
        {
            public IBuffBuilder Igniting => Create("Igniting Conflux");

            public IBuffBuilder Shocking => Create("Shocking Conflux");

            public IBuffBuilder Chilling => Create("Chilling Conflux");

            public IBuffBuilder Elemental => Create("Elemental Conflux");
        }
    }
}
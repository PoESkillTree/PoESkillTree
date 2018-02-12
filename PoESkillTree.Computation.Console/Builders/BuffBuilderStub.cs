using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Buffs;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class BuffBuilderStub : EffectBuilderStub, IBuffBuilder
    {
        public BuffBuilderStub(string stringRepresentation, Resolver<IEffectBuilder> resolver)
            : base(stringRepresentation, resolver)
        {
        }

        public IStatBuilder Effect => CreateStat(This, o => $"Effect of {o}");

        public IActionBuilder Action =>
            Create<IActionBuilder, IEffectBuilder>(ActionBuilderStub.SelfToAny, this, b => $"{b} application");
    }


    public class BuffBuilderCollectionStub : BuilderCollectionStub<IBuffBuilder>,
        IBuffBuilderCollection
    {
        public BuffBuilderCollectionStub(
            string stringRepresentation, Resolver<IBuilderCollection<IBuffBuilder>> resolver)
            : base(new BuffBuilderStub("Buff", (current, _) => current), stringRepresentation, resolver)
        {
        }

        private IBuilderCollection<IBuffBuilder> This => this;

        private static IBuilderCollection<IBuffBuilder> Construct(
            string stringRepresentation, Resolver<IBuilderCollection<IBuffBuilder>> resolver)
        {
            return new BuffBuilderCollectionStub(stringRepresentation, resolver);
        }

        public IStatBuilder CombinedLimit => CreateStat(This, o => $"{o} combined limit");

        public IStatBuilder Effect => CreateStat(This, o => $"Effect of {o}");

        public IBuffBuilderCollection ExceptFrom(params ISkillBuilder[] skills) =>
            (IBuffBuilderCollection) Create(
                Construct, This, skills,
                (o1, os) => $"{o1}.Where(was not gained from [{string.Join(", ", os)}])");

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

        public IConfluxBuffBuilderFactory Conflux => new ConfluxBuffBuilderFactory();

        public IBuffBuilder Curse(ISkillBuilder skill, IValueBuilder level) =>
            (IBuffBuilder) Create<IEffectBuilder, ISkillBuilder, IValueBuilder>(
                (s, r) => new BuffBuilderStub(s, r),
                skill, level,
                (o1, o2) => $"Curse with level {o2} {o1}");

        public IBuffRotation Rotation(IValueBuilder duration) =>
            (IBuffRotation) Create<IStatBuilder, IValueBuilder>(
                (s, r) => new BuffRotation(s, r),
                duration, o => $"Buff rotation for {o} seconds");

        public IBuffBuilderCollection Buffs(IEntityBuilder source = null, IEntityBuilder target = null)
        {
            string StringRepresentation(IEntityBuilder s, IEntityBuilder t)
            {
                var str = "All buffs";
                if (source != null)
                {
                    str += " by " + source;
                }

                if (target != null)
                {
                    str += " against " + target;
                }

                return str;
            }

            return (IBuffBuilderCollection)
                Create<IBuilderCollection<IBuffBuilder>, IEntityBuilder, IEntityBuilder>(
                    (s, r) => new BuffBuilderCollectionStub(s, r),
                    source, target, StringRepresentation);
        }


        private class ConfluxBuffBuilderFactory : IConfluxBuffBuilderFactory
        {
            public IBuffBuilder Igniting => Create("Igniting Conflux");

            public IBuffBuilder Shocking => Create("Shocking Conflux");

            public IBuffBuilder Chilling => Create("Chilling Conflux");

            public IBuffBuilder Elemental => Create("Elemental Conflux");
        }


        private class BuffRotation : FlagStatBuilderStub, IBuffRotation
        {
            public BuffRotation(string stringRepresentation, Resolver<IStatBuilder> resolver)
                : base(stringRepresentation, resolver)
            {
            }

            public IBuffRotation Step(IValueBuilder duration, params IBuffBuilder[] buffs) =>
                (IBuffRotation) Create<IStatBuilder, IValueBuilder, IEffectBuilder>(
                    (s, r) => new BuffRotation(s, r),
                    this, duration, buffs,
                    (o1, o2, o3) => $"{o1} {{ {string.Join(", ", o3)} for {o2} seconds }}");
        }
    }
}
using System;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Equipment;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class SkillBuilderStub : BuilderStub, ISkillBuilder
    {
        private readonly Resolver<ISkillBuilder> _resolver;

        public SkillBuilderStub(string stringRepresentation, Resolver<ISkillBuilder> resolver)
            : base(stringRepresentation)
        {
            _resolver = resolver;
        }

        private ISkillBuilder This => this;

        public IActionBuilder Cast =>
            Create<IActionBuilder, ISkillBuilder>(
                ActionBuilderStub.SelfToAny,
                This, o => $"{o} cast");

        public IStatBuilder Instances =>
            CreateStat(This, o => $"{o} instance count");

        public IConditionBuilder HasInstance =>
            CreateCondition(This, o => $"{o} has any instances");

        public IStatBuilder Duration =>
            CreateStat(This, o => $"{o} duration");

        public IStatBuilder Cost =>
            CreateStat(This, o => $"{o} cost");

        public IStatBuilder Reservation =>
            CreateStat(This, o => $"{o} reservation");

        public IStatBuilder CooldownRecoverySpeed =>
            CreateStat(This, o => $"{o} cooldown recovery speed");

        public IStatBuilder DamageEffectiveness =>
            CreateStat(This, o => $"{o} effectiveness of added damage");

        public IStatBuilder Speed =>
            CreateStat(This, o => $"{o} cast/attack speed");

        public IStatBuilder AreaOfEffect =>
            CreateStat(This, o => $"{o} area of effect");

        public ISkillBuilder Resolve(ResolveContext context) =>
            _resolver(this, context);
    }


    public class SkillBuilderCollectionStub : BuilderCollectionStub<ISkillBuilder>,
        ISkillBuilderCollection
    {
        public SkillBuilderCollectionStub(
            string stringRepresentation, Resolver<IBuilderCollection<ISkillBuilder>> resolver)
            : base(new SkillBuilderStub("Skill", (c, _) => c), stringRepresentation, resolver)
        {
        }

        private IBuilderCollection<ISkillBuilder> This => this;

        public ISkillBuilderCollection this[params IKeywordBuilder[] keywords] =>
            (ISkillBuilderCollection) Create(
                (s, r) => new SkillBuilderCollectionStub(s, r),
                This, keywords,
                (o1, os) => $"{o1}.Where(has keywords [{string.Join(", ", os)}])");

        public ISkillBuilderCollection this[ItemSlot slot] =>
            (ISkillBuilderCollection) Create(
                (s, r) => new SkillBuilderCollectionStub(s, r),
                This,
                o => $"{o}.Where(is socketed in {slot})");

        public ISkillBuilderCollection this[IItemSlotBuilder slot] =>
            (ISkillBuilderCollection) Create(
                (s, r) => new SkillBuilderCollectionStub(s, r),
                This, slot,
                (o1, o2) => $"{o1}.Where(is socketed in {o2})");

        public ISkillBuilderCollection Where(Func<ISkillBuilder, IConditionBuilder> predicate) =>
            (ISkillBuilderCollection) Create(
                (s, r) => new SkillBuilderCollectionStub(s, r),
                This, predicate(DummyElement),
                (o1, o2) => $"{o1}.Where({o2})");

        public IStatBuilder CombinedInstances =>
            CreateStat(This, o => $"{o} combined instance count");

        public IStatBuilder Duration =>
            CreateStat(This, o => $"{o} duration");

        public IStatBuilder Cost =>
            CreateStat(This, o => $"{o} cost");

        public IStatBuilder Reservation =>
            CreateStat(This, o => $"{o} reservation");

        public IStatBuilder CooldownRecoverySpeed =>
            CreateStat(This, o => $"{o} cooldown recovery speed");

        public IStatBuilder DamageEffectiveness =>
            CreateStat(This, o => $"{o} damage effectiveness");

        public IStatBuilder Speed =>
            CreateStat(This, o => $"{o} attack/cast speed");

        public IStatBuilder AreaOfEffect =>
            CreateStat(This, o => $"{o} area of effect");

        public IFlagStatBuilder ApplyStatsToEntity(IEntityBuilder entity) =>
            CreateFlagStat(This, entity, (o1, o2) => $"apply stats of {o1} to {o2}");

        public IActionBuilder Cast =>
            Create<IActionBuilder, IBuilderCollection<ISkillBuilder>>(
                ActionBuilderStub.SelfToAny,
                This, o => $"{o} cast");
    }


    public class SkillBuildersStub : ISkillBuilders
    {
        private static ISkillBuilder Create(string s)
            => new SkillBuilderStub(s, (c, _) => c);

        public ISkillBuilderCollection Skills =>
            new SkillBuilderCollectionStub("Skills", (current, _) => current);

        public ISkillBuilderCollection Combine(params ISkillBuilder[] skills) =>
            (ISkillBuilderCollection) Create<IBuilderCollection<ISkillBuilder>, ISkillBuilder>(
                (s, r) => new SkillBuilderCollectionStub(s, r),
                skills,
                os => $"[{string.Join(", ", os)}]");

        public ISkillBuilder SummonSkeleton => Create("Summon Skeleton");

        public ISkillBuilder VaalSummonSkeletons => Create("Vaal Summon Skeletons");

        public ISkillBuilder RaiseSpectre => Create("Raise Spectre");

        public ISkillBuilder RaiseZombie => Create("Raise Zombie");

        public ISkillBuilder DetonateMines => Create("Detonate Mines");

        public ISkillBuilder BloodRage => Create("Blood Rage");

        public ISkillBuilder MoltenShell => Create("Molten Shell");

        public ISkillBuilder BoneOffering => Create("Bone Offering");

        public ISkillBuilder FleshOffering => Create("Flesh Offering");

        public ISkillBuilder SpiritOffering => Create("Spirit Offering");
    }
}
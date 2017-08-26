using System;
using System.Collections.Generic;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Parsing.Builders.Actions;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Equipment;
using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Console.Builders
{
    public class SkillBuilderStub : BuilderStub, ISkillBuilder
    {
        private readonly IConditionBuilders _conditionBuilders;

        public SkillBuilderStub(string stringRepresentation, IConditionBuilders conditionBuilders) 
            : base(stringRepresentation)
        {
            _conditionBuilders = conditionBuilders;
        }

        public IActionBuilder<ISelfBuilder, IEntityBuilder> Cast =>
            new SelfToAnyActionBuilderStub($"{this} cast", _conditionBuilders);

        public IStatBuilder Instances =>
            new StatBuilderStub($"{this} instance count", _conditionBuilders);

        public IConditionBuilder HasInstance =>
            new ConditionBuilderStub($"{this} has any instances");

        public IStatBuilder Duration =>
            new StatBuilderStub($"{this} duration", _conditionBuilders);

        public IStatBuilder Cost =>
            new StatBuilderStub($"{this} cost", _conditionBuilders);

        public IStatBuilder Reservation =>
            new StatBuilderStub($"{this} reservation", _conditionBuilders);

        public IStatBuilder CooldownRecoverySpeed =>
            new StatBuilderStub($"{this} cooldown recovery speed", _conditionBuilders);

        public IStatBuilder DamageEffectiveness =>
            new StatBuilderStub($"{this} effectiveness of added damage", _conditionBuilders);

        public IStatBuilder Speed =>
            new StatBuilderStub($"{this} cast/attack speed", _conditionBuilders);

        public IStatBuilder AreaOfEffect =>
            new StatBuilderStub($"{this} area of effect", _conditionBuilders);
    }


    public class SkillBuilderCollectionStub : BuilderCollectionStub<ISkillBuilder>, 
        ISkillBuilderCollection
    {
        public SkillBuilderCollectionStub(IReadOnlyList<ISkillBuilder> elements,
            IConditionBuilders conditionBuilders) 
            : base(elements, conditionBuilders)
        {
        }

        private SkillBuilderCollectionStub(SkillBuilderCollectionStub source,
            string stringRepresentation)
            : base(source, stringRepresentation)
        {
            
        }

        public ISkillBuilderCollection this[params IKeywordBuilder[] keywords] =>
            new SkillBuilderCollectionStub(this, 
                $"{this}.Where(has keywords [{string.Join<IKeywordBuilder>(", ", keywords)}])");

        public ISkillBuilderCollection this[ItemSlot slot] =>
            new SkillBuilderCollectionStub(this,
                $"{this}.Where(is socketed in {slot})");

        public ISkillBuilderCollection this[IItemSlotBuilder slot] =>
            new SkillBuilderCollectionStub(this,
                $"{this}.Where(is socketed in {slot})");

        public IStatBuilder CombinedInstances =>
            new StatBuilderStub($"{this} combined instance count", ConditionBuilders);

        public IStatBuilder Duration =>
            new StatBuilderStub($"{this} duration", ConditionBuilders);

        public IStatBuilder Cost =>
            new StatBuilderStub($"{this} cost", ConditionBuilders);

        public IStatBuilder Reservation =>
            new StatBuilderStub($"{this} reservation", ConditionBuilders);

        public IStatBuilder CooldownRecoverySpeed =>
            new StatBuilderStub($"{this} cooldown recovery speed", ConditionBuilders);

        public IStatBuilder DamageEffectiveness =>
            new StatBuilderStub($"{this} damage effectiveness", ConditionBuilders);

        public IStatBuilder Speed =>
            new StatBuilderStub($"{this} attack/cast speed", ConditionBuilders);

        public IStatBuilder AreaOfEffect =>
            new StatBuilderStub($"{this} area of effect", ConditionBuilders);

        public IFlagStatBuilder ApplyStatsToEntity(IEntityBuilder entity) =>
            new FlagStatBuilderStub($"apply stats of {this} to {entity}", ConditionBuilders);

        public ISkillBuilderCollection Where(Func<ISkillBuilder, IConditionBuilder> predicate) =>
            new SkillBuilderCollectionStub(this,
                $"{this}.Where({predicate(new SkillBuilderStub("skill", ConditionBuilders))})");

        public IActionBuilder<ISelfBuilder, IEntityBuilder> Cast =>
            new SelfToAnyActionBuilderStub($"{this} cast", ConditionBuilders);
    }


    public class SkillBuildersStub : ISkillBuilders
    {
        private readonly IConditionBuilders _conditionBuilders;

        public SkillBuildersStub(IConditionBuilders conditionBuilders)
        {
            _conditionBuilders = conditionBuilders;

            ISkillBuilder[] skills =
            {
                new SkillBuilderStub("skill1", _conditionBuilders),
                new SkillBuilderStub("skill2", _conditionBuilders),
                new SkillBuilderStub("skill3", _conditionBuilders),
                new SkillBuilderStub("...", _conditionBuilders),
            };
            Skills = new SkillBuilderCollectionStub(skills, _conditionBuilders);
        }

        public ISkillBuilderCollection Skills { get; }

        public ISkillBuilderCollection Combine(params ISkillBuilder[] skills) =>
            new SkillBuilderCollectionStub(skills, _conditionBuilders);

        public ISkillBuilder SummonSkeleton =>
            new SkillBuilderStub("Summon Skeleton", _conditionBuilders);

        public ISkillBuilder VaalSummonSkeletons =>
            new SkillBuilderStub("Vaal Summon Skeletons", _conditionBuilders);

        public ISkillBuilder RaiseSpectre =>
            new SkillBuilderStub("Raise Spectre", _conditionBuilders);

        public ISkillBuilder RaiseZombie =>
            new SkillBuilderStub("Raise Zombie", _conditionBuilders);

        public ISkillBuilder DetonateMines =>
            new SkillBuilderStub("Detonate Mines", _conditionBuilders);

        public ISkillBuilder BloodRage =>
            new SkillBuilderStub("Blood Rage", _conditionBuilders);

        public ISkillBuilder MoltenShell =>
            new SkillBuilderStub("Molten Shell", _conditionBuilders);

        public ISkillBuilder BoneOffering =>
            new SkillBuilderStub("Bone Offering", _conditionBuilders);

        public ISkillBuilder FleshOffering =>
            new SkillBuilderStub("Flesh Offering", _conditionBuilders);

        public ISkillBuilder SpiritOffering =>
            new SkillBuilderStub("Spirit Offering", _conditionBuilders);
    }
}
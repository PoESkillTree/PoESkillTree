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
        public SkillBuilderStub(string stringRepresentation) 
            : base(stringRepresentation)
        {
        }

        public IActionBuilder<ISelfBuilder, IEntityBuilder> Cast =>
            new SelfToAnyActionBuilderStub($"{this} cast");

        public IStatBuilder Instances =>
            new StatBuilderStub($"{this} instance count");

        public IConditionBuilder HasInstance =>
            new ConditionBuilderStub($"{this} has any instances");

        public IStatBuilder Duration =>
            new StatBuilderStub($"{this} duration");

        public IStatBuilder Cost =>
            new StatBuilderStub($"{this} cost");

        public IStatBuilder Reservation =>
            new StatBuilderStub($"{this} reservation");

        public IStatBuilder CooldownRecoverySpeed =>
            new StatBuilderStub($"{this} cooldown recovery speed");

        public IStatBuilder DamageEffectiveness =>
            new StatBuilderStub($"{this} effectiveness of added damage");

        public IStatBuilder Speed =>
            new StatBuilderStub($"{this} cast/attack speed");

        public IStatBuilder AreaOfEffect =>
            new StatBuilderStub($"{this} area of effect");
    }


    public class SkillBuilderCollectionStub : BuilderCollectionStub<ISkillBuilder>, 
        ISkillBuilderCollection
    {
        public SkillBuilderCollectionStub(IReadOnlyList<ISkillBuilder> elements) 
            : base(elements)
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
            new StatBuilderStub($"{this} combined instance count");

        public IStatBuilder Duration =>
            new StatBuilderStub($"{this} duration");

        public IStatBuilder Cost =>
            new StatBuilderStub($"{this} cost");

        public IStatBuilder Reservation =>
            new StatBuilderStub($"{this} reservation");

        public IStatBuilder CooldownRecoverySpeed =>
            new StatBuilderStub($"{this} cooldown recovery speed");

        public IStatBuilder DamageEffectiveness =>
            new StatBuilderStub($"{this} damage effectiveness");

        public IStatBuilder Speed =>
            new StatBuilderStub($"{this} attack/cast speed");

        public IStatBuilder AreaOfEffect =>
            new StatBuilderStub($"{this} area of effect");

        public IFlagStatBuilder ApplyStatsToEntity(IEntityBuilder entity) =>
            new FlagStatBuilderStub($"apply stats of {this} to {entity}");

        public ISkillBuilderCollection Where(Func<ISkillBuilder, IConditionBuilder> predicate) =>
            new SkillBuilderCollectionStub(this,
                $"{this}.Where({predicate(new SkillBuilderStub("skill"))})");

        public IActionBuilder<ISelfBuilder, IEntityBuilder> Cast =>
            new SelfToAnyActionBuilderStub($"{this} cast");
    }


    public class SkillBuildersStub : ISkillBuilders
    {
        public SkillBuildersStub()
        {
            ISkillBuilder[] skills =
            {
                new SkillBuilderStub("skill1"),
                new SkillBuilderStub("skill2"),
                new SkillBuilderStub("skill3"),
                new SkillBuilderStub("..."),
            };
            Skills = new SkillBuilderCollectionStub(skills);
        }

        public ISkillBuilderCollection Skills { get; }

        public ISkillBuilderCollection Combine(params ISkillBuilder[] skills) =>
            new SkillBuilderCollectionStub(skills);

        public ISkillBuilder SummonSkeleton =>
            new SkillBuilderStub("Summon Skeleton");

        public ISkillBuilder VaalSummonSkeletons =>
            new SkillBuilderStub("Vaal Summon Skeletons");

        public ISkillBuilder RaiseSpectre =>
            new SkillBuilderStub("Raise Spectre");

        public ISkillBuilder RaiseZombie =>
            new SkillBuilderStub("Raise Zombie");

        public ISkillBuilder DetonateMines =>
            new SkillBuilderStub("Detonate Mines");

        public ISkillBuilder BloodRage =>
            new SkillBuilderStub("Blood Rage");

        public ISkillBuilder MoltenShell =>
            new SkillBuilderStub("Molten Shell");

        public ISkillBuilder BoneOffering =>
            new SkillBuilderStub("Bone Offering");

        public ISkillBuilder FleshOffering =>
            new SkillBuilderStub("Flesh Offering");

        public ISkillBuilder SpiritOffering =>
            new SkillBuilderStub("Spirit Offering");
    }
}
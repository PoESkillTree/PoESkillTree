using System.Collections.Generic;
using PoESkillTree.Computation.Parsing.Builders.Actions;
using PoESkillTree.Computation.Parsing.Builders.Buffs;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Console.Builders
{
    public class BuffBuilderStub : EffectBuilderStub, IBuffBuilder
    {
        public BuffBuilderStub(string stringRepresentation,
            IConditionBuilders conditionBuilders) : base(stringRepresentation, conditionBuilders)
        {
        }

        public IStatBuilder Effect =>
            new StatBuilderStub($"Effect of {this}", ConditionBuilders);

        public IActionBuilder<ISelfBuilder, IEntityBuilder> Action =>
            new ActionBuilderStub<ISelfBuilder, IEntityBuilder>(
                new SelfBuilderStub(ConditionBuilders), 
                new EntityBuilderStub("Any Entity", ConditionBuilders), 
                $"{this} application", ConditionBuilders);
    }


    public class BuffBuilderCollectionStub : BuilderCollectionStub<IBuffBuilder>, 
        IBuffBuilderCollection
    {
        public BuffBuilderCollectionStub(IReadOnlyList<IBuffBuilder> elements, 
            IConditionBuilders conditionBuilders) : base(elements, conditionBuilders)
        {
        }

        private BuffBuilderCollectionStub(BuilderCollectionStub<IBuffBuilder> source, 
            string stringRepresentation) : base(source, stringRepresentation)
        {
        }

        public IStatBuilder CombinedLimit =>
            new StatBuilderStub($"{this} combined limit", ConditionBuilders);

        public IStatBuilder Effect =>
            new StatBuilderStub($"Effect of {this}", ConditionBuilders);

        public IBuffBuilderCollection ExceptFrom(params ISkillBuilder[] skills) =>
            new BuffBuilderCollectionStub(this,
                $"{this}.Where(was not gained from [{string.Join<ISkillBuilder>(", ", skills)}])");

        public IBuffBuilderCollection With(IKeywordBuilder keyword) =>
            new BuffBuilderCollectionStub(this, $"{this}.Where(has keyword {keyword})");

        public IBuffBuilderCollection Without(IKeywordBuilder keyword) =>
            new BuffBuilderCollectionStub(this, $"{this}.Where(does not have keyword {keyword})");
    }


    public class BuffBuildersStub : IBuffBuilders
    {
        private readonly IConditionBuilders _conditionBuilders;

        public BuffBuildersStub(IConditionBuilders conditionBuilders)
        {
            _conditionBuilders = conditionBuilders;
        }

        public IBuffBuilder Fortify => new BuffBuilderStub("Fortify", _conditionBuilders);
        public IBuffBuilder Maim => new BuffBuilderStub("Maim", _conditionBuilders);
        public IBuffBuilder Intimidate => new BuffBuilderStub("Intimidate", _conditionBuilders);
        public IBuffBuilder Taunt => new BuffBuilderStub("Taunt", _conditionBuilders);
        public IBuffBuilder Blind => new BuffBuilderStub("Blind", _conditionBuilders);

        public IConfluxBuffBuilderFactory Conflux =>
            new ConfluxBuffBuilderFactory(_conditionBuilders);

        public IBuffBuilder Curse(ISkillBuilder skill, ValueBuilder level) =>
            new BuffBuilderStub($"Curse with level {level} {skill}", _conditionBuilders);

        public IBuffRotation Rotation(ValueBuilder duration) =>
            new BuffRotation($"Buff rotation for {duration} seconds:", _conditionBuilders);

        public IBuffBuilderCollection Buffs(IEntityBuilder source = null,
            IEntityBuilder target = null)
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
            var buff = new BuffBuilderStub(str, _conditionBuilders);
            return new BuffBuilderCollectionStub(new[] { buff }, _conditionBuilders);
        }


        private class ConfluxBuffBuilderFactory : IConfluxBuffBuilderFactory
        {
            private readonly IConditionBuilders _conditionBuilders;

            public ConfluxBuffBuilderFactory(IConditionBuilders conditionBuilders)
            {
                _conditionBuilders = conditionBuilders;
            }

            public IBuffBuilder Igniting =>
                new BuffBuilderStub("Igniting Conflux", _conditionBuilders);

            public IBuffBuilder Shocking =>
                new BuffBuilderStub("Shocking Conflux", _conditionBuilders);

            public IBuffBuilder Chilling =>
                new BuffBuilderStub("Chilling Conflux", _conditionBuilders);

            public IBuffBuilder Elemental =>
                new BuffBuilderStub("Elemental Conflux", _conditionBuilders);
        }


        private class BuffRotation : FlagStatBuilderStub, IBuffRotation
        {
            public BuffRotation(string stringRepresentation, 
                IConditionBuilders conditionBuilders) 
                : base(stringRepresentation, conditionBuilders)
            {
            }

            public IBuffRotation Step(ValueBuilder duration, params IBuffBuilder[] buffs)
            {
                var str = $"{string.Join<IBuffBuilder>(", ", buffs)} for {duration} seconds";
                return new BuffRotation(this + " { " + str + " }", ConditionBuilders);
            }
        }
    }
}
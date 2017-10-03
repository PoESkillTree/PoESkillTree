using System.Collections.Generic;
using PoESkillTree.Computation.Parsing.Builders.Actions;
using PoESkillTree.Computation.Parsing.Builders.Buffs;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Console.Builders
{
    public class BuffBuilderStub : EffectBuilderStub, IBuffBuilder
    {
        public BuffBuilderStub(string stringRepresentation) : base(stringRepresentation)
        {
        }

        public IStatBuilder Effect =>
            new StatBuilderStub($"Effect of {this}");

        public IActionBuilder<ISelfBuilder, IEntityBuilder> Action =>
            new ActionBuilderStub<ISelfBuilder, IEntityBuilder>(
                new SelfBuilderStub(), 
                new EntityBuilderStub("Any Entity"), 
                $"{this} application");
    }


    public class BuffBuilderCollectionStub : BuilderCollectionStub<IBuffBuilder>, 
        IBuffBuilderCollection
    {
        public BuffBuilderCollectionStub(IReadOnlyList<IBuffBuilder> elements) : base(elements)
        {
        }

        private BuffBuilderCollectionStub(BuilderCollectionStub<IBuffBuilder> source, 
            string stringRepresentation) : base(source, stringRepresentation)
        {
        }

        public IStatBuilder CombinedLimit =>
            new StatBuilderStub($"{this} combined limit");

        public IStatBuilder Effect =>
            new StatBuilderStub($"Effect of {this}");

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
        public IBuffBuilder Fortify => new BuffBuilderStub("Fortify");
        public IBuffBuilder Maim => new BuffBuilderStub("Maim");
        public IBuffBuilder Intimidate => new BuffBuilderStub("Intimidate");
        public IBuffBuilder Taunt => new BuffBuilderStub("Taunt");
        public IBuffBuilder Blind => new BuffBuilderStub("Blind");

        public IConfluxBuffBuilderFactory Conflux => new ConfluxBuffBuilderFactory();

        public IBuffBuilder Curse(ISkillBuilder skill, ValueBuilder level) =>
            new BuffBuilderStub($"Curse with level {level} {skill}");

        public IBuffRotation Rotation(ValueBuilder duration) =>
            new BuffRotation($"Buff rotation for {duration} seconds:");

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
            var buff = new BuffBuilderStub(str);
            return new BuffBuilderCollectionStub(new[] { buff });
        }


        private class ConfluxBuffBuilderFactory : IConfluxBuffBuilderFactory
        {
            public IBuffBuilder Igniting =>
                new BuffBuilderStub("Igniting Conflux");

            public IBuffBuilder Shocking =>
                new BuffBuilderStub("Shocking Conflux");

            public IBuffBuilder Chilling =>
                new BuffBuilderStub("Chilling Conflux");

            public IBuffBuilder Elemental =>
                new BuffBuilderStub("Elemental Conflux");
        }


        private class BuffRotation : FlagStatBuilderStub, IBuffRotation
        {
            public BuffRotation(string stringRepresentation) 
                : base(stringRepresentation)
            {
            }

            public IBuffRotation Step(ValueBuilder duration, params IBuffBuilder[] buffs)
            {
                var str = $"{string.Join<IBuffBuilder>(", ", buffs)} for {duration} seconds";
                return new BuffRotation(this + " { " + str + " }");
            }
        }
    }
}
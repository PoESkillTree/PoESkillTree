using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Equipment;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Skills;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class ConditionBuilderStub : BuilderStub, IConditionBuilder
    {
        private readonly Resolver<IConditionBuilder> _resolver;

        public ConditionBuilderStub(string stringRepresentation, Resolver<IConditionBuilder> resolver) 
            : base(stringRepresentation)
        {
            _resolver = resolver;
        }

        private IConditionBuilder This => this;

        public IConditionBuilder And(IConditionBuilder condition) =>
            CreateCondition(This, condition, (l, r) => $"{l} and {r}");

        public IConditionBuilder Or(IConditionBuilder condition) =>
            CreateCondition(This, condition, (l, r) => $"{l} or {r}");

        public IConditionBuilder Not =>
            CreateCondition(This, o => $"not {o}");

        public IConditionBuilder Resolve(ResolveContext context) =>
            _resolver(this, context);
    }


    public class ConditionBuildersStub : IConditionBuilders
    {
        public IConditionBuilder WhileLeeching =>
            CreateCondition("While Leeching");

        public IConditionBuilder With(ISkillBuilderCollection skills) =>
            CreateCondition((IBuilderCollection<ISkillBuilder>) skills, o => $"With {o}");

        public IConditionBuilder With(ISkillBuilder skill) =>
            CreateCondition(skill, o => $"With {o}");

        public IConditionBuilder For(params IEntityBuilder[] entities) =>
            CreateCondition(entities, os => "For " + string.Join(", ", os));

        public IConditionBuilder BaseValueComesFrom(IEquipmentBuilder equipment) =>
            CreateCondition(equipment, o => $"If base value comes from {o}");

        public IConditionBuilder Unique(string name = "$0") =>
            CreateCondition(name);

        public IConditionBuilder True =>
            CreateCondition("unconditional");
    }
}
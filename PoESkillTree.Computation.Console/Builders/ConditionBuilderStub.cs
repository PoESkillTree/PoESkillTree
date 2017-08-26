using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Equipment;
using PoESkillTree.Computation.Parsing.Builders.Skills;

namespace PoESkillTree.Computation.Console.Builders
{
    public class ConditionBuilderStub : BuilderStub, IConditionBuilder
    {
        public ConditionBuilderStub(string stringRepresentation) : base(stringRepresentation)
        {
        }
    }


    public class ConditionBuildersStub : IConditionBuilders
    {
        public IConditionBuilder WhileLeeching => 
            new ConditionBuilderStub("While Leeching");

        public IConditionBuilder With(ISkillBuilderCollection skills) =>
            new ConditionBuilderStub("With " + skills);

        public IConditionBuilder With(ISkillBuilder skill) =>
            new ConditionBuilderStub("With " + skill);

        public IConditionBuilder For(params IEntityBuilder[] entities) => 
            new ConditionBuilderStub("For " + string.Join<IEntityBuilder>(", ", entities));

        public IConditionBuilder BaseValueComesFrom(IEquipmentBuilder equipment) =>
            new ConditionBuilderStub("If base value comes from " + equipment);

        public IConditionBuilder Unique(string name = "$0") =>
            new ConditionBuilderStub(name);

        public IConditionBuilder And(params IConditionBuilder[] conditions) =>
            new ConditionBuilderStub(string.Join<IConditionBuilder>(" and ", conditions));

        public IConditionBuilder Or(params IConditionBuilder[] conditions) =>
            new ConditionBuilderStub(string.Join<IConditionBuilder>(" or ", conditions));

        public IConditionBuilder Not(IConditionBuilder condition) =>
            new ConditionBuilderStub("not " + condition);

        public IConditionBuilder True =>
            new ConditionBuilderStub("unconditional");
    }
}
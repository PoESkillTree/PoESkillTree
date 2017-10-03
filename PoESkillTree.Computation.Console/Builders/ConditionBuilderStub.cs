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

        public IConditionBuilder And(IConditionBuilder condition) => 
            new ConditionBuilderStub(this + " and " + condition);

        public IConditionBuilder Or(IConditionBuilder condition) => 
            new ConditionBuilderStub(this + " or " + condition);

        public IConditionBuilder Not => 
            new ConditionBuilderStub("not " + this);
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

        public IConditionBuilder True =>
            new ConditionBuilderStub("unconditional");
    }
}
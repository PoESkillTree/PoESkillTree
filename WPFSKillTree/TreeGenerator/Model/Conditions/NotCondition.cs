namespace POESKillTree.TreeGenerator.Model.Conditions
{
    public class NotCondition : ICondition
    {
        public ICondition Condition { get; private set; }

        public NotCondition(ICondition condition)
        {
            Condition = condition;
        }
        
        public bool Eval(ConditionSettings settings, params object[] placeholder)
        {
            return !Condition.Eval(settings, placeholder);
        }
    }

}
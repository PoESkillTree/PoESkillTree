namespace POESKillTree.TreeGenerator.Model.PseudoAttributes
{
    public class NotCondition : ICondition
    {
        public ICondition Condition { get; private set; }

        public NotCondition(ICondition condition)
        {
            Condition = condition;
        }
        
        public bool Eval(ConditionSettings settings, params string[] placeholder)
        {
            return !Condition.Eval(settings, placeholder);
        }
    }

}
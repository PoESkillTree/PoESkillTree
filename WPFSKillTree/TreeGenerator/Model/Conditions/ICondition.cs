namespace POESKillTree.TreeGenerator.Model.Conditions
{
    public interface ICondition
    {
        bool Eval(ConditionSettings settings, params object[] placeholder);
    }
}
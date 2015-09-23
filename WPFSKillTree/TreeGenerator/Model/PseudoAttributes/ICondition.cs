namespace POESKillTree.TreeGenerator.Model.PseudoAttributes
{
    public interface ICondition
    {
        bool Eval(ConditionSettings settings, params string[] placeholder);
    }
}
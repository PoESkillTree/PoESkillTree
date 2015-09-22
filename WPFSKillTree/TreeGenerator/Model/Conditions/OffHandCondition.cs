namespace POESKillTree.TreeGenerator.Model.Conditions
{
    public class OffHandCondition : ICondition
    {
        public string Alias { get; private set; }

        public OffHandCondition(string alias)
        {
            Alias = alias;
        }
        
        public bool Eval(ConditionSettings settings, params object[] placeholder)
        {
            return settings.OffHand.HasAlias(string.Format(Alias, placeholder));
        }
    }
}
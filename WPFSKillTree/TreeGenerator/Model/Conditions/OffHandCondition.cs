namespace POESKillTree.TreeGenerator.Model.Conditions
{
    public class OffHandCondition : ICondition
    {
        public string OffHandAlias { get; set; }
        
        public bool Eval(ConditionSettings settings, params object[] placeholder)
        {
            return settings.OffHand.HasAlias(string.Format(OffHandAlias, placeholder));
        }
    }
}
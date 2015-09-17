namespace POESKillTree.TreeGenerator.Model.Conditions
{
    public class WeaponClassCondition : ICondition
    {
        public string WeaponClassAlias { get; set; }
        
        public bool Eval(ConditionSettings settings, params object[] placeholder)
        {
            return settings.WeaponClass.HasAlias(string.Format(WeaponClassAlias, placeholder));
        }
    }
}
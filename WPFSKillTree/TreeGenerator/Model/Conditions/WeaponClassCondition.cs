namespace POESKillTree.TreeGenerator.Model.Conditions
{
    public class WeaponClassCondition : ICondition
    {
        public string Alias { get; private set; }

        public WeaponClassCondition(string alias)
        {
            Alias = alias;
        }
        
        public bool Eval(ConditionSettings settings, params object[] placeholder)
        {
            return settings.WeaponClass.HasAlias(string.Format(Alias, placeholder));
        }
    }
}
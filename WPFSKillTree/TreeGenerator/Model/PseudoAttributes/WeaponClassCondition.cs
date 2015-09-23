namespace POESKillTree.TreeGenerator.Model.PseudoAttributes
{
    public class WeaponClassCondition : ICondition
    {
        public string Alias { get; private set; }

        public WeaponClassCondition(string alias)
        {
            Alias = alias;
        }
        
        public bool Eval(ConditionSettings settings, params string[] placeholder)
        {
            return settings.WeaponClass.HasAlias(string.Format(Alias, placeholder));
        }
    }
}
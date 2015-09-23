namespace POESKillTree.TreeGenerator.Model.PseudoAttributes
{
    public class TagCondition : ICondition
    {
        public string Alias { get; private set; }

        public TagCondition(string alias)
        {
            Alias = alias;
        }
        
        public bool Eval(ConditionSettings settings, params string[] placeholder)
        {
            return settings.Tags.HasAlias(string.Format(Alias, placeholder));
        }
    }
}
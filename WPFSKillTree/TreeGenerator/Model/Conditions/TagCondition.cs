namespace POESKillTree.TreeGenerator.Model.Conditions
{
    public class TagCondition : ICondition
    {
        public string TagAlias { get; set; }

        public bool Eval(ConditionSettings settings, params object[] placeholder)
        {
            return settings.Tags.HasAlias(string.Format(TagAlias, placeholder));
        }
    }
}
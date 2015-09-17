using System.Linq;

namespace POESKillTree.TreeGenerator.Model.Conditions
{
    public class KeystoneCondition : ICondition
    {
        public string Keystone { get; set; }

        public bool Eval(ConditionSettings settings, params object[] placeholder)
        {
            return settings.Keystones.Any(k => k == Keystone);
        }
    }
}
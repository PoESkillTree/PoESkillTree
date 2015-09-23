using System.Linq;

namespace POESKillTree.TreeGenerator.Model.PseudoAttributes
{
    public class KeystoneCondition : ICondition
    {
        public string Keystone { get; private set; }

        public KeystoneCondition(string keystone)
        {
            Keystone = keystone;
        }

        public bool Eval(ConditionSettings settings, params string[] placeholder)
        {
            return settings.Keystones.Any(k => k == Keystone);
        }
    }
}
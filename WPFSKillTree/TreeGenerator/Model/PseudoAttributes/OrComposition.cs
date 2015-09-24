using System.Collections.Generic;
using System.Linq;

namespace POESKillTree.TreeGenerator.Model.PseudoAttributes
{
    public class OrComposition : ICondition
    {
        public List<ICondition> Conditions { get; private set; }

        public OrComposition()
        {
            Conditions = new List<ICondition>();
        }

        public bool Eval(ConditionSettings settings, params string[] placeholder)
        {
            return Conditions.Count <= 0 || Conditions.Any(c => c.Eval(settings, placeholder));
        }
    }
}
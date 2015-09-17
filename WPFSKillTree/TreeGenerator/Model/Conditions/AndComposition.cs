using System.Collections.Generic;
using System.Linq;

namespace POESKillTree.TreeGenerator.Model.Conditions
{
    public class AndComposition : ICondition
    {
        public List<ICondition> Conditions { get; set; }

        public bool Eval(ConditionSettings settings, params object[] placeholder)
        {
            return Conditions.All(c => c.Eval(settings, placeholder));
        }
    }
}
using PoESkillTree.Computation.Providers.Forms;
using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation
{
    public class GivenStatData
    {
        public GivenStatData(IFormProvider form, IStatProvider stat, double value)
        {
            Form = form;
            Stat = stat;
            Value = value;
        }

        public IFormProvider Form { get; }

        public IStatProvider Stat { get; }

        public double Value { get; }
    }
}
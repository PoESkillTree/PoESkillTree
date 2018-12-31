using NUnit.Framework;
using PoESkillTree.Computation.Console;

namespace PoESkillTree.Computation.IntegrationTests
{
    public abstract class CompositionRootTestBase
    {
        protected static CompositionRoot CompositionRoot { get; private set; }

        [OneTimeSetUp]
        public static void CreateCompositionRoot()
        {
            Program.SetupLogger();
            if (CompositionRoot is null)
            {
                CompositionRoot = new CompositionRoot();
            }
        }
    }
}
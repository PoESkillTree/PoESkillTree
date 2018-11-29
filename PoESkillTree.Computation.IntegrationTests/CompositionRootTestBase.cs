using NUnit.Framework;
using PoESkillTree.Computation.Console;

namespace PoESkillTree.Computation.IntegrationTests
{
    public abstract class CompositionRootTestBase
    {
        protected static AsyncCompositionRoot CompositionRoot { get; private set; }

        [OneTimeSetUp]
        public static void CreateCompositionRoot()
        {
            Program.SetupLogger();
            CompositionRoot = new AsyncCompositionRoot();
        }
    }
}
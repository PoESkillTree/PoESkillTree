using System.Threading.Tasks;
using NUnit.Framework;
using PoESkillTree.Computation.Console;

namespace PoESkillTree.Computation.IntegrationTests
{
    public abstract class CompositionRootTestBase
    {
        protected static CompositionRoot CompositionRoot { get; private set; }

        [OneTimeSetUp]
        public static async Task CreateCompositionRootAsync()
        {
            CompositionRoot = await CompositionRoot.CreateAsync().ConfigureAwait(false);
        }
    }
}
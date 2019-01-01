using System;
using System.Threading.Tasks;
using NUnit.Framework;
using PoESkillTree.Computation.Console;
using PoESkillTree.Computation.Data.Steps;
using PoESkillTree.Computation.Parsing;

namespace PoESkillTree.Computation.IntegrationTests
{
    public abstract class CompositionRootTestBase
    {
        private static Lazy<Task<ICoreParser>> _coreParser;

        protected static CompositionRoot CompositionRoot { get; private set; }
        protected static Task<ICoreParser> CoreParserTask => _coreParser.Value;

        [OneTimeSetUp]
        public static void CreateCompositionRoot()
        {
            Program.SetupLogger();
            if (CompositionRoot is null)
            {
                CompositionRoot = new CompositionRoot();
                _coreParser = new Lazy<Task<ICoreParser>>(CreateCoreParserAsync);
            }
        }

        private static async Task<ICoreParser> CreateCoreParserAsync()
            => new CoreParser<ParsingStep>(
                await CompositionRoot.ParsingData.ConfigureAwait(false),
                await CompositionRoot.BuilderFactories.ConfigureAwait(false));
    }
}
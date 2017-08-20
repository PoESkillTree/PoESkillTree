using NUnit.Framework;
using PoESkillTree.Computation.Parsing.Steps;

namespace PoESkillTree.Computation.Parsing.Tests.Steps
{
    [TestFixture]
    public class ValueConversionStepTest : ParsingStepTest
    {
        protected override IStep<ParsingStep, bool> Sut =>
            new ValueConversionStep();

        protected override IStep<ParsingStep, bool> ExpectedNextFalse =>
            new FormAndStatStep();

        protected override IStep<ParsingStep, bool> ExpectedNextTrue =>
            new FormAndStatStep();
    }
}
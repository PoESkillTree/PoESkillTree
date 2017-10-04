using NUnit.Framework;
using PoESkillTree.Computation.Parsing.Steps;

namespace PoESkillTree.Computation.Parsing.Tests.Steps
{
    [TestFixture]
    public class StatManipulationStepTest : ParsingStepTest
    {
        protected override IStep<ParsingStep, bool> Sut =>
            new StatManipulatorStep();

        protected override IStep<ParsingStep, bool> ExpectedNextFalse => 
            new ValueConversionStep();

        protected override IStep<ParsingStep, bool> ExpectedNextTrue =>
            new ValueConversionStep();
    }
}
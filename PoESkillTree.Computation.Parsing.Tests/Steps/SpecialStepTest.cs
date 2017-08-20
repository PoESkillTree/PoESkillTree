using NUnit.Framework;
using PoESkillTree.Computation.Parsing.Steps;

namespace PoESkillTree.Computation.Parsing.Tests.Steps
{
    [TestFixture]
    public class SpecialStepTest : ParsingStepTest
    {
        protected override IStep<ParsingStep, bool> Sut =>
            new SpecialStep();

        protected override IStep<ParsingStep, bool> ExpectedNextFalse => 
            new StatManipulationStep();

        protected override IStep<ParsingStep, bool> ExpectedNextTrue =>
            new CompletedStep<ParsingStep, bool>(true, ParsingStep.Invalid);
    }
}
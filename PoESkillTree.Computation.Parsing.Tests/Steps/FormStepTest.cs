using NUnit.Framework;
using PoESkillTree.Computation.Parsing.Steps;

namespace PoESkillTree.Computation.Parsing.Tests.Steps
{
    [TestFixture]
    public class FormStepTest : ParsingStepTest
    {
        protected override IStep<ParsingStep, bool> Sut =>
            new FormStep();

        protected override IStep<ParsingStep, bool> ExpectedNextFalse =>
            new CompletedStep<ParsingStep, bool>(false, ParsingStep.Invalid);

        protected override IStep<ParsingStep, bool> ExpectedNextTrue =>
            new GeneralStatStep();
    }
}
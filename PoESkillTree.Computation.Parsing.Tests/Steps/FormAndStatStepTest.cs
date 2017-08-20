using NUnit.Framework;
using PoESkillTree.Computation.Parsing.Steps;

namespace PoESkillTree.Computation.Parsing.Tests.Steps
{
    [TestFixture]
    public class FormAndStatStepTest : ParsingStepTest
    {
        protected override IStep<ParsingStep, bool> Sut =>
            new FormAndStatStep();

        protected override IStep<ParsingStep, bool> ExpectedNextFalse =>
            new FormStep();

        protected override IStep<ParsingStep, bool> ExpectedNextTrue =>
            new ConditionStep();
    }
}
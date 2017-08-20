using NUnit.Framework;
using PoESkillTree.Computation.Parsing.Steps;

namespace PoESkillTree.Computation.Parsing.Tests.Steps
{
    [TestFixture]
    public class GeneralStatStepTest : ParsingStepTest
    {
        protected override IStep<ParsingStep, bool> Sut =>
            new GeneralStatStep();

        protected override IStep<ParsingStep, bool> ExpectedNextFalse =>
            new DamageStatStep();

        protected override IStep<ParsingStep, bool> ExpectedNextTrue =>
            new ConditionStep();
    }
}
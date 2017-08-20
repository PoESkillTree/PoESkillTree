using NUnit.Framework;
using PoESkillTree.Computation.Parsing.Steps;

namespace PoESkillTree.Computation.Parsing.Tests.Steps
{
    [TestFixture]
    public class DamageStatStepTest : ParsingStepTest
    {
        protected override IStep<ParsingStep, bool> Sut =>
            new DamageStatStep();

        protected override IStep<ParsingStep, bool> ExpectedNextFalse =>
            new PoolStatStep();

        protected override IStep<ParsingStep, bool> ExpectedNextTrue =>
            new ConditionStep();
    }
}
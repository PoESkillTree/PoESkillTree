using System.Collections.Generic;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Skills;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Parsing;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Builders.Tests.Skills
{
    [TestFixture]
    public class SkillBuildersTest
    {
        [TestCaseSource(nameof(SuccessTestCases))]
        public void ModifierSourceSkillBuildsToCorrectSkill(ModifierSource modifierSource)
        {
            var sut = CreateSut();

            var builder = sut.ModifierSourceSkill;
            var actual = Build(builder, modifierSource);

            Assert.AreEqual(SkillDefinition, actual);
        }

        private static IEnumerable<ModifierSource> SuccessTestCases()
        {
            yield return new ModifierSource.Local.Skill(SkillId, "");
            yield return new ModifierSource.Global(new ModifierSource.Local.Skill(SkillId, ""));
            yield return new ModifierSource.Local.Gem(default, 0, SkillId, "");
        }

        [TestCaseSource(nameof(FailureTestCases))]
        public void ModifierSourceSkillBuildThrowsWithNonSkillSource(ModifierSource modifierSource)
        {
            var sut = CreateSut();

            var builder = sut.ModifierSourceSkill;

            Assert.Throws<ParseException>(() => Build(builder, modifierSource));
        }

        private static IEnumerable<ModifierSource> FailureTestCases()
        {
            yield return new ModifierSource.Local.Given(SkillId);
            yield return new ModifierSource.Global(new ModifierSource.Local.Item(default, SkillId));
        }

        private static SkillDefinition Build(ISkillBuilder builder, ModifierSource modifierSource)
        {
            var buildParameters = new BuildParameters(modifierSource, default, default);
            return builder.Build(buildParameters);
        }

        private static SkillBuilders CreateSut()
            => new SkillBuilders(new StatFactory(), new SkillDefinitions(new[] { SkillDefinition }));
        
        private static readonly SkillDefinition SkillDefinition =
            SkillDefinition.CreateActive(SkillId, 0, "", null, null, null, null);

        private const string SkillId = "skill";
    }
}
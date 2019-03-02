using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Forms;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Common.Parsing;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Parsing.Tests
{
    [TestFixture]
    public class CoreParserTest
    {
        [Test]
        public void ParseReturnsCorrectResultWhenSuccessful()
        {
            var builderFactories = Mock.Of<IBuilderFactories>();
            var parser = new CoreParser<Step>(new ParsingData(), builderFactories);

            var (failedLines, remaining, result) = parser.Parse("matched", ExpectedModifier.Source, default);

            Assert.IsEmpty(failedLines);
            Assert.IsEmpty(remaining);
            Assert.AreEqual(new[] { ExpectedModifier }, result);
        }

        [Test]
        public void ParseReturnsCorrectResultWhenFailing()
        {
            var builderFactories = Mock.Of<IBuilderFactories>();
            var parser = new CoreParser<Step>(new ParsingData(), builderFactories);

            var (failedLines, remaining, result) =
                parser.Parse("matched unmatched", ExpectedModifier.Source, default);

            Assert.AreEqual(new[] { "matched unmatched" }, failedLines);
            Assert.AreEqual(new[] { "unmatched" }, remaining);
            Assert.IsEmpty(result);
        }

        [Test]
        public void ParseReturnsCorrectResultWhenThrowing()
        {
            var builderFactories = Mock.Of<IBuilderFactories>();
            var parser = new CoreParser<Step>(new ParsingData(), builderFactories);

            var (failedLines, remaining, result) =
                parser.Parse("matched throwing", ExpectedModifier.Source, default);

            Assert.AreEqual(new[] { "matched throwing" }, failedLines);
            Assert.AreEqual(new[] { "matched throwing" }, remaining);
            Assert.IsEmpty(result);
        }

        private class ParsingData : IParsingData<Step>
        {
            public IReadOnlyList<IReferencedMatchers> ReferencedMatchers => new IReferencedMatchers[0];
            public IReadOnlyList<IStatMatchers> StatMatchers { get; } = new[] { new Matchers(), };
            public IReadOnlyList<StatReplacerData> StatReplacers => new StatReplacerData[0];
            public IReadOnlyList<IGivenStats> GivenStats { get; } = new IGivenStats[0];
            public IStepper<Step> Stepper { get; } = new Stepper();
            public IStatMatchers SelectStatMatcher(Step step) => StatMatchers[0];
        }

        private enum Step
        {
            Start,
            Success,
            Failure
        }

        private class Matchers : IStatMatchers
        {
            public Matchers()
            {
                var buildParameters = new BuildParameters(ExpectedModifier.Source, default, ExpectedModifier.Form);
                var form = new Mock<IFormBuilder>();
                form.Setup(b => b.Build()).Returns((ExpectedModifier.Form, Funcs.Identity));
                form.Setup(b => b.Resolve(It.IsAny<ResolveContext>())).Returns(form.Object);
                var throwingForm = new Mock<IFormBuilder>();
                throwingForm.Setup(b => b.Build()).Throws(new ParseException("message"));
                throwingForm.Setup(b => b.Resolve(It.IsAny<ResolveContext>())).Returns(throwingForm.Object);
                var stat = new Mock<IStatBuilder>();
                stat.Setup(b => b.Build(buildParameters)).Returns(new[]
                    { new StatBuilderResult(ExpectedModifier.Stats, ExpectedModifier.Source, Funcs.Identity) });
                stat.Setup(b => b.Resolve(It.IsAny<ResolveContext>())).Returns(stat.Object);
                var value = new Mock<IValueBuilder>();
                value.Setup(b => b.Build(buildParameters)).Returns(ExpectedModifier.Value);
                value.Setup(b => b.Resolve(It.IsAny<ResolveContext>())).Returns(value.Object);
                Data = new[]
                {
                    new MatcherData("matched", new SimpleIntermediateModifier(new[]
                    {
                        new IntermediateModifierEntry()
                            .WithForm(form.Object).WithStat(stat.Object).WithValue(value.Object)
                    }, Funcs.Identity, Funcs.Identity)),
                    new MatcherData("throwing", new SimpleIntermediateModifier(new[]
                    {
                        new IntermediateModifierEntry()
                            .WithForm(throwingForm.Object).WithStat(stat.Object).WithValue(value.Object)
                    }, Funcs.Identity, Funcs.Identity)),
                };
            }

            public IReadOnlyList<MatcherData> Data { get; }
            public IReadOnlyList<string> ReferenceNames => new string[0];
            public bool MatchesWholeLineOnly => false;
        }

        private class Stepper : IStepper<Step>
        {
            public Step InitialStep => Step.Start;
            public Step NextOnSuccess(Step current) => Step.Success;
            public Step NextOnFailure(Step current) => Step.Failure;
            public bool IsTerminal(Step step) => step != Step.Start;
            public bool IsSuccess(Step step) => step == Step.Success;
        }

        private static readonly Modifier ExpectedModifier =
            new Modifier(new IStat[0], Form.BaseAdd, new Constant(5), new ModifierSource.Global());
    }
}
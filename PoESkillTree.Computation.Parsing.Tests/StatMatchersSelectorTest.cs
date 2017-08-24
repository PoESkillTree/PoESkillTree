using System;
using System.Collections.Generic;
using NUnit.Framework;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Data;
using PoESkillTree.Computation.Parsing.Steps;

namespace PoESkillTree.Computation.Parsing.Tests
{
    [TestFixture]
    public class StatMatchersSelectorTest
    {
        [Test]
        public void GetWithUnknownThrows()
        {
            var sut = new StatMatchersSelector();

            Assert.Throws<InvalidOperationException>(() => sut.Get(ParsingStep.Invalid));
        }

        [TestCase(ParsingStep.ValueConversion, ExpectedResult = typeof(ValueConversionMatchers))]
        [TestCase(ParsingStep.FormAndStat, ExpectedResult = typeof(FormAndStatMatchers))]
        [TestCase(ParsingStep.Form, ExpectedResult = typeof(FormMatchers))]
        public Type GetWithKnownReturnsCorrectResult(ParsingStep parsingStep)
        {
            var sut = new StatMatchersSelector(new SpecialMatchers(), new ValueConversionMatchers(), 
                new FormAndStatMatchers(), new FormMatchers(), new FormZMatchers());

            var statMatchers = sut.Get(parsingStep);

            return statMatchers.GetType();
        }


        private class SpecialMatchers : IStatMatchers
        {
            public IReadOnlyList<MatcherData> Matchers { get; }
        }

        private class ValueConversionMatchers : IStatMatchers
        {
            public IReadOnlyList<MatcherData> Matchers { get; }
        }

        private class FormAndStatMatchers : IStatMatchers
        {
            public IReadOnlyList<MatcherData> Matchers { get; }
        }

        private class FormMatchers : IStatMatchers
        {
            public IReadOnlyList<MatcherData> Matchers { get; }
        }

        private class FormZMatchers : IStatMatchers
        {
            public IReadOnlyList<MatcherData> Matchers { get; }
        }
    }
}
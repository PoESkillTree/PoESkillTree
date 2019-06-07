using System;
using System.Collections.Generic;
using NUnit.Framework;
using PoESkillTree.Computation.Common.Data;

namespace PoESkillTree.Computation.Data.Steps
{
    [TestFixture]
    public class StatMatchersSelectorTest
    {
        [Test]
        public void GetWithUnknownThrows()
        {
            var sut = CreateSut();

            Assert.Throws<InvalidOperationException>(() => sut.Get(ParsingStep.Success));
        }

        [TestCase(ParsingStep.ValueConversion, ExpectedResult = typeof(ValueConversionMatchers))]
        [TestCase(ParsingStep.FormAndStat, ExpectedResult = typeof(FormAndStatMatchers))]
        [TestCase(ParsingStep.Form, ExpectedResult = typeof(FormMatchers))]
        public Type GetWithKnownReturnsCorrectResult(ParsingStep parsingStep)
        {
            var sut = CreateSut(new SpecialMatchers(), new ValueConversionMatchers(),
                new FormAndStatMatchers(), new FormMatchers(), new FormZMatchers());

            var statMatchers = sut.Get(parsingStep);

            return statMatchers.GetType();
        }

        private static StatMatchersSelector CreateSut(params IStatMatchers[] candidates)
        {
            return new StatMatchersSelector(candidates);
        }


        private class StatMatchersStub : IStatMatchers
        {
            public IReadOnlyList<MatcherData> Data { get; } = new MatcherData[0];

            public IReadOnlyList<string> ReferenceNames { get; } = new string[0];

            public bool MatchesWholeLineOnly => false;
        }


        private class SpecialMatchers : StatMatchersStub
        {
        }

        private class ValueConversionMatchers : StatMatchersStub
        {
        }

        private class FormAndStatMatchers : StatMatchersStub
        {
        }

        private class FormMatchers : StatMatchersStub
        {
        }

        private class FormZMatchers : StatMatchersStub
        {
        }
    }
}
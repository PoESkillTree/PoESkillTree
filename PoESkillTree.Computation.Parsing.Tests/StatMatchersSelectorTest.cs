using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
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


        private class StatMatchersStub : IStatMatchers
        {
            public IEnumerator<MatcherData> GetEnumerator()
            {
                return Enumerable.Empty<MatcherData>().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

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
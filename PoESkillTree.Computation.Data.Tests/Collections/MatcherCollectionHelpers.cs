using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace PoESkillTree.Computation.Data.Tests.Collections
{
    internal static class MatcherCollectionHelpers
    {
        internal static MatchBuilderStub AssertSingle(this IEnumerable<MatcherData> sut, 
            string regex, string substitution = "")
        {
            var data = sut.Single();
            Assert.AreEqual(regex, data.Regex);
            Assert.IsInstanceOf<MatchBuilderStub>(data.MatchBuilder);
            Assert.AreEqual(substitution, data.MatchSubstitution);
            return (MatchBuilderStub) data.MatchBuilder;
        }
    }
}
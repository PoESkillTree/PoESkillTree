using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Data;

namespace PoESkillTree.Computation.Data.Tests.Collections
{
    internal static class MatcherCollectionHelpers
    {
        internal static ModifierBuilderStub AssertSingle(this IEnumerable<MatcherData> sut, 
            string regex, string substitution = "")
        {
            var data = sut.Single();
            Assert.AreEqual(regex, data.Regex);
            Assert.IsInstanceOf<ModifierBuilderStub>(data.ModifierBuilder);
            Assert.AreEqual(substitution, data.MatchSubstitution);
            return (ModifierBuilderStub) data.ModifierBuilder;
        }
    }
}
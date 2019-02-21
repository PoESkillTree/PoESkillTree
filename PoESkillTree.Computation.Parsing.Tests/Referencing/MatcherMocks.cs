using System.Collections.Generic;
using System.Linq;
using Moq;
using PoESkillTree.Computation.Common.Data;

namespace PoESkillTree.Computation.Parsing.Tests.Referencing
{
    internal static class MatcherMocks
    {
        internal static readonly IReadOnlyList<IReferencedMatchers> DefaultReferencedMatchersList =
            new[]
            {
                MockReferencedMatchers("Matchers1", "a", "b"),
                MockReferencedMatchers("Matchers2", "1", "2", "3")
            };

        internal static IReferencedMatchers MockReferencedMatchers(string referenceName,
            params string[] patterns)
        {
            var data = patterns.Select(p => new ReferencedMatcherData(p, null)).ToList();
            return Mock.Of<IReferencedMatchers>(m =>
                m.ReferenceName == referenceName &&
                m.Data == data);
        }

        internal static readonly IReadOnlyList<IStatMatchers> DefaultStatMatchersList =
            new[]
            {
                MockStatMatchers(new[] { "SMatchers1" }, "a", "aa", "aaa"),
                MockStatMatchers(new[] { "SMatchers2", "SMatchers1" }, "1"),
                MockStatMatchers(new[] { "SMatchers3" }, "q", "w"),
                MockStatMatchers(new string[0], "x"),
            };

        internal static IStatMatchers MockStatMatchers(IReadOnlyList<string> referenceNames,
            params string[] patterns)
        {
            var data = patterns.Select(p => new MatcherData(p, null)).ToList();
            return Mock.Of<IStatMatchers>(m =>
                m.ReferenceNames == referenceNames &&
                m.Data == data);
        }
    }
}
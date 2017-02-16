using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTests.TestBuilds.Utils
{
    /// <summary>
    /// Aggregates testing build url models and provides searching methods.
    /// </summary>
    public class BuildUrlCollection : List<BuildUrlTestModel>
    {
        public BuildUrlTestModel FindByName(string buildName)
        {
            return this.FirstOrDefault(build => build.Name.Equals(buildName, StringComparison.Ordinal));
        }

        public ICollection<BuildUrlTestModel> FindByTag(string tag)
        {
            return this.Where(build =>
                build.Tags.Any(buildTag => buildTag.Equals(tag, StringComparison.Ordinal))).ToList();
        }

        public ICollection<BuildUrlTestModel> FindByTags(params string[] tags)
        {
            return this.Where(build => build.Tags.Any(buildTag =>
                tags.Any(t => t.Equals(buildTag, StringComparison.Ordinal)))).ToList();
        }
    }
}
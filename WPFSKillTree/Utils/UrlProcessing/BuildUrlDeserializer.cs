using System.Linq;
using POESKillTree.SkillTreeFiles;

namespace POESKillTree.Utils.UrlProcessing
{
    /// <summary>
    /// Represents an object that extracts build information from the tree url.
    /// </summary>
    public abstract class BuildUrlDeserializer
    {
        protected static IAscendancyClasses AscendancyClasses;
        protected string BuildUrl { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildUrlDeserializer"/> class.
        /// </summary>
        /// <param name="buildUrl">The PoE build url.</param>
        /// <param name="ascendancyClasses">The instance of the <see cref="ascendancyClasses"/>
        /// to access general information about skill tree.</param>
        protected BuildUrlDeserializer(string buildUrl, IAscendancyClasses ascendancyClasses)
        {
            BuildUrl = buildUrl ?? string.Empty;
            AscendancyClasses = ascendancyClasses;
        }

        /// <summary>
        /// Returns the basic build data.
        /// </summary>
        public abstract BuildUrlData GetBuildData();

        /// <summary>
        /// Returns the id of a character class decoded from the tree url.
        /// </summary>
        public abstract int GetCharacterClassId();

        /// <summary>
        /// Returns the id of an ascendancy class decoded from the tree url.
        /// </summary>
        public abstract int GetAscendancyClassId();

        /// <summary>
        /// Returns the number of non-ascendancy points the given tree url uses.
        /// </summary>
        public virtual int GetPointsCount(bool includeAscendancy = false)
        {
            if (includeAscendancy)
                return GetBuildData().SkilledNodesIds.Count(id => !SkillTree.RootNodeList.Contains(id));

            return GetBuildData().SkilledNodesIds.Count(id =>
            {
                SkillNode skillNode;
                if (SkillTree.Skillnodes.TryGetValue(id, out skillNode))
                {
                    return !SkillTree.RootNodeList.Contains(id) && skillNode.ascendancyName == null;
                }

                return false;
            });
        }

        /// <summary>
        /// Returns the character class of the given build url.
        /// </summary>
        public virtual string GetCharacterClass()
        {
            var classId = GetCharacterClassId();

            return CharacterNames.GetClassNameFromChartype(classId);
        }

        /// <summary>
        /// Returns the ascendancy class of the given build url.
        /// Returns null if the tree has no ascendancy class selected.
        /// </summary>
        public virtual string GetAscendancyClass()
        {
            var ascendancyId = GetAscendancyClassId();

            // No Ascendancy class selected.
            if (ascendancyId == 0)
                return null;

            var classId = GetCharacterClassId();

            return AscendancyClasses.GetClassName(classId, ascendancyId);
        }
    }
}
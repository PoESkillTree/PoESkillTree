using System;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Utils
{
    public class SemanticVersion : IComparable<SemanticVersion>, IEquatable<SemanticVersion>
    {
        private SemanticVersion(int major, int minor, int patch, string preReleaseType, int preReleaseNumber)
            => (Major, Minor, Patch, PreReleaseType, PreReleaseNumber) =
                (major, minor, patch, preReleaseType, preReleaseNumber);

        public int Major { get; }
        public int Minor { get; }
        public int Patch { get; }

        public string PreReleaseType { get; }
        public int PreReleaseNumber { get; }

        public static SemanticVersion Parse(string version)
        {
            var parts = version.Split('.', '-');
            var major = parts[0].ParseInt();
            var minor = parts[1].ParseInt();
            var patch = parts[2].ParseInt();
            var preReleaseType = parts.Length > 3 ? parts[3] : null;
            var preReleaseNumber = parts.Length > 4 ? parts[4].ParseInt() : -1;
            return new SemanticVersion(major, minor, patch, preReleaseType, preReleaseNumber);
        }

        public int CompareTo(SemanticVersion other)
        {
            if (Major != other.Major)
                return Major.CompareTo(other.Major);
            if (Minor != other.Minor)
                return Minor.CompareTo(other.Minor);
            if (Patch != other.Patch)
                return Patch.CompareTo(other.Patch);

            if (PreReleaseType != other.PreReleaseType)
            {
                if (PreReleaseType is null)
                    return 1;
                if (other.PreReleaseType is null)
                    return -1;
                return string.CompareOrdinal(PreReleaseType, other.PreReleaseType);
            }
            return PreReleaseNumber.CompareTo(other.PreReleaseNumber);
        }

        public override bool Equals(object obj)
            => Equals((SemanticVersion) obj);

        public bool Equals(SemanticVersion other)
            => other != null && Major == other.Major && Minor == other.Minor && Patch == other.Patch &&
               PreReleaseType == other.PreReleaseType && PreReleaseNumber == other.PreReleaseNumber;

        public override int GetHashCode()
            => (Major, Minor, Patch, PreReleaseType, PreReleaseNumber).GetHashCode();
    }
}
namespace PoESkillTree.Model.Builds
{
    public enum Realm
    {
        PC,
        XBox,
        Sony
    }

    public static class RealmExtensions
    {
        public static string ToGGGIdentifier(this Realm @this) => @this.ToString().ToLowerInvariant();
    }
}
namespace PoESkillTree.ViewModels
{
    public class AttributeGroup
    {
        public string GroupName { get; set; }

        // Used in MainWindow's ContainerStyleIsExpanded via GroupStringConverter
        public bool IsExpanded { get; set; } = true;

        public AttributeGroup(string groupGroupName)
        {
            GroupName = groupGroupName;
        }

        public override bool Equals(object? obj)
        {
            var other = obj as AttributeGroup;
            if (other == null) return false;

            return GroupName == other.GroupName;
        }

        public override int GetHashCode()
        {
            return GroupName.GetHashCode();
        }

        public override string ToString()
        {
            return GroupName;
        }
    }
}
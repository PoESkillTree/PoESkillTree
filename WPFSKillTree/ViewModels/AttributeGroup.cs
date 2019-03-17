using System;

namespace PoESkillTree.ViewModels
{
    public class AttributeGroup
    {
        public string GroupName { get; set; }
        private bool _isExpanded = true;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { _isExpanded = value; }
        }

        public AttributeGroup(string groupGroupName)
        {
            GroupName = groupGroupName;
            IsExpanded = true;
        }

        public override bool Equals(Object obj)
        {
            if (obj == null) return false;

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
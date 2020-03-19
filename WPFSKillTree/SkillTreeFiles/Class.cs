using System.Collections.Generic;
using System.Drawing;

namespace PoESkillTree.SkillTreeFiles
{
    public class Class
    {
        public Class(int order, string name, string displayName, string flavourText, Vector2D flavourTextRect, Color flavourTextColour)
        {
            Order = order;
            Name = name;
            DisplayName = displayName;
            FlavourText = flavourText;
            FlavourTextRect = flavourTextRect;
            FlavourTextColour = flavourTextColour;
        }

        public int Order { get; }
        public string Name { get; }
        public string DisplayName { get; }
        public string FlavourText { get; }
        public Vector2D FlavourTextRect { get; }
        public Color FlavourTextColour { get; }
    }
}
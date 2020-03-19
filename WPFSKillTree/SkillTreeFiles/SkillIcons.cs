using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PoESkillTree.SkillTreeFiles
{
    public class SkillIcons
    {
        /// <summary>
        /// Dictionary that maps sprite image names to the actual image.
        /// </summary>
        public readonly Dictionary<string, BitmapImage> Images = new Dictionary<string, BitmapImage>();

        /// <summary>
        /// Dictionary that maps the icon name for a skill node (see <see cref="PassiveNodeViewModel.IconKey"/>) to the position
        /// in its sprite image.
        /// </summary>
        public readonly Dictionary<string, Rect> SkillPositions = new Dictionary<string, Rect>();

        /// <summary>
        /// Dictionary that maps the icon name for a skill node (see <see cref="PassiveNodeViewModel.IconKey"/>) to the sprite
        /// image name it is contained in.
        /// </summary>
        public readonly Dictionary<string, string> SkillImages = new Dictionary<string, string>();

        /// <summary>
        /// Gets the sprite image that contains the icon for the skill node with the given icon key
        /// </summary>
        /// <param name="iconKey"></param>
        /// <returns></returns>
        public BitmapImage GetSkillImage(string iconKey)
        {
            return Images[SkillImages[iconKey]];
        }
    }
}
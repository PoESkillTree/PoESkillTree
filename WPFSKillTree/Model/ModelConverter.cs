using System.Linq;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.PassiveTree;
using PoESkillTree.Model.Items.Mods;
using PoESkillTree.SkillTreeFiles;
using OldItem = PoESkillTree.Model.Items.Item;

namespace PoESkillTree.Model
{
    /// <summary>
    /// The PoESkillTree.GameModel project should at some point replace all game-related model classes.
    /// Because the Computation projects already use the new model, this class has to exist to convert between
    /// the old and new model.
    /// </summary>
    public static class ModelConverter
    {
        public static PassiveNodeDefinition Convert(SkillNode skillNode)
            => new PassiveNodeDefinition(
                skillNode.Id,
                skillNode.Type,
                skillNode.Name,
                skillNode.ascendancyName != null,
                !skillNode.IsRootNode && !skillNode.IsAscendancyStart && !skillNode.IsMultipleChoiceOption,
                skillNode.passivePointsGranted,
                skillNode.attributes);

        public static Item Convert(OldItem oldItem)
        {
            var quality = (int) oldItem.Properties.First("Quality: +#%", 0, 0);
            var levelMod = oldItem.Requirements.FirstOrDefault(m => m.Attribute.Contains("Level #"));
            var level = (int) (levelMod?.Values.FirstOrDefault() ?? 0);
            var isCorrupted = oldItem.Mods.Any(m => m.Attribute == "Corrupted");
            var mods = oldItem.Mods.Select(m => m.ToModifierString()).ToList();
            return new Item(
                oldItem.BaseType.MetadataId,
                oldItem.Name,
                quality,
                level,
                oldItem.Frame,
                isCorrupted,
                mods,
                oldItem.IsEnabled);
        }
    }
}
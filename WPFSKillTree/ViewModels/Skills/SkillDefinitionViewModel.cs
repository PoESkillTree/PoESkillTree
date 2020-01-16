using System.Linq;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.Skills;
using PoESkillTree.Model.Items;

namespace PoESkillTree.ViewModels.Skills
{
    public class SkillDefinitionViewModel
    {
        public SkillDefinition Model { get; }
        public string Id => Model.Id;
        public string Name => Model.BaseItem?.DisplayName ?? "";
        public int MaxLevel => Model.Levels.Keys.Max();

        public ItemImage Icon { get; }

        public SkillDefinitionViewModel(ItemImageService itemImageService, SkillDefinition model)
        {
            Model = model;
            Icon = new ItemImage(itemImageService, Name,
                model.IsSupport ? ItemClass.SupportSkillGem : ItemClass.ActiveSkillGem);
        }
    }
}
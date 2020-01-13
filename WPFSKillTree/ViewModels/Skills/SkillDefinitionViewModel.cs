using System.Linq;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.Skills;
using PoESkillTree.Model.Items;

namespace PoESkillTree.ViewModels.Skills
{
    public class SkillDefinitionViewModel
    {
        private readonly SkillDefinition _skill;
        public string Id => _skill.Id;
        public string Name => _skill.BaseItem?.DisplayName ?? "";
        public int MaxLevel => _skill.Levels.Keys.Max();

        public ItemImage Icon { get; }

        public SkillDefinitionViewModel(ItemImageService itemImageService, SkillDefinition skill)
        {
            _skill = skill;
            Icon = new ItemImage(itemImageService, Name,
                skill.IsSupport ? ItemClass.SupportSkillGem : ItemClass.ActiveSkillGem);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.Skills;
using PoESkillTree.Model.Items.Mods;
using PoESkillTree.Utils;

namespace PoESkillTree.Model.Items
{
    public class SkillItem : Notifier, IHasItemToolTip
    {
        public SkillItem(string name)
        {
            TypeLine = name;
            Properties = Array.Empty<ItemMod>();
            Requirements = Array.Empty<ItemMod>();
            ImplicitMods = Array.Empty<ItemMod>();
            ExplicitMods = Array.Empty<ItemMod>();
        }

        public SkillItem(SkillTooltipDefinition tooltipDefinition, int level, int? gemLevel, int quality)
        {
            TypeLine = tooltipDefinition.Name;
            Properties = GetProperties(tooltipDefinition, level, gemLevel, quality).ToList();
            Requirements = tooltipDefinition.Requirements.Select(ConvertTranslatedStat).ToList();
            ImplicitMods = tooltipDefinition.QualityStats
                .Select(s => new TranslatedStat(s.FormatText, s.Values.Select(d => d * quality).ToArray()))
                .Select(ConvertTranslatedStat).ToList();
            ExplicitMods = tooltipDefinition.Stats.Select(ConvertTranslatedStat).ToList();
        }

        public FrameType Frame => FrameType.Gem;
        public string NameLine => "";
        public bool HasNameLine => false;
        public string TypeLine { get; }
        public IReadOnlyList<ItemMod> Properties { get; }
        public IReadOnlyList<ItemMod> Requirements { get; }
        public IReadOnlyList<ItemMod> ImplicitMods { get; }
        public IReadOnlyList<ItemMod> ExplicitMods { get; }
        public IReadOnlyList<ItemMod> CraftedMods => Array.Empty<ItemMod>();
        public string? FlavourText => null;
        public bool HasFlavourText => false;

        private static IEnumerable<ItemMod> GetProperties(SkillTooltipDefinition tooltipDefinition, int level, int? gemLevel, int quality)
        {
            foreach (var stat in tooltipDefinition.Properties)
            {
                if (gemLevel != null && level != gemLevel && stat.FormatText.StartsWith("Level: {0}"))
                {
                    yield return new ItemMod("Level: # (#+#)", true,
                        new[] {level, gemLevel.Value, (float) (level - gemLevel)},
                        new[] {ValueColoring.LocallyAffected, ValueColoring.White, ValueColoring.LocallyAffected});
                }
                else
                {
                    yield return ConvertTranslatedStat(stat);
                }
            }

            if (quality > 0)
            {
                yield return new ItemMod("Quality: +#%", true, new[] {(float) quality}, new[] {ValueColoring.LocallyAffected});
            }
        }

        private static ItemMod ConvertTranslatedStat(TranslatedStat stat)
            => new ItemMod(stat.ToString(), true);
    }
}
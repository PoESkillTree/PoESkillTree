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

        public SkillItem(SkillTooltipDefinition tooltipDefinition, int quality)
        {
            TypeLine = tooltipDefinition.Name;
            Properties = tooltipDefinition.Properties.Select(ConvertTranslatedStat).ToList();
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

        private static ItemMod ConvertTranslatedStat(TranslatedStat stat)
            => new ItemMod(stat.ToString(), true);
    }
}
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MoreLinq;
using PoESkillTree.GameModel.Items;
using POESKillTree.Model.Items;
using POESKillTree.Model.Items.Mods;

namespace POESKillTree.ViewModels.Crafting
{
    /// <summary>
    /// View model for crafting unique items.
    /// </summary>
    public class UniqueCraftingViewModel : AbstractCraftingViewModel<UniqueBase>
    {
        private const string JewelRadiusStat = "local_jewel_effect_base_radius";
        private const string JewelRadiusPropertyPrefix = "Radius: ";

        private static readonly IReadOnlyDictionary<int, string> JewelRadiusToString = new Dictionary<int, string>
        {
            { 800, "Small" },
            { 1200, "Medium" },
            { 1500, "Large" },
        };

        private IReadOnlyList<ModSelectorViewModel> _msExplicits = new ModSelectorViewModel[0];
        public IReadOnlyList<ModSelectorViewModel> MsExplicits
        {
            get { return _msExplicits; }
            private set { SetProperty(ref _msExplicits, value); }
        }

        public UniqueCraftingViewModel(EquipmentData equipmentData)
            : base(equipmentData, equipmentData.UniqueBases)
        {
            Init();
        }

        protected override void UpdateBaseSpecific()
        {
            MsExplicits.ForEach(ms => ms.PropertyChanged -= MsOnPropertyChanged);
            var modSelectors = new List<ModSelectorViewModel>();
            foreach (var explicitMod in SelectedBase.ExplicitMods)
            {
                var modSelector = new ModSelectorViewModel(EquipmentData.StatTranslator, false)
                {
                    Affixes = new[]
                    {
                        new Affix(explicitMod)
                    }
                };
                modSelector.PropertyChanged += MsOnPropertyChanged;
                modSelectors.Add(modSelector);
            }
            MsExplicits = modSelectors;
        }

        protected override (IEnumerable<StatIdValuePair> explicitStats, IEnumerable<StatIdValuePair> craftedStats)
            RecalculateItemSpecific(out int requiredLevel)
        {
            Item.NameLine = SelectedBase.UniqueName;
            Item.Frame = FrameType.Unique;
            requiredLevel = MsExplicits
                .Select(ms => ms.Query().RequiredLevel)
                .DefaultIfEmpty()
                .Max();
            return (MsExplicits.SelectMany(ms => ms.GetStatValues()), new StatIdValuePair[0]);
        }

        protected override IEnumerable<ItemMod> GetAdditionalProperties()
        {
            var rangeStat = MsExplicits.SelectMany(ms => ms.Query().Stats).FirstOrDefault(s => s.Id == JewelRadiusStat);
            if (rangeStat != null)
            {
                yield return CreateJewelRangeProperty(rangeStat.Range.From);
            }
        }

        private static ItemMod CreateJewelRangeProperty(int range)
        {
            var rangeString = JewelRadiusPropertyPrefix + JewelRadiusToString[range];
            return new ItemMod(rangeString, false);
        }

        private void MsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModSelectorViewModel.SelectedValues))
            {
                RecalculateItem();
            }
        }
    }
}
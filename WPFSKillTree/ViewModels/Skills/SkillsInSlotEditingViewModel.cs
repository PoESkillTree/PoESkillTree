using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using MoreLinq;
using PoESkillTree.Common.ViewModels;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.Skills;
using PoESkillTree.Engine.Utils;
using PoESkillTree.Engine.Utils.Extensions;
using PoESkillTree.Localization;
using PoESkillTree.Model.Items;
using PoESkillTree.Utils.Wpf;

namespace PoESkillTree.ViewModels.Skills
{
    public class SkillsInSlotEditingViewModelProxy : BindingProxy<SkillsInSlotEditingViewModel>
    {
    }

    /// <summary>
    /// View model for editing skills socketed in an item.
    /// </summary>
    public sealed class SkillsInSlotEditingViewModel : CloseableViewModel, IDisposable
    {
        private readonly SkillDefinitions _skillDefinitions;
        private readonly ItemImageService _itemImageService;
        private readonly ItemAttributes _itemAttributes;
        public ItemSlot Slot { get; }

        public IReadOnlyList<SkillDefinitionViewModel> AvailableGems { get; }

        public CollectionViewSource GemsViewSource { get; }

        private readonly ObservableCollection<GemViewModel> _gems = new ObservableCollection<GemViewModel>();

        private IReadOnlyList<Gem>? _gemModels;

        public ICommand AddGemCommand { get; }
        public ICommand RemoveGemCommand { get; }

        public int NumberOfSockets
            => _itemAttributes.GetItemInSlot(Slot, null)?.BaseType.MaximumNumberOfSockets ?? 0;

        private GemViewModel _newGem;
        /// <summary>
        /// Gets the currently edited gem that can be socketed into the item with AddGemCommand.
        /// </summary>
        public GemViewModel NewGem
        {
            get => _newGem;
            private set => SetProperty(ref _newGem, value);
        }

        private static readonly string DefaultSummary = L10n.Message("no active skills");

        private string _summary = DefaultSummary;

        public string Summary
        {
            get => _summary;
            private set => SetProperty(ref _summary, value);
        }

        private string? _longSummary;

        public string? LongSummary
        {
            get => _longSummary;
            private set => SetProperty(ref _longSummary, value);
        }

        public SkillsInSlotEditingViewModel(
            SkillDefinitions skillDefinitions, ItemImageService itemImageService, ItemAttributes itemAttributes,
            ItemSlot slot)
        {
            _skillDefinitions = skillDefinitions;
            _itemImageService = itemImageService;
            _itemAttributes = itemAttributes;
            Slot = slot;
            AvailableGems = skillDefinitions.Skills
                .Where(d => d.BaseItem != null)
                .Where(d => d.BaseItem!.ReleaseState == ReleaseState.Released ||
                            d.BaseItem.ReleaseState == ReleaseState.Legacy)
                .OrderBy(d => d.BaseItem!.DisplayName)
                .Select(CreateSkillDefinitionViewModel).ToList();
            _newGem = new GemViewModel(AvailableGems[0])
            {
                Level = 20,
                Quality = 0,
                Group = 1,
                SocketIndex = -1,
                IsEnabled = true,
            };
            AddGemCommand = new RelayCommand(AddGem);
            RemoveGemCommand = new RelayCommand<GemViewModel>(RemoveGem);

            GemsViewSource = new CollectionViewSource
            {
                Source = _gems
            };
            GemsViewSource.SortDescriptions.Add(new SortDescription(
                nameof(GemViewModel.Group),
                ListSortDirection.Ascending));
            GemsViewSource.SortDescriptions.Add(new SortDescription(
                nameof(GemViewModel.SocketIndex),
                ListSortDirection.Ascending));

            UpdateGemsFromItemAttributes();
            UpdateSkillsFromItemAttributes();
            itemAttributes.Gems.CollectionChanged += GemsOnCollectionChanged;
            itemAttributes.Skills.CollectionChanged += SkillsOnCollectionChanged;
        }

        private void UpdateGemsFromItemAttributes()
        {
            foreach (var gemVm in _gems)
            {
                DisposeGem(gemVm);
            }
            _gems.Clear();

            _gemModels = _itemAttributes.GetGemsInSlot(Slot);
            foreach (var gem in _gemModels)
            {
                var definition = AvailableGems.FirstOrDefault(d => d.Id == gem.SkillId);
                if (definition is null)
                    continue;

                var gemVm = new GemViewModel(definition)
                {
                    Level = gem.Level,
                    Quality = gem.Quality,
                    Group = gem.Group,
                    SocketIndex = gem.SocketIndex,
                    IsEnabled = gem.IsEnabled,
                };
                gemVm.PropertyChanged += GemViewModelOnPropertyChanged;
                _gems.Add(gemVm);
            }

            UpdateSummary();
        }

        private void UpdateSkillsFromItemAttributes()
        {
            var skillModels = _itemAttributes.GetSkillsInSlot(Slot);
            foreach (var (gem, skills) in skillModels.Where(s => s.Gem != null).GroupBy(s => s.Gem!))
            {
                var gemVm = _gems.FirstOrDefault(v => v.Definition?.Id == gem.SkillId && v.SocketIndex == gem.SocketIndex);
                if (gemVm is null)
                    continue;

                foreach (var skillVm in gemVm.Skills)
                {
                    skillVm.PropertyChanged -= SkillViewModelOnPropertyChanged;
                }

                var skillVms = new List<SkillViewModel>();
                foreach (var skill in skills)
                {
                    var definition = CreateSkillDefinitionViewModel(_skillDefinitions.GetSkillById(skill.Id));
                    var skillVm = new SkillViewModel(gemVm, skill.Level, skill.Quality, skill.SkillIndex, definition)
                    {
                        IsEnabled = skill.IsEnabled
                    };
                    skillVm.PropertyChanged += SkillViewModelOnPropertyChanged;
                    skillVms.Add(skillVm);
                }
                gemVm.Skills = skillVms;
            }
        }

        private SkillDefinitionViewModel CreateSkillDefinitionViewModel(SkillDefinition definition) =>
            new SkillDefinitionViewModel(_itemImageService, definition);

        private void AddGem()
        {
            var addedGem = NewGem.Clone();
            if (addedGem.SocketIndex < 0)
            {
                addedGem.SocketIndex = _gems.Any() ? _gems.Max(g => g.SocketIndex) + 1 : 1;
            }
            addedGem.PropertyChanged += GemViewModelOnPropertyChanged;
            _gems.Add(addedGem);
            UpdateItemAttributes();
        }

        private void RemoveGem(GemViewModel gem)
        {
            DisposeGem(gem);
            _gems.Remove(gem);
            NewGem = gem;
            UpdateItemAttributes();
        }

        private void GemViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(GemViewModel.Definition)
                || args.PropertyName == nameof(GemViewModel.Level)
                || args.PropertyName == nameof(GemViewModel.Quality)
                || args.PropertyName == nameof(GemViewModel.SocketIndex)
                || args.PropertyName == nameof(GemViewModel.Group)
                || args.PropertyName == nameof(GemViewModel.IsEnabled))
            {
                UpdateItemAttributes();
            }

            if (args.PropertyName == nameof(GemViewModel.Group) || args.PropertyName == nameof(GemViewModel.SocketIndex))
            {
                GemsViewSource.View.Refresh();
            }
        }

        private void SkillViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(SkillViewModel.IsEnabled) && sender is SkillViewModel skillVm && skillVm.Gem != null)
            {
                _itemAttributes.SkillEnabler.SetIsEnabled(skillVm.Gem.ToGem(Slot), skillVm.SkillIndex, skillVm.IsEnabled);
            }
        }

        private void UpdateItemAttributes()
        {
            _gemModels = _gems
                .Where(g => g.Definition != null)
                .Select(g => g.ToGem(Slot))
                .ToList();
            _itemAttributes.SetGemsInSlot(_gemModels, Slot);
            UpdateSummary();
        }

        private void GemsOnCollectionChanged(object sender, CollectionChangedEventArgs<IReadOnlyList<Gem>> args)
        {
            if (!ReferenceEquals(_gemModels, _itemAttributes.GetGemsInSlot(Slot)))
            {
                UpdateGemsFromItemAttributes();
            }
        }

        private void SkillsOnCollectionChanged(object sender, CollectionChangedEventArgs<IReadOnlyList<Skill>> args)
        {
            if (args.AddedItems.Concat(args.RemovedItems).Flatten().Any(s => s.ItemSlot == Slot))
            {
                UpdateSkillsFromItemAttributes();
            }
        }

        private void UpdateSummary()
        {
            var activeSkills = _gems
                .Where(s => s.Definition != null)
                .Where(s => !s.Definition!.Model.IsSupport)
                .ToList();

            if (activeSkills.IsEmpty())
            {
                Summary = DefaultSummary;
                LongSummary = null;
            }
            else
            {
                Summary = activeSkills
                    .Select(s => s.DisplayName)
                    .ToDelimitedString(", ");
                LongSummary = activeSkills
                    .Select(s => $"{s.DisplayName} ({s.Level}/{s.Quality})")
                    .ToDelimitedString("\n");
            }
        }

        private void DisposeGem(GemViewModel gem)
        {
            gem.PropertyChanged -= GemViewModelOnPropertyChanged;
            foreach (var skill in gem.Skills)
            {
                skill.PropertyChanged -= SkillViewModelOnPropertyChanged;
            }
        }

        public void Dispose()
        {
            _itemAttributes.Gems.CollectionChanged -= GemsOnCollectionChanged;
            _itemAttributes.Skills.CollectionChanged -= SkillsOnCollectionChanged;
        }
    }
}
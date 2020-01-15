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
        private readonly ItemAttributes _itemAttributes;
        public ItemSlot Slot { get; }

        public IReadOnlyList<SkillDefinitionViewModel> AvailableSkills { get; }

        public CollectionViewSource SkillsViewSource { get; }

        private readonly ObservableCollection<SkillViewModel> _skills
            = new ObservableCollection<SkillViewModel>();

        private IReadOnlyList<Skill>? _skillModels;

        public ICommand AddSkillCommand { get; }
        public ICommand RemoveSkillCommand { get; }

        public int NumberOfSockets
            => _itemAttributes.GetItemInSlot(Slot, null)?.BaseType.MaximumNumberOfSockets ?? 0;

        private SkillViewModel _newSkill;
        /// <summary>
        /// Gets the currently edited skill that can be socketed into the item with AddSkillCommand.
        /// </summary>
        public SkillViewModel NewSkill
        {
            get => _newSkill;
            private set => SetProperty(ref _newSkill, value);
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
            _itemAttributes = itemAttributes;
            Slot = slot;
            AvailableSkills = skillDefinitions.Skills
                .Where(d => d.BaseItem != null)
                .Where(d => d.BaseItem!.ReleaseState == ReleaseState.Released ||
                            d.BaseItem.ReleaseState == ReleaseState.Legacy)
                .OrderBy(d => d.BaseItem!.DisplayName)
                .Select(d => new SkillDefinitionViewModel(itemImageService, d)).ToList();
            _newSkill = new SkillViewModel(AvailableSkills[0])
            {
                Level = 20,
                Quality = 0,
                GemGroup = 1,
                SocketIndex = -1,
                IsEnabled = true,
            };
            AddSkillCommand = new RelayCommand(AddSkill);
            RemoveSkillCommand = new RelayCommand<SkillViewModel>(RemoveSkill);

            SkillsViewSource = new CollectionViewSource
            {
                Source = _skills
            };
            SkillsViewSource.SortDescriptions.Add(new SortDescription(
                nameof(SkillViewModel.GemGroup),
                ListSortDirection.Ascending));
            SkillsViewSource.SortDescriptions.Add(new SortDescription(
                nameof(SkillViewModel.SocketIndex),
                ListSortDirection.Ascending));

            UpdateFromItemAttributes();
            itemAttributes.Skills.CollectionChanged += SkillsOnCollectionChanged;
        }

        private void UpdateFromItemAttributes()
        {
            _skills.Clear();
            _skillModels = _itemAttributes.GetSkillsInSlot(Slot);
            foreach (var skill in _skillModels)
            {
                var gemBase = AvailableSkills.FirstOrDefault(g => g.Id == skill.Id);
                if (gemBase == null)
                {
                    continue;
                }
                var socketedGem = new SkillViewModel(gemBase)
                {
                    Level = skill.Level,
                    Quality = skill.Quality,
                    GemGroup = skill.GemGroup + 1,
                    SocketIndex = skill.SocketIndex,
                    IsEnabled = skill.IsEnabled,
                };
                socketedGem.PropertyChanged += SocketedGemsOnPropertyChanged;
                _skills.Add(socketedGem);
            }
            UpdateSummary();
        }

        private void AddSkill()
        {
            var addedGem = NewSkill.Clone();
            addedGem.PropertyChanged += SocketedGemsOnPropertyChanged;
            _skills.Add(addedGem);
            UpdateItemAttributes();
        }

        private void RemoveSkill(SkillViewModel skill)
        {
            skill.PropertyChanged -= SocketedGemsOnPropertyChanged;
            _skills.Remove(skill);
            NewSkill = skill;
            UpdateItemAttributes();
        }

        private void SocketedGemsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(SkillViewModel.SocketIndex))
            {
                UpdateItemAttributes();
            }
            if (args.PropertyName == nameof(SkillViewModel.GemGroup) || args.PropertyName == nameof(SkillViewModel.SocketIndex))
            {
                SkillsViewSource.View.Refresh();
            }
        }

        private void UpdateItemAttributes()
        {
            var nextSocketIndex = _skills.Max(s => (int?) s.SocketIndex) + 1 ?? 0;
            var skills = new List<Skill>();
            foreach (var skillVm in _skills)
            {
                if (skillVm.SocketIndex < 0)
                {
                    skillVm.SocketIndex = nextSocketIndex++;
                }
                if (skillVm.Definition is null)
                    continue;
                var skill = new Skill(skillVm.Definition.Id, skillVm.Level, skillVm.Quality, Slot,
                    skillVm.SocketIndex, skillVm.GemGroup - 1, skillVm.IsEnabled);
                skills.Add(skill);
            }

            _skillModels = skills;
            _itemAttributes.SetSkillsInSlot(_skillModels, Slot);
            UpdateSummary();
        }

        private void SkillsOnCollectionChanged(object sender, CollectionChangedEventArgs<IReadOnlyList<Skill>> args)
        {
            if (!ReferenceEquals(_skillModels, _itemAttributes.GetSkillsInSlot(Slot)))
            {
                UpdateFromItemAttributes();
            }
        }

        private void UpdateSummary()
        {
            var activeSkills = _skills
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

        public void Dispose()
        {
            _itemAttributes.Skills.CollectionChanged -= SkillsOnCollectionChanged;
        }
    }
}
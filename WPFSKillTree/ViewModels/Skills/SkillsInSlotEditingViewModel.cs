using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using PoESkillTree.Common.ViewModels;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.Skills;
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
    public class SkillsInSlotEditingViewModel : CloseableViewModel
    {
        private readonly ItemAttributes _itemAttributes;
        private readonly ItemSlot _slot;

        public IReadOnlyList<SkillDefinitionViewModel> AvailableSkills { get; }

        public CollectionViewSource SkillsViewSource { get; }

        private readonly ObservableCollection<SkillViewModel> _skills
            = new ObservableCollection<SkillViewModel>();

        public ICommand AddSkillCommand { get; }
        public ICommand RemoveSkillCommand { get; }

        public int NumberOfSockets
            => _itemAttributes.GetItemInSlot(_slot, null)?.BaseType.MaximumNumberOfSockets ?? 0;

        private SkillViewModel _newSkill;
        /// <summary>
        /// Gets the currently edited skill that can be socketed into the item with AddSkillCommand.
        /// </summary>
        public SkillViewModel NewSkill
        {
            get => _newSkill;
            private set => SetProperty(ref _newSkill, value);
        }

        public SkillsInSlotEditingViewModel(
            SkillDefinitions skillDefinitions, ItemImageService itemImageService, ItemAttributes itemAttributes,
            ItemSlot slot)
        {
            _itemAttributes = itemAttributes;
            _slot = slot;
            AvailableSkills = skillDefinitions.Skills
                .Where(d => d.BaseItem != null)
                .Where(d => d.BaseItem!.ReleaseState == ReleaseState.Released ||
                            d.BaseItem.ReleaseState == ReleaseState.Legacy)
                .OrderBy(d => d.BaseItem!.DisplayName)
                .Select(d => new SkillDefinitionViewModel(itemImageService, d)).ToList();
            _newSkill = new SkillViewModel
            {
                Definition = AvailableSkills[0],
                Level = 20,
                Quality = 0,
                GemGroup = 1,
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
                nameof(SkillViewModel.Definition) + "." + nameof(SkillDefinitionViewModel.Name),
                ListSortDirection.Ascending));

            // convert currently socketed gem Items into SocketedGemViewModels
            foreach (var skill in _itemAttributes.GetSkillsInSlot(_slot))
            {
                var gemBase = AvailableSkills.FirstOrDefault(g => g.Id == skill.Id);
                if (gemBase == null)
                {
                    continue;
                }
                var socketedGem = new SkillViewModel
                {
                    Definition = gemBase,
                    Level = skill.Level,
                    Quality = skill.Quality,
                    GemGroup = skill.GemGroup + 1,
                    IsEnabled = skill.IsEnabled,
                };
                socketedGem.PropertyChanged += SocketedGemsOnPropertyChanged;
                _skills.Add(socketedGem);
            }
        }

        private void AddSkill()
        {
            var addedGem = NewSkill.Clone();
            addedGem.PropertyChanged += SocketedGemsOnPropertyChanged;
            _skills.Add(addedGem);
            SetSkillsInSlot();
        }

        private void RemoveSkill(SkillViewModel skill)
        {
            skill.PropertyChanged -= SocketedGemsOnPropertyChanged;
            _skills.Remove(skill);
            NewSkill = skill;
            SetSkillsInSlot();
        }

        private void SocketedGemsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(SkillViewModel.GemGroup))
            {
                SkillsViewSource.View.Refresh();
            }
            SetSkillsInSlot();
        }

        private void SetSkillsInSlot()
        {
            var skills = new List<Skill>();
            for (var i = 0; i < _skills.Count; i++)
            {
                var gem = _skills[i];
                var skill = new Skill(gem.Definition.Id, gem.Level, gem.Quality, _slot, i, gem.GemGroup - 1,
                    gem.IsEnabled);
                skills.Add(skill);
            }

            _itemAttributes.SetSkillsInSlot(skills, _slot);
        }
    }
}
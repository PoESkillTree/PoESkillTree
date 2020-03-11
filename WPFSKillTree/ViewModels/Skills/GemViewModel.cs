using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.Skills;
using PoESkillTree.Model.Items;
using PoESkillTree.Utils;

namespace PoESkillTree.ViewModels.Skills
{
    public class GemViewModel : Notifier
    {
        private int _level;
        private int _quality;
        private int _group;
        private int _socketIndex;
        private SkillDefinitionViewModel? _definition;
        private IReadOnlyList<SkillViewModel> _skills = Array.Empty<SkillViewModel>();
        private bool _isEnabled;
        private IHasItemToolTip _toolTip;

        public GemViewModel(SkillDefinitionViewModel? definition)
        {
            _definition = definition;
            _toolTip = CreateToolTip();
        }

        /// <summary>
        /// Gets or sets the level of this skill.
        /// </summary>
        public int Level
        {
            get => _level;
            set => SetProperty(ref _level, value);
        }

        /// <summary>
        /// Gets or sets the quality of this skill (in percent).
        /// </summary>
        public int Quality
        {
            get => _quality;
            set => SetProperty(ref _quality, value);
        }

        /// <summary>
        /// Gets or sets the socket group this gems is in. Gems of the same group are linked.
        /// </summary>
        public int Group
        {
            get => _group;
            set => SetProperty(ref _group, value);
        }

        public int SocketIndex
        {
            get => _socketIndex;
            set => SetProperty(ref _socketIndex, value);
        }

        public SkillDefinitionViewModel? Definition
        {
            get => _definition;
            set => SetProperty(ref _definition, value);
        }

        public IReadOnlyList<SkillViewModel> Skills
        {
            get => _skills;
            set => SetProperty(ref _skills, value, () => OnPropertyChanged(nameof(DisplaySkills)));
        }

        public bool DisplaySkills => Skills.Count(s => !s.Definition.Model.IsSupport) > 1;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public string DisplayName => Definition?.Model.DisplayName ?? "";

        public IHasItemToolTip ToolTip
        {
            get => _toolTip;
            private set => SetProperty(ref _toolTip, value);
        }

        public GemViewModel Clone() =>
            new GemViewModel(Definition)
            {
                Group = Group,
                SocketIndex = SocketIndex,
                Quality = Quality,
                Level = Level,
                Skills = Skills,
                IsEnabled = IsEnabled,
                ToolTip = ToolTip,
            };

        public Gem ToGem(ItemSlot slot)
        {
            if (Definition is null)
                throw new InvalidOperationException("Can only convert GemViewModels with Definitions to Gems");
            return new Gem(Definition.Id, Level, Quality, slot, SocketIndex, Group, IsEnabled);
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            if (propertyName == nameof(Definition) && Level > Definition?.MaxLevel)
            {
                Level = Definition.MaxLevel;
            }
            if (propertyName == nameof(Level) || propertyName == nameof(Quality))
            {
                foreach (var skill in Skills)
                {
                    skill.ReCreateToolTip();
                }
                ToolTip = CreateToolTip();
            }
            else if (propertyName == nameof(Definition) || propertyName == nameof(Skills))
            {
                ToolTip = CreateToolTip();
            }
            base.OnPropertyChanged(propertyName);
        }

        private IHasItemToolTip CreateToolTip() =>
            Skills.FirstOrDefault()?.ToolTip ?? new SkillItem(DisplayName);
    }
}
using PoESkillTree.Utils;

namespace PoESkillTree.ViewModels.Skills
{
    public class SkillViewModel : Notifier
    {
        private int _level;
        private int _quality;
        private int? _gemGroup;
        private int _socketIndex;
        private SkillDefinitionViewModel _definition;
        private bool _isEnabled;
        private string? _toolTip;

        public SkillViewModel(SkillDefinitionViewModel definition)
        {
            _definition = definition;
            UpdateToolTip();
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
        /// Gets or sets the socket group this skill is in. skill of the same group are linked.
        /// </summary>
        public int? GemGroup
        {
            get => _gemGroup;
            set => SetProperty(ref _gemGroup, value);
        }

        public int SocketIndex
        {
            get => _socketIndex;
            set => SetProperty(ref _socketIndex, value);
        }

        public SkillDefinitionViewModel Definition
        {
            get => _definition;
            set => SetProperty(ref _definition, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public string? ToolTip
        {
            get => _toolTip;
            private set => SetProperty(ref _toolTip, value);
        }

        public SkillViewModel Clone() =>
            new SkillViewModel(Definition)
            {
                GemGroup = GemGroup,
                SocketIndex = SocketIndex,
                Quality = Quality,
                Level = Level,
                IsEnabled = IsEnabled,
                ToolTip = ToolTip,
            };

        protected override void OnPropertyChanged(string propertyName)
        {
            if (propertyName != nameof(ToolTip))
            {
                UpdateToolTip();
            }
            base.OnPropertyChanged(propertyName);
        }

        private void UpdateToolTip()
        {
            if (Definition.Model.Levels.TryGetValue(Level, out var levelDefinition))
            {
                ToolTip = levelDefinition.Tooltip.Name;
            }
            else
            {
                ToolTip = null;
            }
        }
    }
}
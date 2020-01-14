using PoESkillTree.Utils;

namespace PoESkillTree.ViewModels.Skills
{
    public class SkillViewModel : Notifier
    {
        private int _level;
        private int _quality;
        private int? _gemGroup;
        private int _socketIndex;
        private SkillDefinitionViewModel _definition = default!;
        private bool _isEnabled;

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

        public SkillViewModel Clone()
        {
            return new SkillViewModel
            {
                Definition = Definition,
                GemGroup = GemGroup,
                SocketIndex = SocketIndex,
                Quality = Quality,
                Level = Level,
                IsEnabled = IsEnabled,
            };
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.ViewModels;
using PoESkillTree.Model.Builds;

namespace PoESkillTree.ViewModels.Builds
{
    public class EditBuildViewModel : ErrorInfoViewModel<bool>
    {
        private readonly IBuildViewModel<PoEBuild> _buildVm;
        private readonly BuildValidator _buildValidator;
        private string _name;
        private string? _note;
        private string? _characterName;
        private string? _accountName;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string? Note
        {
            get => _note;
            set => SetProperty(ref _note, value);
        }

        public string? CharacterName
        {
            get => _characterName;
            set => SetProperty(ref _characterName, value);
        }

        public string? AccountName
        {
            get => _accountName;
            set => SetProperty(ref _accountName, value);
        }

        public DateTime LastUpdated { get; }

        public EditBuildViewModel(IBuildViewModel<PoEBuild> buildVm, BuildValidator buildValidator)
        {
            _buildVm = buildVm;
            _buildValidator = buildValidator;
            var build = buildVm.Build;
            _name = build.Name;
            _note = build.Note;
            _characterName = build.CharacterName;
            _accountName = build.AccountName;
            LastUpdated = build.LastUpdated;
        }

        protected override IEnumerable<string?> ValidateProperty(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(Name):
                    return new[] {_buildValidator.ValidateExistingFileName(Name, _buildVm)};
                default:
                    return Enumerable.Empty<string>();
            }
        }

        protected override bool CanClose(bool param)
        {
            // Always allow canceling
            return !param || base.CanClose(true);
        }
    }
}
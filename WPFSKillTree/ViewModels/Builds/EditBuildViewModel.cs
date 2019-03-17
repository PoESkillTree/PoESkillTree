using System;
using System.Collections.Generic;
using PoESkillTree.Common.ViewModels;
using PoESkillTree.Model.Builds;

namespace PoESkillTree.ViewModels.Builds
{
    public class EditBuildViewModel : ErrorInfoViewModel<bool>
    {
        private readonly IBuildViewModel<PoEBuild> _buildVm;
        private readonly BuildValidator _buildValidator;
        private string _name;
        private string _note;
        private string _characterName;
        private string _accountName;

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public string Note
        {
            get { return _note; }
            set { SetProperty(ref _note, value); }
        }

        public string CharacterName
        {
            get { return _characterName; }
            set { SetProperty(ref _characterName, value); }
        }

        public string AccountName
        {
            get { return _accountName; }
            set { SetProperty(ref _accountName, value); }
        }

        public DateTime LastUpdated { get; }

        public EditBuildViewModel(IBuildViewModel<PoEBuild> buildVm, BuildValidator buildValidator)
        {
            _buildVm = buildVm;
            _buildValidator = buildValidator;
            var build = buildVm.Build;
            Name = build.Name;
            Note = build.Note;
            CharacterName = build.CharacterName;
            AccountName = build.AccountName;
            LastUpdated = build.LastUpdated;
        }

        protected override IEnumerable<string> ValidateProperty(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(Name):
                    return new[] {_buildValidator.ValidateExistingFileName(Name, _buildVm)};
                default:
                    return null;
            }
        }

        protected override bool CanClose(bool param)
        {
            // Always allow canceling
            return !param || !HasErrors;
        }
    }
}
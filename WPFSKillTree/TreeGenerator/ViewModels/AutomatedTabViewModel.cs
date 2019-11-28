using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoESkillTree.Controls.Dialogs;
using PoESkillTree.Localization;
using PoESkillTree.Model.JsonSettings;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.TreeGenerator.Settings;
using PoESkillTree.TreeGenerator.Solver;

namespace PoESkillTree.TreeGenerator.ViewModels
{
    public sealed class AutomatedTabViewModel : GeneratorTabViewModel
    {
        protected override string Key { get; } = "AutomatedTab";

        public override string DisplayName { get; } = L10n.Message("Automated");

        protected override IReadOnlyList<ISetting> SubSettings { get; } = new ISetting[0];

        public AutomatedTabViewModel(SkillTree tree, IDialogCoordinator dialogCoordinator, object dialogContext,
            Action<GeneratorTabViewModel> runCallback)
            : base(tree, dialogCoordinator, dialogContext, 1, runCallback)
        {
        }

        protected override Task<ISolver?> CreateSolverAsync(SolverSettings settings)
        {
            throw new NotImplementedException();
        }
    }
}
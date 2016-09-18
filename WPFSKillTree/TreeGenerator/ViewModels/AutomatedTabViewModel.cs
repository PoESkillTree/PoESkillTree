using System;
using System.Collections.Generic;
using POESKillTree.Controls.Dialogs;
using POESKillTree.Localization;
using POESKillTree.Model.JsonSettings;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.Settings;
using POESKillTree.TreeGenerator.Solver;

namespace POESKillTree.TreeGenerator.ViewModels
{
    public sealed class AutomatedTabViewModel : GeneratorTabViewModel
    {
        protected override string Key { get; } = "AutomatedTab";

        protected override IReadOnlyList<ISetting> SubSettings { get; } = new ISetting[0];

        public AutomatedTabViewModel(SkillTree tree, IDialogCoordinator dialogCoordinator,
            Action<GeneratorTabViewModel> runCallback)
            : base(tree, dialogCoordinator, 1, runCallback)
        {
            DisplayName = L10n.Message("Automated");
        }

        protected override ISolver CreateSolver(SolverSettings settings)
        {
            throw new System.NotImplementedException();
        }
    }
}
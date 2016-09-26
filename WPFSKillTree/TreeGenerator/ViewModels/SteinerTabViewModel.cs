using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using POESKillTree.Controls.Dialogs;
using POESKillTree.Localization;
using POESKillTree.Model.JsonSettings;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.Settings;
using POESKillTree.TreeGenerator.Solver;

namespace POESKillTree.TreeGenerator.ViewModels
{
    /// <summary>
    /// GeneratorTabViewModel for skilling tagged nodes with SteinerSolver.
    /// Nothing special done here.
    /// </summary>
    public sealed class SteinerTabViewModel : GeneratorTabViewModel
    {
        protected override string Key { get; } = "SteinerTab";

        protected override IReadOnlyList<ISetting> SubSettings { get; }

        /// <summary>
        /// Instantiates a new SteinerTabViewModel.
        /// </summary>
        /// <param name="tree">The (not null) SkillTree instance to operate on.</param>
        /// <param name="dialogCoordinator">The <see cref="IDialogCoordinator"/> used to display dialogs.</param>
        /// <param name="dialogContext">The context used for <paramref name="dialogCoordinator"/>.</param>
        /// <param name="runCallback">The action that is called when RunCommand is executed.</param>
        public SteinerTabViewModel(SkillTree tree, IDialogCoordinator dialogCoordinator, object dialogContext,
            Action<GeneratorTabViewModel> runCallback)
            : base(tree, dialogCoordinator, dialogContext, 1, runCallback)
        {
            DisplayName = L10n.Message("Tagged Nodes");
            SubSettings = new[] {ExcludeCrossed};
        }

        protected override async Task<ISolver> CreateSolverAsync(SolverSettings settings)
        {
            if (!settings.Checked.Any())
            {
                // todo "this" as context is not registered when running without window
                await DialogCoordinator.ShowInfoAsync(DialogContext,
                        L10n.Message("Please tag non-skilled nodes by right-clicking them."));
                return null;
            }
            return new SteinerSolver(Tree, settings);
        }
    }
}
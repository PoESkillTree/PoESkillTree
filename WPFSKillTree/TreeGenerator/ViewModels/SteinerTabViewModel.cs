using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PoESkillTree.Controls.Dialogs;
using PoESkillTree.Localization;
using PoESkillTree.Model.JsonSettings;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.TreeGenerator.Settings;
using PoESkillTree.TreeGenerator.Solver;

namespace PoESkillTree.TreeGenerator.ViewModels
{
    /// <summary>
    /// GeneratorTabViewModel for skilling tagged nodes with SteinerSolver.
    /// Nothing special done here.
    /// </summary>
    public sealed class SteinerTabViewModel : GeneratorTabViewModel
    {
        protected override string Key { get; } = "SteinerTab";

        public override string DisplayName { get; } = L10n.Message("Tagged Nodes");

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
            SubSettings = new[] {ExcludeCrossed};
        }

        protected override async Task<ISolver?> CreateSolverAsync(SolverSettings settings)
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
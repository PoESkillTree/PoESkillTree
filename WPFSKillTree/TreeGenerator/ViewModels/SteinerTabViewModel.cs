using System.Collections.Generic;
using System.Linq;
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

        protected override IReadOnlyList<ISetting> SubSettings { get; } = new ISetting[0];

        /// <summary>
        /// Instantiates a new SteinerTabViewModel.
        /// </summary>
        /// <param name="tree">The (not null) SkillTree instance to operate on.</param>
        /// <param name="dialogCoordinator">The <see cref="IDialogCoordinator"/> used to display dialogs.</param>
        public SteinerTabViewModel(SkillTree tree, IDialogCoordinator dialogCoordinator)
            : base(tree, dialogCoordinator)
        {
            DisplayName = L10n.Message("Tagged Nodes");
            UsesFullSettingsSet = false;
        }

        public override ISolver CreateSolver(SolverSettings settings)
        {
            if (settings.Iterations != 1)
            {
                settings = new SolverSettings(settings.Level, settings.TotalPoints, settings.Checked,
                    settings.Crossed, settings.SubsetTree, settings.InitialTree, 1);
            }
            if (!settings.Checked.Any())
            {
                DialogCoordinator.ShowInfoAsync(this,
                        L10n.Message("Please tag non-skilled nodes by right-clicking them."));
                return null;
            }
            return new SteinerSolver(Tree, settings);
        }
    }
}
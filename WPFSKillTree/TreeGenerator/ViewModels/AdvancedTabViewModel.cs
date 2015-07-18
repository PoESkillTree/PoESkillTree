using System;
using System.Collections.Generic;
using POESKillTree.Localization;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.Settings;
using POESKillTree.TreeGenerator.Solver;

namespace POESKillTree.TreeGenerator.ViewModels
{
    public sealed class AdvancedTabViewModel : GeneratorTabViewModel
    {
        // TODO basic advanced generator with only stat constraints
        // TODO GeneticAlgorithm.randomBitArray() flipped bits dependent upon Total points (larger tree -> more bits set)
        // TODO option to load stat constraints from current tree
        // TODO extend advanced generator with combined stats
        // TODO automatically generate constraints -> automated generator

        public AdvancedTabViewModel(SkillTree tree) : base(tree)
        {
            DisplayName = L10n.Message("Advanced");
        }

        public override ISolver CreateSolver(SolverSettings settings)
        {
            var statConstraints = new Dictionary<string, Tuple<float, double>>
            {
                {"#% increased maximum Life", new Tuple<float, double>(100, 1)}
            };
            return new AdvancedSolver(Tree, new AdvancedSolverSettings(settings, statConstraints, null));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using POESKillTree.Localization;
using POESKillTree.Model;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.Settings;
using POESKillTree.TreeGenerator.Solver;

namespace POESKillTree.TreeGenerator.ViewModels
{

    public class StatConstraint
    {
        public string Stat { get; set; }

        public float TargetValue { get; set; }

        public double Weight { get; set; }

        public StatConstraint()
        {
            TargetValue = 0;
            Weight = 100;
        }

        public StatConstraint(string stat)
            : this()
        {
            Stat = stat;
        }
    }

    public sealed class AdvancedTabViewModel : GeneratorTabViewModel
    {
        // TODO UI for stat constraints
        // - weight slider
        // - auto generated stat names (from Skill tree)
        // - use for CreateSolver()

        // TODO better way of calculating weighting in csvs
        // TODO GeneticAlgorithm.randomBitArray() flipped bits dependent upon Total points (larger tree -> more bits set)?
        // TODO some kind of heuristic that notables (or full clusters) are generally better?
        // TODO exclude keystones not explicitly included (check-tagged)
        // TODO option to load stat constraints from current tree

        // TODO extend advanced generator with combined stats
        // - tab in the normal UI to switch between stats imported from gear and manually typed stats
        // - bandit support
        // - some way to display different skill gems, support gems (maybe), weapon types

        // TODO automatically generate constraints -> automated generator

        public string[] Stats { get; private set; }

        public ObservableCollection<StatConstraint> StatConstraints { get; private set; }

        private RelayCommand _removeStatConstraintCommand;

        public ICommand RemoveStatConstraintCommand
        {
            get
            {
                return _removeStatConstraintCommand ??
                       (_removeStatConstraintCommand =
                           new RelayCommand(
                               param =>
                               {
                                   var constraint = param as StatConstraint;
                                   if (constraint != null)
                                       StatConstraints.Remove(constraint);
                               },
                               param =>
                               {
                                   var constraint = param as StatConstraint;
                                   return constraint != null && constraint.Stat != null;
                               }));
            }
        }

        public AdvancedTabViewModel(SkillTree tree) : base(tree)
        {
            Stats = new[]
            {
                "#% increased maximum life",
                "+# to maximum frenzy charges",
                "#% increased physical damage",
                "#% increased attack speed"
            };
            StatConstraints = new ObservableCollection<StatConstraint>(new[]
            {
                new StatConstraint(Stats[2]),
                new StatConstraint(Stats[3]),
                new StatConstraint(Stats[0])
            });
            DisplayName = L10n.Message("Advanced");
        }

        public override ISolver CreateSolver(SolverSettings settings)
        {
            var statConstraints = new Dictionary<string, Tuple<float, double>>
            {
                // https://www.pathofexile.com/passive-skill-tree/AAAAAwMAAv4EBwSzBUIHHg3RDkgPqxEtEZYTcRZAFm8Wvxo4HRQgbiSLJKomlScvKU8qOCycLR8t0jbpOlg8BT1fQZZCw0SrRZ1G10lRTLNQMFF0UlNTUlVLVa5VxlcpV5RYrlnzXfJepV8qYeJjQ2SdZp5o8mu3cFJw1XGFcg98g3_GghCCm4PbhX2J04w2jmSO6Y9Gj6aP-pBVlSCVLpeVmjua4J8-n9-iAKKjppmnCK6zsUK2-sBmwfPDOtD11abbXt-E34rfsONq6-Tr7uv17TzvfPAf8NXyHfMR96b60g==
                // Checked: Vaal Pact, Celestial Judgement, Celestial Punishment, Spell Damage per Power Charge
                {"#% increased maximum Life", new Tuple<float, double>(130, 1)},
                {"#% increased Mana Regeneration Rate", new Tuple<float, double>(100, 1)},
                {"+# Maximum Endurance Charge", new Tuple<float, double>(4, 1)},
                {"+# Maximum Power Charge", new Tuple<float, double>(6, 1)},
                {"#% increased Cold Damage", new Tuple<float, double>(70, 1)},
                {"#% increased Radius of Area Skills", new Tuple<float, double>(36, 1)},
                {"#% increased Area Damage", new Tuple<float, double>(20, 1)},
                {"#% increased Elemental Damage", new Tuple<float, double>(75, 1)},
                {"#% increased Critical Strike Chance for Spells", new Tuple<float, double>(265, 1)},
                {"#% increased Global Critical Strike Chance while wielding a Staff", new Tuple<float, double>(80, 1)},
                {"#% increased Global Critical Strike Multiplier while wielding a Staff", new Tuple<float, double>(30, 1)},
                {"#% increased Critical Strike Chance", new Tuple<float, double>(120, 1)},
                {"#% increased Critical Strike Multiplier", new Tuple<float, double>(30, 1)},
                {"#% increased Critical Strike Multiplier for Spells", new Tuple<float, double>(50, 1)},
                {"#% increased Cast Speed", new Tuple<float, double>(20, 1)},
                {"#% increased Elemental Damage with Spells", new Tuple<float, double>(16, 1)},
                {"#% increased Spell Damage", new Tuple<float, double>(100, 1)}
            };
            return new AdvancedSolver(Tree, new AdvancedSolverSettings(settings, statConstraints, null));
        }
    }
}
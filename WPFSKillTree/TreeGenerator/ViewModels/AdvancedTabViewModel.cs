using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using POESKillTree.Localization;
using POESKillTree.Model;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.Model;
using POESKillTree.TreeGenerator.Settings;
using POESKillTree.TreeGenerator.Solver;
using POESKillTree.Utils;

namespace POESKillTree.TreeGenerator.ViewModels
{
    public sealed class AdvancedTabViewModel : GeneratorTabViewModel
    {
        // TODO UI for attribute constraints
        // - substring match searchable ComboBox (something like the poe.trade comboboxes)
        // - use for CreateSolver()

        // TODO exclude keystones not check-tagged
        // TODO option to load stat constraints from current tree

        // TODO better way of calculating weighting in csvs
        // TODO GeneticAlgorithm.randomBitArray() flipped bits dependent upon Total points (larger tree -> more bits set)?
        // TODO some kind of heuristic that notables (or full clusters) are generally better?
        // TODO proper delete-row-button-icon

        // TODO extend advanced generator with combined stats
        // - tab in the normal UI to switch between stats imported from gear and manually typed stats
        // - bandit support
        // - some way to display different skill gems, support gems (maybe), weapon types

        // TODO automatically generate constraints -> automated generator

        private static readonly GroupStringConverter AttrGroupConverter = new GroupStringConverter();

        private static readonly Dictionary<string, int> AttrGroupOrder = new Dictionary<string, int>()
        {
            // General
            {L10n.Message("Core Attributes"), 0},
            {L10n.Message("General"), 1},
            {L10n.Message("Keystone"), 2},
            {L10n.Message("Charges"), 3},
            // Defense
            {L10n.Message("Defense"), 4},
            {L10n.Message("Block"), 5},
            {L10n.Message("Shield"), 6},
            // Offense
            {L10n.Message("Weapon"), 7},
            {L10n.Message("Spell"), 8},
            {L10n.Message("Critical Strike"), 9},
            // Alternate Spell groups
            {L10n.Message("Aura"), 10},
            {L10n.Message("Curse"), 11},
            {L10n.Message("Minion"), 12},
            {L10n.Message("Trap"), 13},
            {L10n.Message("Totem"), 14},
            {"Everything else", 15}
        };

        public ObservableCollection<string> Attributes { get; }

        public ObservableCollection<AttributeConstraint> AttributeConstraints { get; }

        private bool _canAddAttrConstraints = true;

        public bool CanAddAttrConstraints
        {
            get { return _canAddAttrConstraints; }
            set
            {
                if (value == _canAddAttrConstraints) return;
                _canAddAttrConstraints = value;
                OnPropertyChanged("CanAddAttrConstraints");
            }
        }

        private RelayCommand _removeAttributeConstraintCommand;

        public ICommand RemoveAttributeConstraintCommand
        {
            get
            {
                return _removeAttributeConstraintCommand ??
                       (_removeAttributeConstraintCommand =
                           new RelayCommand(
                               param =>
                               {
                                   AttributeConstraints.Remove((AttributeConstraint) param);
                                   // The whole property is needed because the DataGrid doesn't add a new NewItemPlaceholder if
                                   // you delete a new row while adding it. Resetting CanUserAddRows seems to be the only workaround for it.
                                   CanAddAttrConstraints = !CanAddAttrConstraints;
                                   CanAddAttrConstraints = !CanAddAttrConstraints;
                               },
                               param => param is AttributeConstraint));
            }
        }

        public AdvancedTabViewModel(SkillTree tree) : base(tree)
        {
            var attrList = CreatePossibleAttributes();
            attrList.Sort((s1, s2) => AttrGroupOrder[AttrGroupConverter.Convert(s1).GroupName] - AttrGroupOrder[AttrGroupConverter.Convert(s2).GroupName]);
            Attributes = new ObservableCollection<string>(attrList);

            AttributeConstraints = new ObservableCollection<AttributeConstraint>
            {
                new AttributeConstraint(Attributes[100]),
                new AttributeConstraint(Attributes[200]),
                new AttributeConstraint(Attributes[0])
            };

            DisplayName = L10n.Message("Advanced");
        }

        private static List<string> CreatePossibleAttributes()
        {
            var all = new HashSet<string>();
            var nonUnique = new HashSet<string>();
            foreach (var node in SkillTree.Skillnodes)
            {
                foreach (var attribute in node.Value.Attributes)
                {
                    if (all.Contains(attribute.Key))
                    {
                        nonUnique.Add(attribute.Key);
                    }
                    else
                    {
                        all.Add(attribute.Key);
                    }
                }
            }
            return nonUnique.ToList();
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
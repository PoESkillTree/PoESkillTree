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
        // TODO initial stats (SettingsViewModel)
        // TODO substring match searchable ComboBox (something like the poe.trade comboboxes)
        // TODO better way of calculating weighting in csvs
        // TODO GeneticAlgorithm.randomBitArray() flipped bits dependent upon Total points (larger tree -> more bits set)?
        // TODO some kind of heuristic that notables (or full clusters) are generally better?
        // TODO proper delete-row-button-icon
        // TODO inform the user that keystones must be checked if they should be included
        //      (can lead to unconnected trees if notables behind keystones are checked)

        // TODO extend advanced generator with combined stats
        // - tab in the normal UI to switch between stats imported from gear and manually typed stats
        // - bandit support
        // - some way to display different skill gems, support gems (maybe), weapon types
        // - Compute.cs refactoring to be used in Fitness Function

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

        private RelayCommand _loadAttributesCommand;

        public ICommand LoadAttributesCommand
        {
            get
            {
                return _loadAttributesCommand ??
                       (_loadAttributesCommand = new RelayCommand(param => LoadAttributesFromTree()));
            }
        }

        public AdvancedTabViewModel(SkillTree tree) : base(tree)
        {
            var attrList = CreatePossibleAttributes();
            attrList.Sort((s1, s2) =>
            {
                var attrSort = AttrGroupOrder[AttrGroupConverter.Convert(s1).GroupName] -
                               AttrGroupOrder[AttrGroupConverter.Convert(s2).GroupName];
                if (attrSort != 0) return attrSort;
                return string.Compare(s1, s2, StringComparison.Ordinal);
            });
            Attributes = new ObservableCollection<string>(attrList);

            AttributeConstraints = new ObservableCollection<AttributeConstraint>();

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

        private void LoadAttributesFromTree()
        {
            Tree.UntagAllNodes();

            // TODO once initial stats generation is implemented in SettingsViewModel, they need to be added here too
            var attributes = new Dictionary<string, float>();
            foreach (var node in Tree.SkilledNodes)
            {
                var skillNode = SkillTree.Skillnodes[node];
                var hasUniqueAttribute = false;
                foreach (var attribute in skillNode.Attributes)
                {
                    var attr = attribute.Key;
                    if (Attributes.Contains(attr))
                    {
                        if (attribute.Value.Count == 0)
                        {
                            continue;
                        }
                        if (attributes.ContainsKey(attr))
                        {
                            attributes[attr] += attribute.Value[0];
                        }
                        else
                        {
                            attributes[attr] = attribute.Value[0];
                        }
                    }
                    else
                    {
                        hasUniqueAttribute = true;
                    }
                }
                if (hasUniqueAttribute)
                {
                    Tree.CycleNodeTagForward(skillNode);
                }
            }

            AttributeConstraints.Clear();
            foreach (var attribute in attributes)
            {
                AttributeConstraints.Add(new AttributeConstraint(attribute.Key) {TargetValue = attribute.Value});
            }
        }

        public override ISolver CreateSolver(SolverSettings settings)
        {
            var attributeConstraints = AttributeConstraints.ToDictionary(constraint => constraint.Attribute,
                constraint => new Tuple<float, double>(constraint.TargetValue, constraint.Weight / 100.0));
            return new AdvancedSolver(Tree, new AdvancedSolverSettings(settings, attributeConstraints, null));
        }
    }
}
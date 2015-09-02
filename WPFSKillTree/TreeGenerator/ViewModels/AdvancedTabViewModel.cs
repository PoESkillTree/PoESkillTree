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

        private static readonly Dictionary<string, float> AttributesPerLevel = new Dictionary<string, float>()
        {
            {"+# to maximum Mana", SkillTree.ManaPerLevel},
            {"+# to maximum Life", SkillTree.LifePerLevel},
            {"+# Accuracy Rating", SkillTree.AccPerLevel},
            {"Evasion Rating: #", SkillTree.EvasPerLevel}
        };

        private static readonly List<string> AttributeBlackList = new List<string>()
        {
            "+# to maximum Mana",
            "+# to maximum Life",
            "+# Accuracy Rating",
            "+# to maximum Energy Shield"
        };

        #region Presentation

        public ObservableCollection<string> Attributes { get; private set; }

        public ObservableCollection<AttributeConstraint> AttributeConstraints { get; private set; }

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
        
        private bool _importItems;

        public bool ImportItems
        {
            get { return _importItems; }
            set
            {
                if (value == _importItems) return;
                _importItems = value;
                OnPropertyChanged("ImportItems");
            }
        }

        #endregion

        #region Commands

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

        #endregion

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
                    var key = attribute.Key;
                    if (AttributeBlackList.Contains(key)) continue;

                    if (all.Contains(key))
                    {
                        nonUnique.Add(key);
                    }
                    else
                    {
                        all.Add(key);
                    }
                }
            }
            return nonUnique.ToList();
        }

        private void LoadAttributesFromTree()
        {
            Tree.UntagAllNodes();
            
            // Character class changes after calling this method will not influence the attributes
            // (which have the old character class calculated into them)
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

            foreach (var attr in CreateInitialAttributes())
            {
                if (attributes.ContainsKey(attr.Key))
                {
                    attributes[attr.Key] += attr.Value;
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
            return new AdvancedSolver(Tree, new AdvancedSolverSettings(settings, CreateInitialAttributes(), attributeConstraints, null));
        }
        
        private Dictionary<string, float> CreateInitialAttributes()
        {
            // base attributes: SkillTree.BaseAttributes, SkillTree.CharBaseAttributes
            var stats = new Dictionary<string, float>(SkillTree.BaseAttributes);
            foreach (var attr in SkillTree.CharBaseAttributes[Tree.Chartype])
            {
                stats[attr.Key] = attr.Value;
            }
            // Level attributes (flat mana, life, evasion and accuracy) are blacklisted, because they are also dependent
            // on core attributes, which are dependent on the actual tree and are pretty pointless as basic attributes anyway.
            // For the calculation of pseudo attributes, they need to be included however.
            foreach (var attr in AttributesPerLevel)
            {
                if (stats.ContainsKey(attr.Key))
                {
                    stats[attr.Key] += Tree.Level*attr.Value;
                }
                else
                {
                    stats[attr.Key] = Tree.Level*attr.Value;
                }
            }

            if (_importItems)
            {
                // TODO add attributes from items (tree+gear mode)
            }
            return stats;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using POESKillTree.Localization;
using POESKillTree.Model;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.Model;
using POESKillTree.TreeGenerator.Model.PseudoAttributes;
using POESKillTree.TreeGenerator.Settings;
using POESKillTree.TreeGenerator.Solver;
using POESKillTree.Utils.Converter;

namespace POESKillTree.TreeGenerator.ViewModels
{
    public sealed class AdvancedTabViewModel : GeneratorTabViewModel
    {

        #region Attribute constants

        /// <summary>
        /// Converts attribute strings to their group names.
        /// </summary>
        private static readonly GroupStringConverter AttrGroupConverter = new GroupStringConverter();

        /// <summary>
        /// Order in which the attribute groups are shown.
        /// </summary>
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

        /// <summary>
        /// Dictionary of attributes influenced by character level with the ratio per level.
        /// </summary>
        private static readonly Dictionary<string, float> AttributesPerLevel = new Dictionary<string, float>()
        {
            {"+# to maximum Mana", SkillTree.ManaPerLevel},
            {"+# to maximum Life", SkillTree.LifePerLevel},
            {"+# Accuracy Rating", SkillTree.AccPerLevel},
            {"Evasion Rating: #", SkillTree.EvasPerLevel}
        };

        /// <summary>
        /// List of not selectable attributes.
        /// </summary>
        private static readonly List<string> AttributeBlackList = new List<string>()
        {
            "+# to maximum Mana",
            "+# to maximum Life",
            "+# Accuracy Rating",
            "+# to maximum Energy Shield"
        };

        #endregion
        
        public static IEnumerable<WeaponClass> WeaponClassValues
        {
            get { return Enum.GetValues(typeof(WeaponClass)).Cast<WeaponClass>(); }
        }

        #region Presentation

        public ObservableCollection<string> Attributes { get; private set; }

        public ObservableCollection<AttributeConstraint> AttributeConstraints { get; private set; }

        private bool _canAddAttrConstraints = true;

        public bool CanAddAttrConstraints
        {
            get { return _canAddAttrConstraints; }
            set { SetProperty(ref _canAddAttrConstraints, value); }
        }

        private readonly HashSet<PseudoAttribute> _addedPseudoAttributes = new HashSet<PseudoAttribute>();

        private readonly ObservableCollection<PseudoAttribute> _pseudoAttributes;

        public ListCollectionView PseudoAttributesView { get; private set; }

        public ObservableCollection<PseudoAttributeConstraint> PseudoAttributeConstraints { get; private set; }

        private PseudoAttributeConstraint _newPseudoAttributeConstraint;

        public PseudoAttributeConstraint NewPseudoAttributeConstraint
        {
            get { return _newPseudoAttributeConstraint; }
            private set { SetProperty(ref _newPseudoAttributeConstraint, value); }
        }

        private bool _importItems;

        public bool ImportItems
        {
            get { return _importItems; }
            set { SetProperty(ref _importItems, value); }
        }

        private WeaponClass _weaponClass = WeaponClass.Unarmed;

        public WeaponClass WeaponClass
        {
            get { return _weaponClass; }
            set
            {
                SetProperty(ref _weaponClass, value,
                    () => WeaponClassIsTwoHanded = _weaponClass.IsTwoHanded());
            }
        }

        private bool _weaponClassIsTwoHanded;

        public bool WeaponClassIsTwoHanded
        {
            get { return _weaponClassIsTwoHanded; }
            private set
            {
                SetProperty(ref _weaponClassIsTwoHanded, value,
                    () => OffHand = _weaponClassIsTwoHanded ? OffHand.TwoHanded : OffHand.Shield);
            }
        }

        private OffHand _offHand = OffHand.Shield;

        public OffHand OffHand
        {
            get { return _offHand; }
            set { SetProperty(ref _offHand, value); }
        }

        private Tags _tags = Tags.None;

        public Tags Tags
        {
            get { return _tags; }
            set { SetProperty(ref _tags, value); }
        }

        #endregion

        #region Commands

        private RelayCommand _removeAttributeConstraintCommand;

        public ICommand RemoveAttributeConstraintCommand
        {
            get
            {
                return _removeAttributeConstraintCommand ?? (_removeAttributeConstraintCommand = new RelayCommand(
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

        private RelayCommand _addPseudoConstraintCommand;

        public ICommand AddPseudoConstraintCommand
        {
            get
            {
                return _addPseudoConstraintCommand ?? (_addPseudoConstraintCommand = new RelayCommand(
                    param =>
                    {
                        var newConstraint = (PseudoAttributeConstraint) NewPseudoAttributeConstraint.Clone();
                        _addedPseudoAttributes.Add(newConstraint.Data);
                        PseudoAttributesView.Refresh();

                        NewPseudoAttributeConstraint.Data = _addedPseudoAttributes.Count < _pseudoAttributes.Count ? (PseudoAttribute)PseudoAttributesView.GetItemAt(0) : null;
                        PseudoAttributeConstraints.Add(newConstraint);
                    },
                    param => _addedPseudoAttributes.Count < _pseudoAttributes.Count));
            }
        }

        private RelayCommand _removePseudoConstraintCommand;

        public ICommand RemovePseudoConstraintCommand
        {
            get
            {
                return _removePseudoConstraintCommand ?? (_removePseudoConstraintCommand = new RelayCommand(
                    param =>
                    {
                        var oldConstraint = (PseudoAttributeConstraint) param;
                        _addedPseudoAttributes.Remove(oldConstraint.Data);
                        PseudoAttributesView.Refresh();

                        NewPseudoAttributeConstraint = oldConstraint;
                        PseudoAttributeConstraints.Remove(oldConstraint);
                    },
                    param => param is PseudoAttributeConstraint));
            }
        }

        #endregion

        public AdvancedTabViewModel(SkillTree tree) : base(tree)
        {
            var attrList = CreatePossibleAttributes().ToList();
            attrList.Sort((s1, s2) =>
            {
                var attrSort = AttrGroupOrder[AttrGroupConverter.Convert(s1).GroupName] -
                               AttrGroupOrder[AttrGroupConverter.Convert(s2).GroupName];
                if (attrSort != 0) return attrSort;
                return string.CompareOrdinal(s1, s2);
            });
            Attributes = new ObservableCollection<string>(attrList);

            AttributeConstraints = new ObservableCollection<AttributeConstraint>();

            var pseudos = new PseudoAttributeLoader().LoadPseudoAttributes();
            _pseudoAttributes = new ObservableCollection<PseudoAttribute>(pseudos);
            PseudoAttributesView = (ListCollectionView) CollectionViewSource.GetDefaultView(_pseudoAttributes);
            PseudoAttributesView.Filter = item => !_addedPseudoAttributes.Contains((PseudoAttribute) item);
            PseudoAttributesView.SortDescriptions.Add(new SortDescription("Group", ListSortDirection.Ascending));
            PseudoAttributesView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            Debug.Assert(PseudoAttributesView.GroupDescriptions != null, "PseudoAttributesView.GroupDescriptions != null");
            PseudoAttributesView.GroupDescriptions.Add(new PropertyGroupDescription("Group"));

            PseudoAttributeConstraints = new ObservableCollection<PseudoAttributeConstraint>();
            NewPseudoAttributeConstraint = new PseudoAttributeConstraint((PseudoAttribute)PseudoAttributesView.GetItemAt(0));

            DisplayName = L10n.Message("Advanced");
        }

        /// <summary>
        /// Creates possible attributes from the SkillTree nodes.
        /// Only non unique and not blacklisted attributes are selected.
        /// </summary>
        private static IEnumerable<string> CreatePossibleAttributes()
        {
            return from node in SkillTree.Skillnodes.Values
                   from attr in SkillTree.ExpandHybridAttributes(node.Attributes)
                   where !AttributeBlackList.Contains(attr.Key)
                   group attr by attr.Key into attrGroup
                   where attrGroup.Count() > 1
                   select attrGroup.First().Key;
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
                foreach (var attribute in SkillTree.ExpandHybridAttributes(skillNode.Attributes))
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
            var attributeConstraints = AttributeConstraints.ToDictionary(
                constraint => constraint.Data,
                constraint => new Tuple<float, double>(constraint.TargetValue, constraint.Weight / 100.0));
            var pseudoConstraints = PseudoAttributeConstraints.ToDictionary(
                constraint => constraint.Data,
                constraint => new Tuple<float, double>(constraint.TargetValue, constraint.Weight / 100.0));
            return new AdvancedSolver(Tree, new AdvancedSolverSettings(settings, CreateInitialAttributes(), attributeConstraints,
                pseudoConstraints, WeaponClass, Tags, OffHand));
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
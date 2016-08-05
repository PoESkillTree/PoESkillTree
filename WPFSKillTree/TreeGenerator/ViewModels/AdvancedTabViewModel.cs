using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using POESKillTree.Common.ViewModels;
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
    // Some aliases to make things clearer without the need of extra classes.
    using AttributeConstraint = TargetWeightConstraint<string>;
    using PseudoAttributeConstraint = TargetWeightConstraint<PseudoAttribute>;

    /// <summary>
    /// GeneratorTabViewModel that uses user specified constraints based
    /// on attributes to generate skill trees.
    /// </summary>
    public sealed class AdvancedTabViewModel : GeneratorTabViewModel
    {
        /// <summary>
        /// Converts attributes to their groups. Similar to <see cref="GroupStringConverter"/>
        /// except that it additionally groups attributes in <see cref="PopularAttributes"/> together
        /// and caches the calculations to a dictionary.
        /// </summary>
        private class AttributeToGroupConverter : IValueConverter
        {
            private static readonly GroupStringConverter GroupStringConverter = new GroupStringConverter();

            private readonly Dictionary<string, string> _attributeToGroupDictionary = new Dictionary<string, string>();

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return Convert(value.ToString());
            }

            public string Convert(string attrName)
            {
                string groupName;
                if (!_attributeToGroupDictionary.TryGetValue(attrName, out groupName))
                {
                    groupName = PopularAttributes.Contains(attrName)
                        ? PopularGroupName
                        : GroupStringConverter.Convert(attrName).GroupName;
                    _attributeToGroupDictionary[attrName] = groupName;
                }
                return groupName;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }

        #region Attribute constants

        /// <summary>
        /// Converts attribute strings to their group names.
        /// </summary>
        private static readonly AttributeToGroupConverter AttrToGroupConverter = new AttributeToGroupConverter();

        private static readonly string PopularGroupName = L10n.Message("Popular");

        /// <summary>
        /// Order in which the attribute groups are shown.
        /// </summary>
        private static readonly Dictionary<string, int> AttrGroupOrder = new Dictionary<string, int>()
        {
            {PopularGroupName, -1},
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
            {L10n.Message("Flasks"), 15 },
            {L10n.Message("Everything Else"), 16}
        };

        /// <summary>
        /// List of attributes that should be displayed before others.
        /// </summary>
        private static readonly HashSet<string> PopularAttributes = new HashSet<string>()
        {
            "+# to Dexterity", "+# to Intelligence", "+# to Strength",
            "#% increased Movement Speed", "#% increased maximum Life", "#% of Life Regenerated per Second",
            "#% of Physical Attack Damage Leeched as Mana",
            "#% increased effect of Auras you Cast", "#% reduced Mana Reserved",
            "+# to Jewel Socket"
        };

        /// <summary>
        /// Dictionary of attributes influenced by character level with the ratio per level.
        /// </summary>
        private static readonly Dictionary<string, float> AttributesPerLevel = new Dictionary<string, float>()
        {
            {"+# to maximum Mana", Constants.ManaPerLevel},
            {"+# to maximum Life", Constants.LifePerLevel},
            {"+# Accuracy Rating", Constants.AccPerLevel},
            {"Evasion Rating: #", Constants.EvasPerLevel}
        };

        /// <summary>
        /// List of not selectable attributes.
        /// </summary>
        private static readonly HashSet<string> AttributeBlackList = new HashSet<string>()
        {
            "+# to maximum Mana",
            "+# to maximum Life",
            "+# Accuracy Rating",
            "+# to maximum Energy Shield"
        };

        #endregion

        /// <summary>
        /// Gets all values of the WeaponClass Enum.
        /// </summary>
        public static IEnumerable<WeaponClass> WeaponClassValues
        {
            get { return Enum.GetValues(typeof(WeaponClass)).Cast<WeaponClass>(); }
        }

        #region Presentation

        private readonly HashSet<string> _addedAttributes = new HashSet<string>();

        /// <summary>
        /// The collection of attributes that can be used in AttributeConstraints.
        /// </summary>
        private readonly List<string> _attributes;

        /// <summary>
        /// Gets the CollectionView to the attribute names the user can use.
        /// </summary>
        public ICollectionView AttributesView { get; private set; }

        /// <summary>
        /// Gets the collection of AttributeConstraints the user specified.
        /// </summary>
        public ObservableCollection<AttributeConstraint> AttributeConstraints { get; private set; }

        private AttributeConstraint _newAttributeConstraint;
        /// <summary>
        /// Gets the AttributeConstraint used for creating new AttributeConstraints by the user.
        /// </summary>
        public AttributeConstraint NewAttributeConstraint
        {
            get { return _newAttributeConstraint; }
            private set { SetProperty(ref _newAttributeConstraint, value); }
        }

        /// <summary>
        /// HashSet of PseudoAttributes already added as PseudoAttributeConstraint.
        /// </summary>
        private readonly HashSet<PseudoAttribute> _addedPseudoAttributes = new HashSet<PseudoAttribute>();

        /// <summary>
        /// Collection of pseudo attributes that can be used in PseudoAttributeConstraints.
        /// </summary>
        private readonly ObservableCollection<PseudoAttribute> _pseudoAttributes = new ObservableCollection<PseudoAttribute>();

        /// <summary>
        /// Gets the CollectionView to the PseudoAttributes the user can use.
        /// </summary>
        public ICollectionView PseudoAttributesView { get; private set; }

        /// <summary>
        /// Gets the collection of PseudoAttributeConstraints the user specified.
        /// </summary>
        public ObservableCollection<PseudoAttributeConstraint> PseudoAttributeConstraints { get; private set; }

        /// <summary>
        /// Placeholder for the PseudoAttributeConstraint the user is editing that can be added.
        /// </summary>
        private PseudoAttributeConstraint _newPseudoAttributeConstraint;

        /// <summary>
        /// Gets the PseudoAttributeConstraint used for creating new ones by the user.
        /// </summary>
        public PseudoAttributeConstraint NewPseudoAttributeConstraint
        {
            get { return _newPseudoAttributeConstraint; }
            private set { SetProperty(ref _newPseudoAttributeConstraint, value); }
        }

        private const bool TreePlusItemsModeDefaultValue = false;
        private bool _treePlusItemsMode = TreePlusItemsModeDefaultValue;
        /// <summary>
        /// Gets or sets if the Tab should use 'Tree + Items' or 'Tree only' mode.
        /// (has no effect at the moment)
        /// </summary>
        public bool TreePlusItemsMode
        {
            get { return _treePlusItemsMode; }
            set { SetProperty(ref _treePlusItemsMode, value); }
        }

        private const WeaponClass WeaponClassDefaultValue = WeaponClass.Unarmed;
        private WeaponClass _weaponClass = WeaponClassDefaultValue;
        /// <summary>
        /// Gets or sets the WeaponClass used for pseudo attribute calculations.
        /// </summary>
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
        /// <summary>
        /// Gets whether the currently selected WeaponClass is a two handed class.
        /// </summary>
        public bool WeaponClassIsTwoHanded
        {
            get { return _weaponClassIsTwoHanded; }
            private set
            {
                SetProperty(ref _weaponClassIsTwoHanded, value,
                    () => OffHand = _weaponClassIsTwoHanded ? OffHand.TwoHanded : OffHand.Shield);
            }
        }

        private const OffHand OffHandDefaultValue = OffHand.Shield;
        private OffHand _offHand = OffHandDefaultValue;
        /// <summary>
        /// Gets or sets the OffHand used for pseudo attribute calculations.
        /// </summary>
        public OffHand OffHand
        {
            get { return _offHand; }
            set { SetProperty(ref _offHand, value); }
        }

        private const Tags TagsDefaultValue = Tags.None;
        private Tags _tags = TagsDefaultValue;
        /// <summary>
        /// Gets or sets the Tags used for pseudo attribute calculations.
        /// </summary>
        public Tags Tags
        {
            get { return _tags; }
            set { SetProperty(ref _tags, value); }
        }

        #endregion

        #region Commands

        private RelayCommand _addAttributeConstraintCommand;
        /// <summary>
        /// Gets the command to add an AttributeConstraint to the collection.
        /// </summary>
        public ICommand AddAttributeConstraintCommand
        {
            get
            {
                return _addAttributeConstraintCommand ?? (_addAttributeConstraintCommand = new RelayCommand(
                    () =>
                    {
                        var newConstraint = (AttributeConstraint)NewAttributeConstraint.Clone();
                        _addedAttributes.Add(newConstraint.Data);
                        AttributesView.Refresh();

                        AttributesView.MoveCurrentToFirst();
                        NewAttributeConstraint.Data = AttributesView.CurrentItem as string;
                        AttributeConstraints.Add(newConstraint);
                    },
                    () => _addedAttributes.Count < _attributes.Count));
            }
        }

        private ICommand _removeAttributeConstraintCommand;
        /// <summary>
        /// Gets the command to remove an AttributeConstraint from the collection.
        /// </summary>
        public ICommand RemoveAttributeConstraintCommand
        {
            get
            {
                return _removeAttributeConstraintCommand ?? (_removeAttributeConstraintCommand = new RelayCommand<AttributeConstraint>(
                    param =>
                    {
                        var oldConstraint = param;
                        _addedAttributes.Remove(oldConstraint.Data);
                        AttributesView.Refresh();

                        NewAttributeConstraint = oldConstraint;
                        AttributeConstraints.Remove(oldConstraint);
                    }));
            }
        }

        private RelayCommand _loadAttributesFromTreeCommand;
        /// <summary>
        /// Gets the command to load the attributes from the current tree as AttributeConstraints.
        /// </summary>
        public ICommand LoadAttributesFromTreeCommand
        {
            get
            {
                return _loadAttributesFromTreeCommand ??
                       (_loadAttributesFromTreeCommand = new RelayCommand(LoadAttributesFromTree));
            }
        }

        private RelayCommand _addPseudoConstraintCommand;
        /// <summary>
        /// Gets the command to add a PseudoAttributeConstraint to the collection.
        /// </summary>
        public ICommand AddPseudoConstraintCommand
        {
            get
            {
                return _addPseudoConstraintCommand ?? (_addPseudoConstraintCommand = new RelayCommand(
                    () =>
                    {
                        var newConstraint = (PseudoAttributeConstraint) NewPseudoAttributeConstraint.Clone();
                        _addedPseudoAttributes.Add(newConstraint.Data);
                        PseudoAttributesView.Refresh();

                        PseudoAttributesView.MoveCurrentToFirst();
                        NewPseudoAttributeConstraint.Data = PseudoAttributesView.CurrentItem as PseudoAttribute;
                        PseudoAttributeConstraints.Add(newConstraint);
                    },
                    () => _addedPseudoAttributes.Count < _pseudoAttributes.Count));
            }
        }

        private ICommand _removePseudoConstraintCommand;
        /// <summary>
        /// Gets the command to remove a PseudoAttributeConstraint from the collection.
        /// </summary>
        public ICommand RemovePseudoConstraintCommand
        {
            get
            {
                return _removePseudoConstraintCommand ?? (_removePseudoConstraintCommand = new RelayCommand<PseudoAttributeConstraint>(
                    param =>
                    {
                        var oldConstraint = param;
                        _addedPseudoAttributes.Remove(oldConstraint.Data);
                        PseudoAttributesView.Refresh();

                        NewPseudoAttributeConstraint = oldConstraint;
                        PseudoAttributeConstraints.Remove(oldConstraint);
                    }));
            }
        }

        private RelayCommand _reloadPseudoAttributesCommand;
        /// <summary>
        /// Gets the command to reload the possible PseudoAttributes from the filesystem.
        /// Removes all user specified PseudoAttributeConstraints.
        /// </summary>
        public ICommand ReloadPseudoAttributesCommand
        {
            get
            {
                return _reloadPseudoAttributesCommand ??
                       (_reloadPseudoAttributesCommand = new RelayCommand(ReloadPseudoAttributes));
            }
        }

        private RelayCommand _convertAttributeToPseudoConstraintsCommand;
        /// <summary>
        /// Gets the command to converts attribute constraints to pseudo attribute constraints where possible.
        /// </summary>
        public ICommand ConvertAttributeToPseudoConstraintsCommand
        {
            get
            {
                return _convertAttributeToPseudoConstraintsCommand ??
                       (_convertAttributeToPseudoConstraintsCommand = new RelayCommand(
                           ConverteAttributeToPseudoAttributeConstraints,
                           () => AttributeConstraints.Count > 0));
            }
        }

        #endregion

        private readonly PseudoAttributeLoader _pseudoAttributeLoader = new PseudoAttributeLoader();

        /// <summary>
        /// Instantiates a new AdvancedTabViewModel.
        /// </summary>
        /// <param name="tree">The (not null) SkillTree instance to operate on.</param>
        public AdvancedTabViewModel(SkillTree tree) : base(tree)
        {
            _attributes = CreatePossibleAttributes().ToList();
            AttributesView = new ListCollectionView(_attributes)
            {
                Filter = item => !_addedAttributes.Contains(item),
                CustomSort = Comparer<string>.Create((s1, s2) =>
                {
                    // Sort by group as in AttrGroupOrder first and then by name.
                    var groupCompare = AttrGroupOrder[AttrToGroupConverter.Convert(s1)].CompareTo(
                        AttrGroupOrder[AttrToGroupConverter.Convert(s2)]);
                    return groupCompare != 0 ? groupCompare : string.CompareOrdinal(s1, s2);
                })
            };
            AttributesView.GroupDescriptions.Add(new PropertyGroupDescription(".", AttrToGroupConverter));
            AttributesView.MoveCurrentToFirst();
            AttributeConstraints = new ObservableCollection<AttributeConstraint>();
            NewAttributeConstraint = new AttributeConstraint(AttributesView.CurrentItem as string);

            PseudoAttributesView = new ListCollectionView(_pseudoAttributes)
            {
                Filter = item => !_addedPseudoAttributes.Contains((PseudoAttribute) item)
            };
            PseudoAttributesView.SortDescriptions.Add(new SortDescription("Group", ListSortDirection.Ascending));
            PseudoAttributesView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            PseudoAttributesView.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            PseudoAttributeConstraints = new ObservableCollection<PseudoAttributeConstraint>();

            ReloadPseudoAttributes();

            DisplayName = L10n.Message("Advanced");
        }

        public override void Reset()
        {
            _addedAttributes.Clear();
            AttributeConstraints.Clear();
            AttributesView.Refresh();
            AttributesView.MoveCurrentToFirst();
            NewAttributeConstraint = new AttributeConstraint(AttributesView.CurrentItem as string);
            _addedPseudoAttributes.Clear();
            PseudoAttributeConstraints.Clear();
            PseudoAttributesView.Refresh();
            PseudoAttributesView.MoveCurrentToFirst();
            NewPseudoAttributeConstraint =
                new PseudoAttributeConstraint(PseudoAttributesView.CurrentItem as PseudoAttribute);
            TreePlusItemsMode = TreePlusItemsModeDefaultValue;
            WeaponClass = WeaponClassDefaultValue;
            OffHand = OffHandDefaultValue;
            Tags = TagsDefaultValue;
        }

        /// <summary>
        /// Creates possible attributes from the SkillTree nodes.
        /// Unique and blacklisted attributes are not taken.
        /// Attributes of ascendancy nodes are ignored.
        /// Attributes must have at least one '#' in their name (which means they have a value).
        /// </summary>
        private static IEnumerable<string> CreatePossibleAttributes()
        {
            return from node in SkillTree.Skillnodes.Values
                   where node.ascendancyName == null
                   from attr in SkillTree.ExpandHybridAttributes(node.Attributes)
                   where !AttributeBlackList.Contains(attr.Key) && attr.Key.Contains("#")
                   group attr by attr.Key into attrGroup
                   where attrGroup.Count() > 1
                   select attrGroup.First().Key;
        }

        /// <summary>
        /// Reloads the possible PseudoAttributes from the filesystem.
        /// Resets PseudoAttributeConstraints entered by the user.
        /// </summary>
        private void ReloadPseudoAttributes()
        {
            _addedPseudoAttributes.Clear();
            _pseudoAttributes.Clear();
            foreach (var pseudo in _pseudoAttributeLoader.LoadPseudoAttributes())
            {
                _pseudoAttributes.Add(pseudo);
            }
            PseudoAttributeConstraints.Clear();
            PseudoAttributesView.MoveCurrentToFirst();
            NewPseudoAttributeConstraint = new PseudoAttributeConstraint(PseudoAttributesView.CurrentItem as PseudoAttribute);
        }

        /// <summary>
        /// Loads the attributes from the current tree as AttributeConstraints
        /// and Check-tags nodes that have unique attributes.
        /// </summary>
        /// <remarks>
        /// Character class changes after calling this method will not influence the attributes
        /// (which have the old character class calculated into them)
        /// </remarks>
        private void LoadAttributesFromTree()
        {
            Tree.UntagAllNodes();

            var attributes = new Dictionary<string, float>();
            foreach (var node in Tree.SkilledNodes)
            {
                var skillNode = SkillTree.Skillnodes[node];
                var hasUniqueAttribute = false;
                foreach (var attribute in SkillTree.ExpandHybridAttributes(skillNode.Attributes))
                {
                    var attr = attribute.Key;
                    if (_attributes.Contains(attr))
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
                    else if (!AttributeBlackList.Contains(attr))
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

        /// <summary>
        /// Converts attribute constraints to pseudo attribute constraints with the set Tags, WeaponClass,
        /// OffHand and check-tagged keystones where possible.
        /// Str, Int and Dex are not removed from the attribute constraint list but still converted.
        /// </summary>
        private void ConverteAttributeToPseudoAttributeConstraints()
        {
            var keystones = from id in Tree.GetCheckedNodes()
                            where SkillTree.Skillnodes[id].Type == NodeType.Keystone
                            select SkillTree.Skillnodes[id].Name;
            var conditionSettings = new ConditionSettings(Tags, OffHand, keystones.ToArray(), WeaponClass);
            var convertedConstraints = new List<AttributeConstraint>();
            foreach (var attributeConstraint in AttributeConstraints)
            {
                var attrName = attributeConstraint.Data;
                // Select the pseudo attributes and the multiplier for attributes which match the given one and evaluate to true.
                var pseudos =
                    from pseudo in _pseudoAttributes
                    let matches = (from attr in pseudo.Attributes
                                   where attr.MatchesAndEvaluates(conditionSettings, attrName)
                                   select attr)
                    where matches.Any()
                    select new { PseudoAttribute = pseudo, Multiplier = matches.First().ConversionMultiplier };

                // Add attribute target and weight to the pseudo attributes.
                var converted = false;
                foreach (var pseudo in pseudos)
                {
                    var pseudoAttribute = pseudo.PseudoAttribute;
                    converted = true;
                    if (_addedPseudoAttributes.Contains(pseudoAttribute))
                    {
                        foreach (var pseudoAttributeConstraint in PseudoAttributeConstraints)
                        {
                            if (pseudoAttributeConstraint.Data == pseudoAttribute)
                            {
                                pseudoAttributeConstraint.TargetValue += attributeConstraint.TargetValue * pseudo.Multiplier;
                            }
                        }
                    }
                    else
                    {
                        _addedPseudoAttributes.Add(pseudoAttribute);
                        PseudoAttributeConstraints.Add(new PseudoAttributeConstraint(pseudoAttribute)
                        {
                            TargetValue = attributeConstraint.TargetValue * pseudo.Multiplier
                        });
                    }
                }

                if (converted &&
                    attrName != "+# to Intelligence" && attrName != "+# to Dexterity" && attrName != "+# to Strength")
                {
                    convertedConstraints.Add(attributeConstraint);
                }
            }

            // Update the attribute constraint related collections.
            foreach (var convertedConstraint in convertedConstraints)
            {
                _addedAttributes.Remove(convertedConstraint.Data);
                AttributeConstraints.Remove(convertedConstraint);
            }
            if (convertedConstraints.Count > 0)
            {
                AttributesView.Refresh();
                NewAttributeConstraint = convertedConstraints[0];
                PseudoAttributesView.Refresh();
                PseudoAttributesView.MoveCurrentToFirst();
                NewPseudoAttributeConstraint.Data = PseudoAttributesView.CurrentItem as PseudoAttribute;
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

        /// <summary>
        /// Creates the attributes the skill tree has with these settings initially
        /// (without any tree generating done).
        /// </summary>
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

            if (_treePlusItemsMode)
            {
                // TODO add attributes from items (tree+gear mode)
            }
            return stats;
        }
    }
}
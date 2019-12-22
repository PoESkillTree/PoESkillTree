using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using MoreLinq;
using Newtonsoft.Json.Linq;
using PoESkillTree.Common.ViewModels;
using PoESkillTree.Controls.Dialogs;
using PoESkillTree.Engine.GameModel.PassiveTree;
using PoESkillTree.Engine.Utils.Extensions;
using PoESkillTree.Localization;
using PoESkillTree.Model;
using PoESkillTree.Model.JsonSettings;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.TreeGenerator.Model;
using PoESkillTree.TreeGenerator.Model.PseudoAttributes;
using PoESkillTree.TreeGenerator.Settings;
using PoESkillTree.TreeGenerator.Solver;
using PoESkillTree.Utils.Converter;

namespace PoESkillTree.TreeGenerator.ViewModels
{
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

            public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            {
                return Convert(value?.ToString());
            }

            [return: NotNullIfNotNull("attrName")]
            public string? Convert(string? attrName)
            {
                if (attrName is null)
                    return null;
                if (!_attributeToGroupDictionary.TryGetValue(attrName, out var groupName))
                {
                    groupName = PopularAttributes.Contains(attrName)
                        ? PopularGroupName
                        : GroupStringConverter.Convert(attrName).GroupName;
                    _attributeToGroupDictionary[attrName] = groupName;
                }
                return groupName;
            }

            public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Setting that converts between JSON and AttributeConstraints/PseudoAttributeConstraints.
        /// </summary>
        private class ConstraintsSetting : ISetting
        {
            private const string AttributeKey = "Attribute";
            private const string TargetValueKey = "TargetValue";
            private const string WeightKey = "Weight";

            private readonly AdvancedTabViewModel _vm;

            public ConstraintsSetting(AdvancedTabViewModel vm)
            {
                _vm = vm;
            }

            public void LoadFrom(JObject jObject)
            {
                _vm.ClearAttributeConstraints();
                if (jObject.TryGetValue(nameof(AttributeConstraints), out var token) && token.Any())
                {
                    var newConstraints = new List<TargetWeightConstraint<string>>();
                    foreach (var element in token)
                    {
                        var obj = element as JObject;
                        if (obj == null)
                            continue;
                        if (!obj.TryGetValue(AttributeKey, out var attrToken)
                            || !obj.TryGetValue(TargetValueKey, out var targetToken)
                            || !obj.TryGetValue(WeightKey, out var weightToken))
                            continue;

                        var attr = attrToken.ToObject<string>()!;
                        newConstraints.Add(new TargetWeightConstraint<string>(attr)
                        {
                            TargetValue = targetToken.ToObject<float>(),
                            Weight = weightToken.ToObject<int>()
                        });
                        _vm._addedAttributes.Add(attr);
                    }

                    _vm.AttributesView.Refresh();
                    _vm.AttributesView.MoveCurrentToFirst();
                    _vm.NewAttributeConstraint.Data = _vm.AttributesView.CurrentItem as string;
                    _vm.AttributeConstraints.AddRange(newConstraints);
                }

                _vm.ClearPseudoAttributeConstraints();
                if (jObject.TryGetValue(nameof(PseudoAttributeConstraints), out token) && token.Any())
                {
                    var pseudoDict = _vm._pseudoAttributes.ToDictionary(p => p.Name);

                    var newConstraints = new List<TargetWeightConstraint<PseudoAttribute>>();
                    foreach (var element in token)
                    {
                        var obj = element as JObject;
                        if (obj == null)
                            continue;
                        if (!obj.TryGetValue(AttributeKey, out var attrToken)
                            || !obj.TryGetValue(TargetValueKey, out var targetToken)
                            || !obj.TryGetValue(WeightKey, out var weightToken))
                            continue;

                        if (!pseudoDict.TryGetValue(attrToken.ToObject<string>()!, out var attr))
                            continue;
                        newConstraints.Add(new TargetWeightConstraint<PseudoAttribute>(attr)
                        {
                            TargetValue = targetToken.ToObject<float>(),
                            Weight = weightToken.ToObject<int>()
                        });
                        _vm._addedPseudoAttributes.Add(attr);
                    }

                    _vm.PseudoAttributesView.Refresh();
                    _vm.PseudoAttributesView.MoveCurrentToFirst();
                    _vm.NewPseudoAttributeConstraint.Data = _vm.PseudoAttributesView.CurrentItem as PseudoAttribute;
                    _vm.PseudoAttributeConstraints.AddRange(newConstraints);
                }
            }

            public bool SaveTo(JObject jObject)
            {
                var changed = false;
                var attrArray = new JArray();
                _vm.AttributeConstraints.ForEach(c => AddTo(attrArray, c.Data, c.TargetValue, c.Weight));
                if (jObject.TryGetValue(nameof(AttributeConstraints), out var oldToken))
                {
                    changed = !JToken.DeepEquals(attrArray, oldToken);
                }
                jObject[nameof(AttributeConstraints)] = attrArray;

                var pseudoArray = new JArray();
                _vm.PseudoAttributeConstraints.ForEach(c => AddTo(pseudoArray, c.Data.Name, c.TargetValue, c.Weight));
                if (!changed && jObject.TryGetValue(nameof(PseudoAttributeConstraints), out oldToken)
                    && !JToken.DeepEquals(pseudoArray, oldToken))
                {
                    changed = true;
                }
                jObject[nameof(PseudoAttributeConstraints)] = pseudoArray;
                return changed;
            }

            private static void AddTo(JArray array, string attribute, float targetValue, int weight)
            {
                array.Add(new JObject
                {
                    {AttributeKey, new JValue(attribute)},
                    {TargetValueKey, new JValue(targetValue)},
                    {WeightKey, new JValue(weight)}
                });
            }

            public void Reset()
            {
                _vm.ClearAttributeConstraints();
                _vm.ClearPseudoAttributeConstraints();
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
        public static IEnumerable<WeaponClass> WeaponClassValues => Enum.GetValues(typeof(WeaponClass)).Cast<WeaponClass>();

        public override string DisplayName { get; } = L10n.Message("Advanced");

        protected override string Key { get; } = "AdvancedTab";

        protected override IReadOnlyList<ISetting> SubSettings { get; }

        #region Presentation

        private int _totalPoints;

        /// <summary>
        /// The number of points on top of those provided by level that
        /// the solver can use.
        /// </summary>
        public LeafSetting<int> AdditionalPoints { get; }

        /// <summary>
        /// Gets the total number of points the solver can use.
        /// Equals Level - 1 + <see cref="AdditionalPoints"/>.
        /// </summary>
        public int TotalPoints
        {
            get => _totalPoints;
            private set => SetProperty(ref _totalPoints, value);
        }

        private readonly HashSet<string> _addedAttributes = new HashSet<string>();

        /// <summary>
        /// The collection of attributes that can be used in AttributeConstraints.
        /// </summary>
        private readonly List<string> _attributes;

        /// <summary>
        /// Gets the CollectionView to the attribute names the user can use.
        /// </summary>
        public ICollectionView AttributesView { get; }

        /// <summary>
        /// Gets the collection of AttributeConstraints the user specified.
        /// </summary>
        public ObservableCollection<TargetWeightConstraint<string>> AttributeConstraints { get; }

        private TargetWeightConstraint<string?> _newAttributeConstraint;
        /// <summary>
        /// Gets the AttributeConstraint used for creating new AttributeConstraints by the user.
        /// </summary>
        public TargetWeightConstraint<string?> NewAttributeConstraint
        {
            get => _newAttributeConstraint;
            private set => SetProperty(ref _newAttributeConstraint, value);
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
        public ICollectionView PseudoAttributesView { get; }

        /// <summary>
        /// Gets the collection of PseudoAttributeConstraints the user specified.
        /// </summary>
        public ObservableCollection<TargetWeightConstraint<PseudoAttribute>> PseudoAttributeConstraints { get; }

        /// <summary>
        /// Placeholder for the PseudoAttributeConstraint the user is editing that can be added.
        /// </summary>
        private TargetWeightConstraint<PseudoAttribute?> _newPseudoAttributeConstraint = default!;

        /// <summary>
        /// Gets the PseudoAttributeConstraint used for creating new ones by the user.
        /// </summary>
        public TargetWeightConstraint<PseudoAttribute?> NewPseudoAttributeConstraint
        {
            get => _newPseudoAttributeConstraint;
            private set => SetProperty(ref _newPseudoAttributeConstraint, value);
        }

        /// <summary>
        /// Whether the Tab should use 'Tree + Items' or 'Tree only' mode.
        /// (has no effect at the moment)
        /// </summary>
        public LeafSetting<bool> TreePlusItemsMode { get; }

        /// <summary>
        /// WeaponClass used for pseudo attribute calculations.
        /// </summary>
        public LeafSetting<WeaponClass> WeaponClass { get; }

        private bool _weaponClassIsTwoHanded;
        /// <summary>
        /// Gets whether the currently selected WeaponClass is a two handed class.
        /// </summary>
        public bool WeaponClassIsTwoHanded
        {
            get => _weaponClassIsTwoHanded;
            private set
            {
                SetProperty(ref _weaponClassIsTwoHanded, value,
                    () => OffHand.Value = _weaponClassIsTwoHanded ? Model.PseudoAttributes.OffHand.TwoHanded : Model.PseudoAttributes.OffHand.Shield);
            }
        }

        /// <summary>
        /// OffHand used for pseudo attribute calculations.
        /// </summary>
        public LeafSetting<OffHand> OffHand { get; }

        /// <summary>
        /// Tags used for pseudo attribute calculations.
        /// </summary>
        public LeafSetting<Tags> Tags { get; }

        public IPersistentData PersistentData { get; }

        #endregion

        #region Commands

        private RelayCommand? _resetCommand;
        /// <summary>
        /// Resets all Properties to the values they had on construction.
        /// Calls <see cref="GeneratorTabViewModel.Reset"/> on all tabs.
        /// </summary>
        public ICommand ResetCommand => _resetCommand ??= new RelayCommand(Reset);

        private RelayCommand? _addAttributeConstraintCommand;
        /// <summary>
        /// Gets the command to add an AttributeConstraint to the collection.
        /// </summary>
        public ICommand AddAttributeConstraintCommand
        {
            get
            {
                return _addAttributeConstraintCommand ??= new RelayCommand(
                    () =>
                    {
                        var newConstraint = (TargetWeightConstraint<string>)NewAttributeConstraint.Clone();
                        _addedAttributes.Add(newConstraint.Data);
                        AttributesView.Refresh();

                        AttributesView.MoveCurrentToFirst();
                        NewAttributeConstraint.Data = AttributesView.CurrentItem as string;
                        AttributeConstraints.Add(newConstraint);
                    },
                    () => _addedAttributes.Count < _attributes.Count);
            }
        }

        private ICommand? _removeAttributeConstraintCommand;
        /// <summary>
        /// Gets the command to remove an AttributeConstraint from the collection.
        /// </summary>
        public ICommand RemoveAttributeConstraintCommand
        {
            get
            {
                return _removeAttributeConstraintCommand ??= new RelayCommand<TargetWeightConstraint<string>>(
                    param =>
                    {
                        var oldConstraint = param;
                        _addedAttributes.Remove(oldConstraint.Data);
                        AttributesView.Refresh();

                        NewAttributeConstraint.Data = oldConstraint.Data;
                        NewAttributeConstraint.TargetValue = oldConstraint.TargetValue;
                        NewAttributeConstraint.Weight = oldConstraint.Weight;
                        AttributeConstraints.Remove(oldConstraint);
                    });
            }
        }

        private RelayCommand? _loadAttributesFromTreeCommand;
        /// <summary>
        /// Gets the command to load the attributes from the current tree as AttributeConstraints.
        /// </summary>
        public ICommand LoadAttributesFromTreeCommand =>
            _loadAttributesFromTreeCommand ??= new RelayCommand(LoadAttributesFromTree);

        private RelayCommand? _addPseudoConstraintCommand;
        /// <summary>
        /// Gets the command to add a PseudoAttributeConstraint to the collection.
        /// </summary>
        public ICommand AddPseudoConstraintCommand
        {
            get
            {
                return _addPseudoConstraintCommand ??= new RelayCommand(
                    () =>
                    {
                        var newConstraint = (TargetWeightConstraint<PseudoAttribute>) NewPseudoAttributeConstraint.Clone();
                        _addedPseudoAttributes.Add(newConstraint.Data);
                        PseudoAttributesView.Refresh();

                        PseudoAttributesView.MoveCurrentToFirst();
                        NewPseudoAttributeConstraint.Data = PseudoAttributesView.CurrentItem as PseudoAttribute;
                        PseudoAttributeConstraints.Add(newConstraint);
                    },
                    () => _addedPseudoAttributes.Count < _pseudoAttributes.Count);
            }
        }

        private ICommand? _removePseudoConstraintCommand;
        /// <summary>
        /// Gets the command to remove a PseudoAttributeConstraint from the collection.
        /// </summary>
        public ICommand RemovePseudoConstraintCommand
        {
            get
            {
                return _removePseudoConstraintCommand ??= new RelayCommand<TargetWeightConstraint<PseudoAttribute>>(
                    param =>
                    {
                        var oldConstraint = param;
                        _addedPseudoAttributes.Remove(oldConstraint.Data);
                        PseudoAttributesView.Refresh();

                        NewPseudoAttributeConstraint.Data = oldConstraint.Data;
                        NewPseudoAttributeConstraint.TargetValue = oldConstraint.TargetValue;
                        NewPseudoAttributeConstraint.Weight = oldConstraint.Weight;
                        PseudoAttributeConstraints.Remove(oldConstraint);
                    });
            }
        }

        private RelayCommand? _reloadPseudoAttributesCommand;
        /// <summary>
        /// Gets the command to reload the possible PseudoAttributes from the filesystem.
        /// Removes all user specified PseudoAttributeConstraints.
        /// </summary>
        public ICommand ReloadPseudoAttributesCommand =>
            _reloadPseudoAttributesCommand ??= new RelayCommand(ReloadPseudoAttributes);

        private RelayCommand? _convertAttributeToPseudoConstraintsCommand;
        /// <summary>
        /// Gets the command to converts attribute constraints to pseudo attribute constraints where possible.
        /// </summary>
        public ICommand ConvertAttributeToPseudoConstraintsCommand
        {
            get
            {
                return _convertAttributeToPseudoConstraintsCommand ??= new RelayCommand(
                    ConvertAttributeToPseudoAttributeConstraints,
                    () => AttributeConstraints.Count > 0);
            }
        }

        #endregion

        private readonly PseudoAttributeLoader _pseudoAttributeLoader = new PseudoAttributeLoader();

        /// <summary>
        /// Instantiates a new AdvancedTabViewModel.
        /// </summary>
        /// <param name="tree">The (not null) SkillTree instance to operate on.</param>
        /// <param name="persistentData"></param>
        /// <param name="dialogCoordinator">The <see cref="IDialogCoordinator"/> used to display dialogs.</param>
        /// <param name="dialogContext">The context used for <paramref name="dialogCoordinator"/>.</param>
        /// <param name="runCallback">The action that is called when RunCommand is executed.</param>
        public AdvancedTabViewModel(SkillTree tree, IPersistentData persistentData, IDialogCoordinator dialogCoordinator, object dialogContext,
            Action<GeneratorTabViewModel> runCallback)
            : base(tree, dialogCoordinator, dialogContext, 3, runCallback)
        {
            PersistentData = persistentData;
            AdditionalPoints = new LeafSetting<int>(nameof(AdditionalPoints), 22, CalculateTotalPoints);
            CalculateTotalPoints();
            TreePlusItemsMode = new LeafSetting<bool>(nameof(TreePlusItemsMode), false);
            WeaponClass = new LeafSetting<WeaponClass>(nameof(WeaponClass), Model.PseudoAttributes.WeaponClass.Unarmed,
                () => WeaponClassIsTwoHanded = WeaponClass.Value.IsTwoHanded());
            OffHand = new LeafSetting<OffHand>(nameof(OffHand), Model.PseudoAttributes.OffHand.Shield);
            Tags = new LeafSetting<Tags>(nameof(Tags), Model.PseudoAttributes.Tags.None);

            PersistentData.PropertyChanging += (sender, args) =>
            {
                if (args.PropertyName == nameof(IPersistentData.CurrentBuild))
                {
                    PersistentData.CurrentBuild.PropertyChanged -= CurrentBuildOnPropertyChanged;
                }
            };
            PersistentData.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(IPersistentData.CurrentBuild))
                {
                    PersistentData.CurrentBuild.PropertyChanged += CurrentBuildOnPropertyChanged;
                    CalculateTotalPoints();
                }
            };

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
            AttributeConstraints = new ObservableCollection<TargetWeightConstraint<string>>();
            _newAttributeConstraint = new TargetWeightConstraint<string?>(AttributesView.CurrentItem as string);

            PseudoAttributesView = new ListCollectionView(_pseudoAttributes)
            {
                Filter = item => !_addedPseudoAttributes.Contains((PseudoAttribute) item)
            };
            PseudoAttributesView.SortDescriptions.Add(new SortDescription(nameof(PseudoAttribute.Group), ListSortDirection.Ascending));
            PseudoAttributesView.SortDescriptions.Add(new SortDescription(nameof(PseudoAttribute.Name), ListSortDirection.Ascending));
            PseudoAttributesView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PseudoAttribute.Group)));
            PseudoAttributeConstraints = new ObservableCollection<TargetWeightConstraint<PseudoAttribute>>();

            ReloadPseudoAttributes();

            SubSettings = new ISetting[]
            {
                AdditionalPoints, Iterations, IncludeChecked, ExcludeCrossed,
                TreePlusItemsMode, WeaponClass, OffHand, Tags,
                new ConstraintsSetting(this)
            };
        }

        private void CurrentBuildOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PoESkillTree.Model.Builds.PoEBuild.Level))
            {
                CalculateTotalPoints();
            }
        }

        private void CalculateTotalPoints() =>
            TotalPoints = PersistentData.CurrentBuild.Level - 1 + AdditionalPoints.Value;

        private void ClearAttributeConstraints()
        {
            _addedAttributes.Clear();
            AttributeConstraints.Clear();
            AttributesView.Refresh();
            AttributesView.MoveCurrentToFirst();
            NewAttributeConstraint = new TargetWeightConstraint<string?>(AttributesView.CurrentItem as string);
        }

        private void ClearPseudoAttributeConstraints()
        {
            _addedPseudoAttributes.Clear();
            PseudoAttributeConstraints.Clear();
            PseudoAttributesView.Refresh();
            PseudoAttributesView.MoveCurrentToFirst();
            NewPseudoAttributeConstraint =
                new TargetWeightConstraint<PseudoAttribute?>(PseudoAttributesView.CurrentItem as PseudoAttribute);
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
                   where !node.IsAscendancyNode
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
            NewPseudoAttributeConstraint = new TargetWeightConstraint<PseudoAttribute?>(PseudoAttributesView.CurrentItem as PseudoAttribute);
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
            var attributes = new Dictionary<string, float>();
            var unique = new List<SkillNode>();
            foreach (var node in Tree.SkilledNodes)
            {
                var hasUniqueAttribute = false;
                foreach (var attribute in SkillTree.ExpandHybridAttributes(node.Attributes))
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
                    unique.Add(node);
                }
            }
            Tree.SetCheckTaggedNodes(unique);

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
                AttributeConstraints.Add(new TargetWeightConstraint<string>(attribute.Key) {TargetValue = attribute.Value});
            }
        }

        /// <summary>
        /// Converts attribute constraints to pseudo attribute constraints with the set Tags, WeaponClass,
        /// OffHand and check-tagged keystones where possible.
        /// Str, Int and Dex are not removed from the attribute constraint list but still converted.
        /// </summary>
        private void ConvertAttributeToPseudoAttributeConstraints()
        {
            var keystones = from node in Tree.GetCheckedNodes()
                            where node.Type == PassiveNodeType.Keystone
                            select node.Name;
            var conditionSettings = new ConditionSettings(Tags.Value, OffHand.Value, keystones.ToArray(), WeaponClass.Value);
            var convertedConstraints = new List<TargetWeightConstraint<string>>();
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
                        PseudoAttributeConstraints.Add(new TargetWeightConstraint<PseudoAttribute>(pseudoAttribute)
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
                NewAttributeConstraint.Data = convertedConstraints[0].Data;
                PseudoAttributesView.Refresh();
                PseudoAttributesView.MoveCurrentToFirst();
                NewPseudoAttributeConstraint.Data = PseudoAttributesView.CurrentItem as PseudoAttribute;
            }
        }

        protected override Task<ISolver?> CreateSolverAsync(SolverSettings settings)
        {
            var attributeConstraints = AttributeConstraints.ToDictionary(
                constraint => constraint.Data,
                constraint => new Tuple<float, double>(constraint.TargetValue, constraint.Weight / 100.0));
            var pseudoConstraints = PseudoAttributeConstraints.ToDictionary(
                constraint => constraint.Data,
                constraint => new Tuple<float, double>(constraint.TargetValue, constraint.Weight / 100.0));
            var solver = new AdvancedSolver(Tree, new AdvancedSolverSettings(settings, TotalPoints,
                CreateInitialAttributes(), attributeConstraints,
                pseudoConstraints, WeaponClass.Value, Tags.Value, OffHand.Value));
            return Task.FromResult<ISolver?>(solver);
        }

        /// <summary>
        /// Creates the attributes the skill tree has with these settings initially
        /// (without any tree generating done).
        /// </summary>
        private Dictionary<string, float> CreateInitialAttributes()
        {
            // base attributes: SkillTree.BaseAttributes, SkillTree.CharBaseAttributes
            var stats = SkillTree.BaseAttributes
                .Concat(SkillTree.CharBaseAttributes[Tree.CharClass])
                .ToDictionary();
            // Level attributes (flat mana, life, evasion and accuracy) are blacklisted, because they are also dependent
            // on core attributes, which are dependent on the actual tree and are pretty pointless as basic attributes anyway.
            // For the calculation of pseudo attributes, they need to be included however.
            var level = PersistentData.CurrentBuild.Level;
            foreach (var attr in AttributesPerLevel)
            {
                if (stats.ContainsKey(attr.Key))
                {
                    stats[attr.Key] += level*attr.Value;
                }
                else
                {
                    stats[attr.Key] = level*attr.Value;
                }
            }

            if (TreePlusItemsMode.Value)
            {
                // TODO add attributes from items (tree+gear mode)
            }
            return stats;
        }
    }
}
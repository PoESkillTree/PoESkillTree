using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json.Linq;
using POESKillTree.Computation.Model;
using POESKillTree.SkillTreeFiles;
using POESKillTree.Utils;

namespace POESKillTree.Model.Builds
{
    /// <summary>
    /// <see cref="IBuild"/> implementation that represents a single build, a leaf in the build tree.
    /// Has a dirty flag that is set with every property change and can be reset. Can be reverted to the state of the
    /// last dirty flag reset.
    /// </summary>
    public class PoEBuild : AbstractBuild<PoEBuild>
    {
        private string _note;
        private string _characterName;
        private string _accountName;
        private string _league;
        private int _level = 1;
        private string _treeUrl = Constants.DefaultTree;
        private string _itemData;
        private DateTime _lastUpdated = DateTime.Now;
        private ObservableCollection<string[]> _customGroups;
        private BanditSettings _bandits;
        private ObservableSet<ushort> _checkedNodeIds;
        private ObservableSet<ushort> _crossedNodeIds;
        private ConfigurationStats _configurationStats;
        private JObject _additionalData;
        private bool _isDirty;
        private IMemento<PoEBuild> _memento;

        /// <summary>
        /// Gets or sets a arbitrary note.
        /// </summary>
        public string Note
        {
            get => _note;
            set => SetProperty(ref _note, value);
        }

        /// <summary>
        /// Gets or sets the character name this builds represents.
        /// </summary>
        public string CharacterName
        {
            get => _characterName;
            set => SetProperty(ref _characterName, value);
        }

        /// <summary>
        /// Gets or sets the account name that owns the represented character.
        /// </summary>
        public string AccountName
        {
            get => _accountName;
            set => SetProperty(ref _accountName, value);
        }

        /// <summary>
        /// Gets or sets the league of the represented character.
        /// </summary>
        public string League
        {
            get => _league;
            set => SetProperty(ref _league, value);
        }

        /// <summary>
        /// Gets or sets the level of the represented character.
        /// </summary>
        public int Level
        {
            get => _level;
            set => SetProperty(ref _level, value);
        }

        /// <summary>
        /// Gets or sets the build defining skill tree URL.
        /// </summary>
        public string TreeUrl
        {
            get => _treeUrl;
            set => SetProperty(ref _treeUrl, value);
        }

        /// <summary>
        /// Gets or sets the item data of this build as serialized JSON.
        /// </summary>
        public string ItemData
        {
            get => _itemData;
            set => SetProperty(ref _itemData, value);
        }

        /// <summary>
        /// Gets or sets the last time this build was updated and saved.
        /// </summary>
        public DateTime LastUpdated
        {
            get => _lastUpdated;
            set => SetProperty(ref _lastUpdated, value);
        }

        /// <summary>
        /// Gets the custom attribute grouping of this build.
        /// </summary>
        public ObservableCollection<string[]> CustomGroups
        {
            get => _customGroups;
            private set => SetProperty(ref _customGroups, value);
        }

        /// <summary>
        /// Gets the bandit settings of this build.
        /// </summary>
        public BanditSettings Bandits
        {
            get => _bandits;
            private set => SetProperty(ref _bandits, value);
        }

        /// <summary>
        /// Gets a set containing the ids of the nodes check tagged in this build.
        /// </summary>
        public ObservableSet<ushort> CheckedNodeIds
        {
            get => _checkedNodeIds;
            private set => SetProperty(ref _checkedNodeIds, value);
        }

        /// <summary>
        /// Gets a set containing the ids of the nodes cross tagged in this build.
        /// </summary>
        public ObservableSet<ushort> CrossedNodeIds
        {
            get => _crossedNodeIds;
            private set => SetProperty(ref _crossedNodeIds, value);
        }

        public ConfigurationStats ConfigurationStats
        {
            get => _configurationStats;
            private set => SetProperty(ref _configurationStats, value);
        }

        /// <summary>
        /// Gets a JSON object containing arbitrary additional data.
        /// Changes to the object will not flag this build dirty, <see cref="FlagDirty"/> needs to be called
        /// explicitly.
        /// </summary>
        public JObject AdditionalData
        {
            get => _additionalData;
            private set => SetProperty(ref _additionalData, value);
        }

        /// <summary>
        /// Gets whether this build was changed since the last <see cref="KeepChanges"/> call.
        /// </summary>
        public bool IsDirty
        {
            get => _isDirty;
            private set => SetProperty(ref _isDirty, value);
        }

        /// <summary>
        /// Gets whether this build can be reverted to an old state. It can be reverted if
        /// <see cref="KeepChanges"/> was called at least once.
        /// </summary>
        public bool CanRevert => _memento != null;

        public PoEBuild()
        {
            PropertyChanged += PropertyChangedHandler;
            Bandits = new BanditSettings();
            CustomGroups = new ObservableCollection<string[]>();
            CheckedNodeIds = new ObservableSet<ushort>();
            CrossedNodeIds = new ObservableSet<ushort>();
            ConfigurationStats = new ConfigurationStats();
            AdditionalData = new JObject();
            PropertyChanging += PropertyChangingHandler;
        }

        public PoEBuild(BanditSettings bandits, IEnumerable<string[]> customGroups,
            IEnumerable<ushort> checkedNodeIds, IEnumerable<ushort> crossedNodeIds,
            IEnumerable<(string, double?)> configurationStats, string additionalData)
        {
            PropertyChanged += PropertyChangedHandler;
            Bandits = bandits ?? new BanditSettings();
            CustomGroups = new ObservableCollection<string[]>(customGroups);
            CheckedNodeIds = new ObservableSet<ushort>(checkedNodeIds);
            CrossedNodeIds = new ObservableSet<ushort>(crossedNodeIds);
            ConfigurationStats = ConfigurationStats.Create(configurationStats);
            AdditionalData = additionalData == null ? new JObject() : JObject.Parse(additionalData);
            PropertyChanging += PropertyChangingHandler;
        }

        private void PropertyChangingHandler(object sender, PropertyChangingEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(CustomGroups):
                    CustomGroups.CollectionChanged -= ChangedHandler;
                    break;
                case nameof(Bandits):
                    Bandits.PropertyChanged -= ChangedHandler;
                    break;
                case nameof(CheckedNodeIds):
                    CheckedNodeIds.CollectionChanged -= ChangedHandler;
                    break;
                case nameof(CrossedNodeIds):
                    CrossedNodeIds.CollectionChanged -= ChangedHandler;
                    break;
                case nameof(ConfigurationStats):
                    ConfigurationStats.ValueChanged -= ChangedHandler;
                    break;
            }
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(CustomGroups):
                    CustomGroups.CollectionChanged += ChangedHandler;
                    break;
                case nameof(Bandits):
                    Bandits.PropertyChanged += ChangedHandler;
                    break;
                case nameof(CheckedNodeIds):
                    CheckedNodeIds.CollectionChanged += ChangedHandler;
                    break;
                case nameof(CrossedNodeIds):
                    CrossedNodeIds.CollectionChanged += ChangedHandler;
                    break;
                case nameof(ConfigurationStats):
                    ConfigurationStats.ValueChanged += ChangedHandler;
                    break;
            }
            if (args.PropertyName != nameof(IsDirty))
                IsDirty = true;
        }

        private void ChangedHandler(object sender, EventArgs args)
        {
            IsDirty = true;
        }

        /// <summary>
        /// Explicitly flags this instance as having unsaved changes.
        /// </summary>
        public void FlagDirty()
        {
            IsDirty = true;
        }

        /// <summary>
        /// Reverts changes made to this instance since the last <see cref="KeepChanges"/>.
        /// </summary>
        /// <exception cref="NullReferenceException">When <see cref="KeepChanges"/> was never called.</exception>
        public void RevertChanges()
        {
            _memento.Restore(this);
            IsDirty = false;
        }

        /// <summary>
        /// Removes the dirty flag and stores the current change so they can be reverted to.
        /// </summary>
        public void KeepChanges()
        {
            _memento = new Memento(this);
            IsDirty = false;
        }

        /// <summary>
        /// Creates a copy of <paramref name="toCopy"/> with the given name that is dirty but can not be reverted.
        /// </summary>
        public static PoEBuild CreateNotRevertableCopy(PoEBuild toCopy, string newName)
        {
            var copy = toCopy.DeepClone();
            copy.Name = newName;
            copy._memento = null;
            return copy;
        }

        protected override Notifier SafeMemberwiseClone()
        {
            var o = (PoEBuild) base.SafeMemberwiseClone();
            o.PropertyChanged += o.PropertyChangedHandler;
            o.PropertyChanging += o.PropertyChangingHandler;
            return o;
        }

        public override PoEBuild DeepClone()
        {
            var o = (PoEBuild) SafeMemberwiseClone();
            o.CustomGroups = new ObservableCollection<string[]>(CustomGroups.Select(a => (string[])a.Clone()));
            o.CheckedNodeIds = new ObservableSet<ushort>(CheckedNodeIds);
            o.CrossedNodeIds = new ObservableSet<ushort>(CrossedNodeIds);
            o.ConfigurationStats = ConfigurationStats.Create(ConfigurationStats.Export());
            o.AdditionalData = new JObject(AdditionalData);
            o.Bandits = Bandits.DeepClone();
            return o;
        }


        private class Memento : IMemento<PoEBuild>
        {
            private readonly PoEBuild _build;

            public Memento(PoEBuild build)
            {
                _build = build.DeepClone();
            }

            public void Restore(PoEBuild target)
            {
                target.Name = _build.Name;
                target.Note = _build.Note;
                target.CharacterName = _build.CharacterName;
                target.AccountName = _build.AccountName;
                target.League = _build.League;
                target.Level = _build.Level;
                target.TreeUrl = _build.TreeUrl;
                target.ItemData = _build.ItemData;
                target.LastUpdated = _build.LastUpdated;
                target.CustomGroups =
                    new ObservableCollection<string[]>(_build.CustomGroups.Select(a => (string[]) a.Clone()));
                target.CheckedNodeIds = new ObservableSet<ushort>(_build.CheckedNodeIds);
                target.CrossedNodeIds = new ObservableSet<ushort>(_build.CrossedNodeIds);
                target.ConfigurationStats = ConfigurationStats.Create(_build.ConfigurationStats.Export());
                target.AdditionalData = new JObject(_build.AdditionalData);
                target.Bandits = _build.Bandits.DeepClone();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using JetBrains.Annotations;
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
        private string _class = Constants.DefaultTreeClass;
        private uint _pointsUsed;
        private string _note;
        private string _characterName;
        private string _accountName;
        private string _league;
        private int _level = 1;
        private string _treeUrl = Constants.DefaultTree;
        private string _itemData;
        private DateTime _lastUpdated = DateTime.Now;
        private List<string[]> _customGroups = new List<string[]>();
        private BanditSettings _bandits = new BanditSettings();
        private string _version;
        private bool _isDirty;
        private IMemento<PoEBuild> _memento;

        /// <summary>
        /// Gets or sets the ingame class of this build (e.g. "Witch").
        /// </summary>
        public string Class
        {
            get { return _class; }
            set { SetProperty(ref _class, value); }
        }

        /// <summary>
        /// Gets or sets the number of points this build uses.
        /// </summary>
        public uint PointsUsed
        {
            get { return _pointsUsed; }
            set { SetProperty(ref _pointsUsed, value); }
        }

        /// <summary>
        /// Gets or sets a arbitrary note.
        /// </summary>
        public string Note
        {
            get { return _note; }
            set { SetProperty(ref _note, value); }
        }

        /// <summary>
        /// Gets or sets the character name this builds represents.
        /// </summary>
        public string CharacterName
        {
            get { return _characterName; }
            set { SetProperty(ref _characterName, value); }
        }

        /// <summary>
        /// Gets or sets the account name that owns the represented character.
        /// </summary>
        public string AccountName
        {
            get { return _accountName; }
            set { SetProperty(ref _accountName, value); }
        }

        /// <summary>
        /// Gets or sets the league of the represented character.
        /// </summary>
        public string League
        {
            get { return _league; }
            set { SetProperty(ref _league, value); }
        }

        /// <summary>
        /// Gets or sets the level of the represented character.
        /// </summary>
        public int Level
        {
            get { return _level; }
            set { SetProperty(ref _level, value); }
        }

        /// <summary>
        /// Gets or sets the build defining skill tree URL.
        /// </summary>
        [XmlElement("Url")]
        public string TreeUrl
        {
            get { return _treeUrl; }
            set { SetProperty(ref _treeUrl, value); }
        }

        /// <summary>
        /// Gets or sets the item data of this build as serialized JSON.
        /// </summary>
        public string ItemData
        {
            get { return _itemData; }
            set { SetProperty(ref _itemData, value); }
        }

        /// <summary>
        /// Gets or sets the last time this build was updated and saved.
        /// </summary>
        public DateTime LastUpdated
        {
            get { return _lastUpdated; }
            set { SetProperty(ref _lastUpdated, value); }
        }

        /// <summary>
        /// Gets the custom attribute grouping of this build.
        /// </summary>
        public List<string[]> CustomGroups
        {
            get { return _customGroups; }
            private set { SetProperty(ref _customGroups, value); }
        }

        /// <summary>
        /// Gets the bandit settings of this build.
        /// Setter only visible for XML serialization.
        /// </summary>
        public BanditSettings Bandits
        {
            get { return _bandits; }
            [UsedImplicitly]
            set { SetProperty(ref _bandits, value); }
        }

        /// <summary>
        /// Gets or sets the build version. (current one is
        /// <see cref="Serialization.SerializationConstants.BuildVersion"/>). Only used for
        /// serialization.
        /// </summary>
        public string Version
        {
            get { return _version; }
            set { SetProperty(ref _version, value); }
        }

        /// <summary>
        /// Gets whether this build was changed since the last <see cref="KeepChanges"/> call.
        /// </summary>
        [XmlIgnore]
        public bool IsDirty
        {
            get { return _isDirty; }
            private set { SetProperty(ref _isDirty, value); }
        }

        /// <summary>
        /// Gets whether this build can be reverted to an old state. It can be reverted if
        /// <see cref="KeepChanges"/> was called at least once.
        /// </summary>
        public bool CanRevert
        {
            get { return _memento != null; }
        }

        public PoEBuild()
        {
            PropertyChanged += PropertyChangedHandler;
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(IsDirty):
                    break;
                default:
                    IsDirty = true;
                    break;
            }
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

        protected override Notifier SafeMemberwiseClone()
        {
            var o = (PoEBuild) base.SafeMemberwiseClone();
            o.PropertyChanged += o.PropertyChangedHandler;
            return o;
        }

        public override PoEBuild DeepClone()
        {
            var o = (PoEBuild) SafeMemberwiseClone();
            o.CustomGroups = CustomGroups.Select(a => (string[])a.Clone()).ToList();
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
                target.Class = _build.Class;
                target.PointsUsed = _build.PointsUsed;
                target.Note = _build.Note;
                target.CharacterName = _build.CharacterName;
                target.AccountName = _build.AccountName;
                target.League = _build.League;
                target.Level = _build.Level;
                target.TreeUrl = _build.TreeUrl;
                target.ItemData = _build.ItemData;
                target.LastUpdated = _build.LastUpdated;
                target.CustomGroups = _build.CustomGroups.Select(a => (string[])a.Clone()).ToList();
                target.Bandits = _build.Bandits.DeepClone();
            }
        }
    }
}
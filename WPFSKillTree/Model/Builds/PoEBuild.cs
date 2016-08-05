using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using POESKillTree.SkillTreeFiles;
using POESKillTree.Utils;

namespace POESKillTree.Model.Builds
{
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

        public string Class
        {
            get { return _class; }
            set { SetProperty(ref _class, value); }
        }

        public uint PointsUsed
        {
            get { return _pointsUsed; }
            set { SetProperty(ref _pointsUsed, value); }
        }

        public string Note
        {
            get { return _note; }
            set { SetProperty(ref _note, value); }
        }

        public string CharacterName
        {
            get { return _characterName; }
            set { SetProperty(ref _characterName, value); }
        }

        public string AccountName
        {
            get { return _accountName; }
            set { SetProperty(ref _accountName, value); }
        }

        public string League
        {
            get { return _league; }
            set { SetProperty(ref _league, value); }
        }

        public int Level
        {
            get { return _level; }
            set { SetProperty(ref _level, value); }
        }

        [XmlElement("Url")]
        public string TreeUrl
        {
            get { return _treeUrl; }
            set { SetProperty(ref _treeUrl, value); }
        }

        public string ItemData
        {
            get { return _itemData; }
            set { SetProperty(ref _itemData, value); }
        }

        public DateTime LastUpdated
        {
            get { return _lastUpdated; }
            set { SetProperty(ref _lastUpdated, value); }
        }

        public List<string[]> CustomGroups
        {
            get { return _customGroups; }
            set { SetProperty(ref _customGroups, value); }
        }

        public BanditSettings Bandits
        {
            get { return _bandits; }
            set { SetProperty(ref _bandits, value); }
        }

        public string Version
        {
            get { return _version; }
            set { SetProperty(ref _version, value); }
        }

        [XmlIgnore]
        public bool IsDirty
        {
            get { return _isDirty; }
            private set { SetProperty(ref _isDirty, value); }
        }

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

        public void RevertChanges()
        {
            _memento.Restore(this);
            IsDirty = false;
        }

        public void KeepChanges()
        {
            _memento = SaveToMemento();
            IsDirty = false;
        }

        public IMemento<PoEBuild> SaveToMemento()
        {
            return new Memento(this);
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

            public IMemento<PoEBuild> Restore(PoEBuild target)
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
                return this;
            }
        }
    }
}
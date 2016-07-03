using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using POESKillTree.Utils;

namespace POESKillTree.Model
{
    public interface IBuild : INotifyPropertyChanged, INotifyPropertyChanging, IDeepCloneable
    {
        string Name { get; }

        new IBuild DeepClone();
    }

    public abstract class AbstractBuild<T> : Notifier, IBuild, IDeepCloneable<T>
        where T : IBuild
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public abstract T DeepClone();

        IBuild IBuild.DeepClone()
        {
            return DeepClone();
        }

        object IDeepCloneable.DeepClone()
        {
            return DeepClone();
        }
    }

    public class BuildFolder : AbstractBuild<BuildFolder>
    {
        private bool _isExpanded = true;
        private ObservableCollection<IBuild> _builds = new ObservableCollection<IBuild>();

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { SetProperty(ref _isExpanded, value); }
        }

        public ObservableCollection<IBuild> Builds
        {
            get { return _builds; }
            set { SetProperty(ref _builds, value); }
        }

        public override BuildFolder DeepClone()
        {
            var o = (BuildFolder) SafeMemberwiseClone();
            o.Builds.Clear();
            foreach (var build in Builds)
            {
                o.Builds.Add(build.DeepClone());
            }
            return o;
        }

        public IEnumerable<PoEBuild> BuildsPreorder()
        {
            foreach (var build in Builds)
            {
                var b = build as PoEBuild;
                if (b != null)
                {
                    yield return b;
                }
                else
                {
                    foreach (var child in ((BuildFolder) build).BuildsPreorder())
                    {
                        yield return child;
                    }
                }
            }
        }
    }

    public class PoEBuild : AbstractBuild<PoEBuild>
    {
        private string _class;
        private uint _pointsUsed;
        private string _note;
        private string _characterName;
        private string _accountName;
        private string _league;
        private int _level;
        private string _treeUrl; // todo Default URL for new builds
        private string _itemData;
        private DateTime _lastUpdated = DateTime.Now;
        private List<string[]> _customGroups = new List<string[]>();
        private BanditSettings _bandits = new BanditSettings();
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

        // todo These should be used better
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

        [XmlIgnore]
        public bool IsDirty
        {
            get { return _isDirty; }
            private set { SetProperty(ref _isDirty, value); }
        }

        public PoEBuild()
        {
            PropertyChanged += (sender, args) => PropertyChangedHandler(args.PropertyName);
        }

        private void PropertyChangedHandler(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(IsDirty):
                    break;
                default:
                    IsDirty = true; // todo Visual for dirty state in BuildsControl doesn't update
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

        public bool CanRevert
        {
            get { return _memento != null; }
        }

        protected override Notifier SafeMemberwiseClone()
        {
            var o = base.SafeMemberwiseClone();
            o.PropertyChanged += (sender, args) => PropertyChangedHandler(args.PropertyName);
            return o;
        }

        public override PoEBuild DeepClone()
        {
            var o = (PoEBuild) SafeMemberwiseClone();
            o.CustomGroups = CustomGroups.Select(a => (string[])a.Clone()).ToList();
            o.Bandits = Bandits.DeepClone();
            o.IsDirty = false;
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
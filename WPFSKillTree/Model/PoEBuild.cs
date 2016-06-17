using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using POESKillTree.Localization;
using POESKillTree.Utils;

namespace POESKillTree.Model
{
    public class PoEBuild : Notifier
    {
        public string Name { get; set; }

        private string _characterName;
        public string CharacterName
        {
            get { return _characterName; }
            set { SetProperty(ref _characterName, value); }
        }

        private string _accountName;
        public string AccountName
        {
            get { return _accountName; }
            set { SetProperty(ref _accountName, value); }
        }

        private string _league;
        public string League
        {
            get { return _league; }
            set { SetProperty(ref _league, value); }
        }

        public string Level { get; set; }
        public string Class { get; set; }
        public string PointsUsed { get; set; }
        public string Url { get; set; }
        public string Note { get; set; }

        private string _itemData;

        public string ItemData
        {
            get { return _itemData; }
            set { SetProperty(ref _itemData, value); }
        }

        public DateTime LastUpdated { get; set; }
        public List<string[]> CustomGroups { get; set; }
        public bool CurrentlyOpen { get; set; }
        public BanditSettings Bandits { get; set; }

        [XmlIgnore]
        public string Image
        {
            get
            {
                var imgPath = "/POESKillTree;component/Images/" +  Class;
                if (CurrentlyOpen)
                    imgPath += "_Highlighted";
                return imgPath + ".jpg";
            }
        }
        [XmlIgnore]
        public string Description
        {
            get
            {
                uint used = 0;
                if (!string.IsNullOrEmpty(PointsUsed)) uint.TryParse(PointsUsed, out used);

                return string.Format(L10n.Plural("{0}, {1} point used", "{0}, {1} points used", used), Class, used);
            }
        }
        [XmlIgnore]
        public bool Visible { get; set; }

        public PoEBuild()
        {
            Visible = true;
            CustomGroups = new List<string[]>();
            Bandits = new BanditSettings();
        }

        public PoEBuild(string name, string poeClass, string pointsUsed, string url, string note)
        {
            Name = name;
            Class = poeClass;
            PointsUsed = pointsUsed;
            Url = url;
            Note = note;
            CustomGroups = new List<string[]>();
            Bandits = new BanditSettings();
        }

        public override string ToString()
        {
            return Name + '\n' + Description;
        }

        public PoEBuild Copy()
        {
            return Copy(this);
        }

        public static PoEBuild Copy(PoEBuild build)
        {
            return new PoEBuild
            {
                Name = build.Name,
                CharacterName = build.CharacterName,
                AccountName = build.AccountName,
                League = build.League,
                Level = build.Level,
                Class = build.Class,
                PointsUsed = build.PointsUsed,
                Url = build.Url,
                Note = build.Note,
                ItemData = build.ItemData,
                LastUpdated = build.LastUpdated,
                CustomGroups = build.CustomGroups.Select(a => (string[]) a.Clone()).ToList(),
                CurrentlyOpen = build.CurrentlyOpen,
                Bandits = build.Bandits.Clone()
            };
        }
    }
}
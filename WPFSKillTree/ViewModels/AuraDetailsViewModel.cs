using POESKillTree.Model;
using POESKillTree.SkillTreeFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using AuraGem = POESKillTree.SkillTreeFiles.ItemDB.Gem;
using GemValue = POESKillTree.SkillTreeFiles.ItemDB.Value;
using GemAttribute = POESKillTree.SkillTreeFiles.ItemDB.Attribute;
using System.Windows.Media.Imaging;

namespace POESKillTree.ViewModels
{
    internal class AuraDetailsViewModel : Notifier
    {
        private bool _isPercentegaBasedReservation;
        private int _baseSkillCost;
        private GemAttribute _gemManaReservationPerLevel;

        private AuraGroupViewModel _groupViewModel;
        private AuraGem _auraGem;
        internal AuraDetailsViewModel(AuraGroupViewModel parentViewModel, AuraGem auragem)
        {
            string gemTooltipCost;
            _groupViewModel = parentViewModel;
            _auraGem = auragem;
            _gemManaReservationPerLevel = _auraGem.Attributes.Find(attr => attr.Name == "# Mana Reserved");

            if (_gemManaReservationPerLevel != null)
            {
                MaxGemLevel = _gemManaReservationPerLevel.Values.Count - 1;
                IsAuraLevelChoosable = true;
                gemTooltipCost = _gemManaReservationPerLevel.Values[0].Text;
            }
            else
            {
                MaxGemLevel = 1;
                IsAuraLevelChoosable = false;
                gemTooltipCost = _auraGem.ManaReserved;
            }
            _isPercentegaBasedReservation = gemTooltipCost.Contains("%");
            int.TryParse(gemTooltipCost.Trim(new char[] { ' ', '\t', '%' }), out _baseSkillCost);
        }

        #region GUI properties
        public int MaxGemLevel { get; set; }

        private bool _isAuraUsed = false;
        public bool IsAuraUsed
        {
            get { return _isAuraUsed; }
            set
            {
                _isAuraUsed = value;
                NotifyPropertyChanged("IsAuraUsed");
                UpdateResourceReservation();
            }
        }

        private bool _isAuraLevelChoosable = false;
        public bool IsAuraLevelChoosable
        {
            get { return _isAuraLevelChoosable; }
            set
            {
                _isAuraLevelChoosable = value;
                NotifyPropertyChanged("IsAuraLevelChoosable");
            }
        }

        public string AuraName
        {
            get { return _auraGem.Name; }
        }

        public BitmapSource AuraGemIcon
        {
            get { return _auraGem.GemIcon; }
        }

        public BitmapSource AuraSkillIcon
        {
            get { return _auraGem.SkillIcon; }
        }

        private int _auraLevel = 1;
        public int AuraLevel
        {
            get { return _auraLevel > 0 ? _auraLevel : 1; }
            set
            {
                if (value != _auraLevel && value > 0)
                {
                    _auraLevel = value;
                    NotifyPropertyChanged("AuraLevel");

                    int.TryParse(_gemManaReservationPerLevel.Values[AuraLevel - 1].Text.Trim(new char[] { ' ', '\t', '%' }), out _baseSkillCost);
                    UpdateResourceReservation();
                }
            }
        }
        public string ManaReserved
        {
            //this is only called by GUI on property changed event
            get { return _finalCost.ToString() + (_isPercentegaBasedReservation ? "%" : ""); }
        }
        #endregion

        #region cost calculation 
        private int _finalCost;
        public int FinalCost
        {
            get { return _isPercentegaBasedReservation ? (int)(_groupViewModel.TotalResource * _finalCost / 100.0) : _finalCost; }
        }

        public void UpdateResourceReservation()
        {
            if (IsAuraUsed == true)
            {
                _finalCost = _baseSkillCost;

                //STEP1: base skill cost multiplied by linked gems, then rounded down (decimal part cut by double -> int conversion)
                // reduced mana gem linked
                _finalCost = (int)(_finalCost * _groupViewModel.ReducedManaGemMultiplier);

                // blood magic gem linked
                _finalCost = (int)(_finalCost * _groupViewModel.BloodMagicGemMultiplier);

                // additional mana multiplier (may be curse on hit, or some kind of unique, or typed by hand)
                _finalCost = (int)(_finalCost * (1 + _groupViewModel.AdditionalCostMultiplier / 100.0));

                //STEP2: reservation reduction calculated, rounded down by to-int conversion, then is substracted from baseSkillCost
                // reduced mana nodes from tree
                _finalCost -= ((int)(_finalCost * _groupViewModel.ReducedManaReserved / 100.0));

                if (_groupViewModel.IsMortalConvinction)
                {
                    //is less, not reduced
                    _finalCost -= (int)(_finalCost * (1 - AuraCalculator.MortalConvinctionReduction / 100.0));
                }
            }
            else
            {
                _finalCost = 0;
            }
            NotifyPropertyChanged("ManaReserved");
            _groupViewModel.NotifyReservationChanged();
        }
        #endregion
    }
}

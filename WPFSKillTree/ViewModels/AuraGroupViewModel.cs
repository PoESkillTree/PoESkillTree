using POESKillTree.SkillTreeFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GemValue = POESKillTree.SkillTreeFiles.ItemDB.Value;
using GemAttribute = POESKillTree.SkillTreeFiles.ItemDB.Attribute;
using AuraGem = POESKillTree.SkillTreeFiles.ItemDB.Gem;
using POESKillTree.ViewModels;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Data;
using POESKillTree.Model;

namespace POESKillTree.ViewModels
{
    internal class AuraGroupViewModel : Notifier
    {
        public int MAXGEMLEVEL { get { return 21; } } //TODO: might change it to max gem lvl available?

        AuraCalculatorViewModel _mainViewModel;

        private GemAttribute _reducedManaGemAttribute;
        private GemAttribute _bloodMagicGemAttribute;
        internal AuraGroupViewModel(AuraCalculatorViewModel parentViewModel)
        {
            _mainViewModel = parentViewModel;
            ItemDB.GetGemsByKeyword(AuraGem.Keyword.Aura).FindAll(gem => gem.Keywords.Contains(AuraGem.Keyword.Support.ToString()) == false).ForEach(auraGem =>
            {
                AvailableAuras.Add(new AuraDetailsViewModel(this, auraGem));
            });
            AvailableAuras.Add(new AuraDetailsViewModel(this, ItemDB.GetGem("Tempest Shield")));

            _reducedManaGemAttribute = ItemDB.GetGem("Reduced Mana").Attributes.Find(attr => attr.Name == "Mana cost multiplier #%");
            _bloodMagicGemAttribute = ItemDB.GetGem("Blood Magic").Attributes.Find(attr => attr.Name == "Mana cost multiplier #%");
        }

        #region GUI properties
        private ObservableCollection<AuraDetailsViewModel> _auralist = new ObservableCollection<AuraDetailsViewModel>();
        public ObservableCollection<AuraDetailsViewModel> AvailableAuras
        {
            get { return _auralist; }
            set
            {
                _auralist = value;
                NotifyPropertyChanged("AvailableAuras");
            }
        }
        #region reduced mana
        private bool _isLinkedWithReducedMana;
        public bool IsLinkedWithReducedMana
        {
            get { return _isLinkedWithReducedMana; }
            set
            {
                _isLinkedWithReducedMana = value;
                NotifyPropertyChanged("ReducedManaGemMultiplier");
                UpdateResourceReservation();
            }
        }
        private int _reducedManaSelectedLevel = 1;
        public int ReducedManaSelectedLevel
        {
            get { return _reducedManaSelectedLevel; }
            set
            {
                if (_reducedManaSelectedLevel != value && value > 0)
                {
                    _reducedManaSelectedLevel = value;
                    NotifyPropertyChanged("ReducedManaGemMultiplier");
                    UpdateResourceReservation();
                }
            }
        }

        public double ReducedManaGemMultiplier
        {
            get { return IsLinkedWithReducedMana ? _reducedManaGemAttribute.Values[ReducedManaSelectedLevel - 1].ToValue()[0] / 100.0 : 1; }
        }

        public int ReducedManaReserved
        {
            get
            {
                return _mainViewModel.ReducedManaReserved +
                  (_mainViewModel.GlobalAlphsaHowlUsed ? AuraCalculator.AlphasHowlReduction : 0) +
                  (IsGemmedInPrismGuardian ? AuraCalculator.PrismGuardianReduction : 0);
            }
        }

        #endregion
        #region blood magic
        private bool _isLinkedWithBloodMagic;
        public bool IsLinkedWithBloodMagic
        {
            get { return _isLinkedWithBloodMagic; }
            set
            {
                _isLinkedWithBloodMagic = value;
                NotifyPropertyChanged("BloodMagicGemMultiplier");
                UpdateResourceReservation();
            }
        }
        private int _bloodMagicSelectedLevel = 1;
        public int BloodMagicSelectedLevel
        {
            get { return _bloodMagicSelectedLevel; }
            set
            {
                if (_bloodMagicSelectedLevel != value && value > 0)
                {
                    _bloodMagicSelectedLevel = value;
                    NotifyPropertyChanged("BloodMagicGemMultiplier");
                    UpdateResourceReservation();
                }
            }
        }

        public double BloodMagicGemMultiplier
        {
            get { return IsLinkedWithBloodMagic ? _bloodMagicGemAttribute.Values[BloodMagicSelectedLevel - 1].ToValue()[0] / 100.0 : 1.0; }
        }

        private bool _isGemmedInPrismGuardian;
        public bool IsGemmedInPrismGuardian
        {
            get { return _isGemmedInPrismGuardian; }
            set
            {
                if (_isGemmedInPrismGuardian != value)
                {
                    _isGemmedInPrismGuardian = value;
                    UpdateResourceReservation();
                }
            }
        }
        public bool IsBloodMagicGroup
        {
            get { return _mainViewModel.GlobalBloodMagicUsed || IsLinkedWithBloodMagic || IsGemmedInPrismGuardian; }
        }

        public bool IsMortalConvinction
        {
            get { return _mainViewModel.GlobalMortalConvictionUsed; }
        }
        #endregion

        private int _additionalCostMultiplier;
        public int AdditionalCostMultiplier
        {
            get { return IsAdditionalManaCostMultiplier ? _additionalCostMultiplier : 0; }
            set
            {
                if (_additionalCostMultiplier != value)
                {
                    _additionalCostMultiplier = value;
                    UpdateResourceReservation();
                }
            }
        }

        private bool _isAdditionalManaCostMultiplier;
        public bool IsAdditionalManaCostMultiplier
        {
            get { return _isAdditionalManaCostMultiplier; }
            set
            {
                _isAdditionalManaCostMultiplier = value;
                UpdateResourceReservation();
            }
        }

        private int _manaReservedByGroup = 0;
        public int ManaReservedByGroup
        {
            get { return _manaReservedByGroup; }
            set
            {
                _manaReservedByGroup = value;
                NotifyPropertyChanged("ManaReservedByGroup");
            }
        }
        private int _lifeReservedByGroup = 0;
        public int LifeReservedByGroup
        {
            get { return _lifeReservedByGroup; }
            set
            {
                _lifeReservedByGroup = value;
                NotifyPropertyChanged("LifeReservedByGroup");
            }
        }

        public void NotifyReservationChanged()
        {
            if (IsBloodMagicGroup)
            {
                ManaReservedByGroup = 0;
                LifeReservedByGroup = AvailableAuras.Aggregate(0, (summ, aura) => summ + aura.FinalCost);
            }
            else
            {
                ManaReservedByGroup = AvailableAuras.Aggregate(0, (summ, aura) => summ + aura.FinalCost);
                LifeReservedByGroup = 0;
            }

            _mainViewModel.NotifyLifeReservationChanged();
            _mainViewModel.NotifyManaReservationChanged();
        }

        public int TotalResource
        {
            get { return IsBloodMagicGroup ? _mainViewModel.TotalLife : _mainViewModel.TotalMana; }
        }
        #endregion

        public void UpdateResourceReservation()
        {
            AvailableAuras.ForEach(aura => aura.UpdateResourceReservation());
        }
    }
}

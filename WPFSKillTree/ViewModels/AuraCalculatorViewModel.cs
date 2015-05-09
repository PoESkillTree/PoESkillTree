using POESKillTree.Model;
using POESKillTree.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace POESKillTree.ViewModels
{
    public static class EnumerableExtensions
    {
        // ForEach extension for ObservableCollections
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var e in enumerable)
            {
                action(e);
            }
        }
    }

    internal class AuraCalculatorViewModel : Notifier
    {
        List<Attribute> _globalAttributes;

        public ICommand AddAuraGroupCommand { get; set; }

        public ICommand RemoveAuraGroupCommand { get; set; }

        internal AuraCalculatorViewModel(List<Attribute> globalAttributes)
        {
            _globalAttributes = globalAttributes;

            AddAuraGroupCommand = new RelayCommand(_ =>
            {
                AuraGroupList.Add(new AuraGroupViewModel(this));
            });

            RemoveAuraGroupCommand = new RelayCommand(auraGroup =>
            {
                AuraGroupList.Remove(auraGroup as AuraGroupViewModel);
            });

            RefreshGlobalData();

            AddAuraGroupCommand.Execute(null);
            UpdateResourceReservation();
        }

        public void RefreshGlobalData()
        {
            if (!IsTreeDataEditable)
            {
                /* update values only if tree data is locked = in sync with the tree
                 * we manually set the values and fire notify events so UpdateResourceReservation in setter is not called millions of times */
                _reducedManaReserved = FetchAttributeValue("reduced Mana Reserved"); 
                NotifyPropertyChanged("ReducedManaReserved");

                _totalLife = FetchAttributeValue("Life: ", 3000); 
                NotifyPropertyChanged("TotalLife");

                _totalMana = FetchAttributeValue("Mana: ", 1000); 
                NotifyPropertyChanged("TotalMana");

                _bloodMagiclUsed = _globalAttributes.Exists(attr => attr.Text.Contains("Removes all mana. Spend Life instead of Mana for Skills")); 
                NotifyPropertyChanged("GlobalBloodMagicUsed");

                _mortalConvictionUsed = _globalAttributes.Exists(attr => attr.Text.Contains("50% less Mana Reserved")); 
                NotifyPropertyChanged("GlobalMortalConvictionUsed");

                UpdateResourceReservation();
            }
        }

        private ObservableCollection<AuraGroupViewModel> _auraGroupList = new ObservableCollection<AuraGroupViewModel>();
        public ObservableCollection<AuraGroupViewModel> AuraGroupList
        {
            get { return _auraGroupList; }
            set
            {
                _auraGroupList = value;
                NotifyPropertyChanged("AuraGroupList");
            }
        }
        #region
        private int _totalLife;
        public int TotalLife
        {
            get { return _totalLife; }
            set
            {
                _totalLife = value;
                NotifyPropertyChanged("TotalLife");
                UpdateResourceReservation();
            }
        }

        private int _totalMana;
        public int TotalMana
        {
            get { return _totalMana; }
            set
            {
                _totalMana = value;
                NotifyPropertyChanged("TotalMana");
                UpdateResourceReservation();
            }
        }

        private bool _alphasHowlUsed;
        public bool GlobalAlphsaHowlUsed
        {
            get { return _alphasHowlUsed; }
            set
            {
                _alphasHowlUsed = value;
                NotifyPropertyChanged("GlobalAlphsaHowlUsed");
                UpdateResourceReservation();
            }
        }

        private bool _bloodMagiclUsed;
        public bool GlobalBloodMagicUsed
        {
            get { return _bloodMagiclUsed; }
            set
            {
                _bloodMagiclUsed = value;
                NotifyPropertyChanged("GlobalBloodMagicUsed");
                UpdateResourceReservation();
                if (!_bloodMagiclUsed) GlobalMortalConvictionUsed = false;
            }
        }

        private bool _mortalConvictionUsed;
        public bool GlobalMortalConvictionUsed
        {
            get { return _mortalConvictionUsed; }
            set
            {
                _mortalConvictionUsed = value;
                NotifyPropertyChanged("GlobalMortalConvictionUsed");
                UpdateResourceReservation();
            }

        }

        private int _reducedManaReserved;
        public int ReducedManaReserved
        {
            get { return _reducedManaReserved; }
            set
            {
                _reducedManaReserved = value;
                NotifyPropertyChanged("ReducedManaReserved");
                UpdateResourceReservation();
            }
        }

        private bool _isTreeDataEditable;
        public bool IsTreeDataEditable
        {
            get { return _isTreeDataEditable; }
            set
            {
                _isTreeDataEditable = value;
                if (!value)
                {
                    RefreshGlobalData();
                    UpdateResourceReservation();
                }
            }
        }
        #endregion
        public int TotalManaLeft
        {
            get { return TotalMana - AuraGroupList.Aggregate(0, (summ, auraGroup) => summ + auraGroup.ManaReservedByGroup); }
        }

        public int TotalLifeLeft
        {
            get { return TotalLife - AuraGroupList.Aggregate(0, (summ, auraGroup) => summ + auraGroup.LifeReservedByGroup); }
        }

        internal void UpdateResourceReservation()
        {
            AuraGroupList.ForEach(aura => aura.UpdateResourceReservation());
        }

        public void NotifyManaReservationChanged()
        {
            NotifyPropertyChanged("TotalManaLeft");
        }
        public void NotifyLifeReservationChanged()
        {
            NotifyPropertyChanged("TotalLifeLeft");
        }

        //TODO: move to the attribute class so it holds array of values, not a raw string
        private int FetchAttributeValue(string attributeDescription, int defaultValue = 0)
        {
            try
            {
                return int.Parse(Regex.Match(_globalAttributes.First(attr => attr.Text.Contains(attributeDescription)).Text, @"\d*").Value);
            }
            catch
            {
                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Calculates amount of mana/life reserved by linked aura group and notifies main view model about a need to re-calculated reservation
    /// </summary>
    /* recalclute mana/life reservation
     * @Mark_GGG: The first thing it calculates is the base cost of the skill, which is the cost of the skill gem and the 
     * multipliers of all support gems that make up the skill. This value must be an integer, and will be rounded 
     * down, because of how integer division works in computers. Anything after the decimal point is simply ignored. 
     * Once it has be base cost of the skill, it calculates how much that is reduced by. Again it rounds down for 
     * the same reason - the reason you see it "rounding up" in your second step is simply that you're looking at 
     * the final cost/reservation, where what's actually being calculated and rounded is the amount taken off the 
     * cost by the reduction. If you had blood magic and took the notable behind it, or otherwise obtained "less" 
     * mana reserved, this would be a part of the second calculation, where it's calculating the amount to take off 
     * the cost, but would stack multiplicatively with the "reduced" passives.
     * 
     * http://www.pathofexile.com/forum/view-thread/567561/page/3#p5144864
       */
    public class AuraCalculator
    {
        #region some constants
        public static readonly int AlphasHowlReduction = 8;
        public static readonly int MortalConvinctionReduction = 50;
        public static readonly int PrismGuardianReduction = 25;
        private static AuraCalculatorViewModel viewModel = null;
        #endregion
        internal static void Show(Window parent, List<Attribute> globalAttributes)
        {
            /*TODO: 
             * 1. although mana reservation works in general, it fails to calculate proper values for BloodMagic gem. I've tried different roundings,
             * but it always differs from ingame data, e.g.:
             * Blood magic lvl 1 + reduced mana lvl 15 + clarity lvl 6 + 26 % reduced mana reservation + 8% alpha's howl:
             * IGN = 226, calculator = 225
             * Apparently, none of the available calculators can do it properly :D
             * 
             * 2. change using of attribute names to some kind of enums?
             */
            viewModel = new AuraCalculatorViewModel(globalAttributes);
            AuraCalculatorView view = new AuraCalculatorView();

            view.Owner = parent;
            view.DataContext = viewModel;
            view.Show();
        }

        internal static void RefreshData()
        {
            if (viewModel != null)
            {
                viewModel.RefreshGlobalData();
            }
        }
    }
}

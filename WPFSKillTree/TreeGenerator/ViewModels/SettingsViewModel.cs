using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using POESKillTree.Localization;
using POESKillTree.Model;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.Settings;

namespace POESKillTree.TreeGenerator.ViewModels
{
    public sealed class SettingsViewModel : CloseableViewModel, IDataErrorInfo
    {

        private readonly SkillTree _tree;

        public ObservableCollection<GeneratorTabViewModel> Tabs { get; private set; }

#region Presentation

        private int _level;

        public int Level
        {
            get { return _level; }
            set
            {
                if (value == _level)  return;

                var diff = value - _level;
                _level = value;
                _totalPoints += diff;

                OnPropertyChanged("Level");
                OnPropertyChanged("TotalPoints");
            }
        }

        private int _additionalPoints = 18;

        public int AdditionalPoints
        {
            get { return _additionalPoints; }
            set
            {
                if (value == _additionalPoints) return;

                var diff = value - _additionalPoints;
                _additionalPoints = value;
                _totalPoints += diff;

                OnPropertyChanged("AdditionalPoints");
                OnPropertyChanged("TotalPoints");
            }
        }

        private int _totalPoints;

        public int TotalPoints
        {
            get { return _totalPoints; }
            set
            {
                if (value == _totalPoints) return;

                var diff = value - _totalPoints;
                _totalPoints = value;

                if (_level + diff < 1)
                {
                    _additionalPoints += diff;
                    OnPropertyChanged("AdditionalPoints");
                }
                else
                {
                    _level += diff;
                    OnPropertyChanged("Level");
                }
                OnPropertyChanged("TotalPoints");
            }
        }

        private bool _includeChecked = true;

        public bool IncludeChecked
        {
            get { return _includeChecked; }
            set
            {
                _includeChecked = value;
                OnPropertyChanged("IncludeChecked");
            }
        }

        private bool _excludeCrossed = true;

        public bool ExcludeCrossed
        {
            get { return _excludeCrossed; }
            set
            {
                _excludeCrossed = value;
                OnPropertyChanged("ExcludeCrossed");
            }
        }

        private bool _importItems = true;

        public bool ImportItems
        {
            get { return _importItems; }
            set
            {
                _importItems = value;
                OnPropertyChanged("ImportItems");
            }
        }

        private bool _treeAsSubset;

        public bool TreeAsSubset
        {
            get { return _treeAsSubset; }
            set
            {
                _treeAsSubset = value;
                OnPropertyChanged("TreeAsSubset");
            }
        }

        private bool _treeAsStart;

        public bool TreeAsStart
        {
            get { return _treeAsStart; }
            set
            {
                _treeAsStart = value;
                OnPropertyChanged("TreeAsStart");
            }
        }

        private int _selectedTabIndex;

        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                _selectedTabIndex = value;
                OnPropertyChanged("SelectedTabIndex");
            }
        }

        #endregion

#region Commands

        private RelayCommand _runCommand;

        public ICommand RunCommand
        {
            get
            {
                if (_runCommand == null)
                {
                    _runCommand = new RelayCommand(o => Run(), o => IsValid);
                }
                return _runCommand;
            }
        }

        // TODO RelayCommand.CanExecuteChanged callen

#endregion

#region Validation

        public string Error
        {
            get { return null; }
        }

        public string this[string columnName]
        {
            get { return GetValidationError(columnName); }
        }

        // see https://msdn.microsoft.com/en-us/magazine/dd419663.aspx

        /// <summary>
        /// Returns true if this object has no validation errors.
        /// </summary>
        public bool IsValid
        {
            get { return ValidatedProperties.All(property => GetValidationError(property) == null); }
        }

        private static readonly string[] ValidatedProperties =
        {
            "Level", "AdditionalPoints", "TotalPoints"
        
        };

        /// <summary>
        /// Returns the result of the appropriate ValidateProperty method,
        /// if it is a validated property and null otherwise.
        /// </summary>
        private string GetValidationError(string propertyName)
        {
            switch (propertyName)
            {
                case "Level":
                    return ValidateLevel();
                case "AdditionalPoints":
                    return ValideAdditionalPoints();
                case "TotalPoints":
                    return ValidateTotalPoints();
                default:
                    return null;
            }
        }

        private string ValidateLevel()
        {
            if (Level < 1 || Level > 100)
            {
                return L10n.Message("Level must be between 1 and 100.");
            }
            return null;
        }

        private string ValideAdditionalPoints()
        {
            if (AdditionalPoints < 0)
            {
                return L10n.Message("Additional points must not be negative.");
            }
            return null;
        }

        private string ValidateTotalPoints()
        {
            if (TotalPoints < 0)
            {
                return L10n.Message("Total points must not be negative.");
            }
            if (Level - 1 + AdditionalPoints != TotalPoints)
            {
                return L10n.Message("Total points must be equal to level plus additional points.");
            }
            return null;
        }

#endregion

        public SettingsViewModel(SkillTree tree)
            : this(tree, null)
        {
        }

        /// <summary>
        /// Constructs a new SettingsViewModel with a fixed GeneratorTabViewModel.
        /// Use this constructor if you don't want to use this ViewModel in a View and
        /// only want to run it.
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="generator"></param>
        public SettingsViewModel(SkillTree tree, GeneratorTabViewModel generator)
        {
            DisplayName = "Skill tree Generator";

            _tree = tree;
            _level = _tree.Level;
            _totalPoints = _level - 1 + _additionalPoints;

            if (generator == null)
            {
                CreateTabs();
            }
            else
            {
                Tabs = new ObservableCollection<GeneratorTabViewModel> { generator };
            }
        }

        private void CreateTabs()
        {
            Tabs = new ObservableCollection<GeneratorTabViewModel>
            {
                new SteinerTabViewModel(_tree), // TODO this one should be removed at some point
                new AutomatedTabViewModel(_tree),
                new AdvancedTabViewModel(_tree)
            };
        }

        private void Run()
        {
            if (StartController == null) return;

            var savedHighlights = _tree.HighlightedNodes;

            var settings = CreateSettings();
            var solver = Tabs[_selectedTabIndex].CreateSolver(settings);
            var controllerVm = new ControllerViewModel(solver, Tabs[_selectedTabIndex].DisplayName);

            // Kinda crude, but I'm not going to write a framework for a few popups.
            StartController(this, new StartControllerEventArgs(controllerVm));

            if (controllerVm.Result == true)
            {
                _tree.SkilledNodes = new HashSet<ushort>(controllerVm.BestSoFar);
            }
            _tree.HighlightedNodes = savedHighlights;
            _tree.DrawNodeBaseSurroundHighlight();
            _tree.DrawHighlights();
            _tree.UpdateAvailNodes();

            Close(controllerVm.Result);
        }

        private SolverSettings CreateSettings()
        {
            var level = _level;
            var totalPoints = _totalPoints;
            var @checked = _includeChecked ? _tree.GetCheckedNodes() : null;
            var crossed = _excludeCrossed ? _tree.GetCrossedNodes() : null;
            var initialStats = ItemsToInitialStats();
            var subsetTree = _treeAsSubset ? _tree.SkilledNodes : null;
            var initialTree = _treeAsStart ? _tree.SkilledNodes : null;
            return new SolverSettings(level, totalPoints, @checked, crossed, initialStats, subsetTree, initialTree);
        }

        private Dictionary<string, List<float>> ItemsToInitialStats()
        {
            // TODO
            // generate stats just from level
            if (_importItems)
            {
                // add stats from items
            }
            return null;
        }

        public event EventHandler<StartControllerEventArgs> StartController;
    }

    public class StartControllerEventArgs : EventArgs
    {
        public ControllerViewModel ViewModel { get; private set; }

        public StartControllerEventArgs(ControllerViewModel viewModel)
        {
            ViewModel = viewModel;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using POESKillTree.Model;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.Settings;
using POESKillTree.Utils;

namespace POESKillTree.TreeGenerator.ViewModels
{
    public sealed class SettingsViewModel : CloseableViewModel
    {

        private readonly SkillTree _tree;

        public ObservableCollection<GeneratorTabViewModel> Tabs { get; private set; }

#region Presentation

        private int _level = 80;

        public int Level
        {
            get { return _level; }
            set
            {
                if (value == _level)  return;

                var diff = value - _level;
                _level = value;
                TotalPoints += diff;

                OnPropertyChanged("Level");
            }
        }

        private int _additionalPoints = 21;

        public int AdditionalPoints
        {
            get { return _additionalPoints; }
            set
            {
                if (value == _additionalPoints) return;

                var diff = value - _additionalPoints;
                _additionalPoints = value;
                TotalPoints += diff;

                OnPropertyChanged("AdditionalPoints");
            }
        }

        private int _totalPoints;

        public int TotalPoints
        {
            get { return _totalPoints; }
            private set
            {
                if (value == _totalPoints) return;
                _totalPoints = value;

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
            get { return _runCommand ?? (_runCommand = new RelayCommand(o => Run())); }
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
        public SettingsViewModel(SkillTree tree, GeneratorTabViewModel generator)
        {
            DisplayName = "Skill tree Generator";

            _tree = tree;
            if (_tree.Level < 2 && _tree.SkilledNodes.Count - _additionalPoints > 0)
            {
                _level = _tree.SkilledNodes.Count - _additionalPoints;
            }
            else if (_tree.Level >= 2)
            {
                _level = _tree.Level;
            }
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
            StartController.Raise(this, new StartControllerEventArgs(controllerVm));

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
            var subsetTree = _treeAsSubset ? _tree.SkilledNodes : null;
            var initialTree = _treeAsStart ? _tree.SkilledNodes : null;
            return new SolverSettings(level, totalPoints, @checked, crossed, subsetTree, initialTree);
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
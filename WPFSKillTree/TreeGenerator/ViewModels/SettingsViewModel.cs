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

        public SkillTree Tree { get { return _tree; } }

        public ObservableCollection<GeneratorTabViewModel> Tabs { get; private set; }

#region Presentation

        private const int AdditionalPointsDefaultValue = 21;

        private const bool IncludeCheckedDefaultValue = true;

        private const bool ExcludeCrossedDefaultValue = true;

        private const bool TreeAsSubsetDefaultValue = false;

        private const bool TreeAsInitialDefaultValue = false;

        private int _additionalPoints = AdditionalPointsDefaultValue;

        public int AdditionalPoints
        {
            get { return _additionalPoints; }
            set
            {
                SetProperty(ref _additionalPoints, value, () => { },
                    v => TotalPoints += v - _additionalPoints);
            }
        }

        private int _totalPoints;

        public int TotalPoints
        {
            get { return _totalPoints; }
            private set { SetProperty(ref _totalPoints, value); }
        }

        private bool _includeChecked = IncludeCheckedDefaultValue;

        public bool IncludeChecked
        {
            get { return _includeChecked; }
            set { SetProperty(ref _includeChecked, value); }
        }

        private bool _excludeCrossed = ExcludeCrossedDefaultValue;

        public bool ExcludeCrossed
        {
            get { return _excludeCrossed; }
            set { SetProperty(ref _excludeCrossed, value); }
        }

        private bool _treeAsSubset = TreeAsSubsetDefaultValue;

        public bool TreeAsSubset
        {
            get { return _treeAsSubset; }
            set { SetProperty(ref _treeAsSubset, value); }
        }

        private bool _treeAsInitial = TreeAsInitialDefaultValue;

        public bool TreeAsInitial
        {
            get { return _treeAsInitial; }
            set { SetProperty(ref _treeAsInitial, value); }
        }

        private int _selectedTabIndex;

        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set { SetProperty(ref _selectedTabIndex, value); }
        }

        #endregion

#region Commands

        private RelayCommand _runCommand;

        public ICommand RunCommand
        {
            get { return _runCommand ?? (_runCommand = new RelayCommand(o => Run())); }
        }

        private RelayCommand _resetCommand;

        public ICommand ResetCommand
        {
            get { return _resetCommand ?? (_resetCommand = new RelayCommand(o => Reset()));}
        }

#endregion

        /// <summary>
        /// Constructs a new SettingsViewModel with a fixed GeneratorTabViewModel.
        /// Use this constructor if you don't want to use this ViewModel in a View and
        /// only want to run it.
        /// </summary>
        public SettingsViewModel(SkillTree tree, GeneratorTabViewModel generator = null)
        {
            DisplayName = "Skill tree Generator";

            _tree = tree;
            if (_tree.Level != SkillTree.UndefinedLevel && _tree.SkilledNodes.Count > 1
                && _tree.SkilledNodes.Count - _tree.Level >= 0)
            {
                _additionalPoints = _tree.SkilledNodes.Count - _tree.Level;
            }
            _totalPoints = _tree.Level - 1 + _additionalPoints;

            tree.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Level")
                {
                    TotalPoints = _tree.Level - 1 + _additionalPoints;
                }
            };

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
                new AdvancedTabViewModel(_tree),
                new AutomatedTabViewModel(_tree),
                new SteinerTabViewModel(_tree) // TODO this one should be removed at some point
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

        private void Reset()
        {
            AdditionalPoints = AdditionalPointsDefaultValue;
            IncludeChecked = IncludeCheckedDefaultValue;
            ExcludeCrossed = ExcludeCrossedDefaultValue;
            TreeAsSubset = TreeAsSubsetDefaultValue;
            TreeAsInitial = TreeAsInitialDefaultValue;
            foreach (var tab in Tabs)
            {
                tab.Reset();
            }
        }

        private SolverSettings CreateSettings()
        {
            var level = Tree.Level;
            var totalPoints = _totalPoints;
            var @checked = _includeChecked ? _tree.GetCheckedNodes() : null;
            var crossed = _excludeCrossed ? _tree.GetCrossedNodes() : null;
            var subsetTree = _treeAsSubset ? _tree.SkilledNodes : null;
            var initialTree = _treeAsInitial ? _tree.SkilledNodes : null;
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
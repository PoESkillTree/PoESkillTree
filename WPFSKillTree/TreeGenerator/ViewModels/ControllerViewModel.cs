using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using POESKillTree.Localization;
using POESKillTree.Model;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.Solver;
using POESKillTree.Utils;

namespace POESKillTree.TreeGenerator.ViewModels
{
    public sealed class ControllerViewModel : CloseableViewModel
    {
        private readonly ISolver _solver;

        private readonly BackgroundWorker _solutionWorker = new BackgroundWorker();
        private readonly BackgroundWorker _initializationWorker = new BackgroundWorker();

        private readonly SkillTree _tree;

        private int _maxSteps;

        private HashSet<ushort> _bestSoFar;

        public HashSet<ushort> BestSoFar
        {
            get { return _bestSoFar; }
            private set
            {
                _bestSoFar = value;
                // TODO get best result text from Solver or somewhere else since it is somewhat solver dependant
                // BestSoFar.Count - 1 because of the hidden character class start node.
                BestResultText = string.Format(L10n.Plural("Best result so far: {0} point spent",
                    "Best result so far: {0} points spent", (uint)_bestSoFar.Count - 1), _bestSoFar.Count - 1);
                _tree.HighlightedNodes = new HashSet<ushort>(_bestSoFar);
                _tree.DrawNodeBaseSurroundHighlight();
            }
        }

        private bool _isPaused;
        private bool _isCanceling;

        // Once solutionWorker_DoWork is done working in the background, all eventually
        // queued up solutionWorker_ProgressChanged calls return without doing anything.
        // This is mainly noticeable for very small searchSpaces (and therefore a high
        // maxGeneration).
        private bool _stopReporting;

#region Presentation

        private double _progressbarMax;

        public double ProgressbarMax
        {
            get { return _progressbarMax; }
            private set
            {
                _progressbarMax = value;
                OnPropertyChanged("ProgressbarMax");
            }
        }

        private double _progressbarCurrent;

        public double ProgressbarCurrent
        {
            get { return _progressbarCurrent; }
            private set
            {
                _progressbarCurrent = value;
                OnPropertyChanged("ProgressbarCurrent");
            }
        }

        private string _progressbarText = L10n.Message("Initializing...");

        public string ProgressbarText
        {
            get { return _progressbarText; }
            private set
            {
                _progressbarText = value;
                OnPropertyChanged("ProgressbarText");
            }
        }

        private bool _progressbarEnabled = true;

        public bool ProgressbarEnabled
        {
            get { return _progressbarEnabled; }
            private set
            {
                _progressbarEnabled = value;
                OnPropertyChanged("ProgressbarEnabled");
            }
        }

        private bool _cancelCloseEnabled;

        public bool CancelCloseEnabled
        {
            get { return _cancelCloseEnabled; }
            private set
            {
                _cancelCloseEnabled = value;
                OnPropertyChanged("CancelCloseEnabled");
            }
        }

        private string _cancelCloseText = L10n.Message("Cancel");

        public string CancelCloseText
        {
            get { return _cancelCloseText; }
            private set
            {
                _cancelCloseText = value;
                OnPropertyChanged("CancelCloseText");
            }
        }

        private bool _pauseResumeEnabled;

        public bool PauseResumeEnabled
        {
            get { return _pauseResumeEnabled; }
            private set
            {
                _pauseResumeEnabled = value;
                OnPropertyChanged("PauseResumeEnabled");
            }
        }

        private string _pauseResumeText = L10n.Message("Pause");

        public string PauseResumeText
        {
            get { return _pauseResumeText; }
            private set
            {
                _pauseResumeText = value;
                OnPropertyChanged("PauseResumeText");
            }
        }

        private string _bestResultText;

        public string BestResultText
        {
            get { return _bestResultText; }
            private set
            {
                _bestResultText = value;
                OnPropertyChanged("BestResultText");
            }
        }

#endregion

#region Commands

        private RelayCommand _cancelCloseCommand;

        public ICommand CancelCloseCommand
        {
            get
            {
                if (_cancelCloseCommand == null)
                {
                    _cancelCloseCommand = new RelayCommand(param => CancelClose());
                }
                return _cancelCloseCommand;
            }
        }

        private RelayCommand _pauseResumeCommand;

        public ICommand PauseResumeCommand
        {
            get
            {
                if (_pauseResumeCommand == null)
                {
                    _pauseResumeCommand = new RelayCommand(param => PauseResume());
                }
                return _pauseResumeCommand;
            }
        }

#endregion

        public ControllerViewModel(ISolver solver, string generatorName)
        {
            _solver = solver;
            DisplayName = L10n.Message("Skill tree generator") + " - " + generatorName;
            _tree = _solver.Tree;

            _initializationWorker.DoWork += InitializationWorkerOnDoWork;
            _initializationWorker.RunWorkerCompleted += InitializationWorkerOnRunWorkerCompleted;

            _solutionWorker.DoWork += SolutionWorkerOnDoWork;
            _solutionWorker.ProgressChanged += SolutionWorkerOnProgressChanged;
            _solutionWorker.RunWorkerCompleted += SolutionWorkerOnRunWorkerCompleted;
            _solutionWorker.WorkerReportsProgress = true;
            _solutionWorker.WorkerSupportsCancellation = true;
        }

        public void WindowLoaded()
        {
            _initializationWorker.RunWorkerAsync();
        }

        private void InitializationWorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
#if DEBUG
            var stopwatch = new Stopwatch();
            stopwatch.Start();
#endif
            _solver.Initialize();
#if DEBUG
            stopwatch.Stop();
            Debug.WriteLine("Initialization took " + stopwatch.ElapsedMilliseconds + " ms\n-----------------");
#endif
        }

        private void InitializationWorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            if (runWorkerCompletedEventArgs.Error is InvalidOperationException)
            {
                // Show a dialog and close this if the omitted nodes disconnect the tree.
                Popup.Warning(L10n.Message("The optimizer was unable to find a conforming tree.\nPlease change skill node highlighting and try again."));
                Close(false);
                return;
            }

            _maxSteps = _solver.MaxGeneration;
            ProgressbarMax = _maxSteps;
            ProgressbarText = "0/" + _maxSteps;

            _isPaused = false;
            _isCanceling = false;
            CancelCloseEnabled = true;
            PauseResumeEnabled = true;
            _solutionWorker.RunWorkerAsync();
        }

        private void SolutionWorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            var worker = (BackgroundWorker)sender;
#if DEBUG
            var stopwatch = new Stopwatch();
            stopwatch.Start();
#endif
            while (!_solver.IsConsideredDone)
            {
                _solver.Step();

                worker.ReportProgress(_solver.CurrentGeneration, _solver.BestSolution);

                if (worker.CancellationPending)
                    break;
            }
#if DEBUG
            stopwatch.Stop();
            Debug.WriteLine("Finished in " + stopwatch.ElapsedMilliseconds + " ms\n==================");
#endif
            doWorkEventArgs.Result = _solver.BestSolution;
            _stopReporting = true;
        }

        private void SolutionWorkerOnProgressChanged(object sender, ProgressChangedEventArgs progressChangedEventArgs)
        {
            if (_isCanceling || _stopReporting)
            {
                return;
            }

            ProgressbarCurrent = progressChangedEventArgs.ProgressPercentage;
            ProgressbarText = progressChangedEventArgs.ProgressPercentage + "/" + _maxSteps;
            BestSoFar = (HashSet<ushort>)(progressChangedEventArgs.UserState);
        }

        private void SolutionWorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            if (_isCanceling)
            {
                PauseResumeEnabled = true;
                _isCanceling = false;
                return;
            }

            ProgressbarText = L10n.Message("Finished!");
            CancelCloseText = L10n.Message("Close");
            PauseResumeEnabled = false;
            _isPaused = true;

            // Draw the final solution in case not all ProgressChangeds get executed.
            ProgressbarCurrent = _maxSteps;
            BestSoFar = (HashSet<ushort>)runWorkerCompletedEventArgs.Result;
        }

        private void CancelClose()
        {
            // Stop the optimizer and/or close the window.
            if (_isPaused)
            {
                Close(true);
            }
            else
            {
                _solutionWorker.CancelAsync();
                _isCanceling = true;
                Close(false);
            }
        }

        private void PauseResume()
        {
            // Pause the optimizer
            if (_isPaused)
            {
                PauseResumeText = L10n.Message("Pause");
                CancelCloseText = L10n.Message("Cancel");
                ProgressbarEnabled = true;
                _solutionWorker.RunWorkerAsync();
                _isPaused = false;
            }
            else
            {
                PauseResumeText = L10n.Message("Continue");
                CancelCloseText = L10n.Message("Close");
                // Disable the button until the worker has actually finished.
                PauseResumeEnabled = false;
                ProgressbarEnabled = false;
                _solutionWorker.CancelAsync();
                _isCanceling = true;
                _isPaused = true;
            }
        }
    }
}
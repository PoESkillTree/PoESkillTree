using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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

        // Once solutionWorker_DoWork is done working in the background, all eventually
        // queued up solutionWorker_ProgressChanged calls return without doing anything.
        // This is mainly noticeable for very small searchSpaces (and therefore a high
        // maxGeneration).
        private bool _stopReporting;

        private CancellationTokenSource _cts;

        private readonly IProgress<Tuple<int, HashSet<ushort>>> _progress;

#region Presentation

        private double _progressbarMax;

        public double ProgressbarMax
        {
            get { return _progressbarMax; }
            private set
            {
                _progressbarMax = value;
                OnPropertyChanged();
            }
        }

        private double _progressbarCurrent;

        public double ProgressbarCurrent
        {
            get { return _progressbarCurrent; }
            private set
            {
                _progressbarCurrent = value;
                OnPropertyChanged();
            }
        }

        private string _progressbarText = L10n.Message("Initializing...");

        public string ProgressbarText
        {
            get { return _progressbarText; }
            private set
            {
                _progressbarText = value;
                OnPropertyChanged();
            }
        }

        private bool _progressbarEnabled = true;

        public bool ProgressbarEnabled
        {
            get { return _progressbarEnabled; }
            private set
            {
                _progressbarEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _cancelCloseEnabled;

        public bool CancelCloseEnabled
        {
            get { return _cancelCloseEnabled; }
            private set
            {
                _cancelCloseEnabled = value;
                OnPropertyChanged();
            }
        }

        private string _cancelCloseText = L10n.Message("Cancel");

        public string CancelCloseText
        {
            get { return _cancelCloseText; }
            private set
            {
                _cancelCloseText = value;
                OnPropertyChanged();
            }
        }

        private bool _pauseResumeEnabled;

        public bool PauseResumeEnabled
        {
            get { return _pauseResumeEnabled; }
            private set
            {
                _pauseResumeEnabled = value;
                OnPropertyChanged();
            }
        }

        private string _pauseResumeText = L10n.Message("Pause");

        public string PauseResumeText
        {
            get { return _pauseResumeText; }
            private set
            {
                _pauseResumeText = value;
                OnPropertyChanged();
            }
        }

        private string _bestResultText;

        public string BestResultText
        {
            get { return _bestResultText; }
            private set
            {
                _bestResultText = value;
                OnPropertyChanged();
            }
        }

#endregion

#region Commands

        private RelayCommand _cancelCloseCommand;

        public ICommand CancelCloseCommand
        {
            get { return _cancelCloseCommand ?? (_cancelCloseCommand = new RelayCommand(param => CancelClose())); }
        }

        private RelayCommand _pauseResumeCommand;

        public ICommand PauseResumeCommand
        {
            get { return _pauseResumeCommand ?? (_pauseResumeCommand = new RelayCommand(param => PauseResume())); }
        }

#endregion

        public ControllerViewModel(ISolver solver, string generatorName)
        {
            _solver = solver;
            DisplayName = L10n.Message("Skill tree generator") + " - " + generatorName;
            _tree = _solver.Tree;

            _progress = new Progress<Tuple<int, HashSet<ushort>>>(tuple => ReportProgress(tuple.Item1, tuple.Item2));
        }

        public async void WindowLoaded()
        {
            if (await InitializeAsync())
            {
                // only start Solver if initialization was successful.
                await SolveAsync();
            }
        }

        private async Task<bool> InitializeAsync()
        {
            try
            {
                _maxSteps = await Task.Run(() =>
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
                    return _solver.MaxGeneration;
                });

            }
            catch (InvalidOperationException)
            {
                // Show a dialog and close this if the omitted nodes disconnect the tree.
                Popup.Warning(L10n.Message("The optimizer was unable to find a conforming tree.\nPlease change skill node highlighting and try again."));
                Close(false);
                return false;
            }

            ProgressbarMax = _maxSteps;
            ProgressbarText = "0/" + _maxSteps;

            CancelCloseEnabled = true;
            PauseResumeEnabled = true;

            return true;
        }

        private async Task SolveAsync()
        {
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            _stopReporting = false;

            HashSet<ushort> result;
            try
            {
                result = await Task.Run(() =>
                {
#if DEBUG
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
#endif
                    while (!_solver.IsConsideredDone)
                    {
                        _solver.Step();

                        _progress.Report(new Tuple<int, HashSet<ushort>>(_solver.CurrentGeneration, _solver.BestSolution));

                        token.ThrowIfCancellationRequested();
                    }
#if DEBUG
                    stopwatch.Stop();
                    Debug.WriteLine("Finished in " + stopwatch.ElapsedMilliseconds + " ms\n==================");
#endif
                    return _solver.BestSolution;
                }, token);
            }
            catch (OperationCanceledException)
            {
                PauseResumeEnabled = true;
                _stopReporting = true;
                return;
            }

            _stopReporting = true;

            ProgressbarText = L10n.Message("Finished!");
            CancelCloseText = L10n.Message("Close");
            PauseResumeEnabled = false;
            _isPaused = true;

            // Draw the final solution in case not all ProgressChangeds get executed.
            ProgressbarCurrent = _maxSteps;
            BestSoFar = result;
        }

        private void ReportProgress(int step, HashSet<ushort> bestSoFar)
        {
            if (_stopReporting)
            {
                return;
            }

            ProgressbarCurrent = step;
            ProgressbarText = step + "/" + _maxSteps;
            BestSoFar = bestSoFar;
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
                _cts.Cancel();
                Close(false);
            }
        }

        private async void PauseResume()
        {
            // Pause the optimizer
            if (_isPaused)
            {
                PauseResumeText = L10n.Message("Pause");
                CancelCloseText = L10n.Message("Cancel");
                ProgressbarEnabled = true;
                _isPaused = false;
                await SolveAsync();
            }
            else
            {
                PauseResumeText = L10n.Message("Continue");
                CancelCloseText = L10n.Message("Close");
                // Disable the button until the worker has actually finished.
                PauseResumeEnabled = false;
                ProgressbarEnabled = false;
                _cts.Cancel();
                _isPaused = true;
            }
        }
    }
}
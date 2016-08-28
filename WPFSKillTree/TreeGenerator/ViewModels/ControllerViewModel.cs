using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using POESKillTree.Common.ViewModels;
using POESKillTree.Controls.Dialogs;
using POESKillTree.Localization;
using POESKillTree.Model;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.Algorithm;
using POESKillTree.TreeGenerator.Solver;
using POESKillTree.Utils;
using POESKillTree.ViewModels;

namespace POESKillTree.TreeGenerator.ViewModels
{
    /// <summary>
    /// ViewModel that runs a solver and reports its results.
    /// </summary>
    public sealed class ControllerViewModel : CloseableViewModel
    {
        private static readonly string IterationPrefix = L10n.Message("Current iteration:") + " ";

        /// <summary>
        /// The solver run by this ViewModel.
        /// </summary>
        private readonly ISolver _solver;

        private readonly SkillTree _tree;

        private readonly IDialogCoordinator _dialogCoordinator;

        private HashSet<ushort> _bestSoFar;

        private readonly TaskCompletionSource<IEnumerable<ushort>> _solverTcs = new TaskCompletionSource<IEnumerable<ushort>>();

        private bool _isSolving;

        /// <summary>
        /// Used to skip calls to <see cref="ReportProgress"/> that got stacked up while
        /// the method was processing the last call.
        /// </summary>
        private readonly Stopwatch _reportStopwatch = new Stopwatch();

        /// <summary>
        /// Token source for cancelling the solver.
        /// </summary>
        private CancellationTokenSource _cts;

        /// <summary>
        /// Used to report progress from the solver running thread to the UI thread.
        /// </summary>
        private readonly IProgress<Tuple<int, int, IEnumerable<ushort>>> _progress;

#region Presentation

        private double _progressbarMax;
        /// <summary>
        /// Gets the maximum value the progress can reach.
        /// </summary>
        public double ProgressbarMax
        {
            get { return _progressbarMax; }
            private set { SetProperty(ref _progressbarMax, value); }
        }

        private double _progressbarCurrent = 1;
        /// <summary>
        /// Gets the current progress value.
        /// </summary>
        public double ProgressbarCurrent
        {
            get { return _progressbarCurrent; }
            private set { SetProperty(ref _progressbarCurrent, value); }
        }

        private string _progressbarText = L10n.Message("Initializing...");
        /// <summary>
        /// Gets additional information about the current progress.
        /// </summary>
        public string ProgressbarText
        {
            get { return _progressbarText; }
            private set { SetProperty(ref _progressbarText, value); }
        }

        private bool _progressbarEnabled = true;
        /// <summary>
        /// Gets whether the solver is currently progressing.
        /// </summary>
        public bool ProgressbarEnabled
        {
            get { return _progressbarEnabled; }
            private set { SetProperty(ref _progressbarEnabled, value); }
        }

        private string _iterationText;
        /// <summary>
        /// Gets the text that should be displayed to indicate in which iteration
        /// the solver currently is.
        /// </summary>
        public string IterationText
        {
            get { return _iterationText; }
            private set { SetProperty(ref _iterationText, value); }
        }

        private bool _cancelCloseEnabled;
        /// <summary>
        /// Gets whether the Cancel/Close button should be enabled.
        /// </summary>
        public bool CancelCloseEnabled
        {
            get { return _cancelCloseEnabled; }
            private set { SetProperty(ref _cancelCloseEnabled, value); }
        }

        private string _cancelCloseText = L10n.Message("Cancel");
        /// <summary>
        /// Gets the string the Cancel/Close button should show.
        /// </summary>
        public string CancelCloseText
        {
            get { return _cancelCloseText; }
            private set { SetProperty(ref _cancelCloseText, value); }
        }

        private bool _pauseResumeEnabled;
        /// <summary>
        /// Gets whether the Pause/Resume button should be enabled.
        /// </summary>
        public bool PauseResumeEnabled
        {
            get { return _pauseResumeEnabled; }
            private set { SetProperty(ref _pauseResumeEnabled, value); }
        }

        private string _pauseResumeText = L10n.Message("Pause");
        /// <summary>
        /// Gets the string the Pause/Resume button should show.
        /// </summary>
        public string PauseResumeText
        {
            get { return _pauseResumeText; }
            private set { SetProperty(ref _pauseResumeText, value); }
        }

        private string _bestResultText;
        /// <summary>
        /// Gets a string describing the currently best result calculated by the solver.
        /// </summary>
        public string BestResultText
        {
            get { return _bestResultText; }
            private set { SetProperty(ref _bestResultText, value); }
        }

#endregion

#region Commands

        private RelayCommand _pauseResumeCommand;
        /// <summary>
        /// Gets a command that pauses or resumes execution of the solver.
        /// </summary>
        public ICommand PauseResumeCommand
        {
            get { return _pauseResumeCommand ?? (_pauseResumeCommand = new RelayCommand(PauseResume)); }
        }

#endregion

        /// <summary>
        /// Instantiates a new ControllerViewModel.
        /// </summary>
        /// <param name="solver">The (not null) solver this object should run.</param>
        /// <param name="generatorName">The name suffix shown as DisplayName as 'Skill tree generator - {generatorName}'</param>
        /// <param name="tree">SkillTree to operate on (not null)</param>
        /// <param name="dialogCoordinator"></param>
        public ControllerViewModel(ISolver solver, string generatorName, SkillTree tree, IDialogCoordinator dialogCoordinator)
        {
            if (solver == null) throw new ArgumentNullException("solver");
            if (tree == null) throw new ArgumentNullException("tree");

            _solver = solver;
            DisplayName = L10n.Message("Skill tree generator") + " - " + generatorName;
            _tree = tree;
            _dialogCoordinator = dialogCoordinator;
            
            if (_solver.Iterations > 1)
                IterationText = IterationPrefix + "1/" + _solver.Iterations;

            _progress = new Progress<Tuple<int, int, IEnumerable<ushort>>>(tuple => ReportProgress(tuple.Item1, tuple.Item2, tuple.Item3));
            _reportStopwatch.Start();

            RequestsClose += _ => CancelClose();
        }

        /// <summary>
        /// Starts executing the solver asynchronously.
        /// </summary>
        public async Task<IEnumerable<ushort>> RunSolverAsync()
        {
            if (await InitializeAsync())
            {
                // only start Solver if initialization was successful.
                await SolveAsync();
            }
            return await _solverTcs.Task;
        }

        /// <summary>
        /// Initializes the solver asynchronously.
        /// </summary>
        /// <returns>Whether the initialization was successful.</returns>
        private async Task<bool> InitializeAsync()
        {
            var success = true;
            try
            {
                await Task.Run(() =>
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
                });

            }
            catch (GraphNotConnectedException)
            {
                // No await in catch
                success = false;
            }
            if (!success)
            {
                // Show a dialog and close this if the omitted nodes disconnect the tree.
                await _dialogCoordinator.ShowWarningAsync(this,
                    L10n.Message("The optimizer was unable to find a conforming tree.\nPlease change skill node tagging and try again."));
                Close();
                return false;
            }

            ProgressbarMax = _solver.Steps * _solver.Iterations;
            ProgressbarText = "1/" + ProgressbarMax;

            CancelCloseEnabled = true;
            PauseResumeEnabled = true;

            return true;
        }

        /// <summary>
        /// Executes the solver asyncronously.
        /// </summary>
        private async Task SolveAsync()
        {
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            _isSolving = true;

            IEnumerable<ushort> result;
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

                        _progress.Report(new Tuple<int, int, IEnumerable<ushort>>(_solver.CurrentStep,
                            _solver.CurrentIteration, _solver.BestSolution));

                        token.ThrowIfCancellationRequested();
                    }
                    _solver.FinalStep();
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
                _isSolving = false;
                return;
            }

            _isSolving = false;

            ProgressbarText = L10n.Message("Finished!");
            CancelCloseText = L10n.Message("Close");
            PauseResumeEnabled = false;

            // Draw the final solution.
            ProgressbarCurrent = ProgressbarMax;
            UpdateBestSoFar(result);
        }

        /// <summary>
        /// Reports solver progress to be displayed.
        /// </summary>
        /// <param name="step">The number of executed steps.</param>
        /// <param name="iteration">The number of executed iterations.</param>
        /// <param name="bestSoFar">The best result generated to this point.</param>
        private void ReportProgress(int step, int iteration, IEnumerable<ushort> bestSoFar)
        {
            if (_solverTcs.Task.IsCompleted || _reportStopwatch.ElapsedMilliseconds < 10)
            {
                return;
            }

            ProgressbarCurrent = step + iteration * _solver.Steps;
            ProgressbarText = ProgressbarCurrent + "/" + ProgressbarMax;
            if (_solver.Iterations > 1)
                IterationText = IterationPrefix + (iteration + 1) + "/" + _solver.Iterations;
            UpdateBestSoFar(bestSoFar);
            
            _reportStopwatch.Restart();
        }

        private void UpdateBestSoFar(IEnumerable<ushort> value)
        {
            _bestSoFar = new HashSet<ushort>(value);
            var nodeCount = _bestSoFar.Count - _solver.UncountedNodes;
            BestResultText = string.Format(L10n.Plural("Best result so far: {0} point spent",
                "Best result so far: {0} points spent", (uint)nodeCount), nodeCount);
            _tree.HighlightedNodes.Clear();
            _tree.HighlightedNodes.UnionWith(_bestSoFar.Select(n => SkillTree.Skillnodes[n]));
        }

        /// <summary>
        /// Stop the optimizer and/or close the window.
        /// </summary>
        private void CancelClose()
        {
            // Don't do this more than once.
            if (_solverTcs.Task.IsCompleted)
                return;
            _solverTcs.TrySetResult(_isSolving ? null : _bestSoFar);
            if (_isSolving && _cts != null)
                _cts.Cancel();
        }

        /// <summary>
        /// Pause/Resume execution of the solver.
        /// </summary>
        private async void PauseResume()
        {
            if (!_isSolving)
            {
                PauseResumeText = L10n.Message("Pause");
                CancelCloseText = L10n.Message("Cancel");
                ProgressbarEnabled = true;
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
            }
        }
    }
}
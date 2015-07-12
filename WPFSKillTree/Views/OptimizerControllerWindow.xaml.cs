using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using POESKillTree.Localization;
using POESKillTree.SkillTreeFiles;
using POESKillTree.SkillTreeFiles.SteinerTrees;
using System.ComponentModel;
using MessageBox = POESKillTree.Views.MetroMessageBox;
using System.Diagnostics;
using POESKillTree.Utils;

namespace POESKillTree.Views
{
    /// <summary>
    /// Interaction logic for OptimizerController.xaml
    /// </summary>
    public partial class OptimizerControllerWindow : MetroWindow
    {
        // TODO remove -- replaced by TreeGenerator.Views.ControllerWindow and TreeGenerator.ViewModels.ControllerViewModel
        private SteinerSolver steinerSolver;
        private SkillTree tree;
        private HashSet<ushort> targetNodes;
        private HashSet<ushort> nodesToOmit;

        private readonly BackgroundWorker solutionWorker = new BackgroundWorker();
        private readonly BackgroundWorker initializationWorker = new BackgroundWorker();

        int maxSteps;

        public HashSet<ushort> bestSoFar;

        private bool isPaused;
        private bool isCanceling;

        // Once solutionWorker_DoWork is done working in the background, all eventually
        // queued up solutionWorker_ProgressChanged calls return without doing anything.
        // This is mainly noticeable for very small searchSpaces (and therefore a high
        // maxGeneration).
        private bool _stopReporting;

        public OptimizerControllerWindow(SkillTree tree, HashSet<ushort> targetNodes, HashSet<ushort> nodesToOmit = null)
        {
            InitializeComponent();
            this.tree = tree;
            steinerSolver = new SteinerSolver(tree);
            this.targetNodes = targetNodes;
            this.nodesToOmit = nodesToOmit;
            
            initializationWorker.DoWork += initializationWorker_DoWork;
            initializationWorker.RunWorkerCompleted += initializationWorker_RunWorkerCompleted;

            solutionWorker.DoWork += solutionWorker_DoWork;
            solutionWorker.ProgressChanged += solutionWorker_ProgressChanged;
            solutionWorker.RunWorkerCompleted += solutionWorker_RunWorkerCompleted;
            solutionWorker.WorkerReportsProgress = true;
            solutionWorker.WorkerSupportsCancellation = true;
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            btnPopupCancelClose.Content = L10n.Message("Cancel");
            btnPopupPauseResume.Content = L10n.Message("Pause");

            initializationWorker.RunWorkerAsync();
        }


        void initializationWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // This is also deferred to a background task as it might take a while.
#if DEBUG
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
#endif
            steinerSolver.InitializeSolver(targetNodes, nodesToOmit);
#if DEBUG
            stopwatch.Stop();
            Debug.WriteLine("Initialization took " + stopwatch.ElapsedMilliseconds + " ms\n-----------------");
#endif
        }

        void initializationWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error is InvalidOperationException)
            {
                // Show a dialog and close this if the omitted nodes disconnect the tree.
                Popup.Warning(L10n.Message("The optimizer was unable to find a conforming tree.\nPlease change skill node tagging and try again."));
                Close();
                return;
            }

            maxSteps = steinerSolver.MaxGeneration;
            progressBar.Maximum = maxSteps;
            lblProgressText.Content = "0/" + maxSteps;

            isPaused = false;
            isCanceling = false;
            btnPopupCancelClose.IsEnabled = true;
            btnPopupPauseResume.IsEnabled = true;
            solutionWorker.RunWorkerAsync();
        }

        void solutionWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
#if DEBUG
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
#endif
            while (!steinerSolver.IsConsideredDone)
            {
                steinerSolver.EvolutionStep();

                worker.ReportProgress(steinerSolver.CurrentGeneration, steinerSolver.BestSolution);

                if (worker.CancellationPending)
                    break;
            }
#if DEBUG
            stopwatch.Stop();
            Debug.WriteLine("Finished in " + stopwatch.ElapsedMilliseconds + " ms\n==================");
#endif
            e.Result = steinerSolver.BestSolution;
            _stopReporting = true;
        }

        void solutionWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (isCanceling || _stopReporting)
            {
                return;
            }

            progressBar.Value = e.ProgressPercentage;
            lblProgressText.Content = e.ProgressPercentage.ToString() + "/" + maxSteps;
            bestSoFar = (HashSet<ushort>)(e.UserState);
            lblBestResult.Content = string.Format(L10n.Plural("Best result so far: {0} additional point spent", "Best result so far: {0} additional points spent", (uint)bestSoFar.Count), bestSoFar.Count);
            tree.HighlightedNodes = new HashSet<ushort>(bestSoFar.Concat(tree.SkilledNodes));
            tree.DrawNodeBaseSurroundHighlight();
        }

        void solutionWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (isCanceling)
            {
                btnPopupPauseResume.IsEnabled = true;
                isCanceling = false;
                return;
            }
            
            lblProgressText.Content = L10n.Message("Finished!");
            btnPopupCancelClose.Content = L10n.Message("Close");
            btnPopupPauseResume.IsEnabled = false;
            bestSoFar = (HashSet<ushort>)e.Result;
            isPaused = true;

            // Draw the final solution in case not all ProgressChangeds get executed.
            progressBar.Value = maxSteps;
            lblBestResult.Content = string.Format(L10n.Plural("Best result so far: {0} additional point spent", "Best result so far: {0} additional points spent", (uint)bestSoFar.Count), bestSoFar.Count);
            tree.HighlightedNodes = new HashSet<ushort>(bestSoFar.Concat(tree.SkilledNodes));
            tree.DrawNodeBaseSurroundHighlight();
        }

        #region UI interaction
        private void btnPopupCancelClose_Click(object sender, RoutedEventArgs e)
        {
            // Stop the optimizer and/or close the window.
            if (!isPaused)
            {
                solutionWorker.CancelAsync();
                isCanceling = true;
                DialogResult = false;
            }
            else
                DialogResult = true;
        }

        private void btnPopupPauseResume_Click(object sender, RoutedEventArgs e)
        {
            // Pause the optimizer
            if (isPaused)
            {
                btnPopupPauseResume.Content = L10n.Message("Pause");
                progressBar.IsEnabled = true;
                solutionWorker.RunWorkerAsync();
                isPaused = false;
            }
            else
            {
                btnPopupPauseResume.Content = L10n.Message("Continue");
                // Disable the button until the worker has actually finished.
                btnPopupPauseResume.IsEnabled = false;
                progressBar.IsEnabled = false;
                //lblProgressText.Content += " (paused)";
                solutionWorker.CancelAsync();
                isCanceling = true;
                isPaused = true;
            }
        }
        #endregion
    }
}

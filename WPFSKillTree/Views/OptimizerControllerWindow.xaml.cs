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
using POESKillTree.SkillTreeFiles;
using POESKillTree.SkillTreeFiles.SteinerTrees;
using System.ComponentModel;
using MessageBox = POESKillTree.Views.MetroMessageBox;

namespace POESKillTree.Views
{
    /// <summary>
    /// Interaction logic for OptimizerController.xaml
    /// </summary>
    public partial class OptimizerControllerWindow : MetroWindow
    {
        private SteinerSolver steinerSolver;
        private SkillTree tree;

        private readonly BackgroundWorker worker = new BackgroundWorker();

        int maxSteps = 200;
        int step;

        public HashSet<ushort> bestSoFar;
        
        private bool isPaused;
        private bool canceling;

        public OptimizerControllerWindow(SkillTree Tree, HashSet<ushort> targetNodes)
        {
            InitializeComponent();
            tree = Tree;
            steinerSolver = new SteinerSolver(Tree);
            // This should maybe also be part of a background task since it might take a moment.
            steinerSolver.InitializeSolver(targetNodes);
            maxSteps = steinerSolver.MaxGeneration;
            progressBar.Maximum = maxSteps;

            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            //for (; step <= maxSteps; step++)
            while (!steinerSolver.IsConsideredDone)
            {
                steinerSolver.EvolutionStep();

                worker.ReportProgress(steinerSolver.CurrentGeneration, steinerSolver.BestSolution);

                if (worker.CancellationPending)
                    break;
            }
            e.Result = steinerSolver.BestSolution;
        }


        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            lblProgressText.Content = e.ProgressPercentage.ToString() + "/" + maxSteps;
            bestSoFar = (HashSet<ushort>)(e.UserState);
            lblBestResult.Content = "Best result so far: " + bestSoFar.Count +
                " additional points spent.";
            tree.HighlightedNodes = bestSoFar;
            tree.DrawNodeBaseSurroundHighlight();
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (canceling)
            {
                btnPopupPauseResume.IsEnabled = true;
                canceling = false;
                return;
            }
            
            lblProgressText.Content = "Finished!";
            btnPopupCancelClose.Content = "Close";
            btnPopupPauseResume.IsEnabled = false;
            bestSoFar = (HashSet<ushort>)e.Result;
            isPaused = true;
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // TODO: Initialize labels
            btnPopupCancelClose.Content = "Cancel";
            btnPopupPauseResume.Content = "Pause";
            step = 0;
            isPaused = false;
            canceling = false;
            worker.RunWorkerAsync();
        }

        private void btnPopupCancelClose_Click(object sender, RoutedEventArgs e)
        {
            // Stop the optimizer and/or close the window.
            if (!isPaused)
            {
                worker.CancelAsync();
                canceling = true;
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
                btnPopupPauseResume.Content = "Pause";
                progressBar.IsEnabled = true;
                worker.RunWorkerAsync();
                isPaused = false;
            }
            else
            {
                btnPopupPauseResume.Content = "Continue";
                // Disable the button until the worker has actually finished.
                btnPopupPauseResume.IsEnabled = false;
                progressBar.IsEnabled = false;
                //lblProgressText.Content += " (paused)";
                worker.CancelAsync();
                canceling = true;
                isPaused = true;
            }
        }



    }
}

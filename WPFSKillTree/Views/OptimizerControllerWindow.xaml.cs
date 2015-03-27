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

namespace POESKillTree.Views
{
    /// <summary>
    /// Interaction logic for OptimizerController.xaml
    /// </summary>
    public partial class OptimizerControllerWindow : MetroWindow
    {
        private Steiner steinerSolver;

        private readonly BackgroundWorker worker = new BackgroundWorker();


        public OptimizerControllerWindow(SkillTree Tree, HashSet<ushort> targetNodes)
        {
            InitializeComponent();

            steinerSolver = new Steiner(Tree);
            // This should maybe also be part of a background task since it might take a moment.
            steinerSolver.InitializeSolver(targetNodes);

            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < 200; i++)
            {
                steinerSolver.EvolutionStep();
                
            }
            e.Result = steinerSolver.BestSolution;
        }


        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            e.ProgressPercentage
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnPopupCancelClose.Content = "Close";
            btnPopupPauseResume.IsEnabled = false;
            lblProgressText.Content = "Finished!";
        }
        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // TODO: Initialize labels
        }

        private void btnPopupCancelClose_Click(object sender, RoutedEventArgs e)
        {
            // Stop the optimizer and/or close the window.
        }

        private void btnPopupPauseResume_Click(object sender, RoutedEventArgs e)
        {
            // Pause the optimizer
        }



    }
}

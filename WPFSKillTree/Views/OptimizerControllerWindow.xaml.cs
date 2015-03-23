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

        public OptimizerControllerWindow(SkillTree Tree, HashSet<ushort> targetNodes)
        {
            steinerSolver = new Steiner(Tree);
            InitializeComponent();
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

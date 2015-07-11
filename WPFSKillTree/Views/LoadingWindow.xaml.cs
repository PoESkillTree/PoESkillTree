using System;
using System.Threading;
using MahApps.Metro.Controls;
using System.ComponentModel;

namespace POESKillTree.Views
{
    /// <summary>
    /// Interaction logic for LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : MetroWindow, INotifyPropertyChanged
    {


        double _progress = 0;

        public double Progress
        {
            get { return _progress; }
            set 
            { 
                _progress = value;
                OnPropertyChanged("Progress");
            }
        }
        double _maxProgress = 0;

        public double MaxProgress
        {
            get { return _maxProgress; }
            set 
            { 
                _maxProgress = value;
                OnPropertyChanged("MaxProgress");
            }

        }

        public LoadingWindow( )
        {
            InitializeComponent( );
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property)
        {
            if(PropertyChanged!=null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}

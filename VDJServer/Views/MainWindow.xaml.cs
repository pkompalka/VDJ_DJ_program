using System;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using VDJServer.ViewModels;

namespace VDJServer
{
    public partial class MainWindow : MetroWindow
    {        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private bool AutoScroll = true;

        private void ScrollViewer_ScrollChanged(Object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange == 0)
            {
                if (myScroll.VerticalOffset == myScroll.ScrollableHeight)
                {
                    AutoScroll = true;
                }
                else
                {
                    AutoScroll = false;
                }
            }

            if (AutoScroll && e.ExtentHeightChange != 0)
            {
                myScroll.ScrollToVerticalOffset(myScroll.ExtentHeight);
            }
        }

        private bool AutoScroll2 = true;

        private void ScrollViewer2_ScrollChanged(Object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange == 0)
            {
                if (myScroll2.VerticalOffset == myScroll2.ScrollableHeight)
                {
                    AutoScroll2 = true;
                }
                else
                {
                    AutoScroll2 = false;
                }
            }

            if (AutoScroll2 && e.ExtentHeightChange != 0)
            {
                myScroll2.ScrollToVerticalOffset(myScroll2.ExtentHeight);
            }

        }
    }
}

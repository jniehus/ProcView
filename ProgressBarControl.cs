using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows;
using System.Threading;

namespace ProcView
{
    public class ProgressBarControl
    {
        public ProgressBarControl()
        {
        }

        public static void Start(ProgressBar progbar, TextBlock progbarLabel)
        {
            progbar.Value = 0;
            progbarLabel.Text = "Processing Image: ";
            progbar.IsIndeterminate = false;
            progbar.Visibility = Visibility.Visible;
        }

        public static void Clear(ProgressBar progbar, TextBlock progbarLabel)
        {
            progbarLabel.Text = "";
            progbar.Value = 0;
            progbar.Visibility = Visibility.Hidden;
            
        }

    }
}

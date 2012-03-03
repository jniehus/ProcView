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

namespace ProcView
{
    /// <summary>
    /// Interaction logic for KernelNameDialog.xaml
    /// </summary>
    public partial class KernelNameDialog : Window
    {
        private string _customKernelName = string.Empty;

        public KernelNameDialog()
        {
            InitializeComponent();
            instructionLabel.Content = "Please enter a name for your custom kernel.";
            kernelNameTextBox.Text = "Custom Kernel";
        }

        public string customKernelName
        {
            get { return _customKernelName; }
        }

        private void acceptNameOKButton_Click(object sender, RoutedEventArgs e)
        {
            _customKernelName = kernelNameTextBox.Text;
            this.Close();
        }
    }
}

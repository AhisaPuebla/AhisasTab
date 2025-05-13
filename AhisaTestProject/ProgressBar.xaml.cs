using Autodesk.Revit.DB.Visual;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AhisaTestProject
{
    /// <summary>
    /// Interaction logic for ProgressBar.xaml
    /// </summary>
    public partial class ProgressBar : Window
    {
        public int Total;
        public bool CancelFlag = false;

        public ProgressBar(int total)
        {
            InitializeComponent();
            Total = total;
            lblText.Text = $"Updating 0 of {Total} elements";
            pbProgress.Minimum = 0;
            pbProgress.Maximum = Total;
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CancelFlag = true;
        }
    }
}

using DeepNestLib;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace DeepNestPort.Core
{
    /// <summary>
    /// Interaction logic for RibbonMenuWpf.xaml
    /// </summary>
    public partial class RibbonMenuWpf : System.Windows.Controls.UserControl
    {
        public RibbonMenuWpf()
        {
            InitializeComponent();
        }

        private void RibbonButton_Click(object sender, RoutedEventArgs e)
        {
            Form1.Form.AddDetail();
        }
        private void Run_Click(object sender, RoutedEventArgs e)
        {
            Form1.Form.RunNest();
        }

        private void RibbonButton_Click_1(object sender, RoutedEventArgs e)
        {
            Form1.Form.StopNesting();
        }
    }
}

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
using WebCompare3.Model;

namespace WebCompare3.View
{
    /// <summary>
    /// Interaction logic for PathDisplay.xaml
    /// </summary>
    public partial class PathDisplay : Window
    {
        public PathDisplay(List<int> path)
        {
            InitializeComponent();
            foreach(int i in path)
            {
                Ellipse node = new Ellipse();
                Line edge = new Line();
                // Use a Graph control to display nodes and edges
            }
        }
    }
}

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
        public PathDisplay()
        {
            InitializeComponent();
        }

        // Window center coordinates
        double centerY = 250;
        double centerX = 250;
        public double CenterY { get { return centerY; } set { centerY = value; } }
        public double CenterX { get { return centerX; } set { centerX = value; } }
        public string SrcText { get; set; }

        private void AddNodeWithLabel(double x, double y, string txt)
        {
            // Output variables
            var node = new Ellipse {
                Width = 20, Height = 20,
                Fill = Brushes.Red
            };

            var nodeLabel = new TextBlock {
                Text = txt,
                Width = 20, Height = 20,
                TextAlignment = TextAlignment.Center
            };
            // Location
            Canvas.SetLeft(node, x); Canvas.SetTop(node, y);
            Canvas.SetLeft(nodeLabel, x); Canvas.SetTop(nodeLabel, y);
            // Order
            Panel.SetZIndex(node, 0);
            Panel.SetZIndex(nodeLabel, 1);
            PathCanvas.Children.Add(node);
            PathCanvas.Children.Add(nodeLabel);
        }

        public void ShowPaths(List<int>[] paths)
        {
            // Display window
            this.DataContext = this;
            this.Show();

            // Update XY coordinates to window center
            CenterY = this.Height / 2;
            CenterX = this.Width / 2;

            // Center selected 

            Ellipse selectedVertex = new Ellipse();
            selectedVertex.Height = 10;
            selectedVertex.Width = 10;

            // Display paths
            for (int p = 0; p < paths.Count(); ++p)
            {
                // Reset location variables
                double newX = CenterX;
                double newY = CenterY;
                int DIST = 10;
                for (int n = 0; n < paths[p].Count(); ++n)
                {
                    switch(p)
                    {
                        case 0:
                            newX += DIST;
                            newY += DIST;
                            break;
                        case 1:
                            newX += DIST;
                            newY -= DIST;
                            break;
                        case 2:
                            newX -= DIST;
                            newY -= DIST;
                            break;
                        case 3:
                            newX -= DIST;
                            newY += DIST;
                            break;
                        default:
                            newX += DIST;
                            newY = CenterY;
                            break;
                    }
                    
                    // Add nodes to the canvas
                    AddNodeWithLabel(newX, newY, paths[p][n].ToString());
                }
            } // End display paths foreach
        } // End ShowPaths()
    } // End PathDisplay class
}

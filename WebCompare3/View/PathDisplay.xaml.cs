using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class PathDisplay : Window, INotifyPropertyChanged
    {
        public PathDisplay()
        {
            InitializeComponent();
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string str)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(str));
            }
        }
        #endregion

        // Window center coordinates
        double centerY = 470;
        double centerX = 470;
        public double CenterY {
            get { return centerY; }
            set { centerY = value;
                NotifyPropertyChanged("CenterY");
            }
        }
        public double CenterX {
            get { return centerX; }
            set { centerX = value;
                NotifyPropertyChanged("CenterX");
            }
        }
        public string SrcText { get; set; }

        private void AddNodeWithLabel(double x, double y, double oldX, double oldY, string txt)
        {
            // Output variables
            var node = new Ellipse {
                Width = 30, Height = 30,
                Fill = Brushes.Red
            };

            var nodeLabel = new TextBlock {
                Text = txt,
                Width = 20, Height = 20,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(5,5,0,0)
            };

            var line = new Line
            {
                X1 = oldX, X2 = x,
                Y1 = oldY, Y2 = y,
                Stroke = Brushes.Black,
                Margin = new Thickness(15, 5, 0, 0)
            };
            // Location
            Canvas.SetLeft(node, x); Canvas.SetTop(node, y);
            Canvas.SetLeft(nodeLabel, x); Canvas.SetTop(nodeLabel, y);
            // Order
            Panel.SetZIndex(node, 0);
            Panel.SetZIndex(nodeLabel, 1);
            PathCanvas.Children.Add(node);
            PathCanvas.Children.Add(nodeLabel);
            PathCanvas.Children.Add(line);
        }

        public void ShowPaths(List<int>[] paths)
        {
            // Display window
            this.DataContext = this;
            this.Show();

            // Update XY coordinates to window center
            CenterY = this.Height / 2 - 30;
            CenterX = this.Width / 2 - 30;

            // Center selected 

            Ellipse selectedVertex = new Ellipse();
            selectedVertex.Height = 10;
            selectedVertex.Width = 10;

            // Display paths
            for (int p = 0; p < paths.Count(); ++p)
            {
                if (paths[p] == null) continue;
                // Reset location variables
                double oldX = CenterX, oldY = CenterY;
                double newX = CenterX;
                double newY = CenterY;
                int DIST = 40;

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
                    AddNodeWithLabel(newX, newY, oldX, oldY, paths[p][n].ToString());
                    oldX = newX; oldY = newY;
                }
            } // End display paths foreach
        } // End ShowPaths()
    } // End PathDisplay class
}

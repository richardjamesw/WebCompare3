using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.ComponentModel;    //for iNotifyPropertyChanged
using System.Windows.Input; // ICommand
using WebCompare3.Model;
using System.Text.RegularExpressions;
using System.IO;
using HtmlAgilityPack;
using System.Windows.Threading;
using Prism.Commands;
using System.Runtime.Serialization.Formatters.Binary;
using WebCompare3.View;
using System.Windows.Controls;

namespace WebCompare3.ViewModel
{
    public class Session
    {
        #region Instance Variables & Constructor
        public readonly BackgroundWorker worker = new BackgroundWorker();
        private WebCompareViewModel wcViewModel = WebCompareViewModel.Instance;
        int SiteTotal = 0;

        private static object lockObj = new object();
        private static volatile Session instance;
        public static Session Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObj)
                    {
                        if (instance == null)
                            instance = new Session();
                    }
                }
                return instance;
            }
        }

        public Session()
        {
            // Register background workers
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
        }
        #endregion

        #region Session

        /// <summary>
        /// Start method
        /// </summary>
        /// <returns></returns>
        public void Start()
        {
            worker.RunWorkerAsync();
            WebCompareViewModel.Instance.GoCommand.RaiseCanExecuteChanged();
        }

        public bool CanStart()
        {
            if (Instance.worker.IsBusy)
            {
                // Disable button
                return false;
            }
            else
            {
                // Enable button
                return true;
            }
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Get selected site
            string selected = WebCompareViewModel.Instance.GraphSitesSelected;
            Vertex selectedVert = LoaderViewModel.Instance.MainGraph.GetVertexWithData(selected);
            AddMessage("Selected source: " + selectedVert.Data);
            // Use dijkstras to find shortest path
            List<int>[] paths = new List<int>[5];
            for (int r = 0; r < 5; ++r)
            {
                paths[r] = new List<int>();
                AddMessage("Searching for path to: " + LoaderViewModel.Instance.Roots[r].Name);
                paths[r] = Dijkstras(selectedVert, LoaderViewModel.Instance.Roots[r].RootVertex);
            }
            AddMessage("Found all available paths.");
            AddMessage("Displaying..\n\n");
            // Graphically display the path
            Application.Current.Dispatcher.Invoke((Action)delegate {

                PathDisplay pd = new PathDisplay();
                if (selectedVert != null)
                    pd.SrcText = selectedVert.ID.ToString();
                pd.ShowPaths(paths);

            }); // End dispatcher
            
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //update ui once worker completes his work
            wcViewModel.GoCommand.RaiseCanExecuteChanged();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Send message to GUI
        /// </summary>
        /// <param name="s"></param>
        private void AddMessage(string s)
        {
            if (s.Equals("")) wcViewModel.Status = "";
            else wcViewModel.Status += "\n" + s;
        }


        public List<int> Dijkstras(Vertex src, Vertex dst)
        {
            // Add Graph Vertices to the Q w/ cost infinity
            PriorityQueue Q = new PriorityQueue(LoaderViewModel.Instance.MainGraph.Vertices);
            List<int> output = new List<int>();

            // Set source Cost to 0
            int src_index = Q.IndexOf(src);
            Q[src_index].Cost = 0;
            // Move to top
            Q.Exchange(0, src_index);

            Vertex p, next;
            // While Q is not empty
            while (Q.Size > 0)
            {
                // Remove lowest from Q
                p = Q.Poll();

                // Don't add source to output path for display purposes
                // Also check we dont have a cost of infinity
                if (p.ID != src.ID && p.Cost < float.MaxValue)
                {
                    output.Add(p.ID);
                    // Found destination?
                    if (p.ID == dst.ID)
                        return output;
                }

                // Foreach neighbor of p
                for (int i = 0; i < p.Neighbors.Count; ++i)
                {
                    if (p.Neighbors[i].ID == 0) break; // No more neighbors
                    float alternateDist = p.Cost + p.Neighbors[i].Weight; // p cost plus the edge weight
                    // Get neighbor index and vertex from Q
                    int neighborNode = p.Neighbors[i].Node2;
                    next = Q.GetVertex(neighborNode);
                    // Calc new possible costs
                    if (next != null)
                    {
                        if (alternateDist < next.Cost)
                        {
                            next.Cost = alternateDist;
                            Q.Reweight(next, p);
                        }
                    }
                } // End for
            } // End while
            return output;
        }


        /// <summary>
        /// Use Dijkstras to find any root
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <returns></returns>
        public List<int> DijkstraShortestPath(Vertex src, Vertex dst)
        {
            List<int> output = null;
            try
            {
                // Check if valid src
                if (!LoaderViewModel.Instance.MainGraph.Vertices.Contains(src))
                {
                    LoaderViewModel.Instance.MainGraph.AddVertex(src);
                }
                // The output variable
                output = new List<int>();

                // Add all nodes to PQ (Cost MaxValue at this point)
                PriorityQueue Q = null;
                Q = new PriorityQueue(LoaderViewModel.Instance.MainGraph.Vertices);
                // Set source Cost to 0
                int src_index = Q.IndexOf(src);
                Q[src_index].Cost = 0;
                // Move to top
                Q.Exchange(0, src_index);

                // While PQ is not empty
                Vertex polld = new Vertex(0);
                Vertex next, last;

                //int temp_index;
                while (Q.Size > 0 && polld.ID != dst.ID)
                {
                    //// Poll (remove root slot)
                    polld = null;
                    polld = Q.Poll();
                    //if (polld == null) return output;

                    // Add to output unless we are at a disconnected vertex or is the source node
                    if (polld.Cost != float.MaxValue)
                    {
                        // Don't add source for display purposes
                        if (polld.Cost > 0)
                            output.Add(polld.ID);
                    }
                    else
                    {
                        continue;
                    }

                    // If we have found a cluster center return Path
                    if (polld.ID == dst.ID)
                        return output;

                    // For each surrounding edge
                    for (int i = 0; i < polld.Neighbors.Count; ++i)
                    {
                        if (polld.Neighbors[i].ID == 0) break; // End of neighbors
                        next = null; // Clear last node

                        // // Get neeighbor index and vertex from Q
                        int neighborNode = polld.Neighbors[i].Node2;
                        next = Q.GetVertex(neighborNode);

                        // Skip src 
                        //if (next.Cost == 0)
                        //    continue;

                        // If cost from src to (p + (cost from p to e)) < next.Cost
                        if (next != null)
                        {
                            float alternateDist = polld.Cost + polld.Neighbors[i].Weight;
                            if (alternateDist < next.Cost)
                            {
                                next.Cost = alternateDist;
                                // Reweight
                                Q.Reweight(next, polld);
                            }
                            last = next;
                        }

                    }
                }
            } // End try
            catch (Exception err)
            {
                Console.WriteLine("Error in Dijkstra's: " + err);
            }

            return output;


        } // End Dijkstras

        #endregion



    }// End Session class
}// Namespace



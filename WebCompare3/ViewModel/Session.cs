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
            // Use dijkstras to find shortest path
            List<int>[] paths = new List<int>[5];
            for (int r = 0; r < 5; ++r)
            {
                paths[r] = new List<int>();
                AddMessage("Searching for path to: " + LoaderViewModel.Instance.Roots[r].Name);
                paths[r] = DijkstraShortestPath(selectedVert, LoaderViewModel.Instance.Roots[r].RootVertex);
            }

            // Graphically display the path
            PathDisplay pd = new PathDisplay();
            pd.SrcText = selectedVert.ID.ToString();
            pd.ShowPaths(paths);
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
            else wcViewModel.Status += s;
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
                if (!LoaderViewModel.Instance.MainGraph.Vertices.Contains(src)) return null;
                // The output variable
                output = new List<int>();

                // Add all nodes to PQ (Cost MaxValue at this point)
                PriorityQueue Q = new PriorityQueue(LoaderViewModel.Instance.MainGraph.Vertices);
                // Set source Cost to 0
                int src_index = Q.IndexOf(src);
                Q[src_index].Cost = 0;
                // Move to top
                Q.Exchange(0, src_index);

                // While PQ is not empty
                Vertex polld, next;
                int temp_index;
                while (!Q.IsEmpty())
                {
                    //// Poll (remove root slot)
                    polld = Q.Poll();
                    // Add to output unless we are at a disconnected vertex
                    if (polld.Cost != float.MaxValue)
                        output.Add(polld.ID);
                    else
                        return output;

                    // If we have found a cluster center return Path
                    if (polld == dst) return output;

                    // For each surrounding edge
                    for (int i = 0; i < polld.Neighbors.Count; ++i)
                    {
                        // Get next vertex & its index in Q
                        next = Q.GetVertex(polld.Neighbors[i].Node2);
                        temp_index = Q.IndexOf(next);

                        // Skip src
                        if (next.Cost == 0) continue;

                        // If cost from src to (p + (cost from p to e)) < next.Cost
                        float alternateDist = polld.Cost + polld.Neighbors[i].Weight;
                        if (alternateDist < next.Cost)
                        {
                            next.Cost = alternateDist;
                            // Reweight
                            Q.Reweight(next);
                        }

                    }
                }
            } // End try
            catch (Exception e)
            {
                Console.WriteLine("Error in Dijkstra's: " + e);
            }

            return output;


        } // End Dijkstras

        #endregion



    }// End Session class
}// Namespace



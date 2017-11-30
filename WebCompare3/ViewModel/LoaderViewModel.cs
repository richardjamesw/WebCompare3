using HtmlAgilityPack;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WebCompare3.Model;

namespace WebCompare3.ViewModel
{
    class LoaderViewModel : INotifyPropertyChanged
    {

        #region Instance Variables
        Graph MainGraph = new Graph();
        List<Root> roots = new List<Root>();
        // Stick with ~500 sites per root
        const int MAXSITES = 200;   // Max sites per root
        const int MAXSEEDS = 10;   // Max number of sites from each site
        int count = MAXSITES;

        int TableNumber = 0;
        int EdgeNumber = 0;
        public readonly BackgroundWorker loadWorker = new BackgroundWorker();
        private static object lockObj = new object();
        private static volatile LoaderViewModel instance;
        

        public static LoaderViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObj)
                    {
                        if (instance == null)
                            instance = new LoaderViewModel();
                    }
                }
                return instance;
            }
        }

        public LoaderViewModel()
        {
            loadWorker.DoWork += loadWorker_DoWork;
            loadWorker.RunWorkerCompleted += loadWorker_RunWorkerCompleted;
            LoadCommand = new DelegateCommand(OnLoad, CanLoad);

            StartCommand = new DelegateCommand(OnStart, CanStart);
        }
        #endregion

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

        #region Properties

        private string loadStatus = "Status...................";
        public string LoadStatus
        {
            get
            {
                return loadStatus;
            }
            set
            {
                loadStatus = value;
                NotifyPropertyChanged("LoadStatus");
            }
        }

        private bool updateIsChecked = true;
        public bool UpdateIsChecked
        {
            get
            {
                return updateIsChecked;
            }
            set
            {
                updateIsChecked = value;
                StartCommand.RaiseCanExecuteChanged();
                LoadCommand.RaiseCanExecuteChanged();
                NotifyPropertyChanged("UpdateIsChecked");
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command for the Load button in Loader Window
        /// </summary>
        public DelegateCommand LoadCommand { get; private set; }
        private void OnLoad()
        {
            NotifyPropertyChanged("LoadStatus");
            loadWorker.RunWorkerAsync();
            LoadCommand.RaiseCanExecuteChanged();
        }
        private bool CanLoad()
        {
            // Only allow button available if Update is checked and LoadWorker is not running
            return !loadWorker.IsBusy && UpdateIsChecked;
        }

        /// <summary>
        /// Command for the Start button in Loader Window
        /// </summary>
        public DelegateCommand StartCommand { get; private set; }
        private void OnStart()
        {
            MainWindow mw = new MainWindow();
            mw.DataContext = WebCompareViewModel.Instance;
            mw.Show();
            StartCommand.RaiseCanExecuteChanged();
        }
        private bool CanStart()
        {
            return !UpdateIsChecked;
        }

        #endregion

        #region Workers

        private void loadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                // Seed site graph construction with Computer Science related sites
                AddMessage("Root being added.");
                roots.Add(new Root("Computer Engineering", BuildGraph("https://en.wikipedia.org/wiki/Computer_engineering", null)));

                count = MAXSITES;
                AddMessage("Root being added.");
                roots.Add(new Root("Computer Performance", BuildGraph("https://en.wikipedia.org/wiki/Computer_performance", null)));

                count = MAXSITES;
                AddMessage("Root being added.");
                roots.Add(new Root("Concurrency", BuildGraph("https://en.wikipedia.org/wiki/Concurrency_(computer_science)", null)));

                count = MAXSITES;
                AddMessage("Root being added.");
                roots.Add(new Root("Copmuter Networking", BuildGraph("https://en.wikipedia.org/wiki/Computer_network", null)));

                count = MAXSITES;
                AddMessage("Root being added.");
                roots.Add(new Root("Computer Security", BuildGraph("https://en.wikipedia.org/wiki/Computer_security", null)));

                // Report number of Spanning Trees for an arbitrary node


            } catch (Exception err)
            { MessageBox.Show("Exception caught: " + err, "Exception:Loader:loaderWorker_DoWork()", MessageBoxButton.OK, MessageBoxImage.Warning); }
        }

        private void loadWorker_RunWorkerCompleted(object sender,
                                                 RunWorkerCompletedEventArgs e)
        {
            AddMessage("LOAD COMPLETED.");
            // Tell GUI everything is done updating
            UpdateIsChecked = false;
        }

        #endregion

        #region HelperMethods

        //Send message to GUI
        public void AddMessage(string s)
        {
            LoadStatus += "\n" + s;
        }

        /// <summary>
        /// Main method to recursively get websites and build graph
        /// </summary>
        /// <param name="site"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public Vertex BuildGraph(string site, Vertex parent)
        {
            try
            {
                // Take one site and a parent vertex
                // Get data and list of sites
                List<string> parsedData;
                List<string> sites;
                GetWebDataAgility(site, out parsedData, out sites);
                // Fill a new HTable (frequency table)
                HTable vTable = new HTable();
                vTable.ID = ++TableNumber;
                vTable.URL = site;
                vTable.Name = site.Substring(30);
                if (parsedData != null)
                {
                    for (int w = 0; w < parsedData.Count; ++w)
                    {
                        vTable.Put(parsedData[w], 1);
                    }
                }
                // Write HTable to file
                vTable.SaveTable(vTable.ID);
                // Create a vertex for this site with the same ID as HTable
                Vertex v = new Vertex(vTable.ID, vTable.URL);

                // Create an edge connecting this vertex and parent vertex
                double similarity = 0;
                if (parent != null)
                {
                    // Calc similiarty to parent
                    HTable parentTable = LoadTable(parent.ID);
                    List<object>[] vector = WebCompareModel.BuildVector(vTable, parentTable);
                    //// Calcualte similarity
                    similarity = WebCompareModel.CosineSimilarity(vector);
                    //Create edge to parent
                    Edge e = new Edge(parent.ID, v.ID, similarity, ++EdgeNumber);
                    MainGraph.AddEdge(e);   // Add edge to Graph list
                    MainGraph.SaveEdge(e);  // Write edge to disk
                }
                // Add Vertex to graph
                MainGraph.AddVertex(v);

                // Add list of sites to this vertex
                //// Forach- add, recursively call this method
                foreach (var s in sites)
                {
                    // Don't get more sites if site tree has been built already 
                    Vertex v2 = MainGraph.HasVertex(s);
                    if (v2 != null)
                    {
                        AddMessage("Old Vertex Found.");
                        // Add eachother as neighbors
                        v.Neighbors.Add(v2.ID);
                        v2.Neighbors.Add(v.ID);
                        // Update/Add to graph
                        MainGraph.AddVertex(v);
                        MainGraph.AddVertex(v2);
                        // Calc similiarty to parent
                        HTable v2Table = LoadTable(v2.ID);
                        similarity = 0;   // clear
                        if (v2Table != null)
                        {
                            List<object>[] vector = WebCompareModel.BuildVector(vTable, v2Table);
                            //// Calcualte similarity
                            similarity = WebCompareModel.CosineSimilarity(vector);
                            //Create edge to parent
                            Edge e = new Edge(v.ID, v2.ID, similarity, ++EdgeNumber);
                            MainGraph.AddEdge(e);   // Add edge to Graph list
                            MainGraph.SaveEdge(e);  // Write edge to disk
                        }
                        MainGraph.SaveVertex(v2);
                    }
                    else
                    {
                        v.Neighbors.Add(BuildGraph(s, v).ID);
                    }
                }
                // Update Vertex to graph and persist
                MainGraph.AddVertex(v);
                MainGraph.SaveVertex(v);
                return v;
            }
            catch(Exception exc) { Console.WriteLine("Error building graph: " + exc); }
            return null;
        }

        // HTMPAglityPack Get
        public static void GetWebDataAgility(string url, out List<string> parsedData, out List<string> sites)
        {
            string data = "";
            parsedData = new List<string>();
            sites = new List<string>();
            try
            {
                // Get data dump
                var webGet = new HtmlWeb();
                var doc = webGet.Load(url);
                string title = "";
                
                // Title
                var node = doc.DocumentNode.SelectSingleNode("//title");
                if (node != null) title = node.InnerText;
                // Websites
                foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                {
                    HtmlAttribute att = link.Attributes["href"];
                    if (att.Value.StartsWith(@"/wiki/"))
                        sites.Add("https://en.wikipedia.org" + att.Value);

                    // Only take x number of sites
                    if (sites.Count >= MAXSEEDS) break;
                }
                // Paragraphs
                var nodes = doc.DocumentNode.SelectNodes("//p");
                // Invalid data 
                if (nodes == null)
                {
                    Console.WriteLine("Site invalid, no data retrieved.");
                    return;
                }
                data = title;
                foreach (var n in nodes)
                {
                    data += " " + n.InnerText;
                }

                // remove random characters
                data = new string(data
                        .Where(x => char.IsWhiteSpace(x) || char.IsLetterOrDigit(x))
                        .ToArray());
                // split into List
                parsedData = data.Split(' ').ToList();
            }
            catch (Exception e) { Console.WriteLine("Error in GetWebDataAgility(): " + e); }
        }

        /// <summary>
        /// Get table from disk
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public HTable LoadTable(int num)
        {
            string FileName = $"tablebin\\table{num}.bin";
            HTable loadedTable = null;
            try
            {
                if (File.Exists(FileName))
                {
                    using (Stream filestream = File.OpenRead(FileName))
                    {
                        BinaryFormatter deserializer = new BinaryFormatter();
                        loadedTable = (HTable)deserializer.Deserialize(filestream);
                    }
                }
                else
                {
                    MessageBox.Show("Table does not exist", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception e) { Console.WriteLine("Error in LoadTable: " + e); }
            return loadedTable;
        }


        /// <summary>
        /// Read number of keys
        /// </summary>
        /// <returns></returns>
        public static int GetNumberOfKeys()
        {
            string DirName = $"tablebin\\";
            try
            {
                if (!Directory.Exists(DirName)) return 0;
                int count = Directory.GetFiles(DirName).Length;
                return count;
            }
            catch { return 0; }
        }
        #endregion


    }
}

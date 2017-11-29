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
            string[] parsedData = null;
            
            int count = 1000;
            try
            {
                // Start with a random site to seed the graph construction
                BuildGraph("https://en.wikipedia.org/wiki/Computer_science", null);

                // create a vertex for 
                parsedData = WebCompareModel.GetWebDataAgility(site);
                // Fill a new HTable (frequency table)
                HTable table = new HTable();
                table.URL = site;
                table.Name = site.Substring(30);
                if (parsedData != null)
                {
                    for (int w = 0; w < parsedData.Length; ++w)
                    {
                        table.Put(parsedData[w], 1);
                    }
                }
                // Write HTable to file
                table.SaveTable(TableNumber);
                // Add HTable to BTree, including write to file
                Session.Instance.Tree.Insert(TableNumber, table.Name);
                ++TableNumber;





                string[][] AllSites = new string[5][];
                TableNumber = GetNumberOfKeys() + 1;
                int sitesRemaining = 1531 - TableNumber;
                for (int i = 0; i < AllSites.Length; ++i)
                {
                    AddMessage($"Getting site list for '{Enum.GetName(typeof (WebCompareModel.SitesEnum), i)}' cluster");
                    AllSites[i] = WebCompareModel.GetSiteList(WebCompareModel.Websites[i], (sitesRemaining/5));
                }

                // Build frequency tables from 1000+ sites
                int sitecateg = 1, statusCount = 1, scx = 1;
                foreach (string[] sites in AllSites)
                {
                    AddMessage("Building frequency tables for cluster.. " + sitecateg);
                    foreach (string site in sites)
                    {
                        // Status display
                        if (statusCount == 100)
                        {
                            AddMessage($"Sites to go..{sites.Count() * 5 - scx + 70}");
                            statusCount = 0;
                        } ++statusCount; ++scx;

                        // Get data from website and parse
                        parsedData = WebCompareModel.GetWebDataAgility(site);
                        // Fill a new HTable (frequency table)
                        HTable table = new HTable();
                        table.URL = site;
                        table.Name = site.Substring(30);
                        if (parsedData != null)
                        {
                            for (int w = 0; w < parsedData.Length; ++w)
                            {
                                table.Put(parsedData[w], 1);
                            }
                        }
                        // Write HTable to file
                        table.SaveTable(TableNumber);
                        // Add HTable to BTree, including write to file
                        Session.Instance.Tree.Insert(TableNumber, table.Name);
                        ++TableNumber;
                    }
                    AddMessage("Completed building frequency table " + sitecateg);
                    ++sitecateg;
                } // End AllSites foreach
            } catch (Exception err) { MessageBox.Show("Exception caught: " + err, "Exception:Loader:loaderWorker_DoWork()", MessageBoxButton.OK, MessageBoxImage.Warning); }
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
                HTable table = new HTable();
                table.ID = ++TableNumber;
                table.URL = site;
                table.Name = site.Substring(30);
                if (parsedData != null)
                {
                    for (int w = 0; w < parsedData.Count; ++w)
                    {
                        table.Put(parsedData[w], 1);
                    }
                }
                // Write HTable to file
                table.SaveTable(table.ID);
                // Create a vertex for this site with the same ID as HTable
                Vertex v = new Vertex(table.ID, table.URL);
                // Calculate similarity to parent vertex htable
                if (parent != null)
                {
                    HTable parentTable = LoadTable(parent.ID);
                    List<object>[] vector = WebCompareModel.BuildVector(table, parentTable);
                    //// Calcualte similarity
                    v.Similarity = WebCompareModel.CosineSimilarity(vector);
                }
                // Create an edge connecting this vertex and parent vertex
                int parentID = 0;
                if (parent != null) parentID = parent.ID;
                Edge e = new Edge(parentID, v.ID, v.Similarity, ++EdgeNumber);
                MainGraph.AddEdge(e);   // Add edge to Graph list
                MainGraph.SaveEdge(e);  // Write edge to disk
                                        // Add list of sites to this vertex
                                        //// Forach- add, recursively call this method
                foreach (var s in sites)
                {
                    // Don't add if site exists already
                    int id = 0;
                    if (MainGraph.HasVertex(s, out id))
                    {
                        v.Neighbors.Add(id);
                    }
                    else v.Neighbors.Add(BuildGraph(s, v).ID);
                }
                // Add Vertex to graph
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

                    // Only take 10 sites
                    if (sites.Count > 9) break;
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

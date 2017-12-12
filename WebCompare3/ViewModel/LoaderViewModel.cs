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
        private Graph mainGraph = new Graph();
        private List<Root> roots = new List<Root>();
        private MainWindow mw = null;
        // Stick with ~500 sites per root
        const int MAXSITES = 200;   // Max sites per root
        const int MAXLEVELS = 20;
        const int MAXSEEDS = 10;   // Max number of sites from each site
        int siteCount = 0;
        int levelCount = MAXLEVELS;

        int TableNumber = 0;
        int EdgeNumber = 0;
        public readonly BackgroundWorker loadWorker = new BackgroundWorker();
        private static object lockObj = new object();
        private static volatile LoaderViewModel instance;

        #region Constructor/Instance
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
        // Getter for the main graph, readonly
        public Graph MainGraph { get { return mainGraph; } }

        public List<Root> Roots
        {
            get
            {
                // Load roots if not loaded yet
                if (roots.Count < 1 && MainGraph.Vertices.Count > 0)
                {
                    roots.Add(
                        new Root( "Computer Engineering", MainGraph.GetVertexWithData(WebCompareModel.RootSites[0]) )
                        );
                    roots.Add(
                        new Root( "Computer Performance", MainGraph.GetVertexWithData(WebCompareModel.RootSites[1]) )
                        );
                    roots.Add(
                        new Root( "Concurrency", MainGraph.GetVertexWithData(WebCompareModel.RootSites[2]) )
                        );
                    roots.Add(
                        new Root( "Computer Networking", MainGraph.GetVertexWithData(WebCompareModel.RootSites[3]) )
                        );
                    roots.Add(
                        new Root( "Computer Security", MainGraph.GetVertexWithData(WebCompareModel.RootSites[4]) )
                        );
                }

                return roots;
            }
        }

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
            // Remove old stored data
           
            try
            {
                var result = MessageBox.Show("Do you want to remove old stored data?", "", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    string graphDir = @"graphbin\";
                    string tableDir = @"tablebin\";
                    string treeDir = @"treebin\";
                    if (Directory.Exists(graphDir))
                        Directory.Delete(graphDir, true);

                    if (Directory.Exists(tableDir))
                        Directory.Delete(tableDir, true);

                    if (Directory.Exists(treeDir))
                        Directory.Delete(treeDir, true);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Could not remove all old stored data. Try manually removing and then select OK.", "Exception:OnLoad", MessageBoxButton.OK, MessageBoxImage.Warning);
                Console.WriteLine("Could not remove all old stored data. Try manually removing and then select OK. \nException: " + exc);
            }

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
            // Populate list of selectable sites
            if (Instance.MainGraph != null && Instance.MainGraph.Vertices.Count > 0)
            {
                WebCompareViewModel.Instance.GraphSites =
                    from vert in LoaderViewModel.Instance.MainGraph.Vertices
                    select vert.Data;
            }
            else
            {
                if (LoaderViewModel.Instance.MainGraph.LoadAllVertices())
                {
                    WebCompareViewModel.Instance.GraphSites =
                        from vert in LoaderViewModel.Instance.MainGraph.Vertices
                        select vert.Data;
                }
                else
                {
                    return;
                }
            }

            mw = new MainWindow();
            mw.DataContext = WebCompareViewModel.Instance;
            mw.Show();
            StartCommand.RaiseCanExecuteChanged();
        }
        private bool CanStart()
        {
            return !UpdateIsChecked;
        }

        public void CloseMW()
        {
            if (mw != null)
            {
                mw.Close();
            }
        }
        #endregion

        #region Workers

        private void loadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                // Seed site graph construction with Computer Science related sites
                AddMessage("Building graph\nfirst seed started ");
                Vertex rootNode = BuildGraph(WebCompareModel.RootSites[0], null);
                roots.Add(new Root("Computer Engineering", rootNode));

                // Reset counters
                siteCount = 0;
                levelCount = MAXLEVELS;
                AddMessage("second seed started ");
                rootNode = BuildGraph(WebCompareModel.RootSites[1], rootNode);
                roots.Add(new Root("Computer Performance", rootNode));

                siteCount = 0;
                levelCount = MAXLEVELS;
                AddMessage("third seed started ");
                rootNode = BuildGraph(WebCompareModel.RootSites[2], rootNode);
                roots.Add(new Root("Concurrency", rootNode));

                siteCount = 0;
                levelCount = MAXLEVELS;
                AddMessage("fourth seed started ");
                rootNode = BuildGraph(WebCompareModel.RootSites[3], rootNode);
                roots.Add(new Root("Computer Networking", rootNode));

                siteCount = 0;
                levelCount = MAXLEVELS;
                AddMessage("fifth seed started ");
                rootNode = BuildGraph(WebCompareModel.RootSites[4], rootNode);
                roots.Add(new Root("Computer Security", rootNode));

                // Report number of Spanning Trees for an arbitrary node
                Console.WriteLine("Loading complete.");

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
        /// Determine if Vertex is a Cluster Center
        /// </summary>
        /// <param name="vert"></param>
        /// <returns></returns>
        public bool IsRoot(Vertex vert)
        {
            if (roots == null || roots.Count < 1)
                return false;

            foreach(Root r in roots)
            {
                if (vert.ID == r.RootVertex.ID)
                    return true;
                else
                    return false;
            }
            return false;
        }

        /// <summary>
        /// Main method to recursively get websites and build graph
        /// </summary>
        /// <param name="site"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public Vertex BuildGraph(string site, Vertex parent)
        {
            float sim = 0;
            return BuildGraph(site, parent, out sim);
        }
        public Vertex BuildGraph(string site, Vertex parent, out float sim)
        {
            // Decrement count because we just went down a level
            --levelCount;
            sim = 0;
            if (siteCount >= MAXSITES)
                return null;
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
                if (parent != null)
                {
                    // Calc similiarty to parent
                    HTable parentTable = LoadTable(parent.ID);
                    List<object>[] vector = WebCompareModel.BuildVector(vTable, parentTable);
                    //// Calcualte similarity, 1 minus for shortest path calculation, lower numbers are more similar
                    sim = 1 - (float)WebCompareModel.CosineSimilarity(vector);
                    //Create edge to parent
                    Edge e1 = new Edge(parent.ID, v.ID, sim, ++EdgeNumber);
                    Edge e2 = new Edge(v.ID, parent.ID, sim, ++EdgeNumber);
                    mainGraph.AddEdge(e1);   // Add edge to Graph list
                    mainGraph.SaveEdge(e1);  // Write edge to disk
                    mainGraph.AddEdge(e2);   // Add edge to Graph list
                    mainGraph.SaveEdge(e2);  // Write edge to disk
                    // Add eachother as neighbors
                    parent.AddNeighbor(e1);
                    v.AddNeighbor(e2);
                    // Update parent vertex
                    mainGraph.AddVertex(parent);
                    mainGraph.SaveVertex(parent);
                }
                // Update/Add to graph
                mainGraph.AddVertex(v);
                mainGraph.SaveVertex(v);
                ++siteCount;
                
                // Add list of sites to this vertex
                //// Forach- add, recursively call this method
                foreach (var s in sites)
                {
                    // Check for nulls, Don't compare to itself
                    if (s == null || s == site) continue;

                    // Don't get more sites if site tree has been built already 
                    Vertex v2 = mainGraph.HasVertex(s);
                    if (v2 != null)
                    {
                        LoadStatus += ".";
                        // Calc similiarty to parent
                        HTable v2Table = LoadTable(v2.ID);
                        sim = 0;   // clear
                        if (v2Table != null)
                        {
                            List<object>[] vector = WebCompareModel.BuildVector(vTable, v2Table);
                            //// Calcualte similarity, 1 minus for shortest path calculation, lower numbers are more similar
                            sim = 1 - (float)WebCompareModel.CosineSimilarity(vector);
                            //Create edge to parent
                            Edge e = new Edge(v.ID, v2.ID, (float)sim, ++EdgeNumber);
                            mainGraph.AddEdge(e);   // Add edge to Graph list
                            mainGraph.SaveEdge(e);  // Write edge to disk
                            
                            // Add eachother as neighbors
                            v.AddNeighbor(e);
                            v2.AddNeighbor(new Edge(v2.ID, v.ID, (float)sim, ++EdgeNumber));
                            // Update/Add to graph
                            mainGraph.AddVertex(v);
                            mainGraph.AddVertex(v2);
                        }

                        mainGraph.SaveVertex(v2);
                    }
                    else
                    {
                        float simout = 0;
                        Vertex neighb = null;
                        
                        // Don't build another graph off this node if the level count hits 0
                        if (levelCount > 0)
                        {
                            neighb = BuildGraph(s, v, out simout);
                            // Increment levelcount because we just came back up a level
                            ++levelCount;
                        }
                        else
                        {
                            return v;
                        }

                        if (neighb != null)
                        {
                            Edge newEdge = new Edge(v.ID, neighb.ID, simout, EdgeNumber++);
                            v.AddNeighbor(newEdge);
                            mainGraph.SaveEdge(newEdge);
                        }
                        
                    }
                }
                // Update Vertex to graph and persist
                if(v != null)
                {
                    mainGraph.AddVertex(v);
                    mainGraph.SaveVertex(v);
                }
                
                return v;
            }
            catch(Exception exc) { Console.WriteLine("Error building graph: " + exc); }
            sim = 0;
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
                    // Block specific sites and duplications
                    if (att.Value != null && att.Value.StartsWith(@"/wiki/") && !sites.Contains("https://en.wikipedia.org" + att.Value)
                        && !att.Value.Contains("File:") && !att.Value.Contains("Template:") && !att.Value.Contains("Wikipedia:") && !att.Value.Contains("Wikipedia_")
                        && !att.Value.Contains("Help:") && !att.Value.Contains("Portal:") && !att.Value.Contains("Talk:")
                        && !att.Value.Contains("Help:") && !att.Value.Contains("Category:") && !att.Value.Contains("Special:")
                        && !att.Value.Contains("Media:") && !att.Value.Contains("MediaWiki:") && !att.Value.Contains("User:"))
                    {
                        sites.Add("https://en.wikipedia.org" + att.Value);
                    }

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

        #endregion


    }
}

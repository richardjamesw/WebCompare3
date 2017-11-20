using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WebCompare3.Model;

namespace WebCompare3.ViewModel
{
    class LoaderViewModel : INotifyPropertyChanged
    {

        #region Instance Variables

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
            // Create new tree
            string[] parsedData = null;
            int TableNumber = 0;
            try
            {
                // Get list of websites
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

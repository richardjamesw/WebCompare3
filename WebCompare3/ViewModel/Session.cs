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

            // Populate list of selectable sites
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
            return !Instance.worker.IsBusy;
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Get selected site
            // Use dijkstras to find shortest path
            // Graphically display the path
            
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

        #endregion



    }// End Session class
}// Namespace



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;    //for iNotifyPropertyChanged
using System.Windows.Input; // ICommand
using WebCompare3.Model;
using Prism.Commands;
using System.Collections.ObjectModel;

namespace WebCompare3.ViewModel
{
    public class WebCompareViewModel : INotifyPropertyChanged
    {
        #region Instance Variables
        private static object lockObj = new object();
        private static volatile WebCompareViewModel instance;

        public static WebCompareViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObj)
                    {
                        if (instance == null)
                            instance = new WebCompareViewModel();
                    }
                }
                return instance;
            }
        }

        public WebCompareViewModel()
        {
            GoCommand = new DelegateCommand(OnGo, CanGo);
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

        // user entereed url
        private string userURL = "https://en.wikipedia.org/wiki/World_War_I";
        public string UserURL
        {
            get
            {
                return userURL;
            }
            set
            {
                userURL = value;
                NotifyPropertyChanged("UserURL");

            }
        }


        public string[] ListWebsites
        {
            get
            {
                return WebCompareModel.Websites;
            }
        }

        private IEnumerable<string> graphSites = null;
        public IEnumerable<string> GraphSites
        {
            get
            {
                return graphSites;
            }

            set
            {
                if (graphSites != value)
                {
                    graphSites = value;
                    NotifyPropertyChanged("GraphSites");
                }

            }
        }

      private string graphSitesSelected = "";
      public string GraphSitesSelected
      {
         get
         {
            return graphSitesSelected;
         }

         set
         {
            if (graphSitesSelected != value)
            {
               graphSitesSelected = value;
               NotifyPropertyChanged("GraphSitesSelected");
            }

         }
      }

      private string status = "Enter a URL and press start above.";
        public string Status
        {
            get
            {
                return status;
            }

            set
            {
                if (status != value)
                {
                    status = value;
                    NotifyPropertyChanged("Status");
                }

            }
        }
        #endregion

        #region Commands

        /// <summary>
        /// Command for the Go Button from the main window
        /// </summary>
        public DelegateCommand GoCommand { get; private set; }
        private void OnGo()
        {
            Session.Instance.Start();
        }
        private bool CanGo()
        {
            return Session.Instance.CanStart();
        }
        #endregion

    }
}


// Old method
// Button to call start method
//public ICommand BtnGo
//{
//    get
//    {
//        return new Commands.DelegateCommand(o => Session.Instance.Start());
//    }
//}

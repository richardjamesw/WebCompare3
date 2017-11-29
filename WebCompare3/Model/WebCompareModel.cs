using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace WebCompare3.Model
{
    public class WebCompareModel
    {
        // Websites
        private static string[] websites = {
            "https://petscan.wmflabs.org/?format=csv&psid=1356388",    // Sports
            "https://petscan.wmflabs.org/?format=csv&psid=1356389",    // Calculus
            "https://petscan.wmflabs.org/?format=csv&psid=1356390",    // Geography
            "https://petscan.wmflabs.org/?format=csv&psid=1356391",    // History
            "https://petscan.wmflabs.org/?format=csv&psid=1356392",    // Music   
        };

        public enum SitesEnum {Sports, Calculus, Geography, History, Music};

        public static string[] Websites
        {
            get
            {
                return websites;
            }
        }

        #region Helper Methods

        /// <summary>
        /// Get list of 200 sites 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string[] GetSiteList(string url, int numSites)
        {
            string[] output = new string[numSites];
            try
            {
                string line = "";
                string regex =
                    @"""\d{1,3}"",""(?<url>(\w|\d|\n|[().,-–_''])+?)""";

                WebRequest webRequest;
                webRequest = WebRequest.Create(url);

                using (Stream objStream = webRequest.GetResponse().GetResponseStream())
                using (StreamReader objReader = new StreamReader(objStream)) // get stream of the website list for specific category
                {
                    // skip first line, data not useful
                    objReader.ReadLine();
                    // for each line in the category pull x num of sites
                    for (int s = 0; s < numSites; ++s)
                    {
                        if (objReader.EndOfStream) return output; // quit if we areat the end
                        if (objReader != null)
                        {
                            line = objReader.ReadLine();
                            var result = Regex.Match(line, regex);
                            string newSite = "https://en.wikipedia.org/wiki/" + result.Groups["url"].Value;
                            if (!output.Contains(newSite))
                            {
                                // If site doesn't already exists, add it to the list
                                output[s] = newSite;
                            }
                            else
                            {
                                // decrement s if we find a copied site
                                --s;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception caught: " + e, "Exception:Session:GetSiteList()", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return output;
        }

        // Similarity Calculation
        // Build Vectors
        public static List<object>[] BuildVector(HTable tableA, HTable tableB)
        {
            // Guard
            if (tableA == null || tableB == null) return null;
            // New vector
            List<object>[] vector = new List<object>[3];
            vector[1] = new List<object>();
            vector[2] = new List<object>();
            try
            {
                // Get word lists together, remove duplicates
                var words = tableA.ToList().Union(tableB.ToList());
                // Sort words
                words = words.OrderBy(s => s, StringComparer.CurrentCultureIgnoreCase);
                // Add key words to the vector
                vector[0] = words.ToList<object>();
                // Add the frequencies to the vector
                foreach (string keyword in vector[0])
                {
                    vector[1].Add(tableA.GetValue(keyword));
                    vector[2].Add(tableB.GetValue(keyword));
                }
            }
            catch (Exception e) { Console.WriteLine("Error building vector: " + e); }

            return vector;

        }

        // Cosine Similarity
        public static double CosineSimilarity(List<object>[] vector)
        {
            // convert lists to double arrays
            double[] tableA = vector[1].Select(item => Convert.ToDouble(item)).ToArray();
            double[] tableB = vector[2].Select(item => Convert.ToDouble(item)).ToArray();
            double dotProduct = 0.0, normA = 0.0, normB = 0.0;
            // calculate
            for (int i = 0; i < tableA.Length; i++)
            {
                dotProduct += tableA[i] * tableB[i];
                normA += Math.Pow(tableA[i], 2);
                normB += Math.Pow(tableB[i], 2);
            }
            return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));

        }

        // Add message to Loader Status
        public static void AddLoaderMessage(string s)
        {
            ViewModel.LoaderViewModel.Instance.AddMessage(s);
        }
        #endregion
    }
}

/* Methods No longer used - Now use HTML Agility Pack instead
 * 
 * 
 * 

                // Get messages
                //var nodes = doc.DocumentNode.SelectNodes("//*[@id=\"updates\"]//li");




//public static string RandomWebsite
        //{
        //    get
        //    {
        //        return "https://en.wikipedia.org/wiki/Special:Random";
        //    }
        //}

/// <summary>
/// Get the data from a website as a string
/// </summary>
/// <param name="url">website to pull data from</param>
 public static string GetWebData1(string url)
{

    try
    {
        using (WebClient client = new WebClient())
        {
            string s = client.DownloadString(url);
            return s;
        }
    }
    catch (Exception e)
    {
        MessageBox.Show("Exception caught: " + e, "Exception:Session:GetWebData()", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    return null;
}



    public static string GetWebData2(string url)
        {
            try
            {
                string line = "";
                string parsed = "";
                WebRequest webRequest;
                webRequest = WebRequest.Create(url);

                Stream objStream;
                objStream = webRequest.GetResponse().GetResponseStream();

                StreamReader objReader = new StreamReader(objStream);

                while (objReader != null)
                {
                    line = objReader.ReadLine();
                    parsed += Parser(line) + "\n";
                }
                return parsed;
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception caught: " + e, "Exception:Session:GetWebData()", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return null;
        }
       */


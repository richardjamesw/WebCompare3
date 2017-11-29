using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// for serialization
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Windows;

namespace WebCompare3.Model
{
    [Serializable()]
    public class HTable
    {
        private HEntry[] table;
        private int tableSize = 32;
        private int id;
        private int count;
        private double similarity;
        private string url, name;
        DateTime lastUpdated;

        // Entry class
        [Serializable()]
        public class HEntry
        {
            public string key { get; set; }
            public int value { get; set; }
            public HEntry next;

            // Entry constructor
            public HEntry(string key, int value)
            {
                this.key = key;
                this.value = value;
                this.next = null;
            }
        }

        // Table constructor
        public HTable()
        {
            this.count = 0;
            this.table = new HEntry[tableSize];
            this.lastUpdated = DateTime.Now;
        }


        #region Puts & Gets

        // Add a value to the hash table
        public void Put(string key, int value)
        {
            // Check if we should expand
            if (count > (int)tableSize * .5)
            {
                this.ExpandCapacity();
            }

            // Hash string
            int h = Math.Abs(key.GetHashCode() % tableSize);
            // Create new entry
            HEntry entry = new HEntry(key, value);

            // Insert entry to table array
            if (table[h] == null)
            {
                // No collision, insert entry
                table[h] = entry;
                ++count;
            }
            else
            {
                // Detected collision, step through list
                HEntry current = table[h];

                while (current.next != null)
                {
                    // Check keys for match
                    if (current.key.Equals(entry.key))
                    {
                        // Increment value
                        current.value++;
                        return;
                    }
                    current = current.next;
                }
                // Next is null, insert node
                current.next = entry;
                ++count;
            }
        }   // End Put



        // Find a value using a key
        public int GetValue(string key)
        {
            HEntry[] temp = table;
            int h = Math.Abs(key.GetHashCode() % tableSize);
            // If there is no such key
            if (temp[h] == null)
            {
                return 0;
            }
            else // Else
            {
                HEntry entry = temp[h];   // Hold slot
                while (entry != null && !entry.key.Equals(key))   // Find matching key
                {
                    entry = entry.next;
                }
                if (entry == null)   // Key wasn't found
                {
                    return 0;
                }
                else   // Key was found
                {
                    return entry.value;
                }
            }
        }   // End GetVal



        // Find a key using a value
        public string GetKey(int val)
        {
            HEntry[] temp = table;
            HEntry current;

            for (int t = 0; t < temp.Length; t++)
            { //iterate through array
                if (temp[t] != null)
                {
                    try
                    {
                        //if not found yet, check list
                        current = temp[t];
                        while (!val.Equals(current.value) && current != null)
                        {
                            current = current.next;
                        }
                        // Either we found the value or we're at null
                        if (val.Equals(current.value))
                        {
                            return current.key;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    catch (KeyNotFoundException e)
                    {
                        Console.WriteLine("Error caught while finding Value: " + e);
                    }
                }   // End if
            }   // End for
            return null;
        }//end GetKey


        // Get all numbers 

        #endregion

        #region Properties
        public HEntry[] Table
        {
            get
            {
                return table;
            }
            set
            {
                table = value;
            }
        }
        public int ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }
        public int TableSize
        {
            get
            {
                return tableSize;
            }
            set
            {
                tableSize = value;
            }
        }

        // Number of Elements in the table
        public int Count
        {
            get
            {
                return count;
            }
            set
            {
                count = value;
            }
        }

        // Cosine Vector Similarity Score
        public double Similarity
        {
            get
            {
                return similarity;
            }
            set
            {
                similarity = value;
            }
        }

        // Assigned website url
        public string URL
        {
            get
            {
                return url;
            }
            set
            {
                url = value;
                name = url.Substring(30);
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public DateTime LastUpdated
        {
            get
            {
                return lastUpdated;
            }
            set
            {
                lastUpdated = value;
            }
        }
        #endregion

        #region Serialization

        public void SaveTable(int num)
        {
            try
            {
                var cd = Directory.CreateDirectory("tablebin\\");
                string FileName = $"tablebin\\table{num}.bin";
                using (Stream TestFileStream = File.Create(FileName))
                {
                    BinaryFormatter serializer = new BinaryFormatter();
                    serializer.Serialize(TestFileStream, this);
                }
            }
            catch (Exception e) { Console.WriteLine("Error in SaveTable: " + e); }
        }

        #endregion



        // Get all key words into a string array
        public List<string> ToList()
        {
            HEntry[] temp = table;
            HEntry current;
            List<string> newArr = new List<string>();

            foreach (var t in temp)
            { //iterate through array

                if (t != null)
                {
                    try
                    {
                        newArr.Add(t.key);
                        current = t;
                        while (current.next != null)
                        {
                            current = current.next;
                            newArr.Add(current.key);
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error in HEntry.ToArray: " + e);
                    }
                }
            }
            return newArr;
        }

        public void ExpandCapacity()
        {
            try
            {
                this.tableSize *= 2;
                HEntry[] temp = this.table;
                this.table = null;
                this.table = new HEntry[tableSize];
                foreach (HEntry elem in temp)
                {
                    if(elem != null) this.Put(elem.key, elem.value);
                }
            }
            catch (Exception e) { Console.WriteLine("Error while Expanding table: " + e); }
        }


    }   // End HTable

}   // End namespace









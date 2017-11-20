using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using WebCompare3.Model;

namespace WebCompare3.Model
{
    [Serializable()]
    public class BTree
    {
        static Node root = null;
        static int numOfNodes = 0;
        const int K = 8;  // min degree
        const int kvp_SIZE = (2 * K - 1);  // min degree
        const int children_SIZE = (2 * K);  // min degree
        const int ByteLength = 768;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id">Root </param>
        public BTree()
        {
            // Get the Number of Nodes
            numOfNodes = GetNumberOfNodes();
            // Read in the root
            if (numOfNodes > 0)
                root = ReadTree(1);
            // If Root is still null create a new one
            if (root == null)
            {
                root = new Node();
                root.ID = 1;
                WriteTree(root);
            }
        }

        #region BTree Properties
        public Node Root
        {
            get
            {
                return root;
            }
            set
            {
                root = value;
            }
        }

        public static int NumberOfNodes
        {
            get
            {
                return numOfNodes;
            }
            set
            {
                numOfNodes = value;
                SetNumberOfNodes(numOfNodes);
            }
        }

        /// <summary>
        /// Read number of nodes from file
        /// </summary>
        /// <returns></returns>
        private static int GetNumberOfNodes()
        {
            string FileName = $"treebin\\tree.bin";
            try
            {
                if (!File.Exists(FileName)) return 0;

                long num = new FileInfo(FileName).Length;
                return (int)(num / ByteLength);
            }
            catch { return 0; }
        }

        /// <summary>
        /// Set number of nodes in a file
        /// </summary>
        /// <param name="num"></param>
        private static void SetNumberOfNodes(int num)
        {
            string FileName = $"treebin\\numberofnodes.bin";
            try
            {
                System.IO.FileInfo file = new System.IO.FileInfo(FileName);
                file.Directory.Create(); // If the directory already exists, this method does nothing

                using (StreamWriter sm = new StreamWriter(FileName, false))
                {
                    sm.WriteLine(num);
                }
            }
            catch (IOException err) { Console.WriteLine("Error writing number of nodes: " + err); }
        }

        #endregion

        /// <summary>
        /// Internal Node class
        /// </summary>
        [Serializable()]
        public class Node
        {
            long id;
            bool leaf;
            int numberOfKeys;
            KeyValuePair<float, string>[] kvPairs;
            Node[] children;
            List<long> childIDs;

            public Node(bool inc = true)
            {
                if (inc) id = ++NumberOfNodes; // Don't increment for temporary nodes
                numberOfKeys = 0;
                leaf = true;
                kvPairs = new KeyValuePair<float, string>[kvp_SIZE];
                children = new Node[children_SIZE];
                childIDs = new List<long>();
            }

            #region Node Properties 

            public long ID
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

            public KeyValuePair<float, string>[] KVPairs
            {
                get
                {
                    return kvPairs;
                }
                set
                {
                    kvPairs = value;
                }
            }

            public Node[] Children
            {
                get
                {
                    return children;
                }
                set
                {
                    children = value;
                }
            }
            public List<long> ChildIDs
            {
                get
                {
                    return childIDs;
                }
                set
                {
                    childIDs = value;
                }
            }

            public bool Leaf
            {
                get
                {
                    return leaf;
                }
                set
                {
                    leaf = value;
                }
            }
            public int NumberOfKeys
            {
                get
                {
                    return numberOfKeys;
                }
                set
                {
                    numberOfKeys = value;
                }
            }
            #endregion
        }

        /// <summary>
        /// Read a Node from disk
        /// </summary>
        /// <param name="id">Node identification number.</param>
        public static Node ReadTree(long id)
        {
            string FileName = $"treebin\\tree.bin";
            Node readNode = null;
            try
            {
                if (new FileInfo(FileName).Length > 0)
                {
                    using (FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read))
                    using (BinaryReader reader = new BinaryReader(fs))
                    {
                        readNode = new Node(false);
                        readNode.ID = id;
                        fs.Seek(id * ByteLength, SeekOrigin.Begin);
                        readNode.Leaf = reader.ReadBoolean();
                        //readNode.Parent = reader.ReadInt64();
                        for (int kvi = 0; kvi < kvp_SIZE; ++kvi)
                        {
                            float key = reader.ReadSingle();
                            string val = reader.ReadString();
                            readNode.KVPairs[kvi] = new KeyValuePair<float, string>(key, val);
                            if (key > 0) ++readNode.NumberOfKeys;
                        }
                        for (int ci = 0; ci < children_SIZE; ++ci)
                        {
                            long temp = reader.ReadInt64();
                            if (temp != 0)
                            {
                                readNode.ChildIDs.Add(temp);
                            }
                                
                        }
                    } // end using
                } // end if
            }
            catch (Exception e) { Console.WriteLine("Error in ReadNode: " + e); }
            return readNode;
        }

        /// <summary>
        /// Read list of children of disk
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        private Node[] ReadChildren(List<long> ids)
        {
            Node[] childs = new Node[children_SIZE];
            int child = 0;
            foreach (long id in ids)
            {
                if (id != 0 && id != 1)   // Block invalid childs
                {
                    childs[child] = ReadTree(id);
                    ++child;
                }
            }
            return childs;
        }


        /// <summary>
        /// Write a node to disk
        /// </summary>
        /// <param name="node">Node to be written.</param>
        /// <param name="id">Node identification number.</param>
        private static object lockObj = new object();
        public void WriteTree(Node node)
        {
            string FileName = $"treebin\\tree.bin";
            lock (lockObj)
            {
                try
                {
                    using (FileStream fs = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.Write))
                    using (MemoryStream ms = new MemoryStream())
                    using (BinaryWriter writer = new BinaryWriter(ms))
                    {
                        writer.Write(node.Leaf);
                        for (int kvi = 0; kvi < kvp_SIZE; ++kvi)
                        {
                            if (kvi < node.NumberOfKeys)
                            {
                                writer.Write(node.KVPairs[kvi].Key);
                                writer.Write(node.KVPairs[kvi].Value);
                            }
                            else
                            {
                                writer.Write(default(float));
                                writer.Write(" ");
                            }
                        }
                        int c = 0;
                        if (node.ChildIDs != null)
                        {
                            foreach (long ci in node.ChildIDs)
                            {
                                writer.Write(ci);
                                ++c;
                            }
                        }
                        // Add 0 to maintain correct separation
                        while (c < children_SIZE)
                        {
                            writer.Write(0);
                            ++c;
                        }

                        byte[] bytes = new byte[ByteLength];
                        byte[] tempBytes = ms.ToArray();   // Temp array to fill main array, so main array is correct size
                        for (int b = 0; b < tempBytes.Length; ++b)
                        {
                            bytes[b] = tempBytes[b];
                        }
                        fs.Seek(node.ID * ByteLength, SeekOrigin.Begin);
                        fs.Write(bytes, 0, ByteLength);  // write the stream to file
                    }
                }
                catch (IndexOutOfRangeException e) {
                    Console.WriteLine("Index out of range in WriteNode: " + e); }
                catch (Exception e) { Console.WriteLine("Error in WriteNode: " + e); }
            }
        }


        /// <summary>
        /// Search tree using key
        /// </summary>
        /// <param name="key"></param
        /// <param name="key">Index.</param>
        /// <param name="id">Index.</param>
        public string SearchTree(float key, long id)
        {
            // Get Tree
            Node temp = ReadTree(id);
            try
            {
                // Check keys
                foreach (var kvp in temp.KVPairs)
                {
                    if (key == kvp.Key)
                    {
                        return kvp.Value;
                    }
                }

                // Search Children
                if (temp.ChildIDs.Count > 0)
                {
                    string ck = null;
                    foreach (var cid in temp.ChildIDs)
                    {
                        // Block empty slots
                        if (cid != 0)
                        {
                            ck = SearchTree(key, cid);
                        }
                        if (ck != null) return ck;
                    }
                    return ck;
                }

            } catch (Exception e) { Console.WriteLine("Exception in SearchTree: " + e); }

            return null;
        }


        /// <summary>
        /// Add key value pair to the tree
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Insert(float key, string value)
        {
            if (value == null)
                return;

            try
            {
                Node nd = this.Root;
                if(nd.ChildIDs.Count() > nd.Children.Count())
                {
                    nd.Children = ReadChildren(nd.ChildIDs);
                }
                KeyValuePair<float, string> kvp = new KeyValuePair<float, string>(key, value);

                // If node is not full insert, else split
                if (nd.NumberOfKeys < kvp_SIZE)
                {
                    InsertNonFull(nd, kvp);
                }
                else
                {
                    // New parent
                    Node p = new Node();
                    p.Leaf = false;
                    // Switch root IDs
                    long tempID = p.ID;
                    p.ID = nd.ID;
                    nd.ID = tempID;

                    p.Children[0] = nd;
                    p.ChildIDs.Add(p.Children[0].ID);

                    // Split nd into mini tree
                    SplitChild(p, nd, 0);

                    // Set root
                    if (p == null)
                        return;
                    Root = p;
                    WriteTree(Root);

                    // Move middle key to parent node, append this nodes children to the parents children
                    InsertNonFull(nd, kvp);

                } // End else
            }
            catch (Exception e) { Console.WriteLine("Error during BTree insert: " + e); }
        }

        /// <summary>
        /// Add key value pair to the tree
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void InsertNonFull(Node x, KeyValuePair<float, string> kvp)
        {
            int i = x.NumberOfKeys - 1;
            try
            {


                if (x.Leaf)
                {
                    // Find correct key position and shuffle KVPairs
                    while (i >= 0 && x.KVPairs[i].Key > kvp.Key)
                    {
                        x.KVPairs[i + 1] = x.KVPairs[i];
                        --i;
                    }
                    // Insert the key
                    x.KVPairs[i + 1] = kvp;
                    ++x.NumberOfKeys;
                    WriteTree(x);
                }
                else
                {
                    // Get x children from disk
                    if (x.ChildIDs.Count() > x.Children.Count())
                        x.Children = ReadChildren(x.ChildIDs);
                    // Find child to add new key
                    while (i >= 0 && x.KVPairs[i].Key > kvp.Key)
                    {
                        --i;
                    }
                    // Read where kvp is in x.KVPairs.Children[i]
                    ++i;

                    // Check if child node is full
                    if (x.Children[i] != null)
                    {
                        if (x.Children[i].NumberOfKeys == kvp_SIZE)
                        {
                            // Get childs childs
                            if (x.ChildIDs.Count() > x.Children.Count())
                                x.Children[i].Children = ReadChildren(x.Children[i].ChildIDs);
                            // Split
                            SplitChild(x, x.Children[i], i);

                            // New children are x.Children[i] and x.Children[i + 1]
                            if (kvp.Key > x.KVPairs[i].Key)
                                ++i;
                        }
                    }

                    // Recursive insert now that we are not full
                    if (x.Children[i] != null)
                        InsertNonFull(x.Children[i], kvp);
                }
            }
            catch (Exception e) { Console.WriteLine("Error during Insert non full: " + e); }
        }

        /// <summary>
        /// Split full node
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="nd"></param>
        /// <param name="index"></param>
        public void SplitChild(Node parent, Node oldChild, int index)
        {
            try
            {
                // New child node
                Node newChild = new Node();
                // If old node was a leaf, new node is now a leaf
                newChild.Leaf = oldChild.Leaf;
                // New node will have K-1 keyvalue pairs due to old node splitting in half
                newChild.NumberOfKeys = K - 1;
                // Hand over back keys
                for (int i = 0; i < K - 1; ++i)
                {
                    newChild.KVPairs[i] = oldChild.KVPairs[i + K];
                    oldChild.KVPairs[i + K] = default(KeyValuePair<float, string>);
                }
                // Set old child number of keys (will update again when we pass middle key up to parent)
                oldChild.NumberOfKeys = K;
                // Hand over Childs
                if (!oldChild.Leaf)
                {
                    for (int i = 0; i < K; ++i)
                    {
                        newChild.Children[i] = oldChild.Children[i + K];
                        newChild.ChildIDs.Add(oldChild.Children[i + K].ID);
                    }
                }

                // Shuffle children of the parent we passed in
                for (int i = parent.NumberOfKeys; i > index; --i)
                {
                    parent.Children[i + 1] = parent.Children[i];
                    parent.ChildIDs[i + 1] = parent.Children[i].ID;
                }
                // Add the halved old node and new node to parents children
                parent.Children[index + 1] = newChild;
                parent.ChildIDs.Add(newChild.ID);

                // Shuffle keys
                for (int i = parent.NumberOfKeys - 1; i > index - 1; --i)
                {
                    parent.KVPairs[i + 1] = parent.KVPairs[i];
                }
                // Add middle key of passed in child to parent node
                parent.KVPairs[index] = oldChild.KVPairs[K - 1];
                ++parent.NumberOfKeys;
                // Remove from child node and reset number of keys
                oldChild.KVPairs[K - 1] = default(KeyValuePair<float, string>);
                oldChild.NumberOfKeys = K - 1;

                // Write to disk
                WriteTree(oldChild);
                WriteTree(newChild);
                WriteTree(parent);
            }
            catch (Exception e) { Console.WriteLine("Error during split node: " + e); }
        }
    }
}



/*
 * Not Used
 /// <summary>
        /// Split full node
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="nd"></param>
        /// <param name="index"></param>
        public Node SplitNode(Node nd)
        {
            // Create new child nodes
            Node childA = new Node();
            Node childB = new Node();
            childA.Leaf = true;
            childA.NumberOfKeys = K - 1;
            childB.Leaf = true;
            childB.NumberOfKeys = K - 1;

            // Hand off Keys
            for (int i = 0; i < (K - 1); ++i)
            {
                childA.KVPairs[i] = nd.KVPairs[i];
            }

            // Hand off Keys
            for (int i = kvp_SIZE; i > (K - 1); --i)
            {
                childB.KVPairs[i] = nd.KVPairs[i];
            }

            // Promote middle node 
            Node newParent = new Node();
            newParent.KVPairs[0] = nd.KVPairs[K - 1];

            // Clear nd
            nd = null;

            // Set children
            newParent.Children[0] = childA;
            newParent.Children[1] = childB;

            return newParent;
        }
 */


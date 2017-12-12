using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WebCompare3.Model
{
    class Graph
    {
        private List<Vertex> vertices = new List<Vertex>();
        private List<Edge> edges = new List<Edge>();
        const int VertexByteLength = 512;
        const int EdgeByteLength = 16;
        #region Graph Properties

        public List<Vertex> Vertices
        {
            get
            {
                return vertices;
            }
            set
            {
                vertices = value;
            }
        }
        public List<Edge> Edges
        {
            get
            {
                return edges;
            }
            set
            {
                edges = value;
            }
        }
        #endregion

        #region Graph Methods

        /// <summary>
        /// Add a vertex to the graph's list of vertices
        /// </summary>
        /// <param name="newVertex"></param>
        public void AddVertex(Vertex newVertex)
        {
            if (Vertices == null) return;

            // Remove old value
            Vertices.RemoveAll(vert => vert.ID == newVertex.ID);
            // Add new (updated) value
            Vertices.Add(newVertex);
        }


        /// <summary>
        /// Add an edge to the graph's list of edges
        /// </summary>
        /// <param name="newEdge"></param>
        public void AddEdge(Edge newEdge)
        {
            return;
            this.edges.Add(newEdge);
        }

        /// <summary>
        /// Check if the graph includes a vertex with a certain URL already
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public Vertex HasVertex(string url)
        {
            if (Vertices == null)
            {
                return null;
            }
            else
            {
                return
                    Vertices.FirstOrDefault(vert => vert.Data == url);
            }
        }

        /// <summary>
        /// Locker for writing and saving vertices and edges
        /// </summary>
        private static object lockObj = new object();

        /// <summary>
        /// Save vertex to file
        /// </summary>
        /// <param name="v">vertex to write to file</param>
        public void SaveVertex(Vertex v)
        {
            string FileDir = @"graphbin\";
            string FileName = FileDir + "vertices.bin";
            Directory.CreateDirectory(FileDir);
            lock (lockObj)
            {
                try
                {
                    using (FileStream fs = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.Write))
                    using (MemoryStream ms = new MemoryStream())
                    using (BinaryWriter writer = new BinaryWriter(ms))
                    {
                        writer.Write(v.Data);
                        for (int n = 0; n < Vertex.NumberOfNeighbors; ++n)
                        {
                            if (n < v.Neighbors.Count)
                            {
                                writer.Write(v.Neighbors[n].Node1);
                                writer.Write(v.Neighbors[n].Node2);
                                writer.Write(v.Neighbors[n].Weight);
                                writer.Write(v.Neighbors[n].ID);
                            }
                            else
                            {
                                writer.Write(default(int));
                                writer.Write(default(int));
                                writer.Write(default(float));
                                writer.Write(default(int));
                            }
                        }

                        byte[] bytes = new byte[VertexByteLength];
                        byte[] tempBytes = ms.ToArray();   // Temp array to fill main array, so main array is correct size
                        for (int b = 0; b < tempBytes.Length; ++b)
                        {
                            bytes[b] = tempBytes[b];
                        }
                        fs.Seek(v.ID * VertexByteLength, SeekOrigin.Begin);
                        fs.Write(bytes, 0, VertexByteLength);  // write the stream to file
                    }
                }
                catch (IndexOutOfRangeException e)
                { Console.WriteLine("Index out of range in SaveVertex: " + e); }
                catch (Exception e)
                { Console.WriteLine("Error in SaveVertex: " + e); }
            }
        }

        /// <summary>
        /// Save edge to file
        /// </summary>
        /// <param name="edge"></param>
        public void SaveEdge(Edge edge)
        {
            return;
            string FileDir = @"graphbin\";
            string FileName = FileDir + "edges.bin";
            Directory.CreateDirectory(FileDir);
            lock (lockObj)
            {
                try
                {
                    using (FileStream fs = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.Write))
                    using (MemoryStream ms = new MemoryStream())
                    using (BinaryWriter writer = new BinaryWriter(ms))
                    {
                        writer.Write(edge.Node1);
                        writer.Write(edge.Node2);
                        writer.Write(edge.Weight);
                        byte[] bytes = new byte[EdgeByteLength];
                        byte[] tempBytes = ms.ToArray();   // Temp array to fill main array, so main array is correct size
                        for (int b = 0; b < tempBytes.Length; ++b)
                        {
                            bytes[b] = tempBytes[b];
                        }
                        fs.Seek(edge.ID * EdgeByteLength, SeekOrigin.Begin);
                        fs.Write(bytes, 0, EdgeByteLength);  // write the stream to file
                    }
                }
                catch (IndexOutOfRangeException e)
                { Console.WriteLine("Index out of range in SaveEdge: " + e); }
                catch (Exception e)
                { Console.WriteLine("Error in SaveEdge: " + e); }
            }
        }

        /// <summary>
        /// Pull a vertex from disk. This method should only be run under the assumption 
        ///     that the graph list does not include a vertex with the specified ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool LoadAllVertices()
        {
            bool success = false;
            string FileName = $"graphbin\\vertices.bin";
            Vertex readVertex;
            try
            {
                if (new FileInfo(FileName).Length > 0)
                {
                    using (FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read))
                    using (BinaryReader reader = new BinaryReader(fs))
                    {
                        long len = fs.Length;
                        // IDs start at 1
                        for (int id = 1; id < len / VertexByteLength; ++id)
                        {
                            readVertex = null; // Clear vertex
                            readVertex = new Vertex(id);
                            fs.Seek(id * VertexByteLength, SeekOrigin.Begin);
                            // Get Vertex data from file
                            readVertex.Data = reader.ReadString();
                            for (int n = 0; n < Vertex.NumberOfNeighbors; ++n)
                            {
                                int n1 = reader.ReadInt32();
                                int n2 = reader.ReadInt32();
                                float w = reader.ReadSingle();
                                int nid = reader.ReadInt32();
                                readVertex.Neighbors.Add(new Edge(n1, n2, w, nid));
                            }
                            // Add vertex to this graph
                            this.AddVertex(readVertex);
                        } // End for loop
                        success = true;
                    } // End reader using
                } // End if
            }
            catch (Exception e)
            {
                MessageBox.Show("Error trying to load vertices from disk, please reload Graph.", "Model:Graph:LoadAllVertices()", MessageBoxButton.OK, MessageBoxImage.Warning);
                Console.WriteLine("Error trying to load vertices from disk, please reload Graph. \n\nError: " + e);
                return false;
            }
            return success;
        }

        /// <summary>
        /// Pull an edge from disk. Only use if graph doesn't include an edge with this ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool LoadAllEdges()
        {
            bool success = false;
            string FileName = $"graphbin\\edges.bin";
            Edge readEdge;
            try
            {
                if (new FileInfo(FileName).Length > 0)
                {
                    using (FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read))
                    using (BinaryReader reader = new BinaryReader(fs))
                    {
                        long len = fs.Length;
                        for (int id = 0; id < len / EdgeByteLength; id += EdgeByteLength)
                        {
                            readEdge = null; // Clear edge
                            fs.Seek(id, SeekOrigin.Begin);
                            int n1 = reader.ReadInt32();
                            int n2 = reader.ReadInt32();
                            float w = reader.ReadSingle();
                            readEdge = new Edge(n1, n2, w, id / EdgeByteLength);
                            this.AddEdge(readEdge);
                        } // End for loop
                        success = true;
                    } // end using
                } // end if
            }
            catch (Exception e)
            {
                MessageBox.Show("Error trying to load edges from disk, please reload Graph. \nError: " + e, "Model:Graph:LoadAllEdges()", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return success;
        }

        public Edge GetEdge(int n1, int n2)
        {
            return Edges.FirstOrDefault(x => x.Node1 == n1 && x.Node2 == n2);
        }

        public Vertex GetVertexWithData(string d)
        {
            return Vertices.FirstOrDefault(x => x.Data == d);

        }
        #endregion

    } // End Graph class

    /// <summary>
    /// Vertex nodes of the graph
    /// </summary>
    public class Vertex
    {
        private int id;
        private string data;
        private List<Edge> neighbors;
        public const int NumberOfNeighbors = 20;
        public Vertex(int id, string data)
        {
            this.id = id;
            this.data = data;
            this.neighbors = new List<Edge>();
            this.Cost = float.MaxValue;
        }
        public Vertex(int id)
        {
            this.id = id;
            data = "";
            this.neighbors = new List<Edge>();
            this.Cost = float.MaxValue;
        }
        #region Vertex Properties
        public int ID
        {
            get
            {
                return id;
            }
        }
        public string Data
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
            }
        }
        public List<Edge> Neighbors
        {
            get
            {
                return neighbors;
            }
            set
            {
                neighbors = value;
            }
        }
        // Properties for Priority Queue
        public float Cost { get; set; }
        public int Index { get; set; }

        public void AddNeighbor(Edge edge)
        {
            this.Neighbors.RemoveAll(lam => lam.Node1 == edge.Node1 && lam.Node2 == edge.Node2);
            this.Neighbors.Add(edge);
        }
        #endregion
    } // End Vertex

    /// <summary>
    /// Edges of the graph connecting vertices
    /// </summary>
    public class Edge
    {
        private int node1;
        private int node2;
        private float weight;
        private int id;
        #region Edge Properties
        public int Node1
        {
            get
            {
                return node1;
            }
        }
        public int Node2
        {
            get
            {
                return node2;
            }
        }
        public float Weight
        {
            get
            {
                return weight;
            }
            set
            {
                weight = value;
            }
        }
        public int ID
        {
            get
            {
                return id;
            }
        }
        #endregion
        public Edge(int node1, int node2, float weight, int id)
        {
            this.node1 = node1; this.node2 = node2;
            this.weight = weight; this.id = id;
        }
    }

    public struct Root
    {
        public string Name { get; set; }
        public Vertex RootVertex { get; set; }
        public Root(string n, Vertex v)
        { Name = n; RootVertex = v; }
    }

}

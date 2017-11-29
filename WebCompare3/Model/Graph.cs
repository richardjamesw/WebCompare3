using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCompare3.Model
{
    class Graph
    {
        private static int numOfVertices = 0;
        private List<Vertex> vertices;
        private List<Edge> edges;
        const int VertexByteLength = 256;
        const int EdgeByteLength = 16;
        #region Graph Properties
        public static int NumberOfVertices
        {
            get
            {
                return numOfVertices;
            }
            set
            {
                numOfVertices = value;
            }
        }
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
            this.vertices.Add(newVertex);
        }

        /// <summary>
        /// Add an edge to the graph's list of edges
        /// </summary>
        /// <param name="newEdge"></param>
        public void AddEdge(Edge newEdge)
        {
            this.edges.Add(newEdge);
        }

        /// <summary>
        /// Check if the graph includes a vertex with a certain URL already
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public bool HasVertex(string url, out int id)
        {
            bool rtn = false;
            id = 0;
            if (Vertices == null)
            {
                return rtn;
            }
            else
            {
                IEnumerable<int> ident =
                from vert in Vertices
                where vert.Data == url
                select vert.ID;
                id = ident.FirstOrDefault();
            }

            if (id > 0)
                return true;
            else
                return false;
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
            string FileName = $"graphbin\\vertices.bin";
            lock (lockObj)
            {
                try
                {
                    using (FileStream fs = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.Write))
                    using (MemoryStream ms = new MemoryStream())
                    using (BinaryWriter writer = new BinaryWriter(ms))
                    {
                        writer.Write(v.Data);
                        for (int n = 0; n < v.Neighbors.Count; ++n)
                        {
                            if (n < Vertex.NumberOfNeighbors)
                            {
                                writer.Write(v.Neighbors[n]);
                            }
                            else
                            {
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
                { Console.WriteLine("Index out of range in WriteNode: " + e); }
                catch (Exception e)
                { Console.WriteLine("Error in WriteNode: " + e); }
            }
        }

        /// <summary>
        /// Save edge to file
        /// </summary>
        /// <param name="edge"></param>
        public void SaveEdge(Edge edge)
        {
            string FileName = $"graphbin\\edges.bin";
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
                { Console.WriteLine("Index out of range in WriteNode: " + e); }
                catch (Exception e)
                { Console.WriteLine("Error in WriteNode: " + e); }
            }
        }

        /// <summary>
        /// Pull a vertex from disk. This method should only be run under the assumption 
        ///     that the graph list does not include a vertex with the specified ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Vertex LoadVertex(int id)
        {
            string FileName = $"graphbin\\vertices.bin";
            Vertex readVertex = null;
            try
            {
                if (new FileInfo(FileName).Length > 0)
                {
                    using (FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read))
                    using (BinaryReader reader = new BinaryReader(fs))
                    {
                        readVertex = new Vertex(id);
                        fs.Seek(id * VertexByteLength, SeekOrigin.Begin);
                        readVertex.Data = reader.ReadString();
                        for (int n = 0; n < Vertex.NumberOfNeighbors; ++n)
                        {
                            if (n != 0)
                            {
                                readVertex.Neighbors.Add(reader.ReadInt32());
                            }
                        }
                    } // end using
                } // end if
            }
            catch (Exception e) { Console.WriteLine("Error in ReadNode: " + e); }
            return readVertex;
        }

        /// <summary>
        /// Pull an edge from disk. Only use if graph doesn't include an edge with this ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Edge LoadEdge(int id)
        {
            string FileName = $"graphbin\\edges.bin";
            Edge readEdge = null;
            try
            {
                if (new FileInfo(FileName).Length > 0)
                {
                    using (FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read))
                    using (BinaryReader reader = new BinaryReader(fs))
                    {
                        fs.Seek(id * EdgeByteLength, SeekOrigin.Begin);
                        int n1 = reader.ReadInt32();
                        int n2 = reader.ReadInt32();
                        int w = reader.ReadInt32();
                        readEdge = new Edge(n1, n2, w, id);
                    } // end using
                } // end if
            }
            catch (Exception e) { Console.WriteLine("Error in ReadNode: " + e); }
            return readEdge;
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
        private List<int> neighbors;
        public const int NumberOfNeighbors = 20;
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
        public List<int> Neighbors
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
        public double Similarity { get; set; }
        #endregion
        public Vertex(int id, string data)
        {
            this.id = id;
            this.data = data;
        }
        public Vertex(int id)
        {
            this.id = id;
            data = "";
            neighbors = null;
        }
    }

    /// <summary>
    /// Edges of the graph connecting vertices
    /// </summary>
    public class Edge
    {
        private int node1;
        private int node2;
        private double weight;
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
        public double Weight
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
        public Edge(int node1, int node2, double weight, int id)
        {
            this.node1 = node1; this.node2 = node2;
            this.weight = weight; this.id = id;
        }
    }

}

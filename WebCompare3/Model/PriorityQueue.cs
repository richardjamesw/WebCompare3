using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCompare3.Model
{
    class PriorityQueue
    {
        //ivs
        List<Vertex> arr;
        public List<Vertex> Arr { get { return arr; } }
        public int Size { get; set; }

        //constructor
        public PriorityQueue(List<Vertex> list)
        {
            arr = new List<Vertex>();
            foreach(Vertex v in list)
            {
                v.Cost = float.MaxValue;
                arr.Add(v);
            }
            Size = arr.Count;
        }

        //// Build
        //public void BuildHeap()
        //{
        //    for (int i = Size / 2; i >= 0; --i)
        //    {
        //        QAdd(i);
        //    }
        //}

        //// Rearrange
        //public void QAdd(int parent)
        //{
        //    int left = 2 * parent + 1;
        //    int right = left + 1;
        //    int largest;
        //    // left child
        //    if (left < Size && arr[left].Cost > arr[parent].Cost)
        //    {
        //        largest = left;
        //    }
        //    else
        //    {
        //        largest = parent;
        //    }
        //    // right child
        //    if (right < Size && arr[right].Cost > arr[largest].Cost)
        //    {
        //        largest = right;
        //    }
        //    // Swap if diff
        //    if (largest != parent)
        //    {
        //        Vertex t = arr[parent];
        //        arr[parent] = arr[largest];
        //        arr[largest] = t;
        //    }
        //}
        
        // PQ
        // Min
        Vertex Minimum()
        {
            if (Size == 0)
                return null;
            else
                return arr[0];
        }

        //left of
        int LeftOf(int k)
        {
            return 2 * k + 1;
        }

        // Parent Of
        static int ParentOf(int k)
        {
            if (k == 0) return 0;
            return (k - 1) / 2;   // Divide by 2
        }


        /// <summary>
        /// Remove at root
        /// </summary>
        /// <returns></returns>
        public Vertex Poll()
        {
            if (Size == 0) return null;
            // Pop root, move last to top
            Vertex x = arr[0];
            arr[0] = arr[Size-1];
            arr.RemoveAt(Size-1);
            --Size;

            // Bubble down
            int k = 0;
            // While there are more children
            while (LeftOf(k) < Size - 1)
            {
                // Look at children
                // swap with lower
                int l = LeftOf(k);
                int r = l + 1;

                Vertex left = arr[l];
                if (r > Size - 1)
                {
                    // if left Vertex is smaller than bubbling Vertex
                    if (left.Cost < arr[k].Cost)
                    {
                        // swap
                        Vertex t = arr[l];
                        arr[l] = arr[k];
                        arr[k] = t;
                        k = l;
                    }
                    break;
                }
                else
                {
                    // swap bubbling Vertex with smaller Vertex
                    Vertex right = arr[r];
                    int least = (right.Cost < left.Cost) ? r  : l;
                    
                    if (arr[least].Cost < arr[k].Cost)
                    {
                        // swap
                        Vertex t = arr[least];
                        arr[least] = arr[k];
                        arr[k] = t;
                    }
                    else
                    {
                        // already in correct spot
                        return x;
                    }
                    k = least;
                }
            } // End while
            return x;
        } // End Poll


        /// <summary>
        /// Add Vertex to the Priority Queue
        /// </summary>
        /// <param name="e"></param>
        /// <param name="w"></param>
        public void Add(Vertex e, float w)
        {
            e.Cost = w;
            Add(e);
        }
        public void Add(Vertex e)
        {
            // Throw it at the end
            arr.Add(e);
            ++Size;
            int k = Size - 1;
            // Bubble up
            while (k != 0)
            {
                int p = ParentOf(k);
                Vertex parent = arr[p];

                // switch with other child then check parent
                // add it as a child instead of leaving under all the inifities
                if (arr[k].Cost < arr[LeftOf(p)].Cost)
                {
                    this.Exchange(k, LeftOf(p));
                    k = LeftOf(p);
                }

                if (arr[k].Cost < parent.Cost)
                {
                    // Swap
                    this.Exchange(k, p);
                    k = p;
                }
                else
                {
                    break;
                }
            } // End while
            //e.Index = k;
        } // End Add


        public void Reweight(Vertex next)
        {
            this.Arr.RemoveAll(lam => lam.ID == next.ID);
            --this.Size;
            this.Add(next);
        }


        #region Array properties
        public Vertex this[int index]
        {
            get
            {
                return this.Arr[index];
            }
            set
            {
                this.Arr[index] = value;
            }
        }

        public Vertex GetVertex(int id)
        {
            return this.Arr.FirstOrDefault(lam => lam.ID == id);
        }
        public int IndexOf(Vertex vert)
        {
            return this.Arr.IndexOf(vert);
        }

        public bool IsEmpty()
        {
            if (this.Arr == null || this.Arr.Count < 1)
                return true;
            else
                return false;
        }

        public void Exchange(int i, int j)
        {
            Vertex temp = this.Arr[i];
            this.Arr[i] = this.Arr[j];
            this.Arr[j] = temp;
        }
        #endregion

    } // End PriorityQueue
}

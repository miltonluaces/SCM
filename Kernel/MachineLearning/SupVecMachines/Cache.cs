#region Imports

using System;

#endregion

namespace MachineLearning  {

    internal class Cache {

        #region Fields

        private int count;
        private long size;
        private headT[] head;
        private headT lruHead;

        #endregion

        #region Constructor

        public Cache(int count, long size)  {
            this.count = count;
            this.size = size;
            this.head = new headT[count];
            for (int i = 0; i < count; i++)
                head[i] = new headT(this);
            this.size /= 4;
            this.size -= count * (16 / 4); // sizeof(head_t) == 16
            this.lruHead = new headT(this);
            this.lruHead.next = lruHead.prev = lruHead;
        }

        #endregion

        #region Internal Methods

        // request data [0,len) return some position p where [p,len) need to be filled (p >= len if nothing needs to be filled)
       internal int GetData(int index, ref float[] data, int len)  {
            headT h = head[index];
            if (h.len > 0)
                lru_delete(h);
            int more = len - h.len;

            if (more > 0)  {
                // free old space
                while (size < more)  {
                    headT old = lruHead.next;
                    lru_delete(old);
                    size += old.len;
                    old.data = null;
                    old.len = 0;
                }

                // allocate new space
                float[] new_data = new float[len];
                if (h.data != null)
                    Array.Copy(h.data, 0, new_data, 0, h.len);
                h.data = new_data;
                size -= more;
                swap(ref h.len, ref len);
            }

            lru_insert(h);
            data = h.data;
            return len;
        }

        internal void SwapIndex(int i, int j)  {
            if (i == j)
                return;

            if (head[i].len > 0) lru_delete(head[i]);
            if (head[j].len > 0) lru_delete(head[j]);
            swap(ref head[i].data, ref head[j].data);
            swap(ref head[i].len, ref head[j].len);
            if (head[i].len > 0) lru_insert(head[i]);
            if (head[j].len > 0) lru_insert(head[j]);

            if (i > j)   swap(ref i, ref j);

            for (headT h = lruHead.next; h != lruHead; h = h.next)  {
                if (h.len > i) {
                    if (h.len > j) {
                        swap(ref h.data[i], ref h.data[j]);
                    }
                    else {
                        // give up
                        lru_delete(h);
                        size += h.len;
                        h.data = null;
                        h.len = 0;
                    }
                }
            }
        }

        #endregion
        
        #region Private Methods

        private void lru_delete(headT h)   {
            // delete from current location
            h.prev.next = h.next;
            h.next.prev = h.prev;
        }

        private void lru_insert(headT h) {
            // insert to last position
            h.next = lruHead;
            h.prev = lruHead.prev;
            h.prev.next = h;
            h.next.prev = h;
        }

        private static void swap<T>(ref T lhs, ref T rhs) {
            T tmp = lhs;
            lhs = rhs;
            rhs = tmp;
        }

        #endregion

        #region Inner class Head_t

        private sealed class headT {
            public headT(Cache enclosingInstance)  {
                _enclosingInstance = enclosingInstance;
            }

            private Cache _enclosingInstance;
            public Cache EnclosingInstance  {
                get { return _enclosingInstance; }
            }

            internal headT prev, next; // a cicular list
            internal float[] data;
            internal int len; // data[0,len) is cached in this entry
        }

        #endregion

    }
}

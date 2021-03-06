#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace Maths {

    public class SDict<TKey, TValue> : IDictionary<TKey, TValue>  {

        private Dictionary<TKey, TValue> internaldict = null;
        private SortedList<TKey, object> sortedkeys = null;

        public SDict()  {
            internaldict = new Dictionary<TKey, TValue>();
            sortedkeys = new SortedList<TKey, object>();
        }

        public void Add(TKey key, TValue value)  {
            internaldict.Add(key, value);
            sortedkeys.Add(key, null);
        }

        public bool ContainsKey(TKey key) {
            return internaldict.ContainsKey(key);
        }

        public ICollection<TKey> Keys  {
            get { return sortedkeys.Keys; }
        }

        public bool Remove(TKey key)  {
            if (internaldict.Remove(key))
                return sortedkeys.Remove(key);
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)  {
            return internaldict.TryGetValue(key, out value);
        }

        public ICollection<TValue> Values  {
            get { return internaldict.Values; }
        }

        public TValue this[TKey key]   {
            get { return internaldict[key]; }
            set {
                internaldict[key] = value;
                sortedkeys[key] = value;
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item) { throw new NotImplementedException();  }

        public void Clear()  {
            internaldict.Clear();
            sortedkeys.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)  {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)  {
            throw new NotImplementedException();
        }

        public int Count  {
            get { return internaldict.Count; }
        }

        public bool IsReadOnly  {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)  {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()  {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()  {
            return internaldict.GetEnumerator();
        }
    }
}

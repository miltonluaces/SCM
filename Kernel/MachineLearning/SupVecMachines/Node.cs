#region Imports

using System;

#endregion

namespace MachineLearning  {
    
    //A Node in a Problem vector, with an index and a value (for more efficient representation of sparse data).
    [Serializable]
	public class Node : IComparable<Node>  {

        #region Fields

        internal int index;
        internal double value;

        #endregion

        #region Constructor

        public Node()  {
        }

        public Node(int index, double value) {
            this.index = index;
            this.value = value;
        }

        #endregion

        #region Properties

        public int Index  {
            get { return index; }
            set { index = value; }
        }
        public double Value {
            get { return value; }
            set {this.value = value; }
        }

        #endregion

        #region Internal Methods

        public override string ToString()  {
            return string.Format("{0}:{1}", index, value);
        }

        #endregion

        #region IComparable<Node> Members

        public int CompareTo(Node other)  {  return index.CompareTo(other.index);  }

        #endregion
    }
}
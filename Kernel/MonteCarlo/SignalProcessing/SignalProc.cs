#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using Maths;

#endregion

namespace MonteCarlo {

    /** Class:  Abstract class for any signal processing method  */
    internal abstract class SignalProc {

        #region Fields

        ///  <summary> Original time series  */
        protected List<double> origData;

        ///  <summary> Current time series  */
        protected List<double> data;

        ///  <summary> Trend time series  */
        protected List<double> trend;

        /** Method:  proportion of non-zeros  */
        protected double loadFactor;


        #endregion

        #region Properties

        /** Method:  Original time series  */
        internal List<double> OrigData {
            get { return origData; }
        }

        /** Method:  Current time series  */
        internal List<double> Data {
            get { return data; }
        }

        /** Method:  Time series trend at each point  */
        internal List<double> Trend {
            get { return trend; }
        }

        /** Method:  load factor of grouped non-zero values on total grouped values  */
        internal double LoadFactor {
            get { return loadFactor; }
        }

        #endregion

        #region internal Methods

        /** Method:  Group time series data in groups of n values  
        origData -  original time series 
        firstIndex -  index of first period for calculation 
        group -  grouping index 
        continuous -  if applies continuous grouping or not */
        internal void Group(List<double> origData, int firstIndex, int group, bool continuous) {
            if (continuous) { ContinuousGroup(origData, firstIndex, group); }
            else { SimpleGroup(origData, firstIndex, group); }
        }

        internal void SimpleGroup(List<double> origData, int firstIndex, int group) {
            this.origData = origData;
            this.data = new List<double>();
            int day = 0;
            double totPeriodo = 0.0;
            double val;
            int nonZero = 0;
            for(int i=origData.Count-1;i>=firstIndex;i--) {
                val = origData[i];
                totPeriodo += val;
                day++;
                if(day == group) {
                    data.Insert(0,totPeriodo);
                    day = 0;
                    if(totPeriodo > 0) { nonZero++; }
                    totPeriodo = 0.0;
                }
            }
            if(day > 0) { 
                data.Insert(0,totPeriodo);
                if(totPeriodo > 0) { nonZero++; }
            }
            loadFactor = (double)nonZero /(double)data.Count; 
            this.trend = new List<double>(data);
        }

        internal void ContinuousGroup(List<double> origData, int firstIndex, int group) {
            this.data = new List<double>();
            if (origData == null || origData.Count == 0 || group <= 0) { return; }
            int nonZero = 0;
            for (int i = firstIndex; i < origData.Count - group; i++) {
                double total = 0;

                int l = 0;
                int j = i;
                while (l < group) {
                    if (j >= origData.Count) { return; }
                    if (origData[j] >= 0) { total += origData[j]; }
                    l++;
                    j++;
                }
                data.Add(total);
                if(total > 0) { nonZero++; }
            }
            loadFactor = (double)nonZero / (double)data.Count;
            this.trend = new List<double>(data);
        }
        

        #endregion

    }
}


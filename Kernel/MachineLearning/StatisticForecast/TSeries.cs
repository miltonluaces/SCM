#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Maths;

#endregion
  
namespace MachineLearning {

    internal class TSeries {

         #region Fields

            private Functions func = new Functions();
            private StatForecast model;
            private double[][] moments;
            private Dictionary<int, StatForecast.ObsValue> ts;
            private List<int> obs;
            private int NPeriodsToAverage;

            #endregion
     
         #region Constructor

            internal TSeries(StatForecast Model) {
                this.func = new Functions();
                int NOBS = Model.GetNObservations();
                this.model = Model;

                // Asign memory to OTS matrix and set it to zero
                this.ts = new Dictionary<int, StatForecast.ObsValue>();

                // Asign memory to moments matrix and set it to zero
                this.moments = new double[2][];
                for (int i = 0; i < 2; i++) this.moments[i] = new double[2];
                for (int r = 0; r < 2; r++)
                    for (int c = 0; c < 2; c++)
                        this.moments[r][c] = 0.0;
                this.obs = new List<int>();
                this.NPeriodsToAverage = 8;
            }

            #endregion 

         #region Properties

            internal double[][] Moments {
                get { return moments; }
            }

            internal Dictionary<int, StatForecast.ObsValue> TS {
                get { return ts; }
                set { ts = value; }
            }

            internal List<int> Obs {
                get { return obs; }
                set { obs = value; }
            }

            #endregion

         #region Private Methods
            
            internal int GetNPeriodsToAverage() {
                return (this.NPeriodsToAverage);
            }

            internal void SetObs() {
                //int oldestIndex = this.model.GetFPNtoForecast(); 
                int oldestIndex = 0;
                int newestIndex = this.GetNewestIndex();
                this.obs = new List<int>(newestIndex - oldestIndex);
                for (int i = oldestIndex; i <= newestIndex; i++) {
                    if (ts[i].ObsType != ObsType._MissingObs) { Obs.Add(i); }
                }
            }

            internal int GetFirstObsIndex() {
                int i = this.obs.BinarySearch(this.model.GetFirstPNumber());
                if (i >= 0) return i;
                return 0;
            }

            internal int GetLastObsIndex() {
                return (this.obs.Count - 1);
            }

            internal int GetFirstObsPN() {
                return (this.obs[this.GetFirstObsIndex()]);
            }

            internal int GetLastObsPN() {
                return (this.obs[this.GetLastObsIndex()]);
            }

            internal int GetObsPN(int Index) {
                return (this.obs[Index]);
            }

            internal int GetObsIndex(int PN) {
                if (this.obs.Contains(PN))
                    return (this.obs.BinarySearch(PN));
                else throw new ArgumentException("Period_Number_0_is_a_period_number_of_an_observation_with_an_observed_value" + PN);
            }

            internal List<int> GetNextObs(int PNumber, int k) {
                List<int> Aux = new List<int>();
                int StartIndex = this.obs.BinarySearch(this.GetNearestObs(PNumber));

                if (StartIndex <= this.obs.Count - k)
                    for (int i = 0; i < k; i++)
                        Aux.Add(this.obs[StartIndex + i]);
                return Aux;
            }

            internal int GetNOutliers(int PNumber) {
                int NOutliers = 0, pn = PNumber;

                while (this.ts.ContainsKey(pn)) {
                    if (this.ts[pn].ObsType == ObsType._SystemDef) NOutliers++;
                    pn++;
                }
                return NOutliers;
            }

            internal int GetNearestObs(int PNumber) {
                if (this.obs.Contains(PNumber))
                    return PNumber;
                else {
                    if (this.ExistsInHistPN(PNumber)) {
                        int pn = PNumber;
                        while (this.ts[pn].ObsType == ObsType._MissingObs) pn++;
                        return pn;
                    }
                    else
                        return (this.ts[this.ts.Count].PNumber);
                }
            }

            #endregion 

         #region Internal Methods

            #region Setters & Getters

            internal StatForecast.ObsValue GetTS(int PNumber) {
                int fpn = this.model.GetFirstPNumber();

                if ((PNumber - fpn < (int)0) ||
                    (PNumber > fpn + this.model.GetNObservations()))
                    throw new ArgumentException("Cannot_Get_Period_Number_It_Does_Not_Exit_In_This_Obsevable_Time_Series" + PNumber);
                else
                    return (this.ts[PNumber]);
            }

            internal double[] GetTSValues(bool Normalised) {
                int Oldest = this.OldestPN(), MostRecent = this.GetNewestIndex();

                double[] vector = new double[MostRecent - Oldest + 1];
                if (Normalised)
                    for (int i = Oldest; i <= MostRecent; i++) vector[i - Oldest] = this.ts[i].NValue;
                else
                    for (int i = Oldest; i <= MostRecent; i++) vector[i - Oldest] = this.ts[i].NNValue;
                return (vector);
            }

            internal double[] GetObsValues(bool Normalised) {
                int FirstObsIndex = this.GetFirstObsIndex(), LastObsIndex = this.GetLastObsIndex();

                double[] vector = new double[LastObsIndex - FirstObsIndex + 1];
                if (Normalised)
                    for (int i = FirstObsIndex; i <= LastObsIndex; i++) vector[i - FirstObsIndex] = this.ts[this.obs[i]].NValue;
                else
                    for (int i = FirstObsIndex; i <= LastObsIndex; i++) vector[i - FirstObsIndex] = this.ts[this.obs[i]].NNValue;
                return (vector);
            }

            internal void SetTSNNValue(int PNumber, double v) {
                if (this.TS.ContainsKey(PNumber)) {
                    StatForecast.ObsValue V = this.ts[PNumber];
                    V.NNValue = v;
                    this.ts.Add(PNumber, V);
                }
            }

            internal void SetTSNValue(int PNumber, double v) {
                if (this.TS.ContainsKey(PNumber)) {
                    StatForecast.ObsValue V = this.ts[PNumber];
                    V.NValue = v;
                    this.ts.Add(PNumber, V);
                }
            }

            internal void SetTSPLength(int PNumber, double v) {
                if (this.TS.ContainsKey(PNumber)) {
                    StatForecast.ObsValue V = this.ts[PNumber];
                    V.PLength = v;
                    this.ts.Add(PNumber, V);
                }
            }

            internal void SetTSType(int PNumber, ObsType v) {
                if (this.TS.ContainsKey(PNumber)) {
                    StatForecast.ObsValue V = this.ts[PNumber];
                    V.ObsType = v;
                    this.ts[PNumber] = V;
                }
            }

            #endregion 

            #region Auxiliar Methods

            internal bool NormalizeOTS() {

                int Oldest = this.OldestPN(), MostRecent = this.GetNewestIndex();
                for (int c = Oldest; c <= MostRecent; c++) {
                    if (this.ts[c].PLength == 0.0) {
                        StatForecast.ObsValue obsValue = this.ts[c];
                        obsValue.NValue = 0.0;
                        this.ts[c] = obsValue;
                        return false;
                    }
                    else {
                        StatForecast.ObsValue obsValue = this.ts[c];
                        obsValue.NValue = this.ts[c].NNValue / this.ts[c].PLength;
                        this.ts[c] = obsValue;
                    }
                }
                return true;
            }

            internal void SetValues01() {
                int Oldest = this.OldestPN(), MostRecent = this.GetNewestIndex();
                List<double> values = new List<double>();
                List<int> keys = new List<int>();
                for (int c = Oldest; c <= MostRecent; c++)
                    if (!this.model.IsMissing(c)) {
                        values.Add(this.ts[c].NValue);
                        keys.Add(c);
                    }
                List<double> N = func.MovingAverage(values, this.NPeriodsToAverage, false);
                for (int c = 0; c < keys.Count; c++)
                    if (!this.model.IsMissing(keys[c])) {
                        StatForecast.ObsValue obsValue = this.ts[keys[c]];
                        obsValue.Value01 = N[c];
                        this.ts[keys[c]] = obsValue;
                    }
            }

            internal bool CalcMoments(int FirstPN) {
                double mean1 = (double)0, mean2 = (double)0;
                double sumofsquares1 = 0.0, sumofsquares2 = 0.0;
                int nobs = (int)0;
                int FirstObsIndex = this.model.GetObsIndex(FirstPN), LastObsIndex = this.model.GetLastObsIndex();

                if (FirstPN < this.model.GetFirstPNumber()) {
                    throw new ArgumentException("primer periodo menor que " + this.model.GetFirstPNumber());
                }

                if (FirstPN > this.model.GetLastPNumber()) {
                    throw new ArgumentException("primer periodo mayor que ultimo de la historia en" + this.model.GetLastPNumber());
                }

                // Set to zero all moments

                for (int r = 0; r < 2; r++)
                    for (int c = 0; c < 2; c++)
                        this.moments[r][c] = (double)0;

                for (int Index = FirstObsIndex; Index <= LastObsIndex; Index++) {
                    int pn = this.model.GetObsPN(Index);
                    StatForecast.ObsValue obsValue = this.ts[pn];
                    if (obsValue.ObsType == ObsType._Usable) {
                        mean1 += obsValue.NNValue;
                        mean2 += obsValue.NValue;
                        sumofsquares1 += Math.Pow(obsValue.NNValue, 2);
                        sumofsquares2 += Math.Pow(obsValue.NValue, 2);
                        nobs++;
                    }
                }

                this.moments[0][0] = mean1 / nobs;
                this.moments[1][0] = mean2 / nobs;
                this.moments[0][1] = Math.Sqrt((nobs * sumofsquares1 - Math.Pow(mean1, 2)) / (nobs * (nobs - 1)));
                this.moments[1][1] = Math.Sqrt((nobs * sumofsquares2 - Math.Pow(mean2, 2)) / (nobs * (nobs - 1)));

                return true;
            }

            internal int FirstNonZero() {
                int Oldest = this.OldestPN(), MostRecent = this.GetNewestIndex();
                if ((this.ts[Oldest].NNValue != 0) && (!this.model.IsMissing(this.ts[Oldest].PNumber))) return Oldest;
                for (int i = Oldest; i <= MostRecent; i++)
                    if ((!this.model.IsMissing(i)) && (this.ts[i].NNValue != 0)) return i;
                return MostRecent;
            }

            internal bool DeletePeriods(int n) {
                int Oldest = this.OldestPN();
                for (int i = 0; i < n; i++)
                    this.TS.Remove(this.OldestPN());
                this.model.SetFirstPNumber(this.OldestPN());

                return true;
            }

            internal int OldestPN() {
                int min = int.MaxValue;
                foreach (int k in this.ts.Keys)
                    if (k < min) min = k;
                return (min);
            }

            internal int GetNewestIndex() {
                int min = 0;
                foreach (int k in this.ts.Keys)
                    if (k > min) min = k;
                return (min);
            }

            internal int LastHistPN() {
                int min = 0;
                foreach (int k in this.ts.Keys)
                    if (k > min) min = k;
                return (min);
            }

            internal int GetNObs() {
                if (this.ts != null) {
                    int nobs = 0;
                    foreach (int k in this.ts.Keys)
                        if (!this.model.IsMissing(k)) nobs++;
                    return (nobs);
                }
                else
                    return 0;
            }

            internal int GetNHistPeriods() {
                if (this.ts != null) { return (this.ts.Count); }
                else { return 0; }
            }

            internal bool ExistsInHistPN(int PNumber) {
                return (this.TS.ContainsKey(PNumber));
            }

            #endregion

            #endregion
    }
 }

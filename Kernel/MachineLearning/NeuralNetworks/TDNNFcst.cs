#region Imports

using System;
using System.Collections;
using System.Collections.Generic;
using Maths;

#endregion

namespace MachineLearning {

    internal class TDNNFcst : TsForecast {

        #region Fields

        private int movingWindow;
        private int epochs;
        private int trendValues;
        private double trendPerc;
        private List<double> errors;
        private FFNN ffNN;
        private double max;
        private Functions func; 
        private int iniIndex;
        private StateType state;
        private double normFactor;
        private List<List<List<double>>> model;

        #endregion

        #region Constructor

        internal TDNNFcst(int movingWindow, int epochs) {
            this.movingWindow = movingWindow;
            this.epochs = epochs;
            this.fcstMethod = FcstMethodType.NeuNet;
            this.trendValues = 5;
            this.trendPerc = 10;
            this.func = new Functions(); 
            this.state = StateType.created;
            this.normFactor = 1;
            this.model = null;
        }

        #endregion

        #region Properties

        internal int MovingWindow {
            get { return movingWindow; }
            set { movingWindow = value; }
        }

        internal int Epochs {
            get { return epochs; }
            set { epochs = value; }
        }

        internal int TrendValues {
            get { return trendValues; }
            set { trendValues = value; }
        }

        internal double TrendPerc {
            get { return trendPerc; }
            set { trendPerc = value; }
        }

        internal List<double> Errors {
            get { return errors; }
        }

        internal double NormFactor {
            get { return normFactor; }
        }

        internal List<double> Hist { get { return hist; } }

        #endregion

        #region Internal Methods

        internal double Train(int epochs) {
            //if (state < StateType.loaded) { throw new Exception("Error. System not loaded"); }

            double mse = 0.0;
            for (int e = 0; e < epochs; e++) {
                mse = 0.0;
                List<double> inputData = new List<double>();
                for (int i = iniIndex; i < hist.Count - movingWindow; i++) {
                    inputData = GetInput(hist, i, i + movingWindow - 1);
                    mse += ffNN.Train(inputData, GetNormalized(hist[i + movingWindow]));
                }
                ffNN.Train(inputData, GetNormalized(hist[hist.Count - 1]));
            }
            this.state = StateType.trained;
            return (mse / (hist.Count - movingWindow - 1)) * max;
        }

        internal double TrainLast(double lastValue, int epochs) {
            //if (state < StateType.loaded) { throw new Exception("Error. System not loaded"); }

            double mse = 0.0;
            for (int e = 0; e < epochs; e++)  {
                mse = 0.0;
                List<double> inputData = inputData = GetInput(hist, hist.Count-movingWindow, hist.Count - 1);
                mse += ffNN.Train(inputData, GetNormalized(lastValue));
            }
            this.state = StateType.trained;
            return (mse / (hist.Count - movingWindow - 1)) * max;
        }

        internal double Process()  {
            if (state < StateType.trained) { throw new Exception("Error. System not trained"); }

            List<double> inputData = GetInput(hist, hist.Count - movingWindow, hist.Count - 1);
            ffNN.Process(inputData);
            this.state = StateType.calculated;
            return GetUnNormalized(ffNN.GetOutput()[0]);
        }

        private List<double> GetSerieInput(IList<double> serie, int ini, int end) {
            List<double> inputData = new List<double>();
            for (int i = ini; i <= end; i++) { inputData.Add(GetNormalized(serie[i])); }
            	return inputData;
        	}

        private List<double> GetInput(IList<double> hist, int ini, int end) {
            List<double> input = GetSerieInput(hist, ini, end);
            return input;
        }

        internal double[] ProcessObsolete(int epochs, int fcstHorizon) {
            this.ffNN = new FFNN(1, movingWindow, movingWindow, 1, 0.6, 0.5);
            this.max = func.Max(this.hist);
            double[] forecast = new double[fcstHorizon];
            List<double> seriePrev = new List<double>(hist);
            double mse;
            for (int i = 0; i < fcstHorizon; i++) {
                mse = Train(epochs);
                forecast[i] = Process();
                hist.Add(forecast[i]);
            }
            hist = seriePrev;
            return forecast;
        }

        internal double[] Process(int epochs, int fcstHorizon, int errorWindow)
        {
            double[] forecast = new double[fcstHorizon];
            if (hist.Count < 3) {
                for (int i = 0; i < fcstHorizon; i++) { forecast[i] = hist[hist.Count - 1]; }
                return forecast;
            }
            if (hist.Count <= errorWindow + movingWindow) {
                for (int i = 0; i < fcstHorizon; i++) {
                    Train(epochs);
                    forecast[i] = Process();
                    hist.Add(forecast[i]);
                }
                return forecast;
            }
            List<double> seriePrev = new List<double>(hist);
            List<double> current = new List<double>();
            for (int i = hist.Count - errorWindow - 1; i < hist.Count; i++) {
                current.Add(hist[i]);
            }
            hist.RemoveRange(hist.Count - errorWindow - 1, errorWindow);

            errors = new List<double>();

            //error serie
            double error, fcst;
            for (int i = 0; i < errorWindow; i++) {
                Train(epochs);
                fcst = Process();
                error = current[i] - fcst;
                errors.Add(error > 0 ? error : 0);
                hist.Add(fcst);
            }
            hist.RemoveRange(hist.Count - 1 - errorWindow, errorWindow);
            for (int i = 0; i < fcstHorizon; i++) {
                Train(epochs);
                forecast[i] = Process();
                hist.Add(current[i]);
            }

            //forecast
            for (int i = 0; i < fcstHorizon; i++) {
                Train(epochs);
                forecast[i] = Process();
                hist.Add(forecast[i]);
            }
            hist = seriePrev;
            return forecast;
        }

        internal List<double> ModelFromHistory(int ciclos) {
            List<double> origSerie = this.hist;
            List<double> mHist = new List<double>();
            List<double> serieUntil = new List<double>();
            double fcst;
            for (int i = 0; i < movingWindow * 2; i++) {
                mHist.Add(-1);
                serieUntil.Add(origSerie[i]);
            }
            this.hist = serieUntil;
            for (int i = movingWindow * 2; i < origSerie.Count; i++) {
                this.hist.Add(origSerie[i]);
                Train(ciclos);
                fcst = Process();
                mHist.Add(fcst);
            }
            this.hist = origSerie;
            return mHist;
        }

        #endregion

        #region TsForecast Implementation

        public override void Calculate()  {
            this.max = func.Max(this.hist);
            double trendThres = this.max * (1 - trendPerc / 100.0);
            for (int i = hist.Count - 1; i >= hist.Count - trendValues; i--) {
                if (hist[i] >= trendThres) { this.normFactor = 1 - ((trendPerc * 2) / 100.0); break;  }
            }
            this.ffNN = new FFNN(1, movingWindow, movingWindow, 1, 0.6, 0.5);
            if (model == null) { Train(epochs); }
            else { this.ffNN.SetWeights(model); }
            this.state = StateType.trained;
        }

        public override double[] GetFcst(int horizon)   {
            SetModel(model);
            double[] forecast = new double[horizon];
            forecast[0] = Process();
            List<double> origHist = this.hist;
            for (int i = 1; i < horizon; i++) {
                TrainLast(forecast[i - 1], epochs);
                hist.Add(forecast[i - 1]);
                forecast[i] = Process();
            }
            hist = origHist;
            return forecast;
        }

        public override void SetModel(object modelObj) {
            if (modelObj == null) { return; }
            model = (List<List<List<double>>>)modelObj;
        }

        public override object GetModel()  {
            return this.ffNN.GetWeights(); 
        }
                
        #endregion
        
        #region Private Methods

        internal double GetNormalized(double val) { return normFactor * (val / max); }
        internal double GetUnNormalized(double val) { return (val * max) / normFactor; }

        internal enum StateType {
            created = 0,
            loaded = 1,
            trained = 2,
            calculated = 3
        }

        #endregion
 
   }
}

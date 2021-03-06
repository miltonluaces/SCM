#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABMath.ModelFramework.Models;
using ABMath.ModelFramework.Data;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.RandomSources;
using MathNet.Numerics;
using ABMath.ModelFramework.Transforms;
using Maths;

#endregion

namespace Statistics {
    
    internal class ARIMAFcst : TsForecast  {

        #region Fields

        private int arOrder;
        private int maOrder;
        private int itDs = 10;
        private int itOpt = 10;
        private int consPenalty = 10;
        private ARMAModel armaModel;
        private ForecastTransform fcsting;
        private TimeSeries ts;
        
        #endregion

        #region Constructor
        
        internal ARIMAFcst(int arOrder, int maOrder) {
            this.arOrder = arOrder;
            this.maOrder = maOrder;
        }

        #endregion

        #region Properties

        internal int ItDs {
            get { return itDs; }
            set { itDs = value; }
        }

        internal int ItOpt {
            get { return itOpt; }
            set { itOpt = value; }
        }

        internal int ConsPenalty  {
            get { return consPenalty; }
            set { consPenalty = value; }
        }
        #endregion

        #region Internal Methods

        internal double[] ARMAFcst(List<double> hist, int fcstHorizon) {
            armaModel = new ARMAModel(arOrder, maOrder);
            
            ts = new TimeSeries();
            DateTime date = new DateTime(2001, 1, 1);
            for (int t = 0; t < fcstHorizon; t++)  { ts.Add(date, hist[t], false); date = date.AddDays(1); }

            armaModel.SetInput(0, ts, null);
            armaModel.FitByMLE(itDs, itOpt, consPenalty, null);
            armaModel.ComputeResidualsAndOutputs();

            fcsting = new ForecastTransform();

            int horizon = 8;
            DateTime[] futureTimes = new DateTime[horizon];
            date = ts.GetLastTime();
            for (int t = 0; t < horizon; t++)  { date = date.AddDays(1); futureTimes[t] = date; }
            fcsting.FutureTimes = futureTimes;

            fcsting.SetInput(0, armaModel, null);
            fcsting.SetInput(1, ts, null);
            fcsting.Recompute();
            TimeSeries tsFcst = fcsting.GetOutput(0) as TimeSeries;
            double[] fcst = new double[fcstHorizon];
            for (int t = 0; t < tsFcst.Count; t++) { fcst[t] = tsFcst[t]; }
            return fcst;
        }

        internal void TestARMA() {
            ARMAModel arModel = new ARMAModel(4, 3); 
            ARMAModel maModel = new ARMAModel(0, 1);
            
            StandardDistribution sd = new StandardDistribution();
            TimeSeries ts1 = new TimeSeries();
            DateTime dt = new DateTime(2001, 1, 1);
            StandardDistribution normalSim = new StandardDistribution();
            var current = new DateTime(2001, 1, 1);
            double[] currArr = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20};
            for (int t = 0; t < 20; ++t) {
                ts1.Add(current, currArr[t], false);
                Console.WriteLine(currArr[t] + "");
                current = new DateTime(current.AddDays(1).Ticks);
            }
            Console.WriteLine(" ");
            
            arModel.SetInput(0, ts1, null);
            arModel.FitByMLE(10, 10, 10,null);
            arModel.ComputeResidualsAndOutputs();

            ForecastTransform fcster = new ForecastTransform();

            int horizon = 8;
            DateTime[] futureTimes = new DateTime[horizon];
            DateTime date = ts1.GetLastTime();
            for (int t = 0; t < horizon; t++) {  
                date = date.AddDays(1);     
                futureTimes[t] = date;
            }
            fcster.FutureTimes = futureTimes;

            fcster.SetInput(0, arModel, null);
            fcster.SetInput(1, ts1, null);
            fcster.Recompute();
            TimeSeries tsFcst = fcster.GetOutput(0) as TimeSeries;
            for (int t = 0; t < tsFcst.Count; t++) { Console.WriteLine(tsFcst[t]); }
            
            //TimeSeries preds = arModel.GetOutput(3) as TimeSeries;
            //for (int i = 0; i < preds.Count;i++ ) { Console.WriteLine(preds[i].ToString("0.00") + " "); }
            //Console.WriteLine("");
         
            //string predName = arModel.GetOutputName(3);
            
            //Console.WriteLine(arModel.Description);
       }

        #endregion

        #region TsForecast Implementation

        public override void Calculate()  {
            try  {
                ts = new TimeSeries();
                DateTime date = new DateTime(2001, 1, 1);
                for (int t = 0; t < hist.Count; t++) { ts.Add(date, hist[t], false); date = date.AddDays(1); }

                armaModel = new ARMAModel(arOrder, maOrder);
                armaModel.SetInput(0, ts, null);
                armaModel.FitByMLE(itDs, itOpt, consPenalty, null);
                armaModel.ComputeResidualsAndOutputs();
                fcstRes = FcstResType.Ok;
            }
            catch (Exception ex) {
                fcstRes = FcstResType.Error;
                Console.WriteLine(ex.StackTrace);
            }
        }

        public override double[] GetFcst(int horizon)  {
            fcsting = new ForecastTransform();

            DateTime[] futureTimes = new DateTime[horizon];
            DateTime date = ts.GetLastTime();
            for (int t = 0; t < horizon; t++) { date = date.AddDays(1); futureTimes[t] = date; }
            fcsting.FutureTimes = futureTimes;

            fcsting.SetInput(0, armaModel, null);
            fcsting.SetInput(1, ts, null);
            fcsting.Recompute();

            TimeSeries tsFcst = fcsting.GetOutput(0) as TimeSeries;
            fcst = new double[horizon];
            for (int t = 0; t < tsFcst.Count; t++) { fcst[t] = tsFcst[t]; }
            return fcst;
        }

        public override void SetModel(object model)  {
            throw new NotImplementedException();
        }

        public override object GetModel() {
            throw new NotImplementedException();
        }
        
        #endregion

    }
}

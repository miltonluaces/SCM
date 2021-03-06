#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion
namespace AibuSet {

    internal class TimeSeries : BusObject {

        #region Fields

        private Sku sku;
        private int leadtime;
        private DateTime firstDate;
        private DateTime lastDate;
        private DateTime lastLdtDate;
        private List<double> dayHist;
        private List<double> dayStock;
        private List<double> dayUnsatDmd;
        private List<double> ldtHist;
        private List<double> ldtFcst;
        internal List<double> ldtOver;
        private char separator;
        private string separatorStr;
        
        #endregion

        #region Constructors

        internal TimeSeries() : base() {
            separator = ' ';
            separatorStr = " ";
            dayHist = new List<double>();
            dayStock = new List<double>();
            dayUnsatDmd = new List<double>();
            ldtHist = new List<double>();
            ldtFcst = new List<double>();
            ldtOver = new List<double>();
        }

        internal TimeSeries(Sku sku, DateTime firstDate) : base() {
            this.sku = sku;
            this.firstDate = firstDate;
            this.dayHist = new List<double>();
        }

        internal void LoadData(List<double> values) {
            this.DayHist = values;
        }
        
        #endregion

        #region Properties

        internal Sku Sku {
            get { return sku; }
            set { sku = value; }
        }

        internal int Leadtime {
            get { return leadtime; }
            set { leadtime = value; }
        }

        internal DateTime FirstDate {
            get { return firstDate; }
            set { firstDate = value; }
        }

        internal DateTime LastDate {
            get { return lastDate; }
            set { lastDate = value; }
        }

        internal DateTime LastLdtDate {
            get { return lastLdtDate; }
            set { lastLdtDate = value; }
        }

        internal DateTime FirstFcstDate  {
            get { return LastDate.AddDays(leadtime); }
        }

        internal DateTime LastFcstDate {
            get { return FirstFcstDate.AddDays((LdtFcst.Count-1) * leadtime); }
        }

        internal List<double> DayHist {
            get { return dayHist; }
            set { dayHist = value; }
        }

        internal List<double> DayStock
        {
            get { return dayStock; }
            set { dayStock = value; }
        }

        internal List<double> DayUnsatDmd
        {
            get { return dayUnsatDmd; }
            set { dayUnsatDmd = value; }
        }

        internal List<double> LdtHist
        {
            get { return ldtHist; }
            set { ldtHist = value; }
        }

        internal List<double> LdtFcst {
            get { return ldtFcst; }
            set { ldtFcst = value; }
        }

        internal List<double> LdtOver {
            get { return ldtOver; }
            set { ldtOver = value; }
        }

        internal void AddDayHist(double dh) {
            dayHist.Add(dh);
        }

        internal void AddLtdOver(DateTime date, double value) {
            if(ldtFcst == null || ldtFcst.Count == 0) { throw new Exception("Error. No hay pronóstico"); }
            if (date < FirstFcstDate || date > LastFcstDate) { throw new Exception("Error. Fuera de rango"); }
            if (ldtOver == null) {
                double[] ldtOverArr = new double[ldtFcst.Count];
                ldtOver = new List<double>(ldtOverArr);
            }
            int index = date.Subtract(FirstFcstDate).Days;
            ldtOver[index] += value;
        }

        #endregion

        #region internal Methods
        
        #region Getters

        internal double GetDayHist(int pNumber) {
            return dayHist[pNumber];
        }

        internal double GetDayHist(DateTime date) {
            int index = date.Subtract(firstDate).Days;
            return dayHist[index];
        }

        internal char Separator { get; set; }

        #endregion

        #region Main Methods

        internal List<double> SetSpanHist(int span, bool dailySpan) {
            if (dailySpan) { return SetDailySpanHist(span); }
            else { return SetClassicSpanHist(span); }
        }
        
        internal List<double> SetClassicSpanHist(int span) {
            if (dayHist.Count == 0) { return new List<double>(); }
            double maxRatioForExtrap = 0.2;
            List<double> spanHist = new List<double>();
            int day = 0;
            double spanTot = 0.0;
            double val;
            int nonZero = 0;
            for(int i=dayHist.Count-1;i>=0;i--) {
                val = dayHist[i];
                spanTot += val;
                day++;
                if(day == span) {
                    spanHist.Insert(0,spanTot);
                    day = 0;
                    spanTot = 0;
                }
            }
            if (day > 0 && spanHist.Count > 0 && 1 / spanHist.Count > maxRatioForExtrap) {
                spanHist.Insert(0, spanTot * ((double)span / (double)(day))); 
            }
            ldtHist = spanHist;
            return spanHist;
        }

        internal List<double> SetDailySpanHist(int span) {
            List<double> spanHist = new List<double>();
            if (dayHist.Count < span) { return spanHist; }

            double spanTot = Sum(dayHist, 0, span);
            spanHist.Add(spanTot);
            for (int i = 1; i < dayHist.Count - span +1; i++) {
                spanTot = spanTot - dayHist[i - 1] + dayHist[i + span-1];
                spanHist.Add(spanTot);
            }
            return spanHist;
        }

        #endregion

        #endregion

        #region Private Methods

        private double Sum(IList<double> serie, int ini, int span) {
            double sum = 0;
            for (int i = ini; i < ini+span; i++) { sum += serie[i]; }
            return sum;
        }

        #endregion

        #region Persistence

        internal override Broker GetBroker() { return BrkrMgr.GetInstance().GetBroker((TimeSeries)this); }

        #endregion

    }
}

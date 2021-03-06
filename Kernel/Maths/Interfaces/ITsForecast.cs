namespace Maths  {

    internal interface ITsForecast {

        FcstMethodType GetFcstMethod();
        void LoadData(System.Collections.Generic.IList<double> hist, int iniIndex);
        void Calculate();
        double[] GetFcst(int horizon);
        void SetFcstRes(FcstResType fcstRes);
        FcstResType GetFcstRes();

        void LoadValid(System.Collections.Generic.IList<double> hist, int validPrd);
        double GetMetric(FcstMetric metric);

        void SetModel(object model);
        object GetModel();

    }

    public enum FcstMethodType { Naive=0, Regression=1, ZChart=2, HoltWinters=3, ARIMA=4, DLMKalman=5, NeuNet=6, NNFcsting };
    internal enum FcstResType { Ok, Warn_NoHistory, Error };
    internal enum FcstMetric { MAE, MAPE, MSE, ME, R2 };
}

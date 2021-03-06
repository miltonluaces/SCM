namespace Maths {

    internal interface IConvolution {

        void LoadData(System.Collections.Generic.List<double> data, double nConv);
        void LoadHistogram(double min, double max, double totFreqs, double range, int maxClasses, SDict<int,double> freqs, double nConv);
        double ProbabilityAcum(double x);
        double Quantile(double p);
        bool IsValid();

    }
}


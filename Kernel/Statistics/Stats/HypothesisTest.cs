#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace Statistics  {

    internal class HypoTest {

        #region Fields

        private StatFunctions stat;
        private NormalDistrib norm;
        private double significance;
        private int minCount;
        private Dictionary<string, double> AcCritValues;
        private double cutOff;


        #region Constants for normality test

        // Coefficients for P close to 0.5
        private double A0_p = 3.3871327179E+00, A1_p = 5.0434271938E+01, A2_p = 1.5929113202E+02, A3_p = 5.9109374720E+01, B1_p = 1.7895169469E+01, B2_p = 7.8757757664E+01, B3_p = 6.7187563600E+01;
        // Coefficients for P not close to 0, 0.5 or 1 (names changed to avoid conflict with Calculate)
        private double C0_p = 1.4234372777E+00, C1_p = 2.7568153900E+00, C2_p = 1.3067284816E+00, C3_p = 1.7023821103E-01, D1_p = 7.3700164250E-01, D2_p = 1.2021132975E-01;
        // Coefficients for P near 0 or 1.
        private double E0_p = 6.6579051150E+00, E1_p = 3.0812263860E+00, E2_p = 4.2868294337E-01, E3_p = 1.7337203997E-02, F1_p = 2.4197894225E-01, F2_p = 1.2258202635E-02;
        private double SPLIT1 = 0.425, SPLIT2 = 5.0, CONST1 = 0.180625, CONST2 = 1.6;

        // Constants & polynomial coefficients for alnorm(), slightly renamed to avoid conflicts.
        private double CON_a = 1.28, LTONE_a = 7.0, UTZERO_a = 18.66;
        private double P_a = 0.398942280444, Q_a = 0.39990348504, R_a = 0.398942280385, A1_a = 5.75885480458,
                A2_a = 2.62433121679, A3_a = 5.92885724438, B1_a = -29.8213557807, B2_a = 48.6959930692, C1_a = -3.8052E-8,
                C2_a = 3.98064794E-4, C3_a = -0.151679116635, C4_a = 4.8385912808, C5_a = 0.742380924027,
                C6_a = 3.99019417011, D1_a = 1.00000615302, D2_a = 1.98615381364, D3_a = 5.29330324926,
                D4_a = -15.1508972451, D5_a = 30.789933034;
        private double epsilon = 0.0001;
        #endregion


        #endregion

        #region Constructor

        internal HypoTest(double significance, int minCount)
        {
            this.significance = significance;
            this.minCount = minCount;
            this.cutOff = 0;
            stat = new StatFunctions();
            norm = new NormalDistrib();
        }

        #endregion

        #region Properties

        internal double Significance
        {
            get { return significance; }
        }

        internal double MinCount
        {
            get { return minCount; }
        }

        internal double CutOff
        {
            get { return cutOff; }
            set { cutOff = value; }
        }

        #endregion

        #region Parametric Tests

        #region Homogeneity of two sample distributions

        internal void ParametricHomogeneity(List<double> S1, IList<double> S2, ref double pMean, ref double pVar)
        {

            if (cutOff > 0)
            {
                S1 = stat.Trim((List<double>)S1, 0.05, true);
                S2 = stat.Trim((List<double>)S1, 0.05, true);
            }

            pVar = EqualVar(S1, S2);
            if (pVar < significance) { pMean = 0; return; }
            pMean = EqualMean(S1, S2);
        }

        internal double EqualVar(IList<double> S1, IList<double> S2)
        {
            double v1 = stat.Variance(S1);
            double v2 = stat.Variance(S2);
            if (S1.Count < minCount || S2.Count < minCount || Math.Abs(v1 - v2) < epsilon) { return 1.0; }
            int df1 = S1.Count - 1;
            int df2 = S2.Count - 1;
            double f;
            double p = -1;
            if (v1 > v2)
            {
                f = v1 / v2;
                if (Math.Abs(f) > 10) { return 0; }
                p = stat.FDistribution(f, df1, df2);
            }
            else
            {
                f = v2 / v1;
                if (Math.Abs(f) > 10) { return 0; }
                p = stat.FDistribution(f, df2, df1);
            }
            p = 2 * p;
            if (p > 1) { return 1; }
            return p;
        }

        internal double EqualMean(IList<double> S1, IList<double> S2)
        {
            double m1 = stat.Mean(S1);
            double m2 = stat.Mean(S2);
            double n1 = S1.Count;
            double n2 = S2.Count;

            if (n1 < minCount || n2 < minCount)
            {
                double pn = -1;
                if (m1 > m2) { pn = 1 - norm.pNorm(1 - m2 / m1, 0, m2 / m1); }
                else { pn = 1 - norm.pNorm(1 - m1 / m2, 0, m1 / m2); }
                if (pn > 1) { return 1; }
                return pn;
            }

            List<double> S = new List<double>(S1);
            S.AddRange(S2);
            double var = stat.Variance(S);

            int df = (int)(n1 + n2 - 2);
            double t = (m1 - m2) / Math.Sqrt(var * (1 / n1 + 1 / n2));
            if (t == 0) { return 1; }
            //double p = 2 * Stat.TStudent_acum (t, 0, 0, df, false);
            //double p = Stat.TStudent_acum(t, 0, 0, df, false);
            double p = stat.pt(t, df);

            if (p > 1) { return 1; }
            return p;
        }

        #endregion


        #endregion

        #region Non-Parametric Tests

        #region Homogeneity of two sample distributions

        #region Mann-Withney ranges test (means)

        internal double MannWhitney(IList<double> S1, IList<double> S2, bool twoSided)
        {
            List<Datum> data = new List<Datum>();
            foreach (double val in S1) { data.Add(new Datum(val, 1)); }
            foreach (double val in S2) { data.Add(new Datum(val, 2)); }
            double n1 = (double)S1.Count;
            double n2 = (double)S2.Count;
            return MannWhitney(data, n1, n2, twoSided);
        }

        internal double MannWhitney(List<double> sample, int index, bool twoSided)
        {
            List<Datum> data = new List<Datum>();
            for (int i = 0; i < index; i++) { data.Add(new Datum(sample[i], 1)); }
            for (int i = index; i < sample.Count; i++) { data.Add(new Datum(sample[i], 2)); }
            double n1 = (double)index;
            double n2 = (double)sample.Count - index;
            return MannWhitney(data, n1, n2, twoSided);
        }

        private double MannWhitney(List<Datum> data, double n1, double n2, bool twoSided)
        {
            if (n1 == 0 || n2 == 0) { return 1; }
            //calculate ranges and R1, with tie-breaks
            data.Sort();
            double range = 1;
            double sumTie;
            double promTie;
            double R1 = 0;
            int tie = -1;
            for (int i = 0; i < data.Count; i++)
            {
                data[i].range = range++;
                if (i > 0 && tie == -1 && data[i].value == data[i - 1].value) { tie = i - 1; }
                if (i == data.Count - 1 && tie != -1)
                {
                    sumTie = 0;
                    for (int j = tie; j <= i; j++) { sumTie += data[j].range; }
                    promTie = sumTie / (i - tie + 1);
                    for (int j = tie; j <= i; j++) { data[j].range = promTie; }
                    tie = -1;
                }
                if (tie != -1 && data[i].value != data[i - 1].value)
                {
                    sumTie = 0;
                    for (int j = tie; j < i; j++) { sumTie += data[j].range; }
                    promTie = sumTie / (i - tie);
                    for (int j = tie; j < i; j++) { data[j].range = promTie; }
                    tie = -1;
                }
            }

            //calculate R1, U statistic, Expected value and variance
            foreach (Datum d in data)
            {
                if (d.sample == 1) { R1 += d.range; }
            }
            double U = n1 * n2 + (n1 * (n1 + 1)) / 2 - R1;
            double EU = (n1 * n2) / 2.0;
            double VarU = (n1 * n2 * (n1 + n2 + 1)) / 12.0;

            //calculate z statistic and p-value
            double p = 1 - norm.pNorm(U, EU, Math.Sqrt(VarU));
            if (twoSided) { p = 2 * p; }
            if (p > 1) { p = 1; }
            return p;
        }

        #endregion

        #region Wald-Wolfowitz runs test (means, variances, simmetries)

        internal double WaldWolfowitz(IList<double> S1, IList<double> S2, bool twoSided)
        {

            //load data
            List<Datum> data = new List<Datum>();
            foreach (double val in S1) { data.Add(new Datum(val, 1)); }
            foreach (double val in S2) { data.Add(new Datum(val, 2)); }
            double n1 = (double)S1.Count;
            double n2 = (double)S2.Count;

            //calculate runs
            data.Sort();
            int R = 0;
            int sAnt = -1;
            foreach (Datum d in data)
            {
                if (d.sample != sAnt) { R++; }
                sAnt = d.sample;
            }

            //calculate mean and variance
            double ER = ((2 * n1 * n2) / (n1 + n2)) + 1;
            double VarR = (2 * n1 * n2 * (2 * n1 * n2 - n1 - n2)) / (Math.Pow((n1 + n2), 2) * (n1 + n2 - 1));
            double p = norm.pNorm(R, ER, VarR);
            if (twoSided) { p = 2 * p; }
            if (p > 1) { p = 1; }
            return p;
        }

        #endregion

        #endregion

        #region Normality of a sample

        #region Shapiro-Wilk Test

        #region internal Method

        internal double ShapiroWilk(List<double> sampleList)
        {
            if (sampleList.Count < 3) { return -1; }

            sampleList.Sort();
            double[] sample = new double[sampleList.Count + 1];
            for (int i = 1; i < sample.Length; i++) { sample[i] = sampleList[i - 1]; }

            //constants
            double[] C1 = { double.NaN, 0.0E0, 0.221157E0, -0.147981E0, -0.207119E1, 0.4434685E1, -0.2706056E1 };
            double[] C2 = { double.NaN, 0.0E0, 0.42981E-1, -0.293762E0, -0.1752461E1, 0.5682633E1, -0.3582633E1 };
            double[] C3 = { double.NaN, 0.5440E0, -0.39978E0, 0.25054E-1, -0.6714E-3 };
            double[] C4 = { double.NaN, 0.13822E1, -0.77857E0, 0.62767E-1, -0.20322E-2 };
            double[] C5 = { double.NaN, -0.15861E1, -0.31082E0, -0.83751E-1, 0.38915E-2 };
            double[] C6 = { double.NaN, -0.4803E0, -0.82676E-1, 0.30302E-2 };
            double[] C7 = { double.NaN, 0.164E0, 0.533E0 };
            double[] C8 = { double.NaN, 0.1736E0, 0.315E0 };
            double[] C9 = { double.NaN, 0.256E0, -0.635E-2 };
            double[] G = { double.NaN, -0.2273E1, 0.459E0 };
            double z90 = 0.12816E1;
            double z95 = 0.16449E1;
            double z99 = 0.23263E1;
            double zm = 0.17509E1;
            double zss = 0.56268E0;
            double bf1 = 0.8378E0;
            double xx90 = 0.556E0;
            double xx95 = 0.622E0;
            double sqrth = 0.70711E0;
            double th = 0.375E0;
            double epsilon = 1E-19;
            double pi6 = 0.1909859E1;
            double stqr = 0.1047198E1;
            bool upper = true;

            int n1 = sample.Length - 1;
            int n2 = (sample.Length - 1) / 2;
            double[] a = new double[sample.Length];
            double[] w = new double[1];
            double[] pw = new double[1];
            int result = -1;
            bool init = false;
            int n = sample.Length - 1;
            pw[0] = 1.0;
            if (w[0] >= 0.0) { w[0] = 1.0; }
            double an = n;
            result = 3;
            int nn2 = n / 2;
            if (n2 < nn2) { return pw[0]; }
            result = 1;
            if (n < 3) { return pw[0]; }

            // If init is false, calculates coefficients for the test
            if (!init)
            {
                if (n == 3)
                {
                    a[1] = sqrth;
                }
                else
                {
                    double an25 = an + 0.25;
                    double summ2 = 0.0;
                    for (int i = 1; i <= n2; ++i)
                    {
                        a[i] = ppnd((i - th) / an25);
                        summ2 += a[i] * a[i];
                    }
                    summ2 *= 2.0;
                    double ssumm2 = Math.Sqrt(summ2);
                    double rsn = 1.0 / Math.Sqrt(an);
                    double a1 = poly(C1, 6, rsn) - a[1] / ssumm2;

                    // Normalize coefficients

                    int i1;
                    double fac;
                    if (n > 5)
                    {
                        i1 = 3;
                        double a2 = -a[2] / ssumm2 + poly(C2, 6, rsn);
                        fac = Math.Sqrt((summ2 - 2.0 * a[1] * a[1] - 2.0 * a[2] * a[2]) / (1.0 - 2.0 * a1 * a1 - 2.0 * a2 * a2));
                        a[1] = a1;
                        a[2] = a2;
                    }
                    else
                    {
                        i1 = 2;
                        fac = Math.Sqrt((summ2 - 2.0 * a[1] * a[1]) / (1.0 - 2.0 * a1 * a1));
                        a[1] = a1;
                    }
                    for (int i = i1; i <= nn2; ++i) { a[i] = -a[i] / fac; }
                }
                init = true;
            }
            if (n1 < 3) { return pw[0]; }
            int ncens = n - n1;
            result = 4;
            if (ncens < 0 || (ncens > 0 && n < 20)) { return pw[0]; }
            result = 5;
            double delta = ncens / an;
            if (delta > 0.8) { return pw[0]; }

            // If W input as negative, calculate significance level of -W

            double w1, xx;
            if (w[0] < 0.0)
            {
                w1 = 1.0 + w[0];
                result = 0;
            }
            else
            {

                // Check for zero range
                result = 6;
                double range = sample[n1] - sample[1];
                if (range < epsilon) { return pw[0]; }

                // Check for correct sort order on range - scaled X
                result = 7;
                xx = sample[1] / range;
                double sx = xx;
                double sa = -a[1];
                int j = n - 1;
                for (int i = 2; i <= n1; ++i)
                {
                    double xi = sample[i] / range;
                    // IF (XX-XI .GT. epsilon) PRINT *,' ANYTHING'
                    sx += xi;
                    if (i != j) { sa += sign(1, i - j) * a[Math.Min(i, j)]; }
                    xx = xi;
                    --j;
                }
                result = 0;
                if (n > 5000) { result = 2; }

                // Calculate W statistic as squared correlation between data and coefficients
                sa /= n1;
                sx /= n1;
                double ssa = 0.0;
                double ssx = 0.0;
                double sax = 0.0;
                j = n;
                double asa;
                for (int i = 1; i <= n1; ++i)
                {
                    if (i != j) { asa = sign(1, i - j) * a[Math.Min(i, j)] - sa; } else { asa = -sa; }
                    double xsx = sample[i] / range - sx;
                    ssa += asa * asa;
                    ssx += xsx * xsx;
                    sax += asa * xsx;
                    --j;
                }

                // W1 equals (1-W) calculated to avoid excessive rounding error for W very near 1 (a potential problem in very large samples)
                double ssassx = Math.Sqrt(ssa * ssx);
                w1 = (ssassx - sax) * (ssassx + sax) / (ssa * ssx);
            }
            w[0] = 1.0 - w1;

            // Calculate significance level for W (exact for N=3)
            if (n == 3)
            {
                pw[0] = pi6 * (Math.Asin(Math.Sqrt(w[0])) - stqr);
                return pw[0];
            }
            double y = Math.Log(w1);
            xx = Math.Log(an);
            double m = 0.0;
            double s = 1.0;
            if (n <= 11)
            {
                double gamma = poly(G, 2, an);
                if (y >= gamma)
                {
                    pw[0] = epsilon;
                    return pw[0];
                }
                y = -Math.Log(gamma - y);
                m = poly(C3, 4, an);
                s = Math.Exp(poly(C4, 4, an));
            }
            else
            {
                m = poly(C5, 4, xx);
                s = Math.Exp(poly(C6, 3, xx));
            }
            if (ncens > 0)
            {

                // Censoring by proportion NCENS/N. Calculate mean and sd of normal equivalent deviate of W.
                double ld = -Math.Log(delta);
                double bf = 1.0 + xx * bf1;
                double z90f = z90 + bf * Math.Pow(poly(C7, 2, Math.Pow(xx90, xx)), ld);
                double z95f = z95 + bf * Math.Pow(poly(C8, 2, Math.Pow(xx95, xx)), ld);
                double z99f = z99 + bf * Math.Pow(poly(C9, 2, xx), ld);

                // Regress Z90F,...,Z99F on normal deviates z90,...,z99 to get pseudo-mean and pseudo-sd of z as the slope and intercept
                double zfm = (z90f + z95f + z99f) / 3.0;
                double zsd = (z90 * (z90f - zfm) + z95 * (z95f - zfm) + z99 * (z99f - zfm)) / zss;
                double zbar = zfm - zsd * zm;
                m += zbar * s;
                s *= zsd;
            }
            pw[0] = alnorm((y - m) / s, upper);
            if (result != 0 && result != 2) { return (double)result; }
            return pw[0];
        }

        #endregion

        #region Private Methods

        //Returns an int with abs value of x and sign of y 
        private int sign(int value, int sign)
        {
            int result = Math.Abs(value);
            if (sign < 0) { return -result; }
            return result;
        }

        //Produces the normal deviate Z corresponding to a given lower tail area of P
        private double ppnd(double p)
        {
            double q = p - 0.5;
            double r;
            if (Math.Abs(q) <= SPLIT1)
            {
                r = CONST1 - q * q;
                return q * (((A3_p * r + A2_p) * r + A1_p) * r + A0_p) / (((B3_p * r + B2_p) * r + B1_p) * r + 1.0);
            }
            else
            {
                if (q < 0.0) { r = p; } else { r = 1.0 - p; }
                if (r <= 0.0) { return 0.0; }

                r = Math.Sqrt(-Math.Log(r));
                double normal_dev;
                if (r <= SPLIT2)
                {
                    r -= CONST2;
                    normal_dev = (((C3_p * r + C2_p) * r + C1_p) * r + C0_p) / ((D2_p * r + D1_p) * r + 1.0);
                }
                else
                {
                    r -= SPLIT2;
                    normal_dev = (((E3_p * r + E2_p) * r + E1_p) * r + E0_p) / ((F2_p * r + F1_p) * r + 1.0);
                }
                if (q < 0.0)
                    normal_dev = -normal_dev;
                return normal_dev;
            }
        }

        //Evaluates polynom of order nord-1 with array of coefficients c. Zero order coefficient is c[1]
        private double poly(double[] c, int nord, double x)
        {
            double poly = c[1];
            if (nord == 1) { return poly; }
            double p = x * c[nord];
            if (nord != 2)
            {
                int n2 = nord - 2;
                int j = n2 + 1;
                for (int i = 1; i <= n2; ++i)
                {
                    p = (p + c[j]) * x;
                    --j;
                }
            }
            poly += p;
            return poly;
        }

        //Evaluates the tail area of the standardised normal curve from x to infinity if upper is true or from minus infinity to sample if upper is false.
        private double alnorm(double x, bool upper)
        {
            bool up = upper;
            double z = x;
            if (z < 0.0)
            {
                up = !up;
                z = -z;
            }
            double fn_val;
            if (z > LTONE_a && (!up || z > UTZERO_a))
            {
                fn_val = 0.0;
            }
            else
            {
                double y = 0.5 * z * z;
                if (z <= CON_a) { fn_val = 0.5 - z * (P_a - Q_a * y / (y + A1_a + B1_a / (y + A2_a + B2_a / (y + A3_a)))); } else { fn_val = R_a * Math.Exp(-y) / (z + C1_a + D1_a / (z + C2_a + D2_a / (z + C3_a + D3_a / (z + C4_a + D4_a / (z + C5_a + D5_a / (z + C6_a)))))); }
            }
            if (!up) { fn_val = 1.0 - fn_val; }
            return fn_val;
        }

        #endregion

        #endregion

        #endregion

        #endregion

        #region ANOVA for Nested Models

        internal bool AnovaNestedModels(IList<double> data, IList<double> redModel, IList<double> compModel, int pr, int pc, double alpha) {
            if (data.Count != redModel.Count || data.Count != compModel.Count) { throw new Exception("must have the same size as model"); }
            double sser = SSE(data, redModel);
            double ssec = SSE(data, compModel);
            int n = data.Count;
            if (sser == 0) { return false; }
            if (ssec == 0) { return true; }

            double F = ((sser - ssec) / (pc - pr)) / (ssec / (n - (pc + 1)));
            if (F < 0) { return false; }
            if (ssec < sser && ssec == 0) { return true; }
            int v1 = pc - pr;
            int v2 = n - (pc - 1);

            return stat.Test_F(F, v1, v2) > 1 - alpha;
        }

        private double SSE(IList<double> data, IList<double> model)  {
            if (data.Count != model.Count) { throw new Exception("must have the same size"); }

            double sse = 0.0;
            for (int i = 0; i < data.Count; i++)  { sse += Math.Pow(data[i] - model[i], 2);  }
            return sse;
        }

        #endregion

        #region ACF tests

        #region Durbin-Watson

        internal double GetDWDValue(IList<double> serie, IList<double> model)
        {
            List<double> res = new List<double>(serie);
            if (model != null) { for (int i = 0; i < serie.Count; i++) { res[i] -= model[i]; } }

            double diffSum = 0;
            double sum = res[0];
            for (int i = 1; i < res.Count; i++)
            {
                diffSum += Math.Pow(res[i] - res[i - 1], 2);
                sum += Math.Pow(res[i], 2);
            }
            double d = diffSum / sum;
            return d;
        }

        internal double GetDWCritValue(int n, int p, bool lower, bool signFive)
        {
            if (AcCritValues == null) { InitAutocorrelationTest(); }

            string key = "";

            //number of cases
            if (n <= 15) { key += "15-"; }
            else if (n <= 20) { key += "20-"; }
            else if (n <= 25) { key += "25-"; }
            else if (n <= 30) { key += "30-"; }
            else if (n <= 40) { key += "40-"; }
            else if (n <= 50) { key += "50-"; }
            else if (n <= 60) { key += "60-"; }
            else if (n <= 80) { key += "80-"; }
            else { key += "100-"; }

            //number of parameters
            if (p <= 2) { key += "2-"; }
            else if (p <= 3) { key += "3-"; }
            else if (p <= 4) { key += "4-"; }
            else if (p <= 5) { key += "5-"; }
            else { key += "6-"; }

            //significance 0.05 or 0.01
            if (signFive) { key += "0.05-"; }
            else { key += "0.01"; }

            //lower or upper critical value
            if (lower) { key += "l"; }
            else { key += "u"; }

            if (!AcCritValues.ContainsKey(key)) { throw new Exception("Key " + key + " not found"); }
            return AcCritValues[key];
        }

        internal int IsAutocorrelated(IList<double> serie, IList<double> model, int p, bool signFive)
        {
            double d = GetDWDValue(serie, model);
            int n = serie.Count;
            double cvl = GetDWCritValue(n, p, true, signFive);
            double cvu = GetDWCritValue(n, p, false, signFive);

            if (d < cvl) { return +1; }
            else if (d > cvu) { return -1; }
            else { return 0; }
        }

        internal void InitAutocorrelationTest()
        {
            AcCritValues = new Dictionary<string, double>();
            AcCritValues.Add("15-2-0.05-l", 1.08);
            AcCritValues.Add("15-2-0.05-u", 1.36);
            AcCritValues.Add("15-3-0.05-l", 0.95);
            AcCritValues.Add("15-3-0.05-u", 1.54);
            AcCritValues.Add("15-4-0.05-l", 0.82);
            AcCritValues.Add("15-4-0.05-u", 1.75);
            AcCritValues.Add("15-5-0.05-l", 0.69);
            AcCritValues.Add("15-5-0.05-u", 1.97);
            AcCritValues.Add("15-6-0.05-l", 0.56);
            AcCritValues.Add("15-6-0.05-u", 2.21);
            AcCritValues.Add("15-2-0.01-l", 0.81);
            AcCritValues.Add("15-2-0.01-u", 1.07);
            AcCritValues.Add("15-3-0.01-l", 0.7);
            AcCritValues.Add("15-3-0.01-u", 1.25);
            AcCritValues.Add("15-4-0.01-l", 0.59);
            AcCritValues.Add("15-4-0.01-u", 1.46);
            AcCritValues.Add("15-5-0.01-l", 0.49);
            AcCritValues.Add("15-5-0.01-u", 1.7);
            AcCritValues.Add("15-6-0.01-l", 0.39);
            AcCritValues.Add("15-6-0.01-u", 1.96);

            AcCritValues.Add("20-2-0.05-l", 1.2);
            AcCritValues.Add("20-2-0.05-u", 1.41);
            AcCritValues.Add("20-3-0.05-l", 1.1);
            AcCritValues.Add("20-3-0.05-u", 1.54);
            AcCritValues.Add("20-4-0.05-l", 1.0);
            AcCritValues.Add("20-4-0.05-u", 1.68);
            AcCritValues.Add("20-5-0.05-l", 0.9);
            AcCritValues.Add("20-5-0.05-u", 1.83);
            AcCritValues.Add("20-6-0.05-l", 0.79);
            AcCritValues.Add("20-6-0.05-u", 1.99);
            AcCritValues.Add("20-2-0.01-l", 0.95);
            AcCritValues.Add("20-2-0.01-u", 1.15);
            AcCritValues.Add("20-3-0.01-l", 0.86);
            AcCritValues.Add("20-3-0.01-u", 1.27);
            AcCritValues.Add("20-4-0.01-l", 0.77);
            AcCritValues.Add("20-4-0.01-u", 1.41);
            AcCritValues.Add("20-5-0.01-l", 0.68);
            AcCritValues.Add("20-5-0.01-u", 1.57);
            AcCritValues.Add("20-6-0.01-l", 0.6);
            AcCritValues.Add("20-6-0.01-u", 1.74);

            AcCritValues.Add("25-2-0.05-l", 1.29);
            AcCritValues.Add("25-2-0.05-u", 1.45);
            AcCritValues.Add("25-3-0.05-l", 1.21);
            AcCritValues.Add("25-3-0.05-u", 1.55);
            AcCritValues.Add("25-4-0.05-l", 1.12);
            AcCritValues.Add("25-4-0.05-u", 1.66);
            AcCritValues.Add("25-5-0.05-l", 1.04);
            AcCritValues.Add("25-5-0.05-u", 1.77);
            AcCritValues.Add("25-6-0.05-l", 0.95);
            AcCritValues.Add("25-6-0.05-u", 1.89);
            AcCritValues.Add("25-2-0.01-l", 1.05);
            AcCritValues.Add("25-2-0.01-u", 1.21);
            AcCritValues.Add("25-3-0.01-l", 0.98);
            AcCritValues.Add("25-3-0.01-u", 1.3);
            AcCritValues.Add("25-4-0.01-l", 0.9);
            AcCritValues.Add("25-4-0.01-u", 1.41);
            AcCritValues.Add("25-5-0.01-l", 0.83);
            AcCritValues.Add("25-5-0.01-u", 1.52);
            AcCritValues.Add("25-6-0.01-l", 0.75);
            AcCritValues.Add("25-6-0.01-u", 1.65);

            AcCritValues.Add("30-2-0.05-l", 1.35);
            AcCritValues.Add("30-2-0.05-u", 1.49);
            AcCritValues.Add("30-3-0.05-l", 1.28);
            AcCritValues.Add("30-3-0.05-u", 1.57);
            AcCritValues.Add("30-4-0.05-l", 1.21);
            AcCritValues.Add("30-4-0.05-u", 1.65);
            AcCritValues.Add("30-5-0.05-l", 1.14);
            AcCritValues.Add("30-5-0.05-u", 1.74);
            AcCritValues.Add("30-6-0.05-l", 1.07);
            AcCritValues.Add("30-6-0.05-u", 1.83);
            AcCritValues.Add("30-2-0.01-l", 1.13);
            AcCritValues.Add("30-2-0.01-u", 1.26);
            AcCritValues.Add("30-3-0.01-l", 1.07);
            AcCritValues.Add("30-3-0.01-u", 1.34);
            AcCritValues.Add("30-4-0.01-l", 1.01);
            AcCritValues.Add("30-4-0.01-u", 1.42);
            AcCritValues.Add("30-5-0.01-l", 1.94);
            AcCritValues.Add("30-5-0.01-u", 1.51);
            AcCritValues.Add("30-6-0.01-l", 1.88);
            AcCritValues.Add("30-6-0.01-u", 1.61);

            AcCritValues.Add("40-2-0.05-l", 1.44);
            AcCritValues.Add("40-2-0.05-u", 1.54);
            AcCritValues.Add("40-3-0.05-l", 1.39);
            AcCritValues.Add("40-3-0.05-u", 1.60);
            AcCritValues.Add("40-4-0.05-l", 1.34);
            AcCritValues.Add("40-4-0.05-u", 1.66);
            AcCritValues.Add("40-5-0.05-l", 1.39);
            AcCritValues.Add("40-5-0.05-u", 1.72);
            AcCritValues.Add("40-6-0.05-l", 1.23);
            AcCritValues.Add("40-6-0.05-u", 1.79);
            AcCritValues.Add("40-2-0.01-l", 1.25);
            AcCritValues.Add("40-2-0.01-u", 1.34);
            AcCritValues.Add("40-3-0.01-l", 1.20);
            AcCritValues.Add("40-3-0.01-u", 1.40);
            AcCritValues.Add("40-4-0.01-l", 1.15);
            AcCritValues.Add("40-4-0.01-u", 1.46);
            AcCritValues.Add("40-5-0.01-l", 1.40);
            AcCritValues.Add("40-5-0.01-u", 1.52);
            AcCritValues.Add("40-6-0.01-l", 1.05);
            AcCritValues.Add("40-6-0.01-u", 1.58);

            AcCritValues.Add("50-2-0.05-l", 1.50);
            AcCritValues.Add("50-2-0.05-u", 1.59);
            AcCritValues.Add("50-3-0.05-l", 1.46);
            AcCritValues.Add("50-3-0.05-u", 1.63);
            AcCritValues.Add("50-4-0.05-l", 1.42);
            AcCritValues.Add("50-4-0.05-u", 1.67);
            AcCritValues.Add("50-5-0.05-l", 1.38);
            AcCritValues.Add("50-5-0.05-u", 1.72);
            AcCritValues.Add("50-6-0.05-l", 1.34);
            AcCritValues.Add("50-6-0.05-u", 1.77);
            AcCritValues.Add("50-2-0.01-l", 1.32);
            AcCritValues.Add("50-2-0.01-u", 1.40);
            AcCritValues.Add("50-3-0.01-l", 1.28);
            AcCritValues.Add("50-3-0.01-u", 1.45);
            AcCritValues.Add("50-4-0.01-l", 1.24);
            AcCritValues.Add("50-4-0.01-u", 1.49);
            AcCritValues.Add("50-5-0.01-l", 1.20);
            AcCritValues.Add("50-5-0.01-u", 1.54);
            AcCritValues.Add("50-6-0.01-l", 1.16);
            AcCritValues.Add("50-6-0.01-u", 1.59);

            AcCritValues.Add("60-2-0.05-l", 1.55);
            AcCritValues.Add("60-2-0.05-u", 1.62);
            AcCritValues.Add("60-3-0.05-l", 1.51);
            AcCritValues.Add("60-3-0.05-u", 1.65);
            AcCritValues.Add("60-4-0.05-l", 1.48);
            AcCritValues.Add("60-4-0.05-u", 1.69);
            AcCritValues.Add("60-5-0.05-l", 1.44);
            AcCritValues.Add("60-5-0.05-u", 1.73);
            AcCritValues.Add("60-6-0.05-l", 1.41);
            AcCritValues.Add("60-6-0.05-u", 1.77);
            AcCritValues.Add("60-2-0.01-l", 1.38);
            AcCritValues.Add("60-2-0.01-u", 1.45);
            AcCritValues.Add("60-3-0.01-l", 1.35);
            AcCritValues.Add("60-3-0.01-u", 1.48);
            AcCritValues.Add("60-4-0.01-l", 1.32);
            AcCritValues.Add("60-4-0.01-u", 1.52);
            AcCritValues.Add("60-5-0.01-l", 1.28);
            AcCritValues.Add("60-5-0.01-u", 1.56);
            AcCritValues.Add("60-6-0.01-l", 1.25);
            AcCritValues.Add("60-6-0.01-u", 1.60);

            AcCritValues.Add("80-2-0.05-l", 1.61);
            AcCritValues.Add("80-2-0.05-u", 1.66);
            AcCritValues.Add("80-3-0.05-l", 1.59);
            AcCritValues.Add("80-3-0.05-u", 1.69);
            AcCritValues.Add("80-4-0.05-l", 1.56);
            AcCritValues.Add("80-4-0.05-u", 1.72);
            AcCritValues.Add("80-5-0.05-l", 1.53);
            AcCritValues.Add("80-5-0.05-u", 1.74);
            AcCritValues.Add("80-6-0.05-l", 1.51);
            AcCritValues.Add("80-6-0.05-u", 1.77);
            AcCritValues.Add("80-2-0.01-l", 1.47);
            AcCritValues.Add("80-2-0.01-u", 1.52);
            AcCritValues.Add("80-3-0.01-l", 1.44);
            AcCritValues.Add("80-3-0.01-u", 1.54);
            AcCritValues.Add("80-4-0.01-l", 1.42);
            AcCritValues.Add("80-4-0.01-u", 1.57);
            AcCritValues.Add("80-5-0.01-l", 1.39);
            AcCritValues.Add("80-5-0.01-u", 1.60);
            AcCritValues.Add("80-6-0.01-l", 1.36);
            AcCritValues.Add("80-6-0.01-u", 1.62);

            AcCritValues.Add("100-2-0.05-l", 1.65);
            AcCritValues.Add("100-2-0.05-u", 1.69);
            AcCritValues.Add("100-3-0.05-l", 1.63);
            AcCritValues.Add("100-3-0.05-u", 1.72);
            AcCritValues.Add("100-4-0.05-l", 1.61);
            AcCritValues.Add("100-4-0.05-u", 1.74);
            AcCritValues.Add("100-5-0.05-l", 1.59);
            AcCritValues.Add("100-5-0.05-u", 1.76);
            AcCritValues.Add("100-6-0.05-l", 1.57);
            AcCritValues.Add("100-6-0.05-u", 1.78);
            AcCritValues.Add("100-2-0.01-l", 1.52);
            AcCritValues.Add("100-2-0.01-u", 1.56);
            AcCritValues.Add("100-3-0.01-l", 1.50);
            AcCritValues.Add("100-3-0.01-u", 1.58);
            AcCritValues.Add("100-4-0.01-l", 1.48);
            AcCritValues.Add("100-4-0.01-u", 1.60);
            AcCritValues.Add("100-5-0.01-l", 1.46);
            AcCritValues.Add("100-5-0.01-u", 1.63);
            AcCritValues.Add("100-6-0.01-l", 1.44);
            AcCritValues.Add("100-6-0.01-u", 1.65);
        }

        #endregion

        #region White noise

        internal double GetWNCritValue(int n, double alpha)
        {
            return norm.qNorm(1.0 - alpha / 2.0, 0, 1) / Math.Sqrt((double)n);
        }

        internal bool IsWhiteNoise(IList<double> acfs, double alpha)
        {
            int n = acfs.Count;
            double cv = GetWNCritValue(n, alpha);
            double cvMax = GetWNCritValue(n, 0.01);
            double ac;
            double a = 0;
            for (int i = 1; i < acfs.Count; i++)
            {
                ac = Math.Abs(acfs[i]);
                if (ac > cvMax) { return false; }
                if (ac > cv) { a++; }
            }
            if (a / (double)n > alpha) { return false; }
            return true;
        }

        internal int TestMovingAverage(IList<double> acfs, double alpha)
        {
            int n = acfs.Count;
            double cv = GetWNCritValue(n, alpha);
            double ac;
            for (int i = n - 1; i > 0; i--)
            {
                ac = Math.Abs(acfs[i]);
                if (ac > cv) { return i + 1; }
                if (i == 0) { return i; }
            }
            return -1;
        }

        internal int TestAutorregresive(IList<double> acfs, double alpha)
        {
            List<double> pacfs = stat.PACF(acfs);
            int n = pacfs.Count;
            double cv = GetWNCritValue(n, alpha);
            double ac;
            for (int i = n - 1; i > 0; i++)
            {
                ac = Math.Abs(pacfs[i]);
                if (ac > cv) { return i + 1; }
                if (i == 0) { return i; }
            }
            return -1;
        }

        #endregion

        #endregion

        #region Variance stabilization

        internal double GetBoxCoxLambda(List<double> serie, double lambdaInc, ref double maxLik)
        {
            maxLik = double.MinValue;
            double maxLambda = -1;
            double lik;
            List<double> transf;
            double lambda = -3;
            while (lambda <= 3)
            {
                transf = GetBoxCoxTransf(serie, lambda);
                lik = Likelihood(transf, lambda);
                if (lik > maxLik)
                {
                    maxLik = lik;
                    maxLambda = lambda;
                }
                lambda = Math.Round(lambda + lambdaInc, 2);
            }
            return maxLambda;

        }

        internal List<double> GetBoxCoxTransf(List<double> serie, double lambda)
        {
            List<double> transf = new List<double>();
            foreach (double x in serie) { transf.Add(BoxCoxTransf(x, lambda)); }
            return transf;
        }

        private double Likelihood(List<double> serie, double lambda)
        {
            return this.ShapiroWilk(serie);
        }

        internal double BoxCoxTransf(double x, double lambda)
        {
            if (lambda == 0) { return Math.Log(x); }
            else { return (Math.Pow(x, lambda) - 1) / lambda; }
        }

        internal double BoxCoxTransfInv(double y, double lambda)
        {
            if (lambda == 0) { return Math.Exp(y); }
            else { return Math.Pow(lambda * y + 1, 1 / lambda); }
        }

        #endregion

    }

    #region Class Datum

    internal class Datum : IComparable
    {

        internal double value;
        internal int sample;
        internal double range;

        internal Datum(double value, int sample)
        {
            this.value = value;
            this.sample = sample;
        }

        #region IComparable Members

        int IComparable.CompareTo(object obj)
        {
            if (this.value < ((Datum)obj).value) { return -1; }
            else if (this.value > ((Datum)obj).value) { return 1; }
            else { return 0; }
        }

        #endregion
    }

    #endregion

    #region Class Crit

    internal class Crit
    {
        internal string key;
        internal double low;
        internal double high;
        internal Crit(double low, double high)
        {
            this.low = low;
            this.high = high;
        }
    }

    #endregion

}

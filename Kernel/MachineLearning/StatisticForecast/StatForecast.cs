#region Imports

using System;
using System.Collections;
using MonteCarlo;
using Statistics;
using Maths;
using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace MachineLearning {

    internal class StatForecast {

        #region Fields
        
        private Functions func;

        
        private int FirstPN;
        private int LastPN;
        private int nfutureobservations;
        private int fpntoforecast;

        private int bruteforcemaxite;

        private int polGrade;
        private double perConfArm;
      
        private ObsValue ov;
        private double[][] rv;

        private double phighsusexp;
        private double Adaptability = 95.0;
        private int degenerationshor;
        private Frequencies dataFrequencies = null;
        private List<double> outliers;

        private double[,] initialmean;
        private double[,] initialvar;
        private double inilearnvar_S;
        private double inilearnvar_n;
        private Dictionary<int, ModelValue> model_fcst;
        private double[,] mean;
        private double[,] variance;
        private double learnvar_S;
        private double learnvar_n;
        private OutlierDetection outlierDetection;

        private double[][] df;
        private CandidatePars bestPPLT;
        private CandidatePars bestPPST;
        private int alterDfSearchLevel;

        private int NObsForShortTermFcstError;
        private int ShortTermFcstHorizon;
        private double stablevel = 1;
        private double stderrlevel;
        private double ltincradix;
        private double stincradix;
        private double ltrangeradix;
        private double strangeradix;
        private double fcstLumpyDmdThreshold = 1.0;

        private DLModel structure;

        private TSeries ots;
        private double[] period_length;

        private StatFunctions stat;

        #endregion

        #region Constructors

        internal StatForecast() {
            this.func = new Functions();
            this.nfutureobservations = 12;
            this.fpntoforecast = -1;
            this.polGrade = 2;
            this.fpntoforecast = -1;
            this.structure = new DLModel(this);
            this.ots = new TSeries(this);
            this.CreateDF();
            this.period_length = new double[1];
            this.outlierDetection = new OutlierDetection();
            this.stat = new StatFunctions();
        }

        internal StatForecast(int DaysPerYear)
            : this() {
            this.outlierDetection = new OutlierDetection();
        }

        #endregion
   
        #region Properties

        internal double[][] RV {
            get { return rv; }
            set { rv = value; }
        }

        internal struct RegValue {
            internal double Value;
            internal double CValue;
            internal int PNumber;
        }

        internal int Pol_Grade {
            get { return polGrade; }
            set { polGrade = value; }
        }

        #endregion

        #region Internal Methods

        internal int GetNPeriodsToAverage() {
            return (this.ots.GetNPeriodsToAverage());
        }

        internal bool ExistsInHistPN(int PNumber) {
            return (this.ots.ExistsInHistPN(PNumber));
        }

        internal ObsValue GetTS(int PNumber) {
            return (this.ots.GetTS(PNumber));
        }

        internal double[] GetTS(bool Normalised) {
            return (this.ots.GetTSValues(Normalised));
        }

        internal double[] GetObs(bool Normalised) {
            if (this.ots.Obs == null) this.ots.SetObs();
            return (this.ots.GetObsValues(Normalised));
        }

        internal int GetFirstObsIndex() {
            return (this.ots.GetFirstObsIndex());
        }

        internal int GetLastObsIndex() {
            return (this.ots.GetLastObsIndex());
        }

        internal int GetFirstObsPN() {
            return (this.ots.GetFirstObsPN());
        }

        internal int GetLastObsPN() {
            return (this.ots.GetLastObsPN());
        }

        internal int GetObsPN(int Index) {
            return (this.ots.GetObsPN(Index));
        }

        internal int GetObsIndex(int PN) {
            return (this.ots.GetObsIndex(PN));
        }

        internal void SetDegenerationsHor(int v) {
            this.degenerationshor = v;
        }

        internal int GetDegenerationsHor() {
            return this.degenerationshor;
        }

        internal void SetBruteForceMaxIte(int v) {
            this.bruteforcemaxite = v;
        }

        internal void SetTSValue(int PNumber, double Val, double PLength, ObsType ObsType) {
            int fpn = this.GetFirstPNumber();
            StatForecast.ObsValue V;

            if ((fpn < (int)0) ||
                (PNumber > fpn + this.ots.GetNHistPeriods() + 1))
                throw new ArgumentException("Cannot_Set_Period_Number" + PNumber);
            else {
                V = this.CreateObsValue(Val, PLength, PNumber, ObsType);
                if (this.ots.TS.ContainsKey(PNumber))
                    this.ots.TS[PNumber] = V;
                else {
                    this.ots.TS.Add(PNumber, V);
                    if (PLength != 0) this.ots.Obs.Add(PNumber);
                }
                if ((PNumber > this.LastPN) && (ObsType != ObsType._MissingObs)) LastPN = PNumber;
            }
        }

        internal void SetTSValue(int PNumber, ObsValue v) {
            this.ov = v;
            int fpn = this.GetFirstPNumber();

            if ((fpn < (int)0) ||
                (PNumber > fpn + this.ots.GetNHistPeriods() + 1))
                throw new ArgumentException("Cannot_Set_Period_Number" + PNumber);
            else {
                if (this.ots.TS.ContainsKey(v.PNumber))
                    this.ots.TS[PNumber] = v;
                else {
                    this.ots.TS.Add(PNumber, v);
                    if (v.PLength != 0) this.ots.Obs.Add(v.PNumber);
                }
                if ((v.PNumber > this.LastPN) && (v.ObsType != ObsType._MissingObs)) LastPN = v.PNumber;
            }
        }

        internal StatForecast.ObsValue CreateObsValue(double ObsValue, double PeriodLength, int PN, ObsType Type) {
            StatForecast.ObsValue M;

            M.NNValue = ObsValue;
            M.NValue = ObsValue / PeriodLength;
            M.PLength = PeriodLength;
            M.PNumber = PN;
            M.ObsType = Type;
            M.Value01 = -1.0;
            return M;
        }

        internal StatForecast.ObsValue CreateObsValue(double ObsValue, double PeriodLength, int PN, ObsType Type, double Value01) {
            StatForecast.ObsValue M;

            M.NNValue = ObsValue;
            M.NValue = ObsValue / PeriodLength;
            M.PLength = PeriodLength;
            M.PNumber = PN;
            M.ObsType = Type;
            M.Value01 = Value01;
            return M;
        }

        internal void SetTSNNValue(int PNumber, double v) {
            this.ots.SetTSNNValue(PNumber, v);
        }

        internal void SetTSNValue(int PNumber, double v) {
            this.ots.SetTSNValue(PNumber, v);
        }

        internal void SetTSPLength(int PNumber, double v) {
            this.ots.SetTSPLength(PNumber, v);
        }

        internal void SetTSType(int PNumber, ObsType v) {
            this.ots.SetTSType(PNumber, v);
        }

        internal bool IsValidObservation(int PNumber) {
            return (this.ots.GetTS(PNumber).ObsType == ObsType._Usable);
        }

        internal bool ExistsObservation(int PNumber) {
            return (this.ots.TS.ContainsKey(PNumber));
        }

        internal bool IsOutlier(int PNumber) {
            return (this.ots.GetTS(PNumber).ObsType == ObsType._SystemDef);
        }

        internal bool IsMissing(int PNumber) {
            return (this.ots.GetTS(PNumber).ObsType == ObsType._MissingObs);
        }

        internal bool IsOutlierOrMasked(int PNumber) {
            if (!this.ots.ExistsInHistPN(PNumber)) return false;
            return (((this.ots.GetTS(PNumber).ObsType == ObsType._SystemDef) ||
                     (this.ots.GetTS(PNumber).ObsType == ObsType._UserDef)));
        }

        internal bool IsMaskedObservation(int PNumber) {
            return (this.ots.GetTS(PNumber).ObsType == ObsType._UserDef);
        }

        internal bool IsDegradedObservation(int PNumber) {
            return (this.ots.GetTS(PNumber).ObsType == ObsType._Degradation);
        }

        internal DLModel GetModelStructure() {
            return (this.structure);
        }

        internal void SetModelStructure(DLModel structure) {
            this.structure = structure;
        }

        internal ArrayList GetParamsListActive() {
            return (this.structure.GetParamsListActive());
        }

        internal int GetNActiveParams() {
            return (this.structure.GetNActiveParams());
        }

        internal DLMParameter GetParameter(CandidatePars PP, string Name, int Property) {
            return (PP.GetStructure().GetParam(Name, Property));
        }

        internal int GetParamNumber(DLMParameter p) {
            return p.GetParamNumber();
        }

        internal string GetParamName(DLMParameter p) {
            return p.GetParamName();
        }

        internal int GetParamArmonicNumber(DLMParameter p) {
            return p.GetArmonicNumber();
        }

        internal int GetParamLag(DLMParameter p) {
            return p.GetLag();
        }

        internal CompType GetParamType(DLMParameter p) {
            return p.GetParamType();
        }

        internal double GetParamMeanValue(DLMParameter p) {
            return (p.GetParamMean());
        }

        internal double[] GetParamVarValue(DLMParameter p, ref bool Active) {
            double[] Var = new double[this.structure.GetNActiveParams()];

            Active = p.IsActive();
            int PNumber = p.GetParamNumber();
            for (int c = 0; c < MatrixOp.GetCols(this.variance); c++)
                Var[c] = this.variance[PNumber, c];
            return (Var);
        }


        internal int GetPolynomialGrade() {
            return polGrade;
        }

        internal double GetSTPolynomialLevel() {
            return this.GetBestPPST().GetStructure().GetParam("Level", 0).GetParamMean();
        }

        internal double GetLTPolynomialLevel() {
            return this.GetBestPPLT().GetStructure().GetParam("Level", 0).GetParamMean();
        }

        internal void SetPolinomialGrade(int v) {
            if ((v == 1) || (v == 2))
                polGrade = v;
            else
                throw new ArgumentException("This_version_of_the_software_does_just_manage_polinomial_components_of_grade_1_or_2");
        }

        internal double GetSeasonalityConfidence() {
            return (this.perConfArm);
        }

        internal void SetSeasonalityConfidence(double v) {
            if ((v >= 0) || (v <= 100))
                this.perConfArm = v;
            else
                throw new ArgumentException("Seasonality_Confidence_is_a____so_it_has_to_be_a_value_between_0_and_100__Value_being_set_is" + v);
        }

        internal int GetNModelArmonics() {
            return (this.structure.GetNActiveArmonics());
        }

        internal void SetAdaptability(double val) {
            this.Adaptability = val;
        }

        internal double GetAdaptability() {
            return Adaptability;
        }

        internal void SetFcstLumpyDmdThreshold(double val) {
            fcstLumpyDmdThreshold = val;
        }

        internal double GetFcstLumpyDmdThreshold() {
            return fcstLumpyDmdThreshold;
        }

        internal double GetOutlierProbabilityThreshold() {
            return (this.phighsusexp);
        }

        internal void SetOutlierProbabilityThreshold(double v) {
            if ((v >= 90.0) && (v <= 100.0))
                this.phighsusexp = v;
            else
                throw new ArgumentException("Outlier_Probability_threshold_is_a____so_it_has_to_be_a_value_between_90_and_100__Value_being_set_is" + v);
        }

        internal int GetNOutliers() {
            int NOutliers = 0;

            if (this.ots == null) return 0; // if the ots object has not been created yet, return 0

            foreach (int key in this.ots.TS.Keys)
                if (this.ots.TS[key].ObsType == ObsType._SystemDef)
                    NOutliers++;
            return (NOutliers);
        }

        internal int GetNOutliers(int PNumber) {
            return this.ots.GetNOutliers(PNumber);
        }

        internal void SetBestPPST(CandidatePars pp) {
            this.bestPPST = pp;
        }

        internal CandidatePars GetBestPPST() {
            return this.bestPPST;
        }

        internal void SetBestPPLT(CandidatePars pp) {
            this.bestPPLT = pp;
        }

        internal CandidatePars GetBestPPLT() {
            return this.bestPPLT;
        }

        internal int GetForecastingHorizon() {
            return (this.nfutureobservations);
        }

        internal int GetFirstPNumber() {
            return (this.FirstPN);
        }

        internal int GetLastPNumber() {
            if (this.ots.Obs != null && this.ots.Obs.Count > 0) return this.ots.Obs[this.ots.Obs.Count - 1];
            return LastPN;
        }

        internal int GetLastHistPNumber() {
            if (this.ots.TS != null && this.ots.TS.Count > 0) return (this.ots.LastHistPN()); ;
            return LastPN;
        }

        internal void SetLastPNumber(int LastPN) {
            this.LastPN = LastPN;
        }

        internal int GetFPNtoForecast() {
            return (this.fpntoforecast);
        }

        internal int GetNValidObservations() {
            int FirstPN = (this.fpntoforecast > this.FirstPN ? this.fpntoforecast : this.GetFirstPNumber());
            int NPeriods = 0;

            foreach (int key in this.ots.TS.Keys)
                if ((key >= FirstPN) && (this.ots.TS[key].ObsType != ObsType._Usable))
                    NPeriods++;
            return (NPeriods);
        }

        internal int GetNObservations() {
            if (this.ots != null)
                return (this.ots.TS.Count);
            else
                return 0;
        }

        internal void SetFPNtoForecast(int v) {
            if ((v >= this.GetFirstPNumber()) || (v <= this.GetLastPNumber()))
                this.fpntoforecast = v;
            else
                throw new ArgumentException("The_First_Period_Number_to_forecast_has_to_be_a_number_between_0_and_1_Value_being_set_is_2");

        }

        internal void SetPeriodLength(int LastPNumber, int PNumber, double Val) {
            if (Val < 0)
                throw new ArgumentException("Period_Length_0_Period_Number_1_Period_length_cannot_be_a_0_or_a_negative_number");

            if (PNumber <= LastPNumber)
                throw new ArgumentException("Cannot_Set_Period_Length_for_Period_Number");
            else {
                if ((PNumber - LastPNumber - 1 < 0) || (PNumber - LastPNumber - 1 >= this.nfutureobservations))
                    throw new ArgumentException("Cannot_Set_Period_Length_for_Period_Number_That_Period_Number_does_not_belong_Forecas_Horizon");
                else
                    this.period_length[PNumber - LastPNumber - 1] = Val;
            }
        }

        internal void SetPeriodMasked(int PNumber) {
            if (this.ots.TS.ContainsKey(PNumber))
                this.ots.SetTSType(PNumber, ObsType._UserDef);
            else
                throw new ArgumentException("There_is_no_Period_Number_0_on_history");
        }

        internal void SetPeriodUnMasked(int PNumber) {
            if (this.ots.TS.ContainsKey(PNumber))
                this.ots.SetTSType(PNumber, ObsType._Usable);
            else
                throw new ArgumentException("The_Period_Number_to_UNmask_has_to_be_a_number_between_0_and_1_Value_being_set_is_2");
        }

        internal double[,] GetInitialMean() {
            return (this.initialmean);
        }

        internal void SetInitialMean(double[,] v) {
            this.initialmean = v;
        }

        internal double[,] GetInitialVar() {
            return (this.initialvar);
        }

        internal void SetInitialVar(double[,] v) {
            this.initialvar = v;
        }

        internal Hashtable GetHistDistributionMean(DistType distType) {
            switch (distType) {
                case DistType._ST:
                    //TODO: short term model history
                    Hashtable H = new Hashtable(this.GetNObservations());
                    for (int PN = this.fpntoforecast; PN <= this.GetLastHistPNumber(); PN++)
                        if (!this.IsMissing(PN))
                            H.Add(PN, this.GetBestPPST().GetModelValue(PN).NNMean);
                        else
                            H.Add(PN, 0.0);

                    return H;
                case DistType._LT:
                    //TODO: long term model history
                    H = new Hashtable(this.GetNObservations());
                    for (int PN = this.fpntoforecast; PN <= this.GetLastHistPNumber(); PN++)
                        if (!this.IsMissing(PN))
                            H.Add(PN, this.GetBestPPLT().GetModelValue(PN).NNMean);
                        else
                            H.Add(PN, 0.0);

                    return H;
                default:
                    throw new Exception("Dist_option_is_not_valid");
            }
        }

        internal Hashtable GetHistDistributionVar(DistType distType) {
            switch (distType) {
                case DistType._ST:
                    Hashtable H = new Hashtable(this.GetNObservations());
                    for (int PN = this.fpntoforecast; PN <= this.GetLastHistPNumber(); PN++)
                        if (!this.IsMissing(PN))
                            H.Add(PN, this.GetBestPPST().GetModelValue(PN).NNVar);
                        else
                            H.Add(PN, 0.0);
                    return H;

                case DistType._LT:
                    H = new Hashtable(this.GetNObservations());
                    for (int PN = this.fpntoforecast; PN <= this.GetLastHistPNumber(); PN++)
                        if (!this.IsMissing(PN))
                            H.Add(PN, this.GetBestPPLT().GetModelValue(PN).NNVar);
                        else
                            H.Add(PN, 0.0);
                    return H;

                default:
                    throw new Exception("Dist_option_is_not_valid");
            }
        }

        internal double GetHistDistributionVarForPeriod(int PN, DistType distType) {
            if (!this.IsMissing(PN))
                switch (distType) {
                    case DistType._ST:
                        return this.GetBestPPST().GetModelValue(PN).NNVar;
                    case DistType._LT:
                        return this.GetBestPPLT().GetModelValue(PN).NNVar;
                    default:
                        throw new Exception("Dist_option_is_not_valid");
                }
            else return 0.0;
        }

        internal Hashtable GetHistDistributionDOF(DistType distType) {
            switch (distType) {
                case DistType._ST:
                    Hashtable H = new Hashtable(this.GetNObservations());
                    for (int PN = this.fpntoforecast; PN <= this.GetLastHistPNumber(); PN++)
                        if (!this.IsMissing(PN))
                            H.Add(PN, this.GetBestPPST().GetModelValue(PN).dof);
                        else
                            H.Add(PN, 0.0);
                    return H;

                case DistType._LT:
                    H = new Hashtable(this.GetNObservations());
                    for (int PN = this.fpntoforecast; PN <= this.GetLastHistPNumber(); PN++)
                        if (!this.IsMissing(PN))
                            H.Add(PN, this.GetBestPPLT().GetModelValue(PN).dof);
                        else
                            H.Add(PN, 0.0);
                    return H;

                default:
                    throw new Exception("Dist_option_is_not_valid");
            }
        }

        internal double GetHistDistributionDOFForPeriod(int PN, DistType distType) {
            if (!this.IsMissing(PN))
                switch (distType) {
                    case DistType._ST:
                        return this.GetBestPPST().GetModelValue(PN).dof;
                    case DistType._LT:
                        return this.GetBestPPLT().GetModelValue(PN).dof;
                    default:
                        throw new Exception("Dist_option_is_not_valid");
                }
            else return 0.0;
        }

        internal Hashtable GetHistConfidenceDelta(double v, DistType distType) {
            Hashtable H = new Hashtable(this.GetNObservations());
            double halfalpha = 0.5 - (v / 200.0);

            for (int PN = this.fpntoforecast; PN <= this.GetLastHistPNumber(); PN++)
                H.Add(PN, this.GetHistDistributionVarForPeriod(PN, distType) * stat.TStudent_quantil(halfalpha, this.GetHistDistributionDOFForPeriod(PN, distType), true));
            return (H);
        }

        internal double GetHistConfidenceFactor(int PNumber, double v, DistType distType) {
            return stat.TStudent_quantil(0.5 - (v / 200.0), this.GetHistDistributionDOFForPeriod(PNumber, distType), true);
        }

        internal double GetFcstDistributionMeanForPeriod(int PNumber, bool Normalised) {
            int LastPNumber = this.GetLastHistPNumber();
            if (PNumber <= LastPNumber)
                throw new ArgumentException("Cannot_Get_Forecast_For_Period_Number" + PNumber);
            else
                return (Normalised ?
                    Math.Max(this.model_fcst[PNumber].NMean, 0.0) :
                    Math.Max(this.model_fcst[PNumber].NNMean, 0.0));
        }

        internal double[] GetFcstDistributionMean(bool Normalised) {
            int FirstFcstPN = this.GetLastHistPNumber() + 1;
            double[] F = new double[this.nfutureobservations];

            if (Normalised)
                for (int TimeIndex = 0; TimeIndex < this.nfutureobservations; TimeIndex++)
                    F[TimeIndex] = Math.Max(this.model_fcst[FirstFcstPN + TimeIndex].NMean, 0.0);
            else
                for (int TimeIndex = 0; TimeIndex < this.nfutureobservations; TimeIndex++)
                    F[TimeIndex] = Math.Max(this.model_fcst[FirstFcstPN + TimeIndex].NNMean, 0.0);
            return F;
        }

        internal List<double> GetNMeanFcst(CandidatePars pp) {
            return pp.GetNMeanFcst();
        }

        internal void SetNMeanFcst(CandidatePars pp, List<double> Values) {
            int FirstFcstPN = this.GetLastHistPNumber() + 1, PN;
            ModelValue M;
            if (pp.GetModelFcst() == null) pp.InitModelFcst();
            for (int TimeIndex = 0; TimeIndex < this.nfutureobservations; TimeIndex++) {
                PN = FirstFcstPN + TimeIndex;
                if (pp.GetModelFcst().ContainsKey(PN)) {
                    M = pp.GetModelFcst()[PN];
                    M.NMean = Values[TimeIndex];
                    M.NNMean = Values[TimeIndex] * this.period_length[TimeIndex];
                    pp.SetModelFcst(PN, M);
                }
                else {
                    ModelValue M1;
                    M1.NMean = Values[TimeIndex];
                    M1.NNMean = Values[TimeIndex] * this.period_length[TimeIndex];
                    M1.S = pp.GetLearnS();
                    M1.dof = pp.GetLearnN();
                    M1.NVar = 0.0;
                    M1.NNVar = 0.0;
                    pp.SetModelFcst(PN, M1);
                }
            }
        }

        internal void SetNVarFcst(CandidatePars pp, List<double> Values) {
            int FirstFcstPN = this.GetLastHistPNumber() + 1, PN;
            ModelValue M;
            if (pp.GetModelFcst() == null) pp.InitModelFcst();
            for (int TimeIndex = 0; TimeIndex < this.nfutureobservations; TimeIndex++) {
                PN = FirstFcstPN + TimeIndex;
                if (pp.GetModelFcst().ContainsKey(PN)) {
                    M = pp.GetModelFcst()[PN];
                    M.NVar = Values[TimeIndex];
                    M.NNVar = Values[TimeIndex] * this.period_length[TimeIndex];
                    pp.SetModelFcst(PN, M);
                }
                else {
                    ModelValue M1;
                    M1.NVar = Values[TimeIndex];
                    M1.NNVar = Values[TimeIndex] * this.period_length[TimeIndex];
                    M1.S = pp.GetLearnS();
                    M1.dof = pp.GetLearnN();
                    M1.NMean = 0.0;
                    M1.NNMean = 0.0;
                    pp.SetModelFcst(PN, M1);
                }
            }
        }

        internal List<double> GetNVarFcst(CandidatePars pp) {
            return pp.GetNVarFcst();
        }

        internal double GetFcstDistributionVarForPeriod(int PNumber, bool Normalised) {
            int LastPNumber = this.GetLastHistPNumber();
            if (PNumber <= LastPNumber)
                throw new ArgumentException("Cannot_Get_Forecast_For_Period_Number" + PNumber);
            else                
                return (Normalised ?
                    this.model_fcst[PNumber].NVar :
                    this.model_fcst[PNumber].NNVar);
        }

        internal double[] GetFcstDistributionVar(bool Normalised) {
            int FirstFcstPN = this.GetLastHistPNumber() + 1;
            int type = Normalised ? (int)FcstType._NVar : (int)FcstType._NNVar;
            double[] F = new double[this.nfutureobservations];

            if (Normalised)
                for (int pn = FirstFcstPN; pn < FirstFcstPN + this.nfutureobservations; pn++)
                    F[pn - FirstFcstPN] = Math.Max(this.model_fcst[pn].NVar, 0.0);
            else
                for (int pn = FirstFcstPN; pn < FirstFcstPN + this.nfutureobservations; pn++)
                    F[pn - FirstFcstPN] = Math.Max(this.model_fcst[pn].NNVar, 0.0);
            return F;
        }

        internal ModelValue GetFcstDistributionForPeriod(int PNumber) {
            int LastPNumber = this.GetLastHistPNumber();
            if (PNumber <= LastPNumber)
                throw new ArgumentException("Cannot_Get_Forecast_For_Period_Number" + PNumber);
            else
                return (this.model_fcst[PNumber]);
        }

        internal void SetParamMeanDouble(double[,] vd, DistType DT) {
            switch (DT) {
                case DistType._LT:
                    this.GetBestPPLT().SetMean(vd);
                    break;
                case DistType._ST:
                    this.GetBestPPST().SetMean(vd);
                    break;
                case DistType._MM:
                    this.mean = vd;
                    break;
                default:
                    this.initialmean = vd;
                    break;
            }
        }

        internal double[,] GetParamMean(DistType DT) {
            switch (DT) {
                case DistType._LT:
                    return (this.GetBestPPLT().GetMean());
                case DistType._ST:
                    return (this.GetBestPPST().GetMean());
                case DistType._MM:
                    return (this.mean);
                default:
                    return (this.initialmean);
            }
        }

        internal void SetParamVarianceDouble(double[,] vd, DistType DT) {
            switch (DT) {
                case DistType._LT:
                    this.GetBestPPLT().SetVar(vd);
                    break;
                case DistType._ST:
                    this.GetBestPPST().SetVar(vd);
                    break;
                case DistType._MM:
                    this.variance = vd;
                    break;
                default:
                    this.initialvar = vd;
                    break;
            }
        }

        internal double[,] GetParamVariance(DistType DT) {
            switch (DT) {
                case DistType._LT:
                    return (this.GetBestPPLT().GetVar());
                case DistType._ST:
                    return (this.GetBestPPST().GetVar());
                case DistType._MM:
                    return (this.variance);
                default:
                    return (this.initialvar);
            }
        }

        internal double[,] GetParamVarianceDouble(DistType DT) {
            double[,] m;
            switch (DT) {
                case DistType._LT:
                    m = this.GetBestPPLT().GetVar();
                    break;
                case DistType._ST:
                    m = this.GetBestPPST().GetVar();
                    break;
                case DistType._MM:
                    m = this.variance;
                    break;
                default:
                    m = this.initialvar;
                    break;
            }

            double[,] vd = new double[MatrixOp.GetRows(m), MatrixOp.GetCols(m)];
            for (int i = 0; i < MatrixOp.GetRows(m); i++)
                for (int j = 0; j < MatrixOp.GetCols(m); j++)
                    vd[i, j] = m[i, j];
            return vd;
        }

        internal int GetVarianceSize() {
            return (this.structure.GetNActiveParams());
        }

        internal double GetLearnS() {
            return (this.learnvar_S);
        }

        internal double GetLearnS(DistType DT) {
            switch (DT) {
                case DistType._LT:
                    return (this.GetBestPPLT().GetLearnS());
                case DistType._ST:
                    return (this.GetBestPPST().GetLearnS());
                case DistType._MM:
                    return (this.learnvar_S);
                default:
                    return (this.inilearnvar_S);
            }
        }

        internal void SetLearnS(double v, DistType DT) {
            switch (DT) {
                case DistType._LT:
                    this.GetBestPPLT().SetLearnS(v);
                    break;
                case DistType._ST:
                    this.GetBestPPST().SetLearnS(v);
                    break;
                case DistType._MM:
                    this.learnvar_S = v;
                    break;
                default:
                    this.inilearnvar_S = v;
                    break;
            }
        }

        internal double GetDof(DistType DT) {
            switch (DT) {
                case DistType._LT:
                    return (this.GetBestPPLT().GetLearnN());
                case DistType._ST:
                    return (this.GetBestPPST().GetLearnN());
                case DistType._MM:
                    return (this.learnvar_n);
                default:
                    return (this.inilearnvar_n);
            }
        }

        internal void SetDof(double v, DistType DT) {
            switch (DT) {
                case DistType._LT:
                    this.GetBestPPLT().SetLearnN(v);
                    break;
                case DistType._ST:
                    this.GetBestPPST().SetLearnN(v);
                    break;
                case DistType._MM:
                    this.learnvar_n = v;
                    break;
                default:
                    break;
            }
        }

        internal double GetDof() {
            return (this.learnvar_n);
        }

        internal Hashtable GetFcstConfidenceDelta(double v) {
            Hashtable H = new Hashtable(this.nfutureobservations);
            double halfalpha = 0.5 - (v / 200.0), dof = this.GetDof();
            int FirstPN = this.GetLastHistPNumber() + 1;
            double inc = stat.TStudent_quantil(halfalpha, dof, true);

            for (int PN = FirstPN; PN < FirstPN + this.nfutureobservations; PN++)
                H.Add(PN, Math.Sqrt(Math.Abs(this.GetFcstDistributionVarForPeriod(PN, false) * inc)));
            return (H);
        }

        internal double GetFcstConfidenceFactor(double v) {
            return stat.TStudent_quantil(0.5 - (v / 200.0), this.GetDof(), true);
        }

        internal bool AddDF(CompType CT, double v) {
            if (this.df[(int)CT][0] < 10) {
                this.df[(int)CT][0]++;
                this.df[(int)CT][(int)(this.df[(int)CT][0])] = v;
                return (true);
            }
            else return (false);
        }

        internal void DeleteAllDF(CompType CT) {
            int last = (int)this.df[(int)CT][0];
            int CType = (int)CT;

            for (int i = 0; i <= last; i++) this.df[CType][i] = 0.0;
        }

        internal bool DeleteDF(CompType CT, double v) {
            int last = (int)this.df[(int)CT][0];
            int CType = (int)CT;

            if ((v <= 0) || (v > 1)) return (false);

            if (last == (int)0) return (false);

            int i = 1;
            while (this.df[CType][i++] != v) ;
            if (i == last)
                this.df[CType][i] = (double)0;
            else
                for (int j = i; j < last; j++) this.df[CType][j] = this.df[CType][j + 1];
            this.df[CType][0]--;
            return (true);
        }

        internal void ForceDF(CompType CT, double v) {
            this.df[(int)CT][11] = v;
        }

        internal double GetSelectedDF(CompType CompType) {
            return (this.bestPPLT.DFNormal()[(int)CompType]);
        }

        internal double[] GetSelectedDF() {
            double[] d = new double[4];

            d[0] = this.GetSelectedDF(CompType._Pol);
            d[1] = this.GetSelectedDF(CompType._Sea);
            d[2] = this.GetSelectedDF(CompType._Reg);
            d[3] = this.GetSelectedDF(CompType._Var);

            return (d);
        }

        internal bool CheckDF() {
            int[] Check_Matrix = new int[4];

            // If the variance learning discount factor is nor right, return false;
            if ((this.df[3][11] <= 0.0) || (this.df[3][11] > 1.0))
                return (false);

            Check_Matrix[0] = this.structure.GetNActiveParamsPol();
            Check_Matrix[1] = this.structure.GetNActiveParamsSea();
    
            for (int row = 0; row < 3; row++)
                if ((Check_Matrix[row] > 0) &&
                    ((this.df[row][11] <= 0.0) || (this.df[row][11] > 1.0)))
                    return (false);
            return (true);
        }

        private Crom GetAlternativeDF(DLModel Structure, int StartPN,
                 double[,] PostMean, double[,] PostVar, double[] NormalDF, Frequencies TblFrqResiduos,
                 double ModelDegProb, double S, double N, double NormError) {
            int maxIteracs = 100;
            double[] mins = new double[4];
            double[] maxs = new double[4];

            //determinacion de maximos y minimos
            // this.normalDF = false;
            maxs = NormalDF;
            // Si radix = 1 --> Err/MaxValue = 1 --> mins[i] = 0.1
            // Si radix = 0 --> Err/MaxValue = 0 --> mins[i] = maxs[i] - 0.1
            for (int i = 0; i < 4; i++)
                //mins[i] = (1 - NormError) * maxs[i] + 0.1 * (2 * NormError - 1);
                mins[i] = maxs[i] * 0.5;
            //mins[i] = 0;


            //Montecarlo para determinar el nuevo set de df
            FObjetivoDF ObjetivoDF = new FObjetivoDF(Structure, StartPN, PostMean, PostVar,
                           NormalDF, TblFrqResiduos, ModelDegProb, S, N);

            Crom c = new Crom("", 1, 4);
            SearchAlg srchAlgs;

            if (this.structure.GetNActiveArmonics() == 0) {
                mins[1] = -1.0;
                maxs[1] = -1.0;
            }
            if (true) {
                mins[2] = -1.0;
                maxs[2] = -1.0;
            }
            srchAlgs = new Greedy(1, 4, mins, maxs, ObjetivoDF, null, null, 0.01);

            double[][] gens = ((ISearchAlg)srchAlgs).Search(maxIteracs);
            c = new Crom(gens);
            //Crom cNorm = new Crom("", 1, 4);
            //cNorm.Genes[0] = NormalDF;

            return c;
        }

        internal void SetAlterDfSearchLevel(int iterations) {
            this.alterDfSearchLevel = iterations;
        }

        internal double GetStdErrLevel() {
            return this.stderrlevel;
        }

        internal void SetStdErrLevel(double N) {
            this.stderrlevel = N;
        }

        internal double GetStabLevel() {
            return this.stablevel;
        }

        internal void SetStabLevel(double N) {
            this.stablevel = N;
        }

        internal int GetNObsForShortTermFcstError() {
            return this.NObsForShortTermFcstError;
        }

        internal void SetNObsForShortTermFcstError(int N) {
            this.NObsForShortTermFcstError = N;
        }

        internal int GetShortTermFcstHorizon() {
            return this.ShortTermFcstHorizon;
        }

        internal void SetShortTermFcstHorizon(int L) {
            this.ShortTermFcstHorizon = L;
        }

        internal double GetSTIncRadix() {
            return this.stincradix;
        }

        internal void SetSTIncRadix(double v) {
            this.stincradix = v;
        }

        internal double GetSTRangeRadix() {
            return this.strangeradix;
        }

        internal void SetSTRangeRadix(double v) {
            this.strangeradix = v;
        }

        internal double GetLTIncRadix() {
            return this.ltincradix;
        }

        internal void SetLTIncRadix(double v) {
            this.ltincradix = v;
        }

        internal double GetLTRangeRadix() {
            return this.ltrangeradix;
        }

        internal void SetLTRangeRadix(double v) {
            this.ltrangeradix = v;
        }

        internal void SetDataFrequencies(Dictionary<int, double> dict) {
            if (this.dataFrequencies == null) { this.dataFrequencies = new Frequencies(); }
            this.dataFrequencies.LoadData(dict);
        }

        internal Frequencies GetDataFrequencies() {
            return dataFrequencies;
        }

        internal void SetOutliers(List<double> outliers) {
            this.outliers = outliers;
        }

        internal List<double> GetOutliers() {
            return outliers;
        }

        internal void SetResidualFrequencies(bool shortTerm, Dictionary<int, double> dict) {
            if (shortTerm) {
                if (this.bestPPST.ResidualFrq == null) { this.bestPPST.ResidualFrq = new Frequencies(); }
                this.bestPPST.ResidualFrq.LoadData(dict);
            }
            else {
                if (this.bestPPLT.ResidualFrq == null) { this.bestPPST.ResidualFrq = new Frequencies(); }
                this.bestPPLT.ResidualFrq.LoadData(dict);
            }
        }

        internal Frequencies GetResidualFrequencies(bool shortTerm) {
            if (shortTerm) {
                return this.bestPPST.ResidualFrq;
            }
            else {
                return this.bestPPLT.ResidualFrq;
            }
        }

        internal void SetLastDegResiduals(bool shortTerm, List<double> degenerations) {
            if (shortTerm)
                this.GetBestPPST().SetLastDegResiduals(degenerations);
            else
                this.GetBestPPLT().SetLastDegResiduals(degenerations);
        }

        internal List<double> GetLastDegResiduals(bool shortTerm) {
            if (shortTerm) {
                return this.GetBestPPST().GetLastDegResiduals();
            }
            else {
                return this.GetBestPPLT().GetLastDegResiduals();
            }
        }

        internal void Initialize(int FirstPN) {
            this.SetFirstPNumber(FirstPN);
            // this.LastPN =  this.FirstPN + this.nobservations - 1;
        }

        internal void DeleteFirstOldZeros() {
            if (this.ots.TS.Count == 0) { return; }
            int NPeriods = this.ots.FirstNonZero() - this.GetFirstPNumber();
            if (NPeriods != 0) {
                this.ots.DeletePeriods(NPeriods);
                if (this.fpntoforecast < this.FirstPN)
                    this.fpntoforecast = this.FirstPN;
            }
        }

        internal int[] GetActiveRows() {
            return (this.structure.GetActiveRows());
        }

        internal ArrayList[] GetPolSeasonalCompList() {
            return this.structure.GetPolSeasonalComponents();
        }

        internal Result CalcDLM(bool Test_DF) {
            this.ots.SetObs(); // Set the list of Period Numbers with observable values
            if (this.GetShortTermFcstHorizon() + this.GetNObsForShortTermFcstError() > this.ots.GetObsValues(false).Length) {
                return Result.NotEnoughHistory;
            }

            this.initialmean = null;
            this.initialvar = null;
            this.learnvar_n = 0.0;
            this.learnvar_S = 0.0;
            int FirstPeriodNumber = this.fpntoforecast, LastPeriodNumber = this.GetLastPNumber();
            int FirstFcstPN = LastPeriodNumber + 1;

            // If this object had already identified some outliers, these outliers are deleted
            // for this new calculation
            for (int pn = FirstPeriodNumber; pn <= LastPeriodNumber; pn++)
                if (this.IsOutlier(pn))
                    this.SetTSType(pn, ObsType._Usable);

            this.ots.NormalizeOTS();
            //load frequencies for outlier identification
            this.LoadDataFrequencies(this.GetObs(false));
            this.DetectOutliers(FirstPeriodNumber, 5);

            // Now we build the models, short term and long term
            Result result = this.CalcHistoricalDistributions(Test_DF);
            if (result != Result.Ok) {
                if (bestPPLT != null) { this.bestPPLT.ResetModelHist(this.FirstPN, this.LastPN); }
                if (bestPPST != null) { this.bestPPST.ResetModelHist(this.FirstPN, this.LastPN); }
                return result;
            }

            foreach (int key in this.GetBestPPST().DFAlters().Keys)
                this.SetTSType(key, ObsType._DegradationST);

            foreach (int key in this.GetBestPPLT().DFAlters().Keys)
                if (this.GetBestPPST().DFAlters().ContainsKey(key))
                    this.SetTSType(key, ObsType._Degradation);
                else
                    this.SetTSType(key, ObsType._DegradationLT);

            // Now we calculate the forecast
            FirstFcstPN = LastPeriodNumber + 1;
            this.CalcForecastDistributions(FirstFcstPN, this.GetBestPPLT());
            this.CalcForecastDistributions(FirstFcstPN, this.GetBestPPST());
            this.CalcModelFcst();

            return result;
        }

        internal void SetForecastingHorizon(int FH) {
            this.nfutureobservations = FH;
            this.period_length = new double[FH];
        }

        private void ResetModelHist(CandidatePars pp) {
            pp.ResetModelHist(this.FirstPN, this.LastPN);
        }


        internal void CalcModelFcst() {
            model_fcst = new Dictionary<int, ModelValue>();
            //int FcstFirstPN = this.GetLastHistPNumber() + 1, shortTerm = this.GetShortTermFcstHorizon();
            int FcstFirstPN = this.GetLastPNumber() + 1, shortTerm = this.GetShortTermFcstHorizon();
            double probShort = 1.0;
            double probLong = 0.0;
            double step = 1.0 / (shortTerm - 1);
            int iniTrans = 1;
            int endTrans = iniTrans + shortTerm - 1;
            ModelValue MST, MLT, M;

            //short term
            int ConObservacion = 0, todas = 0;
            while (ConObservacion < iniTrans) {
                this.model_fcst.Add(FcstFirstPN + todas, this.GetBestPPST().GetModelFcst(FcstFirstPN + todas));
                if (period_length[todas] != 0) ConObservacion++;
                todas++;
            }

            //transicion
            ConObservacion = iniTrans;
            while (ConObservacion < endTrans) {
                MST = this.GetBestPPST().GetModelFcst(FcstFirstPN + todas);
                MLT = this.GetBestPPLT().GetModelFcst(FcstFirstPN + todas);
                M.NMean = probShort * MST.NMean + probLong * MLT.NMean;
                M.NVar = probShort * MST.NVar + probLong * MLT.NVar;
                M.NNMean = probShort * MST.NNMean + probLong * MLT.NNMean;
                M.NNVar = probShort * MST.NNVar + probLong * MLT.NNVar;
                M.dof = 300.0; // It behaves during transition a a Normal distribution;
                M.S = probShort * MST.S + probLong * MLT.S;
                this.model_fcst.Add(FcstFirstPN + todas, M);
                if (period_length[todas] != 0) {
                    probShort -= step;
                    probLong += step;
                    ConObservacion++;
                }
                todas++;
            }

            //long term
            for (int i = todas; i < nfutureobservations; i++)
                this.model_fcst.Add(FcstFirstPN + i, this.GetBestPPLT().GetModelFcst(FcstFirstPN + i));
            this.learnvar_n = this.GetBestPPST().GetLearnN();
            this.learnvar_S = this.GetBestPPST().GetLearnS();
        }

        #endregion

        #region Private Methods

        internal void SetFirstPNumber(int FirstPN) {
            this.FirstPN = FirstPN;
        }

        private void CreateDF() {
            this.df = new double[4][];
            for (int i = 0; i < 4; i++) this.df[i] = new double[13];
            SetDF((double)0.0);
        }

        private void SetDF(double v) {
            for (int r = 0; r < 4; r++)
                for (int c = 0; c < 11; c++)
                    this.df[r][c] = v;
        }

        internal double[] GetDF(CompType type) {
            switch (type) {
                case CompType._Pol:
                    return this.df[0];
                case CompType._Sea:
                    return this.df[1];
                case CompType._Reg:
                    return this.df[2];
                case CompType._Var:
                    return this.df[3];
                default:
                    return null;
            }
        }

        private bool CalcWLS(double[,] Matrix_A, double[,] Vector_b, ref double[,] SolutionMeans, ref double[,] SolutionVars, bool AllParams) {
            double MSE = 0.0;
            int LastPeriodNumber = this.GetLastPNumber();
            int Nparams = this.structure.GetNActiveParams();
            IMatrix A = new Matrix(MatrixOp.Multiply(MatrixOp.Transpose(Matrix_A), Matrix_A));
            if (double.IsNaN(A.Determinant)) return false;
            double[,] Ainv = A.Inverse.ConvertToDouble();
            int LastObsIndex = this.GetLastObsIndex();

            if (A.Determinant != 0)
                SolutionMeans = MatrixOp.Multiply(Ainv,
                                                  MatrixOp.Multiply(MatrixOp.Transpose(Matrix_A), Vector_b));
            else
                return false;

            int NOutliers = 0, NObs = 0;
            for (int t = 0; t <= LastObsIndex; t++) {
                int pn = this.GetObsPN(t);
                if (!this.IsMissing(pn)) {
                    if (!this.IsOutlier(pn))
                        MSE += Math.Pow(this.ots.GetTS(pn).NValue - this.CalcValueWLS(pn, SolutionMeans, Matrix_A, AllParams), 2);
                    else
                        NOutliers++;
                    NObs++;
                }
            }
            if (NObs - NOutliers - Nparams != 0)
                MSE = MSE / (NObs - NOutliers - Nparams);
            else
                throw new ArgumentException("Imposible_To_Calculate_The_Initial_Distribution_NObs_NOutliers_Nparams");

            SolutionVars = MatrixOp.Multiply(Ainv, MSE);
            MatrixOp.MakeSimetric(SolutionVars);
            return (true);
        }

        private List<double> CalcModelWLS(double[,] Matrix_A, double[,] Vector_b, ref double[,] SolutionMeans, ref double[,] SolutionVars, bool[] AllParams, bool forAnova) {
            List<double> modelWls = new List<double>();
            for (int t = 0; t <= GetLastObsIndex(); t++) {
                int pn = this.GetObsPN(t);
                if (!this.IsMissing(pn) && !this.IsOutlier(pn)) { modelWls.Add(CalcValueWLS(pn, SolutionMeans, Matrix_A, AllParams)); }
                else { modelWls.Add(-1); }
            }
            return modelWls;
        }


        private bool CalcIniDistLsq() {
            double[,] Matrix_A, Vector_b, SolMean, SolVar;
            double[,] Matrix_A1, Vector_b1, SolMean1, SolVar1;
            int LastObsIndex = this.GetLastObsIndex();
            int FirstPeriodNumber = this.fpntoforecast, LastPeriodNumber = this.GetLastPNumber();
            int Nparams = this.structure.GetNActiveParams(), NObs = LastPeriodNumber - FirstPeriodNumber + 1;

            // STEP 1:  CALCULATION OF THE INITIAL OLS

            if (NObs < Nparams) return false;

            this.initialmean = new double[Nparams, 1];
            SolMean = new double[Nparams, 1];
            this.initialvar = new double[NObs, Nparams];
            SolVar = new double[NObs, Nparams];

            Vector_b = new double[NObs, 1];
            SolMean = new double[Nparams - 1, 1];

            this.structure.CalcMatrixA(true); // Outliers already detected are taken out of the calculation.
            Matrix_A = this.structure.GetMatrixA();

            // To setup Vector_b, is much easier, as each row is each observed value.
            List<double> history = new List<double>();
            for (int r = 0; r <= LastObsIndex; r++) {
                int pn = this.GetObsPN(r);
                if (!IsMissing(pn))
                    if (IsOutlierOrMasked(pn)) {
                        Vector_b[r, 0] = 0.0;
                    }
                    else {
                        Vector_b[r, 0] = this.ots.GetTS(pn).NValue;
                        history.Add(this.ots.GetTS(pn).NValue);
                    }

            }

            // Now we can obtain the result
            // All params are considered, even those which are not active.
            bool iscalculated = this.CalcWLS(Matrix_A, Vector_b, ref SolMean, ref SolVar, true);
            if (!iscalculated) return false;
            this.initialmean = SolMean;
            this.initialvar = SolVar;

            // Armonics selection : ANOVA for Nested Models
            SelectArmonics(history, Matrix_A, Vector_b, SolMean, SolVar, 1 - perConfArm / 100.0);
       
            // STEP 3:  IDENTIFY WHICH ARMONICS OF THE SEASONAL COMPONENT ARE SIGNIFICANT AND 
            //          ARE GOING TO BE USED BY THE DLM MODEL

            if (MatrixOp.IsZero(this.initialmean)) return false;
            //this.structure.SelectArmonics(true);
            SolMean = this.initialmean;
            SolVar = this.initialvar;

            this.initialmean = SolMean;
            this.initialvar = SolVar;
       
            for (int i = 0; i < MatrixOp.GetCols(this.initialvar); i++)
                this.initialvar[i, i] = (Math.Abs(this.initialvar[i, i]) < 0.1 ? Math.Ceiling(this.initialvar[i, i]) * 1.0 : this.initialvar[i, i]);

            this.structure.SetParamDistributions(this.initialmean, this.initialvar);
            this.inilearnvar_n = 0.0;
            this.inilearnvar_S = (this.ots.Moments[1][1] == 0.0 ? 1.0 : this.ots.Moments[1][1]); // Equals the standar deviation
            return true;
        }

        private void SelectArmonics(List<double> history, double[,] Matrix_A, double[,] Vector_b, double[,] SolMean, double[,] SolVar, double alpha) {
            HypoTest hypoTest = new HypoTest(perConfArm / 100.0, 0);
            int nArms = GetNModelArmonics();
            int pc = GetNActiveParams() - nArms + 1;
            List<double> compModel;
            List<double> relevantArmonics = new List<double>();
            List<double> notRelevantArmonics = new List<double>();
            int index;

            bool[] activeArmonics = new bool[this.structure.GetNArmonics()];
            for (int i = 0; i < activeArmonics.Length; i++) { activeArmonics[i] = false; }
            foreach (DLMParameter p in this.structure.GetParamsListActive())
                if ((p.GetParamType() == CompType._Sea) && (p.GetParamName() == "Cos")) { activeArmonics[p.GetArmonicNumber() - 1] = true; }

            //initial model: level & trend
            bool[] tryArmonics = new bool[activeArmonics.Length];
            for (int i = 0; i < tryArmonics.Length; i++) { tryArmonics[i] = false; }
            List<double> redModel = this.CalcModelWLS(Matrix_A, Vector_b, ref SolMean, ref SolVar, tryArmonics, true);

            List<double> redModelFilt = FilterMasked(redModel);
            List<double> compModelFilt;
            int iteration = 1;
            while (iteration <= 3) {
                //forward step
                foreach (DLMParameter p in this.structure.GetSeasonalCosParamsListActive()) {
                    index = p.GetArmonicNumber() - 1;
                    if (activeArmonics[index]) {
                        tryArmonics[index] = true;
                        compModel = this.CalcModelWLS(Matrix_A, Vector_b, ref SolMean, ref SolVar, tryArmonics, false);
                        compModelFilt = FilterMasked(compModel);
                        tryArmonics[index] = false;
                        bool isRelevant = hypoTest.AnovaNestedModels(history, redModelFilt, compModelFilt, pc - 1, pc, alpha);
                        if (isRelevant) { relevantArmonics.Add(index + 1); }
                    }
                }

                if (relevantArmonics.Count == 0) { break; }
                foreach (int num in relevantArmonics) { tryArmonics[num - 1] = true; }
                compModel = this.CalcModelWLS(Matrix_A, Vector_b, ref SolMean, ref SolVar, tryArmonics, false);

                //backward step
                foreach (DLMParameter p in this.structure.GetSeasonalCosParamsListActive()) {
                    index = p.GetArmonicNumber() - 1;
                    if (activeArmonics[index] && tryArmonics[index]) {
                        tryArmonics[index] = false;
                        compModel = this.CalcModelWLS(Matrix_A, Vector_b, ref SolMean, ref SolVar, tryArmonics, false);
                        compModelFilt = FilterMasked(compModel);
                        tryArmonics[index] = true;
                        bool isRelevant = hypoTest.AnovaNestedModels(history, redModelFilt, compModelFilt, pc - 1, pc, alpha);
                        if (!isRelevant) { notRelevantArmonics.Add(index + 1); }
                    }
                }
                if (notRelevantArmonics.Count == 0) { break; }
                foreach (int num in notRelevantArmonics) { tryArmonics[num - 1] = false; }
                redModel = this.CalcModelWLS(Matrix_A, Vector_b, ref SolMean, ref SolVar, tryArmonics, false);
                iteration++;
            }

            //final setting
            for (int i = 0; i < tryArmonics.Length; i++) {
                if (activeArmonics[i] && !tryArmonics[i]) { structure.DeActivateArmonic(i + 1); }

            }

        }

        private List<double> FilterMasked(List<double> serie) {
            List<double> maskedSerie = new List<double>();
            for (int r = 0; r <= this.GetLastObsIndex(); r++) {
                int pn = this.GetObsPN(r);
                if (!IsMissing(pn) && !IsOutlierOrMasked(pn)) { maskedSerie.Add(serie[r]); }
            }
            return maskedSerie;
        }

        internal double GetMSE(CandidatePars pp) {
            if (!pp.IsModelCalculated()) return -1;
            if (pp.GetMSE() == -1) {
                double mse = 0.0;
                for (int pn = this.fpntoforecast; pn <= this.GetLastPNumber(); pn++)
                    if (!this.IsOutlierOrMasked(pn))
                        mse += Math.Pow(this.GetTS(pn).NValue - pp.GetModelValue(pn).NMean, 2);
                mse = mse / ((this.GetLastPNumber() - this.fpntoforecast) - this.GetNOutliers() - pp.Structure.GetNActiveParams());
                pp.SetMSE(mse);
            }
            return (pp.GetMSE());
        }

        private double CalcValueWLS(int pn, double[,] ParamVector, double[,] Matrix_A, bool AllParams) {
            double Result = 0.0;

            if (AllParams)
                for (int c = 0; c < MatrixOp.GetRows(ParamVector); c++)
                    Result += ParamVector[c, 0] * Matrix_A[pn - this.fpntoforecast, c];
            else {
                ArrayList ParamList = this.structure.GetParamsListActive();
                foreach (DLMParameter p in ParamList)
                    Result += ParamVector[p.GetParamNumber(), 0] * Matrix_A[pn - this.fpntoforecast, p.GetParamNumber()];
            }
            return (Result);
        }

        private double CalcValueWLS(int pn, double[,] ParamVector, double[,] Matrix_A, bool[] Params) {
            double Result = 0.0;

            ArrayList ParamList = new ArrayList();
            foreach (DLMParameter p in this.structure.GetParamsListActive()) {
                if (p.GetParamType() != CompType._Sea) { ParamList.Add(p); }
                if ((p.GetParamType() == CompType._Sea) && (Params[p.GetArmonicNumber() - 1])) { ParamList.Add(p); }
            }
            foreach (DLMParameter p in ParamList)
                Result += ParamVector[p.GetParamNumber(), 0] * Matrix_A[pn - this.fpntoforecast, p.GetParamNumber()];
            return (Result);
        }

        private Result CalcHistoricalDistributions(bool Test_DF) {
            ParSelector SelDF = new ParSelector(this);
            ParSelector.Result result = ParSelector.Result.Ok;
            int STFcstHorValidity = 0;
            int FirstPeriodNumber = this.fpntoforecast;

            this.CalcMomentsForAll(FirstPeriodNumber);
            this.SetComponets();

            if (this.CalcIniDistLsq() == false) return Result.NoInitialDistribution;

            this.CalcMomentsForAll(FirstPeriodNumber);
            this.ots.SetValues01(); // Sets the Obs value normalised by period length power to the obs value normalised between 0 and 1

            this.learnvar_n = this.inilearnvar_n;
            this.learnvar_S = this.inilearnvar_S;

            //if (Test_DF)            {
            this.structure.CalcMatrixGk(this.GetShortTermFcstHorizon());
            result = SelDF.SelectDF(this.bruteforcemaxite);

            if (result == ParSelector.Result.NoneSelected) { return Result.BadQualityFcst; }

            this.bestPPLT = SelDF.BestPPLT;
            this.bestPPST = SelDF.BestPPST;

            this.bestPPLT.GetStructure().SetParamDistributions(this.bestPPLT.GetMean(), this.bestPPLT.GetVar());
            this.bestPPST.GetStructure().SetParamDistributions(this.bestPPST.GetMean(), this.bestPPST.GetVar());

            if (this.bestPPLT != null) this.CalcStdErrorOverTime(this.bestPPLT);
            if (this.bestPPST != null) this.CalcStdErrorOverTime(this.bestPPST);

            this.mean = this.bestPPLT.GetMean();
            this.variance = this.bestPPLT.GetVar();
            this.learnvar_n = this.bestPPLT.GetLearnN();
            this.learnvar_S = this.bestPPLT.GetLearnS();

            if (result == ParSelector.Result.OneSelected) {
                STFcstHorValidity = this.nfutureobservations;
                return Result.Ok;
            }
            if ((this.bestPPST.seovertime.Count == 0) && (this.bestPPST.seovertime.Count == 0)) {
                System.Diagnostics.Debug.WriteLine("seovertimes en cero");
                return Result.BadSettings;
            }

            STFcstHorValidity = func.GetFirstIntersectionIndex(this.bestPPLT.seovertime.ToArray(), this.bestPPST.seovertime.ToArray());
            switch (STFcstHorValidity) {
                case -1:
                    STFcstHorValidity = this.GetShortTermFcstHorizon();
                    break;
                case -2:
                    STFcstHorValidity = 0;
                    break;
            }
            return Result.Ok;
        }

        private void CalcHistPrior(DLModel S, double[,] PostMean, double[,] PostVar, double[] DF, ref double[,] PriorMean, ref double[,] PriorVar) {
            PriorMean = MatrixOp.Multiply(S.GetMatrixG(), PostMean);
            PriorVar = MatrixOp.MultiplyT(MatrixOp.Multiply(S.GetMatrixG(), PostVar), MatrixOp.Transpose(S.GetMatrixG()));
            this.structure.ApplyDiscFactors(PriorVar, DF, false);
            MatrixOp.MakeSimetric(PriorVar);
        }

        private void CalcOneStepFcst(DLModel S, double[,] PriorMean, double[,] PriorVar, double Learn_S, ref double Mean, ref double Var) {
            Mean = MatrixOp.ToScalar(MatrixOp.Multiply(MatrixOp.Transpose(S.GetMatrixF()), PriorMean));
            Var = MatrixOp.ToScalar(MatrixOp.Multiply(MatrixOp.Transpose(S.GetMatrixF()), MatrixOp.Multiply(PriorVar, S.GetMatrixF()))) + Learn_S; 
        }

        private void CalcHistPost(DLModel S, double[,] PriorMean, double[,] PriorVar, double[] DF, double Var,
                                  double error, ref double[,] AdapCoef_CVector,
                                  ref double[,] PostMean, ref double[,] PostVar, ref double Learn_S, ref double Learn_N) {
            double Prev_N = Learn_N, Prev_S = Learn_S;

            Learn_N = Prev_N * DF[3] + 1.0;
            Learn_S = (Prev_S / Learn_N) * ((DF[3] * Prev_N) + (Math.Pow(error, 2.0) / Var));
            if (Var != 0.0)
                AdapCoef_CVector = MatrixOp.Multiply(PriorVar, MatrixOp.Multiply(S.GetMatrixF(), 1.0 / Var));
            PostMean = MatrixOp.Addition(PriorMean, MatrixOp.Multiply(AdapCoef_CVector, error));
            PostVar = MatrixOp.Substraction(PriorVar, MatrixOp.Multiply(MatrixOp.MultiplyTranspose(AdapCoef_CVector), Var));
            PostVar = MatrixOp.Multiply(PostVar, Learn_S / Prev_S);
            MatrixOp.MakeSimetric(PostVar);
        }

        internal double[] EvolveDLM(DLModel St, int StartPN,
                                    double[,] PostMean, double[,] PostVar, double[] NormalDF, double[] AltDF,
                                    Frequencies TblFrqResiduos, double Prob, double Learn_S, double Learn_N) {
            int PN, NonActiveObs = 0, LastHistPN = this.GetLastPNumber();
            int NActiveRegressor = 0;
            double[] Evaluation = { 0.0, 0.0, 0.0, 1.0 };
            double[] UsedDF = new double[3];
            double S = Learn_S, N = Learn_N, f = 0.0, Q = 0.0, error = 0.0, residual = 0.0;
       
            double[,] PostM = MatrixOp.Clone(PostMean), PostV = MatrixOp.Clone(PostVar);
            double[,] PriorM = MatrixOp.Clone(PostMean), PriorV = MatrixOp.Clone(PostVar);
            double[,] adapcoef_cvector = new double[MatrixOp.GetCols(PostV), 1];
            bool ModelDeg;
            int FirstObsIndex = this.GetObsIndex(StartPN);

            for (int time = 0; time <= this.degenerationshor; time++) {
                if (time + FirstObsIndex >= this.ots.Obs.Count) break;
                PN = this.GetObsPN(time + FirstObsIndex);
                // If this observation is usable then use it
                if (!this.IsMissing(PN)) {
                    if (!this.IsOutlier(PN)) {
                        if (NActiveRegressor != 0) St.CalcFMatrix(PN);

                        UsedDF = ((time == 0) ? AltDF : NormalDF);
                        this.CalcHistPrior(this.structure, PostM, PostV, UsedDF, ref PriorM, ref PriorV);

                        //     ONE STEP FORECAST

                        this.CalcOneStepFcst(St, PriorM, PriorV, S, ref f, ref Q);
                        error = this.ots.GetTS(PN).NValue - f;
                        int pNum = PN;
                        double denominador = ots.GetTS(pNum).Value01;
                        while ((denominador == 0.0) && (pNum > 0)) {
                            pNum--;
                            denominador = ots.GetTS(pNum).Value01;
                        }
                        double r = 0.0;
                        if (this.ots.GetTS(PN).Value01 != 0.0)
                            r = Math.Abs(100.0 * error / denominador);
                        residual = (r > 10e6 ? -1.0 : (int)r);
                    }
                    else   // If this observation is not usable, then Error = 0.0
                        error = 0.0;

                    if (error != 0) {
                        if (outlierDetection.GrubbsTest(TblFrqResiduos, residual, Prob, 0.0) && (time != 0))
                        //if (Stat.GrubbsTest(TblFrqResiduos, error, Prob, 0.0) && (time != 0))
                        {
                            ModelDeg = true;
                            if ((ModelDeg) && (time != 0)) {
                                Evaluation[3] += 1.0; ;
                            }
                        }

                        this.CalcHistPost(St, PriorM, PriorV, UsedDF, Q, error, ref adapcoef_cvector, ref PostM, ref PostV, ref S, ref N);
                        if (time != 0) {
                            Evaluation[0] += Math.Abs(error);
                            Evaluation[1] += Math.Pow(error, 2);
                            if (N != 0)
                                Evaluation[2] += stat.TStudent(this.ots.GetTS(PN).NValue, f, Q, N, true);
                        }

                    }
                    else
                        NonActiveObs++;
                } 
            }  
            return Evaluation;
        }

        internal void EvolveDLM(CandidatePars pp, bool updatemode) {
            StatForecast dlm = pp.Structure.GetDLM();
            DLModel St = dlm.structure;
            int FirstPNumber = dlm.GetFirstPNumber(), InitialFPN = dlm.fpntoforecast, MissingObs = 0;
            int FirstObs = dlm.fpntoforecast - FirstPNumber, ModelChangeNP = 0;
            int NObs = dlm.ots.Obs.Count;
            int FObs = FirstPNumber + FirstObs;
            double error, observation, residual = 0.0, OutlierProb = this.GetOutlierProbabilityThreshold();
            double[] d = new double[4];
            int NParams = (pp.GetStructure()).GetNActiveParams(), NConsPeriodsDeg = 0;
            int FirstPN = (dlm.fpntoforecast < FirstPNumber) ? FirstPNumber : dlm.fpntoforecast, PN, FirstDeg = 0;
            bool ModelDeg = false, GrubbsTest = false, DegStarted = false;

            int NActiveRegressor = 0, plength;
            double N = dlm.learnvar_n, S = dlm.learnvar_S, f = 0.0, Q = 0.0;
            List<double> EvoVar = new List<double>();
            List<double> StdErr = new List<double>();

            //IMatrix PostM = this.initialmean.Clone(), PostV = this.initialvar.Clone();
            //IMatrix PriorM = new Matrix(NParams, 1), PriorV = new Matrix(NParams, NParams);
            //IMatrix adapcoef_cvector = new Matrix(NParams, 1);

            double[,] PostM = this.initialmean, PostV = this.initialvar;
            double[,] PriorM = new double[NParams, 1], PriorV = new double[NParams, NParams];
            double[,] adapcoef_cvector = new double[NParams, 1];

            //double Prob = dlm.GetOutlierProbabilityThreshold();
            double Prob = dlm.GetAdaptability();
            int STFH = this.GetShortTermFcstHorizon(), NOSTFE = this.GetNObsForShortTermFcstError();
            // Matrix which contains the Standard Error (row,column)
            // Row N has got the K observations of the Standard Error, when it is calculated using the same first N forecasted periods
            // Column K has got the Standard Error values, when r2 is calculated using the first 1, 2, ... N forecasted periods
            double[,] STFError = new double[STFH, NOSTFE];
            double MaxVal = 0.0;
            Frequencies Frq = new Frequencies();
            Crom c = new Crom("", 1, 4);
            ModelValue MV;

            St.CalcGMatrix();
            int MinIndex = 0;
            if (updatemode) {
                FirstPN = pp.GetLastDegPN();
                this.ots.SetObs();
                NObs = this.GetLastPNumber() - FirstPN + 1;
                Frq = pp.GetResidualFrq();
                St.CalcFMatrix(FirstPN);
                d = pp.DFNormal();
                MinIndex = this.ots.Obs.Count - this.GetDegenerationsHor() - 1 - this.GetNOutliers(FirstPN);
            }
            else {
                St.CalcFMatrix(FirstPN);
                MinIndex = 0;
            }
            for (int Index = MinIndex; Index < this.ots.Obs.Count; Index++) {
                PN = this.ots.Obs[Index];
                //Debug.Write(PN + " ");
                //Debug.WriteIf(Index % 18 == 1, "    ");
                plength = (int)this.GetTS(PN).PLength;
                observation = this.ots.GetTS(PN).NValue;
                if (updatemode && this.IsOutlier(PN)) {
                    pp.IncrementZeroErrorPeriods();
                    continue;
                }

                if (!this.IsMissing(PN)) {
                    // If this observation is usable then use it
                    if (!this.IsOutlier(PN)) {
                        if (NActiveRegressor != 0) St.CalcFMatrix(PN);

                        if ((updatemode) && (Index == 0)) d = c.Genes[0];
                        else d = pp.DFNormal();
                        this.CalcHistPrior(St, PostM, PostV, d, ref PriorM, ref PriorV);

                        //     ONE STEP FORECAST

                        this.CalcOneStepFcst(St, PriorM, PriorV, S, ref f, ref Q);
                        if (Q < 0.0) {
                            pp.StdError = this.GetFcstLumpyDmdThreshold() + 1.0;
                            return;
                        }
                        observation = this.ots.GetTS(PN).NValue;
                        error = observation - f;
                        int pNum = PN;
                        double denominador = ots.GetTS(pNum).Value01;
                        while ((denominador == 0.0) && (pNum > 0)) {
                            pNum--;
                            denominador = ots.GetTS(pNum).Value01;
                        }
                        double r = 0.0;
                        if (this.ots.GetTS(PN).Value01 != 0.0)
                            r = Math.Abs(100.0 * error / denominador);
                        residual = (r > 10e6 ? -1.0 : (int)r);
                    }
                    else   // If this observation is not usable, then Error = 0.0
                    {
                        error = 0.0;
                        observation = 0.0;
                        //if ((!updatemode) && (Index >= this.ots.Obs.Count - STFH - NOSTFE) && (Index < this.ots.Obs.Count - STFH))
                        //    StdErr.Add(Stat.StdError(this.GetYtk(PN + 1, this.GetShortTermFcstHorizon()).ToArray(), this.Getftk(PN, PostM, this.GetShortTermFcstHorizon()).ToArray()));
                    }
                    if (error != 0) {
                        pp.InitZeroErrorPeriods();
                        GrubbsTest = outlierDetection.GrubbsTest(pp.ResidualFrq, residual, Prob, 0.0);
                        if (GrubbsTest) {
                            JacknifeEstimation jack = new JacknifeEstimation(pp.ResidualFrq, 25.0, true);
                            ModelDeg = jack.IsOutlier(residual, Prob);
                            Debug.WriteLineIf(!ModelDeg, "Desacuerdo jacknife: " + residual);
                            DegStarted = ((!updatemode && (Index >= NObs - this.degenerationshor) && ((ModelDeg) || (DegStarted))) ||
                                         (updatemode && (Index != MinIndex)));
                            if (ModelDeg) {
                                NConsPeriodsDeg++;

                                if (NConsPeriodsDeg == 0) ModelChangeNP = FirstPNumber + Index;

                                // We have found that the model is degenerating, so generate alternative discount factors
                                MaxVal = GetMaxValue(PostM);
                                if (!DegStarted) {
                                    double factor = (Math.Abs(error / MaxVal) > 1 ? 0.9 : Math.Abs(error / MaxVal));
                                    c = this.GetAlternativeDF(pp.GetStructure(), PN, PostM, PostV, pp.DFNormal(), Frq, Prob, S, N, factor);
                                    pp.AddDFAlter(PN, c.Genes[0]);
                                    d = c.Genes[0];
                                    DegStarted = ((!updatemode && (Index >= NObs - this.degenerationshor) && ((ModelDeg) || (DegStarted))) ||
                                                 (updatemode && (Index != MinIndex)));
                                    this.CalcHistPrior(this.structure, PostM, PostV, d, ref PriorM, ref PriorV);
                                    if ((updatemode) && (Index == MinIndex)) pp.ClearLastDeg();
                                }
                                else {
                                    if (FirstDeg == 0) {
                                        FirstDeg = Index;
                                        pp.SetLearnNDeg(N);
                                        pp.SetLearnSDeg(S);
                                        pp.SetMeanDeg(PostM);
                                        pp.SetVarDeg(PostV);
                                        pp.SetLastDegPN(PN);
                                    }
                                    this.CalcHistPrior(this.structure, PostM, PostV, pp.DFNormal(), ref PriorM, ref PriorV);
                                    pp.AddDFAlter(PN, pp.DFNormal());
                                }

                                //     ONE STEP FORECAST

                                this.CalcOneStepFcst(this.structure, PriorM, PriorV, S, ref f, ref Q);
                                error = observation - f;
                            }
                        }
                        else
                            ModelDeg = false;

                        //     POSTERIOR AT t

                        this.CalcHistPost(St, PriorM, PriorV, d, Q, error, ref adapcoef_cvector, ref PostM, ref PostV, ref S, ref N);
                        if (!updatemode) EvoVar.Add(Math.Pow(Math.Sqrt(Q) * plength, 2)); // Añade la Varianza de Evolución NO NORMALIZADA
                        if ((!updatemode) && (Index >= this.ots.Obs.Count - STFH - NOSTFE) && (Index < this.ots.Obs.Count - STFH))
                            StdErr.Add(stat.StdError(this.GetYtk(PN + 1, this.GetShortTermFcstHorizon()).ToArray(), this.Getftk(PN, PostM, this.GetShortTermFcstHorizon()).ToArray()));

                        if (DegStarted) pp.AddLastDegResiduals(residual); // Añade el residuo a los residuos que han degenerado
                        else if (!ModelDeg) pp.GetResidualFrq().AddAbs(residual);
                    }
                    else
                        pp.IncrementZeroErrorPeriods();
                    // Seteamos los valores del modelo si lo estamos creando
                    if (!updatemode) {
                        MV.NNMean = f * plength;
                        MV.NNVar = Q * plength;
                        MV.NMean = f;
                        MV.NVar = Q;
                        MV.dof = N;
                        MV.S = S;
                        pp.SetModelValue(PN, MV);
                    }

                    if ((!updatemode) && ((PN >= this.ots.Obs[this.ots.Obs.Count - STFH - NOSTFE]) && (PN < this.ots.Obs[this.ots.Obs.Count - STFH])))
                        pp.AddPostM(PN, PostM);
                } // Fin del            if (!this.IsMissing(PN))
                else MissingObs++;
            }  // for (time = 0; ...

            //  Seteamos la distribución posterior al ultimo periodo dentro del Prospect Parama
            pp.SetMean(PostM);
            pp.SetVar(PostV);
            pp.SetLearnS(S);
            pp.SetLearnN(N);

            if (!updatemode) {
                pp.VarMean = stat.Mean(EvoVar.ToArray());
                pp.VarIndex = Math.Sqrt(pp.VarMean) / ots.Moments[0][1];
                pp.StdError = stat.Mean(StdErr.ToArray());
            }
            pp.SetMSE(-1.0); 
        }

        private void CalcStdErrorOverTime(CandidatePars pp) {
            int STFH = this.GetShortTermFcstHorizon(), NOSTFE = this.GetNObsForShortTermFcstError();
            int FirstPN = this.ots.Obs.Count - STFH - NOSTFE;
            // Matrix which contains the Standard Error over time (row,column)
            // Has got as many rows as periods of the short term forecasting horizon
            // Has got as many columns as number of observations to calculate the standar error
            // Value Vrc (Value of row r, column c) has got the Estandar error measured using the next r periods
            // when the model was updated till period LastPeriodNumber - STFH - NOSTFE + c, where
            // STFH is the SHort term forecasting horizon and NOSTFE the number of observations to obtained
            double[,] STFError = new double[STFH, NOSTFE];
            int t = 0;
            for (int c = 0; c < NOSTFE; c++)
                for (int r = 0; r < STFH; r++) {
                    t = this.ots.Obs[FirstPN + c];
                    if (!IsOutlierOrMasked(t))
                        STFError[r, c] = stat.StdError(this.GetYtk(t, r + 1).ToArray(), this.Getftk(t, pp.GetPostM(t), r + 1).ToArray());
                }

            //  Seteamos el Error Standar  dentro del Prospect Parama
            for (int i = 0; i < STFH; i++) {
                double[] aux = new double[NOSTFE];
                for (int j = 0; j < NOSTFE; j++) aux[j] = STFError[i, j];
                pp.seovertime.Add(stat.Mean(aux) + Math.Sqrt(2 * stat.VarMuestral(aux)));
            }
        }

        internal List<double> Getftk(int PN, double[,] PostM, int k) {
            List<double> fk = new List<double>();
            int NActiveRegressor = 0;

            int n = this.GetLeftOutliersOrMasked(PN), MissingObs = 0;
            for (int i = 1 + n; i <= k + n + MissingObs; i++)
                if (!this.IsMissing(PN + i)) {
                    if (NActiveRegressor != 0) this.structure.CalcFMatrix(PN + i);
                    fk.Add(MatrixOp.ToScalar(MatrixOp.Multiply(MatrixOp.Transpose(this.structure.GetMatrixF()),
                                                               MatrixOp.Multiply(this.structure.GetMatrixGk(i - MissingObs),
                                                                                 PostM))));
                }
                else MissingObs++;
            if (NActiveRegressor != 0) this.structure.CalcFMatrix(PN);
            return fk;
        }

        internal int GetLeftOutliersOrMasked(int PN) {
            int i = 1, noutliers = 0;
            if ((PN == this.GetFirstPNumber()) || (!this.IsOutlierOrMasked(PN))) return 0;
            while (this.IsOutlierOrMasked(PN - i)) noutliers++;
            return noutliers + 1;
        }

        internal List<double> GetYtk(int PN, int k) {
            List<double> Ytk = new List<double>();
            List<int> ObsList = this.ots.GetNextObs(PN, k);
            for (int i = 0; i < ObsList.Count; i++)
                Ytk.Add(this.GetTS(ObsList[i]).NValue);
            return Ytk;
        }

        private void CalcForecastDistributions(int NPeriod, CandidatePars pp) {
            int LastPN = this.GetLastPNumber();
            int ZeroErrorPeriods = pp.GetZeroErrorPeriods();
            if (NPeriod <= LastPN)
                throw new ArgumentException("Error_on_DLModel_CalcForecastDistributions_____Imposible_to_set_the_forecast_of_a_Prospect_Param_for_a_historical_value");
            double[,] PriorMean, PriorVar, W_Matrix;
            int NParams = this.structure.GetNActiveParams();
            int Period = NPeriod - this.FirstPN;
            bool HistPeriod = (NPeriod <= LastPN ? true : false);
            ModelValue FV; FV.dof = pp.GetLearnN(); FV.S = pp.GetLearnS();

            PriorMean = new double[NParams, 1];
            PriorVar = new double[NParams, NParams];

            pp.InitModelFcst();
            //    First, we start the calculation when df=1, as the rest of the periods the calculation is different
            //    When df = 1
            DLModel st = pp.GetStructure();
            PriorMean = MatrixOp.Multiply(st.GetMatrixG(), pp.GetMean());
            PriorVar = MatrixOp.MultiplyT(MatrixOp.Multiply(st.GetMatrixG(), pp.GetVar()),
                                          MatrixOp.Transpose(st.GetMatrixG()));
            W_Matrix = MatrixOp.Clone(PriorVar);

            //     For Matrix W_Matrix, divide each component submatrix by it Discount Factor

            st.ApplyDiscFactors(W_Matrix, pp.DFNormal(), true);

            // We now calculate Rt(df)
            PriorVar = MatrixOp.Addition(PriorVar, W_Matrix);

            // Now we can calculate the FORECAST probability distribution of the 
            // observed variable

            FV.NMean = MatrixOp.ToScalar(MatrixOp.Multiply(MatrixOp.Transpose(st.GetMatrixF()), PriorMean));
            FV.NVar = MatrixOp.ToScalar(MatrixOp.Multiply(MatrixOp.Transpose(st.GetMatrixF()),MatrixOp.Multiply(PriorVar,  st.GetMatrixF()))) + pp.GetLearnS();

            FV.NNMean = FV.NMean * this.period_length[0];
            FV.NNVar = FV.NVar * this.period_length[0];
            if (ZeroErrorPeriods == 0)
                pp.SetModelFcst(NPeriod, FV);

            //
            //   Now we calculate for t=1..N, which is always the same.
            //   It is easier as Wt+1 is already calculated and there is no need
            //   to update such matrix over time (Wt+1=Wt+2=...=Wt+n)
            for (int p = 1; p < this.nfutureobservations + ZeroErrorPeriods; p++) {
                if ((p - ZeroErrorPeriods < 0) || (this.period_length[p - ZeroErrorPeriods] != 0.0)) {
                    // To calculate the Parameter Distribution from df=1 to n
                    PriorMean = MatrixOp.Multiply(st.GetMatrixG(), PriorMean);
                    //PriorVar = st.GetMatrixG().Multiply(PriorVar).Multiply(st.GetMatrixG().Transpose());
                    PriorVar = MatrixOp.Multiply(st.GetMatrixG(),
                                                 MatrixOp.Multiply(PriorVar,
                                                                   MatrixOp.Transpose(st.GetMatrixG())));
                    PriorVar = MatrixOp.Addition(PriorVar, W_Matrix);

                    // To calculate the Forecast distribution of the observed variable from df=1 to N
               
                    FV.NMean = MatrixOp.ToScalar(MatrixOp.Multiply(MatrixOp.Transpose(st.GetMatrixF()), PriorMean));
                    FV.NVar = MatrixOp.ToScalar(MatrixOp.Multiply(MatrixOp.Transpose(st.GetMatrixF()),  MatrixOp.Multiply(PriorVar,  st.GetMatrixF()))) + pp.GetLearnS();

                    if (p >= ZeroErrorPeriods) {
                        FV.NNMean = FV.NMean * this.period_length[p - ZeroErrorPeriods];
                        FV.NNVar = FV.NVar * this.period_length[p - ZeroErrorPeriods];
                        pp.SetModelFcst(NPeriod + p - ZeroErrorPeriods, FV);
                    }
                }
                else {
                    FV.dof = 0.0;
                    FV.NMean = 0.0;
                    FV.NNMean = 0.0;
                    FV.NNVar = 0.0;
                    FV.NVar = 0.0;
                    FV.S = 0.0;
                    pp.SetModelFcst(NPeriod + p - ZeroErrorPeriods, FV);
                    FV.dof = pp.GetLearnN(); FV.S = pp.GetLearnS();
                }
            }
        }
   
        internal void CalcMomentsForAll(int FirstPN) {
            this.ots.CalcMoments(FirstPN);
        }

        private bool SetComponets() {
            return this.structure.RestrictModelToNObs();
        }

        private double GetMaxValue(double[,] PostMean) {
            ArrayList lp = this.structure.GetParamsListActive();
            double MaxValue = 0;

            foreach (DLMParameter p in lp) {
                switch (p.GetParamType()) {
                    case (CompType._Pol):
                        if (p.GetParamName() == "Level") MaxValue += PostMean[p.GetParamNumber(), 0];
                        else
                            if (PostMean[p.GetParamNumber(), 0] > 0)
                                MaxValue += PostMean[p.GetParamNumber(), 0];
                        break;

                    case (CompType._Sea):
                        MaxValue += Math.Abs(PostMean[p.GetParamNumber(), 0]);
                        break;

                }
            }
            return MaxValue;
        }

        private double[] ConvertToDoublePositive(List<double> data) {
            double mindata = 0;
            double[] v = data.ToArray();

            for (int i = 0; i < data.Count; i++)
                if (v[i] < mindata) mindata = v[i];

            for (int i = 0; i < data.Count; i++)
                v[i] += mindata;
            return v;
        }

        private int DetectOutliers(int initialPeriod, int iterations) {
            List<int> outlierPeriods = new List<int>();
            double prob = this.GetOutlierProbabilityThreshold();
            double margen = 20;  //porcentaje de tolerancia

            List<ObsValue> datos = new List<ObsValue>();
            List<double> valores = new List<double>();
            foreach (int t in this.ots.Obs) {
                if (this.ots.TS[t].PNumber >= initialPeriod) {
                    datos.Add(this.ots.TS[t]);
                    valores.Add(this.ots.TS[t].NNValue);
                }
            }

            JacknifeEstimation jack = new JacknifeEstimation(valores, 25.0, true);
            for (int d = 0; d < datos.Count; d++) {
                if (outlierDetection.GrubbsTest(valores.ToArray(), d, prob, margen) && jack.IsOutlier(d, prob)) {
                    outlierPeriods.Add(datos[d].PNumber);
                    this.SetTSType(datos[d].PNumber, ObsType._SystemDef);
                }
            }

            return outlierPeriods.Count;
        }

        private bool IsOutlier(double value, double prob) {
            //si es sospechoso por Grubbs c/margen, y lo confirma jacknife
            JacknifeEstimation jack = new JacknifeEstimation(dataFrequencies, 25.0, true);
            double margen = 20.0;
            if (outlierDetection.GrubbsTest(dataFrequencies, value, prob, margen) && jack.IsOutlier(value, prob))
                return true;
            return false;
        }

        private void LoadDataFrequencies(double[] data) {
            dataFrequencies = new Frequencies();
            dataFrequencies.AddRange(data);
        }

        #endregion

        #region Inner classes


        internal struct ObsValue {
            internal double NNValue;
            internal double NValue;
            internal double PLength;
            internal double Value01;
            internal ObsType ObsType;
            internal int PNumber;
        }

        #endregion

    }

    #region Clase FuncionObjetivo

    internal class FObjetivoDF : IMeritFunction {


        internal FObjetivoDF(DLModel Structure, int StartPN, double[,] PostMean, double[,] PostVar, double[] NormalDF, Frequencies TblFrqResiduos, double ModelDegProb, double S, double N) {
            this.Structure = Structure;
            this.StartPN = StartPN;
            this.PostMean = PostMean;
            this.PostVar = PostVar;
            this.NormalDF = NormalDF;
            this.TblFrqRes = TblFrqResiduos;
            this.ModelDegProb = ModelDegProb;
            this.S = S;
            this.N = N;
        }

        internal DLModel Structure;
        internal int StartPN;
        internal double[,] PostMean;
        internal double[,] PostVar;
        internal double[] NormalDF;
        internal double[] AltDF;
        internal Frequencies TblFrqRes;
        internal double ModelDegProb;
        internal double S;
        internal double N;

        double IMeritFunction.Evaluate(double[][] gens, object datos) {
            StatForecast dlm = this.Structure.GetDLM();
            double[] Eval = dlm.EvolveDLM(this.Structure,
                this.StartPN, this.PostMean, this.PostVar, this.NormalDF, gens[0], this.TblFrqRes,
                dlm.GetOutlierProbabilityThreshold(), this.S, this.N);

            return Eval[3] * 10.0e9 + Eval[1];
        }

        bool IMeritFunction.GetResult() { return true; }
   }

    #endregion

    #region Clase Validacion

    internal class ValidationDF : IValidation {
        private DLModel Structure;
        internal int StartPN;
        internal double[,] PostMean;
        internal double[,] PostVar;
        internal double[] NormalDF;
        internal double[] AltDF;
        internal Frequencies TblFrqRes;
        internal double ModelDegProb;
        internal double S;
        internal double N;

        internal ValidationDF(DLModel Structure, int StartPN, double[,] PostMean, double[,] PostVar,
                            double[] NormalDF, double[] AltDF, Frequencies TblFrqResiduos, double ModelDegProb, double S, double N) {
            this.Structure = Structure;
            this.StartPN = StartPN;
            this.PostMean = PostMean;
            this.PostVar = PostVar;
            this.NormalDF = NormalDF;
            this.AltDF = AltDF;
            this.TblFrqRes = TblFrqResiduos;
            this.ModelDegProb = ModelDegProb;
            this.S = S;
            this.N = N;
        }

        bool IValidation.IsValid(double[][] gens, object parametro) {
            StatForecast dlm = this.Structure.GetDLM();
            double[] E = dlm.EvolveDLM(this.Structure, this.StartPN, this.PostMean, this.PostVar, this.NormalDF, this.AltDF, this.TblFrqRes, dlm.GetOutlierProbabilityThreshold(), this.S, this.N);
            return ((E[0] != -1) || (E[1] != -1) || (E[2] != -1));
        }

    }

    #endregion

    #region Enums

    internal enum CompType { _Pol = 0, _Sea = 1, _Reg = 2, _Var = 3 }

    internal enum ObsType { _Usable = 0, _UserDef = 1, _SystemDef = 2, _Degradation = 3, _DegradationST = 4, _DegradationLT = 5, _MissingObs = 6 }

    internal enum CompInfo { _Type = 0, _Description = 1, _NComps = 2, _IniParam = 3, _Nparams = 4, _Active = 5, _RegNumber = 6, _RegCentered = 7, _ParentReg = 8 }

    internal enum FcstType { _NFcst = 0, _NVar = 1, _NNFcst = 2, _NNVar = 3 }

    internal enum CompStatus { _Updated = 0, _Pending = 1 }

    internal enum DistType { _ST = 0, _LT = 1, _MM = 2, _ID = 3 }

    internal enum Result { Ok = 0, BadSettings = -2, AllOutliers = -3, RandomDemand = -4, NonForecastable = -5, NoInitialDistribution = -6, BadQualityFcst = -7, NotEnoughHistory = -8 }

    internal enum UpdateResult { Ok = 0, DegSolved = 1, DegFound = 2, DegSolvedAndFound = 3, ZeroError = 4, OutlierDetected = 10, NoModel = 11, InvalidModel }

    #endregion 

}

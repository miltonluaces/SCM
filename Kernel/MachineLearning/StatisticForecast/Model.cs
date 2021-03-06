#region Imports

using System;
using System.Collections;
using System.Collections.Generic;
using Statistics;
using Maths;

#endregion

namespace MachineLearning {

    internal class Model {

        #region Fields

        private StatForecast model;
        
        private CompStatus status_G;
        private CompStatus status_F;
        private int grade;
        private int maxarm;
        private int minPerForSeason;
        private int validobs;
        private ArrayList[] PolSea;
        private double[,] F;
        private double[,] G;
        private double[,] A;
        private List<double[,]> Gk_List;

        private StatFunctions stat;

        #endregion
        
        #region Constructor

        internal Model(StatForecast Model) {
            this.model = Model;
            this.grade = Model.GetPolynomialGrade();
            this.maxarm = 6;
            this.validobs = model.GetNObservations() - Model.GetNOutliers();

            this.PolSea = new ArrayList[this.maxarm + 1];
            this.AddCompPol();
            this.AddSeasonality();
            this.status_F = CompStatus._Pending;
            this.status_G = CompStatus._Pending;
            this.stat = new StatFunctions();
        }

        #endregion

        #region Properties

        internal int MinObsForSeasonality {
            get { return minPerForSeason; }
            set { minPerForSeason = value; }
        }
        
        #endregion

        #region Internal Methods

        internal void SetGrade(int v) {
            this.grade = v;
        }

        internal void SetParamPosition(string Name, int Property, int v) {
            Parameter p = this.GetParam(Name, Property);
            if (p != null) p.SetParamNumber(v);
        }

        internal StatForecast GetDLM() {
            return this.model;
        }

        internal void SetParamDistributions(double[,] Mean, double[,] Var) {
            int NParams = this.GetNActiveParams();
            double[,] IMean = new double[NParams, 1];
            double[,] IVar = new double[NParams, NParams];

            foreach (ArrayList ListOfParams in this.PolSea)
                if (ListOfParams != null)
                    foreach (Parameter p in ListOfParams)
                        if (p.IsActive()) {
                            int index = p.GetParamNumber();
                            p.SetParamMean(Mean[index, 0]);
                            p.SetParamVar(Var[index, index]);
                            IMean[index, 0] = Mean[index, 0];
                        }

        }

        internal double[,] GetMatrixG() {
            return (this.G);
        }

        internal double[,] GetMatrixF() {
            return (this.F);
        }

        internal double[,] GetMatrixA() {
            return (this.A);
        }

        internal double[,] GetMatrixGk(int k) {
            if (k >= 0) return (this.Gk_List[k - 1]);
            return this.Gk_List[0];
        }

        internal ArrayList[] GetPolSeasonalComponents() {
            return this.PolSea;
        }

        internal ArrayList GetParamsListActive() {
            ArrayList l = new ArrayList();

            foreach (ArrayList ListOfParams in this.PolSea)
                if (ListOfParams != null)
                    foreach (Parameter p in ListOfParams)
                        if (p.IsActive()) l.Add(p);

            return l;
        }

        internal ArrayList GetSeasonalCosParamsListActive() {
            ArrayList l = new ArrayList();

            foreach (ArrayList ListOfParams in this.PolSea)
                if (ListOfParams != null)
                    foreach (Parameter p in ListOfParams)
                        if ((p.GetParamName() == "Cos") && p.IsActive())
                            l.Add(p);
            return l;
        }

        internal int GetNParams() {
            int NParams = 0;

            foreach (ArrayList ListOfParams in this.PolSea)
                if (ListOfParams != null) NParams += ListOfParams.Count;

            return (NParams);
        }

        internal int GetNActiveParams() {
            int NParams = 0;

            foreach (ArrayList ListOfParams in this.PolSea)
                if (ListOfParams != null)
                    if (((Parameter)ListOfParams[0]).IsActive()) NParams += ListOfParams.Count;

            return (NParams);
        }

        internal int GetNParamsPol() {
            if (((Parameter)this.PolSea[0][0]).IsActive()) return (((ArrayList)this.PolSea[0]).Count);
            else return (0);
        }

        internal int GetNActiveParamsPol() {
            int np = 0;

            if (this.PolSea[0] != null) {
                foreach (Parameter p in this.PolSea[0])
                    if (p.IsActive()) np++;
                return np;
            }
            else return 0;
        }

        internal int GetNParamsSea() {
            int NParams = 0;

            for (int i = 1; i < this.PolSea.Length; i++)
                NParams += ((ArrayList)this.PolSea[i]).Count;

            return (NParams);
        }

        internal int GetNActiveParamsSea() {
            int NParams = 0;

            for (int i = 1; i < this.PolSea.Length; i++)
                if (((Parameter)this.PolSea[i][0]).IsActive())
                    NParams += ((ArrayList)this.PolSea[i]).Count;

            return (NParams);
        }

        internal int GetNArmonics() {
            int NArm = 0;

            for (int i = 1; i < this.PolSea.Length; i++) NArm += 1;
            return (NArm);
        }

        internal int GetNActiveArmonics() {
            int NArm = 0;

            for (int i = 1; i < this.PolSea.Length; i++)
                if (((Parameter)this.PolSea[i][0]).IsActive())
                    NArm += 1;

            return (NArm);
        }

        internal Parameter GetParam(string Name, int Property) {
            if (((Name == "Level") || (Name == "Trend")) && (this.PolSea[0] != null))
                foreach (Parameter p in this.PolSea[0])
                    if (p.GetParamName() == Name) return p;
            if (((Name == "Cos") || (Name == "Sin")) && (this.PolSea[Property] != null))
                foreach (Parameter p in this.PolSea[Property])
                    if (p.GetParamName() == Name) return p;
            if (Name == "Sin") return null;
            return null;
        }

        internal bool RestrictModelToNObs() {
            int nMaxArm = 6;
            int MinObsSea = 2 * (nMaxArm - 1) + 1;
            int MinObsReg = 0;
            int UsrMinObsModel = 2;
            int ValidObs = (this.model.GetLastPNumber() - this.model.GetFPNtoForecast()) - this.model.GetNOutliers();
            int ValidObsTmp = ValidObs;
            bool Haschanged = false;

            if (ValidObs < UsrMinObsModel) return false;

            if (!((Parameter)this.PolSea[0][0]).IsActive()) Haschanged = true;
            this.ActivatePol();
            ValidObsTmp -= 2;

            if ((ValidObs >= UsrMinObsModel) && (ValidObs <= UsrMinObsModel + MinObsSea + MinObsReg)) {
                if (this.GetNActiveArmonics() == 0)
                    Haschanged = false;
                else {
                    this.DeActivateArmonic(-1);
                    Haschanged = true;
                }
            }


            for (int i = this.PolSea.Length - 1; i > 0; i--)
        	{
                if (ValidObsTmp <= 0)
                    break;
                Parameter p = (Parameter)this.PolSea[i][0];
                this.ActivateArmonic(p.GetArmonicNumber());
                ValidObsTmp -= 2;
                p.SetActive();
            }
            return Haschanged;
        }

        internal int[] GetActiveRows() {
            int[] ActRows = new int[this.GetNActiveParams()];
            foreach (int i in ActRows) ActRows[i] = (int)0;

            foreach (ArrayList ListOfParams in this.PolSea)
                if (((Parameter)ListOfParams[0]).IsActive()) ActRows[((Parameter)ListOfParams[0]).GetParamNumber()] = 1;

            return ActRows;
        }

        internal void CalcGMatrix() {
            if (this.status_G == CompStatus._Pending) {
                int NParams = this.GetNActiveParams();
                this.G = new double[NParams, NParams];
                int index = 0;

                foreach (ArrayList ListOfParams in this.PolSea) {
                    if (((Parameter)ListOfParams[0]).IsActive()) {

                        index = ((Parameter)ListOfParams[0]).GetParamNumber();
                        switch (((Parameter)ListOfParams[0]).GetParamType()) {
                            case (CompType._Pol):
                                int grade = ((ArrayList)ListOfParams).Count;
                                for (int r = 0; r < grade; r++)
                                    for (int c = r; c < grade; c++)
                                        this.G[index + r, index + c] = 1.0;
                                break;

                            case (CompType._Sea):
                                double w = 2.0 * Math.PI / (double)(2.0 * this.maxarm);
                                int armnumber = ((Parameter)ListOfParams[0]).GetArmonicNumber();
                                this.G[index, index] = stat.RoundTrigFunc(Math.Cos(armnumber * w));
                                if (((ArrayList)ListOfParams).Count != 1) {
                                    this.G[index, index + 1] = stat.RoundTrigFunc(Math.Sin(armnumber * w));
                                    this.G[index + 1, index + 1] = stat.RoundTrigFunc(Math.Cos(armnumber * w));
                                    this.G[index + 1, index] = stat.RoundTrigFunc(-Math.Sin(armnumber * w));
                                }
                                break;
                        }
                    }
                }  

                this.status_G = CompStatus._Updated;
            }
        }

        internal void CalcFMatrix(int PNumber) {
            if (this.status_F == CompStatus._Updated)
                return;

            if (this.status_F == CompStatus._Pending) {
                this.F = new double[this.GetNActiveParams(), 1];

                int FirstObs = (this.model.GetFPNtoForecast() < this.model.GetFirstPNumber()) ? this.model.GetFirstPNumber() : this.model.GetFPNtoForecast();

                if (this.PolSea != null)
                    foreach (ArrayList ListOfParams in this.PolSea)
                        if (ListOfParams != null)
                            if (((Parameter)ListOfParams[0]).IsActive())
                                foreach (Parameter p in ListOfParams)
                                    this.F[p.GetParamNumber(), 0] =
                                        (((p.GetParamName() == "Level") ||
                                        (p.GetParamName() == "Cos")) ? 1.0 : 0.0);

                this.status_F = CompStatus._Updated;
            }
        }

        internal void CalcMatrixA(bool OutliersConsidered) {
            int col = 0;
            int non_exp;
            int nmaxarm = 6;
            int LastObsIndex = this.model.GetLastObsIndex();
            int FirstPeriodNumber = this.model.GetFPNtoForecast(), LastPeriodNumber = this.model.GetLastPNumber();
            double w;
            double[,] Matrix_A = new double[LastPeriodNumber - FirstPeriodNumber + 1, this.GetNActiveParams()];

            foreach (ArrayList ListOfParams in this.PolSea) {
                Parameter p = ((Parameter)ListOfParams[0]);
                if (p.IsActive()) {
                    col = p.GetParamNumber();
                    switch (p.GetParamType()) {
                        // For Polinomial component
                        case (CompType._Pol):
                            for (int r = this.model.GetObsPN(0); r <= this.model.GetObsPN(LastObsIndex); r++)
                                if (!this.model.IsMissing(r)) {
                                    non_exp = (OutliersConsidered ? (this.model.IsOutlierOrMasked(r) ? 0 : 1) : (this.model.IsMaskedObservation(r) ? 0 : 1));
                                    Matrix_A[r - FirstPeriodNumber, col] = 1.0 * non_exp;
                                    if (this.model.GetPolynomialGrade() > 1)
                                        Matrix_A[r - FirstPeriodNumber, col + 1] = -(LastPeriodNumber - r) * non_exp;
                                }
                            break;

                        // For seasonal component
                        case (CompType._Sea):
                            if (ListOfParams.Count == 1) {
                                // For Nyquist armonic
                                for (int r = this.model.GetObsPN(0); r <= this.model.GetObsPN(LastObsIndex); r++)
                                    if (!this.model.IsMissing(r)) {
                                        non_exp = (OutliersConsidered ? (this.model.IsOutlierOrMasked(r) ? 0 : 1) : (this.model.IsMaskedObservation(r) ? 0 : 1));
                                        w = nmaxarm * 2 * Math.PI / (double)(2 * nmaxarm);
                                        Matrix_A[r - FirstPeriodNumber, col] = stat.RoundTrigFunc(Math.Cos(-(LastPeriodNumber - r) * w)) * non_exp;
                                    }
                            }
                            else {
                                for (int r = this.model.GetObsPN(0); r <= this.model.GetObsPN(LastObsIndex); r++)
                                    if (!this.model.IsMissing(r)) {
                                        non_exp = (OutliersConsidered ? (this.model.IsOutlierOrMasked(r) ? 0 : 1) : (this.model.IsMaskedObservation(r) ? 0 : 1));
                                        w = p.GetArmonicNumber() * 2 * Math.PI / (double)(2 * nmaxarm);
                                        Matrix_A[r - FirstPeriodNumber, col] = stat.RoundTrigFunc(Math.Cos(-(LastPeriodNumber - r) * w)) * non_exp;
                                        Matrix_A[r - FirstPeriodNumber, col + 1] = stat.RoundTrigFunc(Math.Sin(-(LastPeriodNumber - r) * w)) * non_exp;
                                    }
                            }
                            break;
                    }
                }
            }  

            this.A = Matrix_A;
        }

        internal void CalcMatrixGk(int k) {
            this.Gk_List = new List<double[,]>();
            this.CalcGMatrix();
            double[,] G = MatrixOp.Clone(this.G);
            (this.Gk_List).Add(G);
            for (int i = 1; i <= k; i++) {
                double[,] Gk = MatrixOp.Clone(Gk_List[i - 1]);
                Gk_List.Add(MatrixOp.Multiply(Gk, this.G));
            }
        }

        internal int ApplyDiscFactors(double[,] M, double[] DF, bool CalcFcst) {
            int NParams = 0, initial, row;
            Parameter p;

            foreach (ArrayList ListOfParams in this.PolSea) {
                p = ((Parameter)ListOfParams[0]);
                if (p.IsActive()) {
                    if (p.GetParamType() == CompType._Pol) row = 0;
                    else row = 1;
                    initial = p.GetParamNumber();
                    foreach (Parameter x in ListOfParams) {
                        if (CalcFcst)
                            M[x.GetParamNumber(), x.GetParamNumber()] *= (1.0 - DF[row]) / DF[row];
                        else
                            M[x.GetParamNumber(), x.GetParamNumber()] *= 1.0 / DF[row];

                    }
                }
            }

            return NParams;
        }

        internal void DeActivateModel() {
            if (this.PolSea != null)
                foreach (ArrayList ListOfParams in this.PolSea)
                    if (ListOfParams != null)
                        foreach (Parameter p in ListOfParams)
                            p.SetNonActive();

            this.ReCalcParamNumber();
            this.status_G = CompStatus._Pending;
            this.status_F = CompStatus._Pending;
        }

        internal void ActivatePol() {
            foreach (Parameter p in this.PolSea[0])
                p.SetActive();
            this.ReCalcParamNumber();

            this.status_G = CompStatus._Pending;
            this.status_F = CompStatus._Pending;
        }

        internal void DeActivatePol(int NARM) {
            foreach (Parameter p in this.PolSea[0])
                p.SetNonActive();
            this.ReCalcParamNumber();

            this.status_G = CompStatus._Pending;
            this.status_F = CompStatus._Pending;
        }

        internal void ActivateArmonic(int NARM) {
            if (NARM == -1)
                foreach (ArrayList ListOfParams in this.PolSea)
                    foreach (Parameter p in ListOfParams)
                        p.SetActive();
            else
                foreach (Parameter p in this.PolSea[NARM])
                    p.SetActive();
            this.ReCalcParamNumber();

            this.status_G = CompStatus._Pending;
            this.status_F = CompStatus._Pending;
        }

        internal void DeActivateArmonic(int NARM) {
            ArrayList L = new ArrayList();

            if (NARM == -1) {
                for (int i = 1; i < this.PolSea.Length; i++)
                    if (this.PolSea[NARM] != null)
                        foreach (Parameter p in this.PolSea[NARM]) {
                            p.SetNonActive();
                            L.Add(p.GetParamNumber());
                        }
            }
            else {
                if (this.PolSea[NARM] != null)
                    foreach (Parameter p in this.PolSea[NARM]) {
                        p.SetNonActive();
                        L.Add(p.GetParamNumber());
                    }
            }
            this.DeleteFromIniDist(L);
            this.ReCalcParamNumber();
            this.status_G = CompStatus._Pending;
            this.status_F = CompStatus._Pending;
        }

        internal void SelectArmonics(bool TestChi2) {
            // armscores [] [0] = p_valor del armonico
            // armscores [] [1] = probabilidad (en 100%) del armonico
            // armscores [] [2] = Amplitud del armónico
            // armscores [] [3] = % de informacion que añade el armonico
            // armscores [] [4] = Numero del armonico
            double[,] C, m;
            int col, T = this.maxarm, narm = 0;
            double p_value, probability, sumamplitud = 0.0;
            double[][] armscores = new double[T][];
            for (int i = 0; i < T; i++) armscores[i] = new double[5];

            for (int i = 1; i < this.PolSea.Length; i++) {
                ArrayList ListOfParams = this.PolSea[i];
                Parameter p = ((Parameter)ListOfParams[0]);
                if (p.IsActive() && (p.GetParamType() == CompType._Sea)) {
                    col = p.GetParamNumber();
                    armscores[narm][4] = p.GetArmonicNumber();
                    if (ListOfParams.Count == 1) {
                        p_value = Math.Pow(this.model.GetInitialMean()[col, 0], 2) /
                            this.model.GetInitialVar()[col, col];
                        p_value = (p_value < 0 ? 999999 : p_value);
                        if (TestChi2) probability = stat.Test_Chi2(p_value, 1);
                        else probability = stat.Test_F(p_value, 1, this.model.GetDof());
                        armscores[narm][2] = Math.Abs(this.model.GetInitialMean()[col, 0]);
                    }
                    else {
                        IMatrix Aux = new Matrix(this.model.GetInitialMean());
                        m = MatrixOp.Clone(Aux.Submatrix(col, col + 1, 0, 0).ConvertToDouble());
                        IMatrix Aux1 = new Matrix(this.model.GetInitialVar());
                        C = MatrixOp.Clone(Aux1.Submatrix(col, col + 1, col, col + 1).ConvertToDouble());
                        IMatrix Aux3 = new Matrix(C);
                        if (Aux3.Determinant != 0) {
                            p_value = MatrixOp.ToScalar(MatrixOp.Multiply(MatrixOp.Transpose(m), MatrixOp.Multiply(Aux3.Inverse.ConvertToDouble(), m)));
                            p_value = (p_value < 0 ? 999999 : p_value);
                            if (TestChi2) probability = stat.Test_Chi2(p_value, 2);
                            else probability = stat.Test_F(p_value, 2, this.model.GetDof());
                            armscores[narm][2] = Math.Sqrt((Math.Pow(m[0, 0], 2) + Math.Pow(m[1, 0], 2)));
                        }
                        else {
                            armscores[narm][2] = Math.Sqrt((Math.Pow(m[0, 0], 2) + Math.Pow(m[1, 0], 2)));
                            p_value = 0.0;
                            probability = 0.0;
                        }
                    }
                    armscores[narm][0] = p_value;
                    armscores[narm++][1] = (p_value != 999999 ? probability * 100.0 : 0.0);
                }
            }

            for (int i = 0; i < narm; i++) sumamplitud += armscores[i][2];
            for (int i = 0; i < narm; i++) armscores[i][3] = armscores[i][2] / sumamplitud * 100;
            for (int i = 0; i < narm; i++) {
                if ((armscores[i][1] > this.model.GetSeasonalityConfidence())) {
                    this.DeActivateArmonic((int)armscores[i][4]);
                    this.status_G = CompStatus._Pending;
                    this.status_F = CompStatus._Pending;
                }
            }
        }

        #endregion
 
        #region Private Methods

        internal void AddCompPol() {
            if (this.PolSea == null)
                this.PolSea = new ArrayList[this.maxarm + 1];

            if ((this.grade > 2) || (this.grade < 1))
                throw new ArgumentException("A_polinomial_component_of_grade___0__cannot_be_added__The_only_possible_values_are_1__just_level__or_2__level_and_trend");

            this.PolSea[0] = new ArrayList();

            if (this.grade == 1)
                this.PolSea[0].Add(new Parameter("Level", CompType._Pol, 1));

            if (this.grade == 2) {
                this.PolSea[0].Add(new Parameter("Level", CompType._Pol, 2));
                this.PolSea[0].Add(new Parameter("Trend", CompType._Pol, 2));
            }
        }

        private void AddSeasonality() {

            if (this.PolSea == null)
                this.PolSea = new ArrayList[this.maxarm];

            for (int i = 1; i <= this.maxarm; i++) this.AddCompSea(i);
        }

        internal bool AddCompSea(int NARM) {
            if (this.PolSea[NARM] != null)
                throw new ArgumentException(string.Format("Armonic_number__0__cannot_be_added__The_maximum_number_of_armonic_to_be_added_is__1"));

            this.PolSea[NARM] = new ArrayList();

            if (NARM == this.maxarm)
                this.PolSea[NARM].Add(new Parameter("Cos", CompType._Sea, NARM));
            else {
                this.PolSea[NARM].Add(new Parameter("Cos", CompType._Sea, NARM));
                this.PolSea[NARM].Add(new Parameter("Sin", CompType._Sea, NARM));
            }
            return true;
        }

        private void ReCalcParamNumber() {
            int ParamNumber = 0;

            if (this.PolSea != null) {
                foreach (ArrayList ListOfParams in this.PolSea) {
                    if (ListOfParams != null) {
                        foreach (Parameter p in ListOfParams) {
                            if (p.IsActive()) {
                                p.SetParamNumber(ParamNumber);
                                ParamNumber++;
                            }
                        }
                    }
                }
            }
        }

        private void DeleteFromIniDist(ArrayList L) {
            int OriginalSize = MatrixOp.GetCols(this.model.GetInitialVar());
            int RowsToBeDeleted = L.Count;
            int NewSize = OriginalSize - RowsToBeDeleted;
            int[] index = new int[NewSize];

            // On index [] we get all indexes which have to be copy to the new matrixes
            int j = 0;
            for (int i = 0; i < OriginalSize; i++)
                if (!L.Contains(i)) index[j++] = i;

            IMatrix Aux = new Matrix(this.model.GetInitialMean());
            this.model.SetInitialMean(Aux.Submatrix(index, 0, 0).ConvertToDouble());
            IMatrix Aux1 = new Matrix(this.model.GetInitialVar());
            this.model.SetInitialVar(Aux1.Submatrix(index, index).ConvertToDouble());
        }

        private void DeleteFromIniDist(int IndexToDelete) {
            int OriginalSize = MatrixOp.GetCols(this.model.GetInitialVar());
            int NewSize = OriginalSize - 1;
            int[] index = new int[NewSize];

            // On index [] we get all indexes which have to be copy to the new matrixes
            int j = 0;
            for (int i = 0; i < OriginalSize; i++)
                if (i != IndexToDelete) index[j++] = i;

            IMatrix Aux = new Matrix(this.model.GetInitialMean());
            this.model.SetInitialMean(Aux.Submatrix(index, 0, 0).ConvertToDouble());
            IMatrix Aux1 = new Matrix(this.model.GetInitialVar());
            this.model.SetInitialVar(Aux1.Submatrix(index, index).ConvertToDouble());

        }

        #endregion

    }
}

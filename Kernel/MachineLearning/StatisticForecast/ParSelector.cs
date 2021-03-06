#region Imports

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using Maths;
using Statistics;

#endregion

namespace MachineLearning {

    internal class ParSelector {

        #region Fields

        private StatForecast dlm;
        private double[] polDF;
        private double[] seaDF;
        private double[] regDF;
        private double[] varDF;
        private CandidatePars bestPPLT;
        private CandidatePars bestPPST;
        private RndGenerator randGen;

        #endregion

        #region Constructor

        internal ParSelector(StatForecast dlm) {
            this.dlm = dlm;
            polDF = dlm.GetDF(CompType._Pol);
            seaDF = dlm.GetDF(CompType._Sea);
            regDF = dlm.GetDF(CompType._Reg);
            varDF = dlm.GetDF(CompType._Var);
            this.randGen = new RndGenerator();
        }

        #endregion

        #region Internal Methods

        internal Result SelectDF(int bruteForceMaxIte) {
            int maxSea = 1;
            int maxReg = 1;
            if (dlm.GetModelStructure().GetNActiveArmonics() != 0) { maxSea = (int)seaDF[0]; }
            int prod = (int)polDF[0] * maxSea * maxReg * (int)varDF[0];
            List<CandidatePars> pps = null;
            pps = GetCandidatePPs();
            if (prod > bruteForceMaxIte) {
                List<CandidatePars> randomPps = new List<CandidatePars>();
                randGen.Reset();
                for (int i = 0; i < bruteForceMaxIte; i++) {
                    int index = randGen.NextInt(0, pps.Count - 1);
                    randomPps.Add(pps[index]);
                    pps.RemoveAt(index);
                }
                pps = randomPps;
            }

            Evaluate(pps);
            return SelectPPs(pps);
        }

        internal CandidatePars BestPPST {
            get { return bestPPST; }
        }

        internal CandidatePars BestPPLT {
            get { return bestPPLT; }
        }

        #endregion

        #region Private Methods

        private List<CandidatePars> GetCandidatePPs() {
            List<CandidatePars> pps = new List<CandidatePars>();
            CandidatePars pp;
            int maxSea = 1;
            int maxReg = 1;
            if (dlm.GetModelStructure().GetNActiveArmonics() != 0) { maxSea = (int)seaDF[0]; }
     
            for (int p = 1; p <= (int)polDF[0]; p++) {
                for (int s = 1; s <= maxSea; s++) {
                    for (int r = 1; r <= maxReg; r++) {
                        for (int v = 1; v <= (int)varDF[0]; v++) {
                            pp = new CandidatePars(this.dlm.GetModelStructure(), polDF[p], seaDF[s], regDF[r], varDF[v]);
                            pps.Add(pp);
                        }
                    }
                }
            }
            return pps;
        }

        private void Evaluate(List<CandidatePars> pps) {
            ParEvaluator ppe;
            int i = 1;
            foreach (CandidatePars pp in pps) {
                ppe = new ParEvaluator(0, dlm, pp);
                ppe.Evaluar();
                i++;
            }
        }

        private Result SelectPPs(List<CandidatePars> pps) {
            List<double> BondadPronostico = new List<double>();
            List<double> VarErrorSobreVarObs = new List<double>();
            CandidatePars bestPPLT = null, bestPPST = null;
            double bestLT = double.MaxValue, bestST = double.MaxValue;
            CandidatePars pp;
            List<CandidatePars> ppsOk = new List<CandidatePars>();
            foreach (CandidatePars Pp in pps) {
                if (Pp.StdError < dlm.GetFcstLumpyDmdThreshold() && (Pp.GetResidualFrq().NFrecs > 3)) {
                    ppsOk.Add(Pp);
                    BondadPronostico.Add(Pp.StdError);
                    VarErrorSobreVarObs.Add(Pp.VarIndex);
                }
            }
            pps = ppsOk;
            if (ppsOk.Count == 0) {
                return Result.NoneSelected;
            }
            if (ppsOk.Count == 1) {
                this.bestPPST = ppsOk[0];
                this.bestPPLT = ppsOk[0];
                return Result.OneSelected;
            }
            if (ppsOk.Count == 2) {
                if (ppsOk[0].StdError <= ppsOk[1].StdError) {
                    if (ppsOk[0].VarIndex <= ppsOk[1].VarIndex) {
                        this.bestPPST = ppsOk[0];
                        this.bestPPLT = ppsOk[0];
                        return Result.OneSelected;
                    }
                    else {
                        this.bestPPST = ppsOk[0];
                        this.bestPPLT = ppsOk[1];
                        return Result.Ok;
                    }
                }
                else {
                    if (ppsOk[0].VarIndex <= ppsOk[1].VarIndex) {
                        this.bestPPST = ppsOk[1];
                        this.bestPPLT = ppsOk[0];
                        return Result.Ok;
                    }
                    else {
                        this.bestPPST = ppsOk[1];
                        this.bestPPLT = ppsOk[1];
                        return Result.OneSelected;
                    }
                }
            }

            Norma BondadPronosticoNorma = new Norma(BondadPronostico);
            Norma VarErrorSobreVarObsNorma = new Norma(VarErrorSobreVarObs);
            BondadPronosticoNorma.Normalize(Norma.NormType.minMax);
            VarErrorSobreVarObsNorma.Normalize(Norma.NormType.minMax);
            for (int i = 0; i < BondadPronosticoNorma.Norm.Length; i++) {
                pps[i].StdError = BondadPronosticoNorma.Norm[i];
                pps[i].VarIndex = VarErrorSobreVarObsNorma.Norm[i];
            }


            //Hash recíprocos
            Hashtable PPByError = new Hashtable();
            Hashtable PPByVar = new Hashtable();
            for (int i = 0; i < BondadPronosticoNorma.Norm.Length; i++) {
                PPByError[BondadPronosticoNorma.Norm[i]] = pps[i];
                PPByVar[VarErrorSobreVarObsNorma.Norm[i]] = pps[i];
            }

            double radioIncST = dlm.GetSTIncRadix();
            double radioIncLT = dlm.GetLTIncRadix();
            double radioClustST = dlm.GetSTRangeRadix();
            double radioClustLT = dlm.GetLTRangeRadix();

            //LinealClustering oneDimST = new LinealClustering(BondadPronosticoNorma.Norm, radioIncST, radioClustST);
            //oneDimST.Clustering(true, true);
            //LinealClustering oneDimLT = new LinealClustering(VarErrorSobreVarObsNorma.Norm, radioIncLT, radioClustLT);
            //oneDimLT.Clustering(true, true);


            double stabLevel = 1.0 - dlm.GetStabLevel() / 1000.0;
            double stdErrLevel = 1.0 - dlm.GetStdErrLevel() / 1000.0;
            //LinealClustering.Cluster clusterST = oneDimST.GetProxCluster(stdErrLevel);
            //LinealClustering.Cluster clusterLT = oneDimLT.GetProxCluster(stabLevel);

            /*
            foreach (double error in clusterST.Datos) {
                pp = (ProspectParams)PPByError[error];
                double IndexST = 10.0 * pp.NAlters + pp.VarIndex;
                if (IndexST < bestST) { bestST = IndexST; bestPPST = pp; }
            }
            foreach (double var in clusterLT.Datos) {
                pp = (ProspectParams)PPByVar[var];
                double IndexLT = 10.0 * pp.NAlters + pp.StdError;
                if (IndexLT < bestLT) { bestLT = IndexLT; bestPPLT = pp; }
            }
            */
            
            this.bestPPLT = bestPPLT;
            this.bestPPST = bestPPST;
    
            if (this.bestPPST == null && this.bestPPLT == null) {
                return Result.NoneSelected;
            }
            if (this.bestPPST == null) {
                this.bestPPST = this.bestPPLT;
                return Result.OneSelected;
            }
            if (this.bestPPLT == null) {
                this.bestPPLT = this.bestPPST;
                return Result.OneSelected;
            }
            return Result.Ok;
        }

        #endregion

        #region Enums

        internal enum Result { NoneSelected, OneSelected, Ok };

        #endregion
    }
}

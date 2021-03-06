#region Imports

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace MachineLearning {

    internal class ParEvaluator {

        #region Fields

        private int index;
        private StatForecast dlm;
        private CandidatePars pp;

        #endregion

        #region Constructor

        internal ParEvaluator(int index, StatForecast dlm, CandidatePars pp) {
            this.index = index;
            this.dlm = dlm;
            this.pp = pp;
        }

        #endregion

        #region Properties

        internal CandidatePars PP {
            get { return pp; }
        }

        #endregion

        #region Internal Methods

        internal void Evaluar() {
            dlm.EvolveDLM(pp, false);
        }

        #endregion

    }
}

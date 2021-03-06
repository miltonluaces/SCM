#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace MachineLearning {

    internal class Parameter {

         #region Fields

            private CompType type;
            private string componentname;
            private int PolGrade;
            private int ArmonicNumber;
            private int RegressorLag;
            private int ParamNumber;
            private bool Active;
            private bool Centered;
            private double CenteredMean;
            private double CenteredVar;
            private double ParamMean;
            private double ParamVar;

            #endregion

         #region Constructor

            internal Parameter(string NAME, CompType TYPE, int Val) {
                if (TYPE == CompType._Reg)
                    throw new ArgumentException("This_constructor_cannot_be_used_to_create_a_Regressor_parameter");

                this.type = TYPE;
                this.componentname = NAME;
                this.Centered = false;
                this.Active = false;
                this.CenteredMean = 0.0;
                this.CenteredVar = 0.0;
                this.ParamMean = 0.0;
                this.ParamVar = 0.0;
                switch (TYPE) {
                    case (CompType._Pol):
                        this.PolGrade = Val;
                        this.ArmonicNumber = -1;
                        this.RegressorLag = -1;
                        break;

                    case (CompType._Sea):
                        this.ArmonicNumber = Val;
                        this.PolGrade = -1;
                        this.RegressorLag = -1;
                        break;
                }
            }

            #endregion

            #region Internal Methods

            internal bool IsActive() {
                return (this.Active);
            }

            internal void SetActive() {
                this.Active = true;
            }

            internal void SetNonActive() {
                this.Active = false;
            }

            internal bool SetCentered() {
                if (this.type == CompType._Reg) {
                    this.Centered = true;
                    return true;
                }
                else {
                    return false;
                }
            }

            internal bool SetNonCentered() {
                if (this.type == CompType._Reg) {
                    this.Centered = false;
                    return true;
                }
                else
                    return false;
            }

            internal string GetParamName() {
                return this.componentname;
            }

            internal int GetParamNumber() {
                return this.ParamNumber;
            }

            internal CompType GetParamType() {
                return this.type;
            }

            internal int GetArmonicNumber() {
                return this.ArmonicNumber;
            }

            internal int GetLag() {
                return this.RegressorLag;
            }

            internal double GetParamMean() {
                return this.ParamMean;
            }

            internal double GetParamVar() {
                return this.ParamVar;
            }

            internal bool IsCentered() {
                return this.Centered;
            }

            internal double GetCenteredMean() {
                return this.CenteredMean;
            }

            internal double GetCenteredVar() {
                return this.CenteredVar;
            }

            internal void SetParamNumber(int v) {
                this.ParamNumber = v;
            }

            internal void SetParamMean(double v) {
                this.ParamMean = v;
            }

            internal void SetParamVar(double v) {
                this.ParamVar = v;
            }

            internal void SetCenteredMean(double v) {
                this.CenteredMean = v;
            }

            internal void SetCenteredVar(double v) {
                this.CenteredVar = v;
            }

            #endregion
    }
}

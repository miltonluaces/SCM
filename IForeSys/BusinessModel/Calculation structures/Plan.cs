#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Planning;
    
#endregion

namespace AibuSet {

    internal class Plan : DbObject {

       #region Fields

       private Sku sku;
       private bool isRoot;
       private DateTime firstDate;
       private int planHorizon;
       private int firstEndLeadTimeIndex;
       private int firstEndReplTimeIndex;
       private List<Planning.Period> periods;

       #endregion

       #region Constructor

       internal Plan() { 
       }

       #endregion

       #region Properties

       /** Method:  Sku a que pertenece el Plan  */
       internal Sku Sku {
           get { return sku; }
           set { sku = value; }
       }

       /** Method: fecha inicial del plan */
       internal DateTime FirstDate {
           get { return firstDate; }
           set { firstDate = value; }
       }

       /** Method: horizonte de planificación */
       internal int PlanHorizon {
           get { return planHorizon; }
           set { planHorizon = value; }
       }

       /** Method: si el nodo es raiz */
       internal bool IsRoot {
           get { return isRoot; }
           set { isRoot = value; }
       }

       /** Method: Periods del plan */
       internal List<Planning.Period> Periods {
           get { return periods; }
           set { periods = value; }
       }

       internal int FirstEndLeadTimeIndex {
           get { return firstEndLeadTimeIndex; }
           set { firstEndLeadTimeIndex = value; }
       }

       internal int FirstEndReplTimeIndex {
           get { return firstEndReplTimeIndex; }
           set { firstEndReplTimeIndex = value; }
       }
       
       #endregion

       #region internal Methods

       internal void LoadData(Sku sku, Planning.Plan plan) {
           this.sku = sku;
           this.firstDate = plan.FirstDate;
           this.planHorizon = plan.Horizon;
           this.isRoot = plan.IsRoot;
           this.firstEndLeadTimeIndex = plan.FirstEndLeadTimeIndex;
           this.firstEndReplTimeIndex = plan.FirstEndReplTimeIndex;
           this.periods = plan.Periods;
       }

       #endregion

       #region Persistence

       internal override Broker GetBroker() { return BrkrMgr.GetInstance().GetBroker((Plan)this); }

       internal string GetString(List<Planning.Period> periods)  {
           StringBuilder planStr = new StringBuilder();
           for (int p = 0; p < periods.Count; p++) {
               planStr.Append(GetString(periods[p]) + "-");
           }
           return planStr.ToString();
       }

       internal List<Planning.Period> GetValues(string periodsStr)   {
           char[] sep = { '-' };
           List<Planning.Period> periods = new List<Planning.Period>();
           string[] tokens = periodsStr.Split(sep);
           for (int i = 0; i < tokens.Length; i++) {
               if (tokens[i] == "") { continue; }
               periods.Add(GetValue(i, tokens[i]));
           }
           return periods;
       }

       internal string GetString(Planning.Period per) {
           return new Planning.Plan(sku.Id, 1, true, this.firstDate, sku.PlanHorizon, sku.Stock).GetString(per);
       }

       internal Planning.Period GetValue(int index, string perStr) {
           return new Planning.Plan(sku.Id, 1, true, this.firstDate, sku.PlanHorizon, sku.Stock).GetValue(index, perStr);
       }


       #endregion

   }
}

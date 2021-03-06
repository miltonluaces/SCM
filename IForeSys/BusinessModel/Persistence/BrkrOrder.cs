#region Imports

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Configuration;
using System.Resources;
using System.Reflection;

#endregion

namespace BusinessModel
{

    public class BrkrOrder : Broker
    {

        #region Fields
        #endregion

        #region Constructor

        public BrkrOrder() : base()  {
        }

        #endregion

        #region Public Virtual Methods

        public override DataSet GetSet() {
            return set;
        }

        protected override string GetTableName() {
            return "IfsOrder";
        }

        protected override string GetPrimaryKey()  {
            return "orderId";
        }
        

        public override void ObjASet(DbObject obj, DataRow row) {
            row[GetPrimaryKey()] = ((Order)obj).Id.ToString();
             row["code"] = ((Order)obj).Code;
             row["skuId"] = ((Order)obj).Sku.Id;
             row["skuPlanning"] = ((Order)obj).SkuPlanning;
             row["totalPlannedQty"] = ((Order)obj).TotalPlannedQty;
             row["releaseDate"] = ((Order)obj).ReleaseDate;
             row["receptionDate"] = ((Order)obj).ReceptionDate;
             row["consumptionDate"] = ((Order)obj).ConsumptionDate;
             row["totalBackorderQty"] = ((Order)obj).TotalBackOrderQty;
             row["totalLeadTimeFcstQty"]=((Order)obj).TotalLeadTimeForecastQty;    
             row["totalOrdersQty"] = ((Order)obj).TotalOrdersQty;
             row["totalBomDemandQty"] = ((Order)obj).TotalBomDemandQty;
             row["totalExtDemandQty"] = ((Order)obj).TotalExtDemandQty;
             row["orderState"] = ((Order)obj).OrderState;
             row["customOrderType"] = ((Order)obj).CustomOrderType;
             row["creation"] = (DateTime.Now).Date;
        }

        public override void SetAObj(DataRow row, DbObject obj) {
            ((Order)obj).Id = (int)row[GetPrimaryKey()];
            ((Order)obj).Code = row["code"].ToString();
	        int skuId = (int)row["skuId"];
	        ((Order)obj).Sku = new Sku(skuId);
	        ((Order)obj).SkuPlanning = row["skuPlanning"].ToString();
	        ((Order)obj).TotalPlannedQty = Convert.ToDouble(row["totalPlannedQty"]);
	        ((Order)obj).ReleaseDate = (DateTime)row["releaseDate"];
	        ((Order)obj).ReceptionDate = (DateTime)row["receptionDate"];
            ((Order)obj).ConsumptionDate = (DateTime)row["consumptionDate"];
	        ((Order)obj).TotalBackOrderQty =Convert.ToDouble(row["totalBackorderQty"]);
            ((Order)obj).TotalLeadTimeForecastQty=Convert.ToDouble(row["totalLeadTimeFcstQty"]);
	        ((Order)obj).TotalOrdersQty = Convert.ToDouble(row["totalOrdersQty"]);
            ((Order)obj).TotalBomDemandQty = Convert.ToDouble(row["totalBomDemandQty"]);
            ((Order)obj).TotalExtDemandQty = Convert.ToDouble(row["totalExtDemandQty"]);
	        ((Order)obj).OrderState = (int)row["orderState"];
	        ((Order)obj).CustomOrderType = row["customOrderType"].ToString();
            ((Order)obj).Creation = ((DateTime)row["creation"]).Date;
        }


        public override void SetAObjs(DataRowCollection rows, List<DbObject> objs)
        {
            Order obj;
            foreach (DataRow row in rows)  {
                obj = new Order();
                SetAObj(row, obj);
                objs.Add(obj);
            }
        }
        
        #endregion
    }
}

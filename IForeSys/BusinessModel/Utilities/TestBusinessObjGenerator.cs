#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion
namespace AibuSet  {

    internal class TestBusObjGenerator {

        #region Constructor

        internal TestBusObjGenerator()
        {
        }

        #endregion

        #region Factory Methods

        internal Calendar CreateCalendar() {
            Calendar cal = new Calendar();
            cal.Id = 1;
            cal.Code = "calendario maya";
            DateTime firstDate = new DateTime(1800, 1, 1);
            cal.FirstDate = firstDate;
            DateTime lastDate = new DateTime(1900, 1, 1);
            cal.LastDate = lastDate;
            bool[] wh = new bool[7] { true, true, true, true, true, false, false };
            cal.WeekHols = wh;
            return cal;
        }

        internal Node CreateNode() {
            Calendar cal = CreateCalendar();

            Node node1 = new Node();
            node1.Id = 1;
            node1.Code = "codigo";
            node1.Descrip = "explicacion del codigo";
            node1.Calendar = cal;
            node1.SchLevel = 4;
            node1.AggrHist = true;
            node1.AggrFcst = false;
            return node1;
        }


        internal Customer CreateCustomer()
        {
            Customer cust = new Customer();
            cust.Id = 1;
            cust.Code = "codigo";
            cust.Name = "agente";
            cust.Address = "direccion";
            cust.City = "ciudad";
            cust.Phone = "telefono";
            return cust;
        }

        internal Product CreateProduct()
        {
            Product prod = new Product();
            prod.Id = 1;
            prod.Code = "codigo";
            prod.Desc = "explicacion del codigo";
            prod.Cost = 32.50;
            prod.Price = 39.60;
            return prod;
        }

        internal Sku CreateSku()
        {
            Product prod = CreateProduct();
            Node node1 = CreateNode();
            Calendar cal = CreateCalendar();
            Supplier sup = CreateSupplier();

            Sku x = new Sku();
            x.Id = 1;
            x.Code = "codigo";
            x.Product = prod;
            x.Node = node1;
            x.Supplier = sup;
            x.RsmpFirstDate = new DateTime(1963, 1, 17);
            x.ServiceLevel = 95;
            x.LeadTime = 7;
            x.ReplenishmentTime = 15;
            x.LotSize = 10;
            x.RoundingQty = 2;
            x.IsPeriodFixed = true;
            x.PlanJustCustOrders = false;
            x.ObsRisk = 25;
            x.ObsExpValue = 14;
            x.PlanningRule = "regla";
            x.SupplierCal = cal;
            x.Policy = new Policy();
            x.RsmpFilteringProb = 0.9;
            x.RsmpNoise = 0.1;
            x.RsmpClusterThreshold = 0.2;
            x.Stock = 32;
            x.BckSafetyStock = 33;
            x.Price = 92;
            x.LtFcstMinPercOver = 92;
            x.OrderMinPercOver = 92;
            x.LastNPeriods = 12;
            x.BomLevel = 2;
            x.FirstSellingDate = new DateTime(1963, 1, 17);
            x.LastSupplyDate = new DateTime(1963, 1, 18);
            x.RsmpRollingFcstHist = 128;
            x.LeadTimeFcstManual = 129;
            x.RsmpRollingFcstManual = 130;
            x.ReplenishmentFcstManual = 131;
            x.LeadTimeFcstOrigin = "hist";
            x.VerySMP = true;
            x.MinServiceLevel = 90;
            x.MaxServiceLevel = 98;
            x.Cost = 5;
            x.Volume = 3;
            return x;
        }

        internal PuOrder CreatePlOrder()
        {
            Sku sku = CreateSku();
            PuOrder x = new PuOrder();
            x.Id = 1;
            x.Code = "codigo";
            x.State = PuOrder.StateType.planned;
            x.RcpDate = new DateTime(1963, 1, 19);
            x.OrdDate = new DateTime(1963, 1, 18);
            x.Sku = sku;
            x.Qty = 6;
            return x;
        }

        internal Demand CreateDemand()
        {
            Customer cust = CreateCustomer();
            Sku sku = CreateSku();

            Demand dem = new Demand();
            dem.Id = 1;
            dem.Code = "codigo";
            dem.Sku = sku;
            dem.OrderId = 10;
            dem.LineId = 11;
            dem.InitialQty = 100;
            dem.ActualQty = 90;
            dem.DesiredDate = new DateTime(1963, 1, 17);
            dem.OrderPrice = 25.30;
            dem.Customer = cust;
            return dem;
        }

        internal Supplier CreateSupplier()
        {
            Supplier sup = new Supplier();
            sup.Id = 1;
            sup.Code = "codigo";
            sup.Name = "agente";
            sup.Address = "direccion";
            sup.City = "ciudad";
            sup.Phone = "telefono";
            return sup;
        }

        internal BomRelation CreateBomRelation()
        {
            Node node1 = CreateNode();
            Node node2 = CreateNode();

            BomRelation x = new BomRelation();
            x.Id = 1;
            x.Code = "codigo";
            x.Origin = node1;
            x.Target = node2;
            x.Qty = 10;
            x.Offset = 5;
            return x;
        }

        internal Supply CreateSupply()
        {
            Supplier sup = CreateSupplier();
            Sku sku = CreateSku();

            Supply x = new Supply();
            x.Id = 1;
            x.OrderId = 12;
            x.LineId = 13;
            x.ReleaseDate = new DateTime(1963, 1, 17);
            x.ReceptionDate = new DateTime(1963, 1, 17);
            x.ConsumptionDate = new DateTime(1963, 1, 17);
            x.InitialQty = 26;
            x.ActualQty = 3;
            x.Supplier = sup;
            x.OrderType = "C";
            x.Sku = sku;
            x.BackOrderQty = 16;
            x.BomQty = 17;
            x.OutQty = 18;
            x.LTFcstQty = 19;
            return x;
        }

        internal TimeSeries CreateTimeSeries()
        {
            Sku sku = CreateSku();

            TimeSeries ts = new TimeSeries();
            ts.Id = 1;
            ts.Code = "Prueba ts";
            ts.Sku = sku;
            ts.FirstDate = new DateTime(1963, 1, 17);
            double[] dayHistArr = { 1.0, 2.3, 4.5, 6.8, 0.3, 9.3, 5.6, 3.2, 6.7, 8.9, 1.2, 2.3, 4.5, 6.7 };
            ts.DayHist = new List<double>(dayHistArr);
            double[] ldtHistArr = { 1, 2, 4, 6, 0, 9, 5, 3, 6, 8, 1, 2, 4, 6 };
            ts.LdtHist = new List<double>(ldtHistArr);
            double[] ldtFcstArr = { 0, 2, 4, 6, 0, 9, 5, 3, 6, 8, 1, 2, 4, 6 };
            ts.LdtFcst = new List<double>(ldtFcstArr);
            return ts;
        }

        #endregion
    }
}

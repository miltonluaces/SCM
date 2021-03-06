#region Importaciones

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Diagnostics;
using Planning;

#endregion


namespace BusinessModel {

    /** Method:  Wrapping for Planning proceses  */
    public class PlanCalc {

        #region Fields

        private Sku sku;
        private PlanCalculator sp;
        private List<PlOrder> PlOrders;
        private Dictionary<decimal, Supply> firmOrders;
        private List<PlOrder> oldPlOrders;
        private Dictionary<decimal, List<PlOrder>> branchesPlOrders;
        private bool checkChanges;
        private CheckResultType checkResult;
        private int minDaysForCalDays;

        private DateTime iniDate;
        private DateTime endDate;
        private List<Supply> demands;
        private int minDmdForOutlierCOrders;
        private int maxConsDmdForOutlierCOrders;
        private bool planExcludeOutOfHorDmds;

        private FcstTSValueCollection fots;
        private ArrayList sovs;

        private List<Bom> boms;
        private Dictionary<string, BomDemand> bomDemands;
        private PlanCalculator spOld;
        private SimDist extDemandFilter;
        private Dictionary<int, List<Supply>> extDemand;
        private int maxIterations;

        private Plan pt = new Plan();

        private List<DBPlanningResultType> results;
        private int nWorkingPeriods = 0;
        private int nReplensihmentWorkingPeriods = 0;
        private DateMgr dateMgr;
        private StatusType status;
        private Calendar skuCalendar;
        private Calendar orderingCalendar;
        private List<Holiday> holidays;
        private Dictionary<DateTime, Holiday> holidaysDict;
        private double Plan_LTToRPLimit;
        private bool onDmd = false;
        private Dictionary<decimal, DateTime> initialPenDates;
        private SkuSimilarity skuSim;

        private Dictionary<decimal, double> qtysOnBackorder;
        private Dictionary<int, UPlanning.Log> log;
        private bool debug;
        private bool dmdsAreSales;
        private PlanFcstTSValueCollection planFcst;
        private int replenishmentPeriod;
            


        #endregion

        #region Constructor

        /** Method:  Constructor  
        checkChanges -  si se desea que chequee cambios con el planning anterior 
        debug - <c>true</c> if logging information is stored. */
        public PlanCalc(bool checkChanges, bool debug) {
            this.checkChanges = checkChanges;
            this.demands = new List<Supply>();
            AppParameter ap = new AppParameter();
            this.minDmdForOutlierCOrders = AppParameter.MinDmdForOutlierCOrders;
            this.maxConsDmdForOutlierCOrders = AppParameter.MaxConsDmdForOutlierCOrders;
            this.planExcludeOutOfHorDmds = AppParameter.Plan_ExcludeOutOfHorDmds;
            this.minDaysForCalDays = AppParameter.Plan_MinDaysForCalDays;
            this.Plan_LTToRPLimit = AppParameter.Plan_LTToRPLimit;
            this.dmdsAreSales = AppParameter.DemandsAreSales;
            
            //maxIterations = ap.GetInteger("Rsmp_MaxIterations");
            maxIterations = 30;
            this.boms = new List<BomRelation>();
            this.bomDemands = new Dictionary<string, BomDemand>();
            this.PlOrders = new List<PlOrder>();
            this.firmOrders = new Dictionary<decimal, Supply>();
            extDemandFilter = new SimDist(30, 1500, 1.0, 1.0, 0.01, 0.1, 100, 0.3, maxIterations, -1, 20, 8, 8, 1);
            //unitConverter = new FactorUnitConverter("prueba", false, 2);
            //((FactorUnitConverter)unitConverter).Factor = 10.0;
            this.results = new List<DBPlanningResultType>();
            this.status = StatusType.created;

            this.initialPenDates = new Dictionary<decimal, DateTime>();
            this.branchesPlOrders = new Dictionary<decimal, List<PlOrder>>();
            this.qtysOnBackorder = new Dictionary<decimal,double>();
            this.debug = debug;
        }
      
        #endregion

        #region Properties

        /** Method:  Sku for the planning  */
        public Sku Sku {
            get { return sku; }
            set { sku = value; }
        }

        /** Method:  Sku Planning  */
        public PlanCalculator SP {
            get { return sp; }
            set { sp = value; }
        }

        /** Method:  If must check changes with previous planning  */
        public bool CheckChanges {
            get { return checkChanges; }
            set { checkChanges = value; }
        }

        /** Method:  If planning has changed with respect to the previous one  */
        public CheckResultType CheckResult {
            get { return checkResult; }
            set { checkResult = value; }
        }

        /** Method:  Min data for ext neccesary for demand detection  */
        public int MinDmdForOutlierCOrders {
            get { return minDmdForOutlierCOrders; }
            set { minDmdForOutlierCOrders = value; }
        }

        /** Method:  Max consecutive detections allowed  */
        public int MaxConsDmdForOutlierCOrders {
            get { return maxConsDmdForOutlierCOrders; }
            set { maxConsDmdForOutlierCOrders = value; }
        }

        /** Method:  Statistical Forecast  */
        public TimeSeries Fots {
            get { return fots; }
            set { fots = value; }
        }

        /** Method:  Stat Override TSValue Collections  */
        public ArrayList Sovs {
            get { return sovs; }
            set { sovs = value; }
        }

        /** Method:  Extraordinary Demand  */
        public Dictionary<int, List<Supply>> ExtDemand {
            get { return extDemand; }
        }

        /** Method:  Planning results  */
        public List<DBPlanningResultType> Results {
            get { return results; }
        }

        /** Method:  Metrics: stockouts  */
        public double MetricStkouts {
            get { return sp.MetricStkouts; }
        }

        /** Method:  Metrics: stock mean  */
        public double MetricStkMean {
            get { return sp.MetricStkMean; }
        }

        /** Method:  Metrics: stock rotation  */
        public double MetricRotation {
            get { return sp.MetricRotation; }
        }

        /** Method:  Metrics: replenishment frequency  */
        public double MetricReplFreq {
            get { return sp.MetricReplFreq; }
        }

        #endregion

        #region Public Methods

        #region Load Methods

        /** Method:  Load Planning from Database sku -  sku a utilizar en el planning */
        public void LoadPlanning(Sku sku) {
            this.sku = sku;
            skuCalendar = sku.OrderingCal;
            
            //sku planning
            SkuPlanningBO sp2 = new SkuPlanningBO();
            db.Load(sp2, sku);
            if(sp2.CurrentPlan != null) { this.spOld = Clone(sp2.CurrentPlan); }
            this.sp = sp2.CurrentPlan;

            //supplies
            //LoadSupplies();

            //planned orders
            db.LoadMany(PlOrders, "SKUID = " + sku.ID);
            if(checkChanges) { LoadForCheckChanges(); }
            status = StatusType.loaded;

            
            planFcst = LoadPlanFcstForSku(sku);

            LoadWarnings();
        }

        private void LoadForCheckChanges() {

            oldPlOrders = new List<PlOrder>();
            PlOrder oldPo;
            PlOrderLine oldPol;
            foreach(PlOrder po in PlOrders) {
                oldPo = new PlOrder();
                oldPo.TotalPlannedQty = po.TotalPlannedQty;
                oldPo.ReleaseDate = po.ReleaseDate;
                oldPo.ReceptionDate = po.ReceptionDate;
                oldPo.EarlierConsumptionDate = po.EarlierConsumptionDate;
                foreach(PlOrderLine pol in po.PlOrderLines) {
                    oldPol = (PlOrderLine)pol.Clone();
                    oldPo.PlOrderLines.Add(oldPol);
                }
                oldPlOrders.Add(oldPo);
            }
            status = StatusType.loaded;
        }

        #endregion

        #region Calculate Methods

        /** Method:  Load Sku properties and data  
        sku -  sku a utilizar en el planning  
        iniDate -  fecha inicial a utilizar en el cálculo */
        public void LoadForCalculate(Sku sku, DateTime iniDate) {
            LoadForCalculate(sku, iniDate, null);
        }

        /** Method: Load Sku properties and data
        sku - sku a utilizar en el planning
        iniDate - fecha inicial a utilizar en el cálculo.
        periodsList - lista de periodos con la que calculamos */
        public void LoadForCalculate(Sku sku, DateTime iniDate, List<Period> periodsList) {
            results.Clear();
            qtysOnBackorder.Clear();
            this.sku = sku;
            if(sku.PlanningRule == PlanningRuleTypes.DeletePlanning) { return; }

            if (sku.OrderingCal == null)
                throw new ApplicationException(Strings.PlanningCalendarIsMissing);

            ProveePersistencia pp = new ProveePersistencia();
            skuCalendar = sku.OrderingCal;
            this.iniDate = iniDate;
            this.endDate = iniDate.AddDays(sku.PlanningHorizon);
            holidays = new List<Holiday>();
            holidaysDict = new Dictionary<DateTime, Holiday>();
            HolidayDb hf = new HolidayDb();

            DateTime iniMonthDate = iniDate.AddDays(-iniDate.Day+1);
            DateTime endMonthDate = endDate.AddMonths(1).AddDays(-(endDate.Day));
            var periodo = string.Format("{0} BETWEEN {1} AND {2}",
                "HOLIDAYDATE",
                //pp.ConvertToSqlDate(iniDate.Date),
                //pp.ConvertToSqlDate(endDate.Date.AddDays(1).AddSeconds(-1)));
                pp.ConvertToSqlDate(iniMonthDate.Date),
                pp.ConvertToSqlDate(endMonthDate.Date.AddDays(1).AddSeconds(-1)));
            var cond1 = "CALENDARID IS NULL AND " + periodo; // las fiestas para todos.
            var cond2 = "CALENDARID = " + skuCalendar.ID + " AND " + periodo; // las fiestas para un Calendar.

            db.LoadMany(holidays, cond1);
            db.LoadMany(holidays, cond2);

       
            foreach (Holiday h in holidays) {
                if (!holidaysDict.ContainsKey(h.HolidayDate)) { holidaysDict.Add(h.HolidayDate, h); }
            }

            while(true) {
                if(!holidaysDict.ContainsKey(this.iniDate) || this.iniDate >= endDate) { break; }
                this.iniDate = this.iniDate.AddDays(1);
                if(!results.Contains(DBPlanningResultType.InitialDateChanged)) { results.Add(DBPlanningResultType.InitialDateChanged); }
            }
            this.iniDate = this.iniDate.AddDays(-1);

            this.sp = new PlanCalculator(sku.ID, sku.OnHandQty, minDaysForCalDays, Plan_LTToRPLimit, demands);
            this.PlOrders.Clear();

            LoadProperties();
            if (this.dmdsAreSales) {
                this.sp.TotalLastFixPeriodDmds = CalcTotalLastFixPeriodDmds();
                this.sp.DmdsAreSales = true;
            }
            if (results.Contains(DBPlanningResultType.LeadTimeZero)) { return; }
            LoadBoms();
            if(periodsList == null) { CreatePeriods(); }
            else { CreatePeriods(periodsList); }
            LoadPolicies();
            LoadFcst();
            if (sp.IsRoot) {
                LoadBranches();
                LoadPlOrders();
                if (!sku.PlanJustCustOrders) { LoadPlanFcst(); }
            }
            LoadDemands();
           
            LoadSupplies();

            //plan fcst to zero
            planFcst = LoadPlanFcstForSku(sku);
            AdjustPeriodsPlanFcst(planFcst);
            foreach (TSValue tsv in planFcst) { tsv.Value = 0; }
        
            status = StatusType.loadedForCalc;

            #region Extraordinary demand (for debugging)

            bool debugExtDemand = false;
            
            if(debugExtDemand) {
                ExtDemand ed = new ExtDemand();
                ed.Initialize(TipoCalendar.Monthly);
                ed.MovingWindow = 3;
                ed.ResidThreshold = 0.999;
                ed.OutlierThreshold = 0.9999;
                ed.MinForResidSelection = 20;
                DateTime ini = iniDate.AddDays(-2000);
                DateTime end = iniDate.AddDays(sku.PlanningHorizon);
                ed.LoadDemands(sku, ini, end, false);
                ed.FilterDemands(true);
                List<Demand> extDemands = ed.Demands;
                ed.SaveDemands(null);
            }

            #endregion
        }

        
        /** Method:  Calculate Planning for a Sku   
        recursive -  si se desea que el cálculo se haga recursivo en niveles de estructura IPAL */
        public void Calculate(bool recursive) {
            if(sku.PlanningRule == PlanningRuleTypes.DeletePlanning || results.Contains(DBPlanningResultType.LeadTimeZero)) { return; }
            if(sku.LastSupplyDate != StaticResources.FechaNula && sku.FirstSellingDate > sku.LastSupplyDate) { sp.AddWarning(UPlanning.Warning.LastSupplyDateLessOrEqualFirstSellingDate); return; }
            if(status < StatusType.loadedForCalc) { throw new ApplicationException("Strings.Error_Must_load_for_calculate_first"); }
            sp.UseRules = false;
            log.Clear();
            sp.Calculate(recursive, log);
            LoadWarnings();
            GenerateBomDemands();
            ConfirmBomDemands();
            if (sku.PlanningRule != PlanningRuleTypes.ShiftFcstToRequirements) { GenerateFirmAndPlOrders(); } 
            else { GeneratePlanFcst(); }
            //SetQtyOnBackorder();
            if (debug) { WriteLog(); } 
            //if(checkChanges) { CheckingChanges(); }
            if(results.Count == 0) { results.Add(DBPlanningResultType.Ok); }
            status = StatusType.calculated;

            /*
            Dictionary<decimal, List<Alloc>> allocsQty = GetAllocsQty(LocalizedAllocType.FromStock);
            Console.WriteLine("Skuid" + "\t" + "Id" + "\t" + "Type" + "\t" + "Qty");
            foreach (decimal skuId in allocsQty.Keys) {
                foreach (Alloc alloc in allocsQty[skuId]) {
                    Console.WriteLine(alloc.SkuID + "\t" +alloc.ID + "\t" + alloc.TypeDmd + "\t" + alloc.Qty);
                }
            }
            */
        
        }

        #endregion

        #region Bom Demands

        /** Method:  Load Boms if there are dependant skus  */
        private void LoadBoms() {
            boms.Clear();
            if(sku.BomLevel != -1) {
                db.LoadMany(boms, "PRODUCEDSKUID = " + sku.ID);
            }
        }

        /** Method:  Load Demands from OrderForBom  */
        private List<Supply> LoadBomDemands() {
            List<BomDemand> bomDemands = new List<BomDemand>();
            db.LoadMany(bomDemands, "CONSUMEDSKUID = " + sku.ID);
            Supply s;
            List<Supply> supplies = new List<Supply>();
            Period per;
            Period firstPer = sp.GetPeriod(sp.FirstPN);
            Period lastPer = sp.GetPeriod(sp.LastPN);

            foreach(BomDemand d in bomDemands) {
                if(d.Qty <= 0) { continue; }
                if(d.LatestProdDate < firstPer.Date) { per = firstPer; }
                else if(d.LatestProdDate > lastPer.Date) { per = lastPer; }
                else { per = sp.GetPeriod(d.LatestProdDate); }
                s = new Supply(TypeSupply.Demand, d.ID, sku.ID, per.PeriodNumber, per.PeriodNumber, 0, d.Qty);
                if (per.Working) { s.ReceptPN = per.PeriodNumber; } 
                else { s.ReceptPN = dateMgr.GetLastWorking(per.PeriodNumber); }
                s.ProdSkuId = d.ProducedSku.ID;
                s.TypeFor = UPlanning.TypeSupplyFor.PlOrderForBom;
                supplies.Add(s);
            }
            return supplies;
        }

        /** Method:  Generate Demands to OrderForBom  */
        private void GenerateBomDemands() {
            if(boms == null || boms.Count == 0) { return; }

            bomDemands.Clear();

            BomDemand bd;
            string key;
            DateTime latestPD;
            foreach(Period per in sp.Periods.Values) {
                if(per.TotalPlOrderSupply == 0) { continue; }
                foreach(BomRelation bom in boms) {
                    bd = new BomDemand();
                    bd.ProducedSku = bom.ProducedSku;
                    bd.ConsumedSku = bom.ConsumedSku;
                    bd.Qty = bom.Qty * per.TotalPlOrderSupply;
                    bd.ConsumptionDate = per.Date;
                    //latestPD = latestPD.AddDays(-bom.ConsumedSku.RsmpLeadTime);
                    int latestPN = dateMgr.GetLastWorking(per.PeriodNumber, bom.ConsumedSku.RsmpLeadTime);
                    if(latestPN < 1) { latestPN = 1; }
                    latestPD = sp.GetPeriod(latestPN).Date;
                    latestPD = latestPD.AddDays(-bom.OffSetDays);
                    if (latestPD < sp.FirstDate) { latestPD = sp.FirstDate; }
                    bd.PlanningDate = latestPD;
                    bd.LatestProdDate = latestPD;
                    key = GetBdKey(per.Date, bd.ProducedSku.ID, bd.ConsumedSku.ID);
                    if(!bomDemands.ContainsKey(key)) {
                        bomDemands.Add(key, bd);
                    }
                }
            }
        }

        /** Method:  Confirm Demands in OrderForBom  */
        private void ConfirmBomDemands() {
            BomDemand bd = new BomDemand();
            string key;
            foreach(Period per in sp.Periods.Values) {
                foreach(Alloc a in per.BackOrderDemands) {
                    if(a.RqstSupply.ProdSkuId != -1 && sku.ID != -1) {
                        key = GetBdKey(per.Date, a.RqstSupply.ProdSkuId, sku.ID);
                        if(bomDemands.ContainsKey(key)) {
                            bomDemands[key].ConfirmedDate = sp.GetPeriod(a.ConfirmPN).Date;
                        }
                    }
                }
            }
        }

        private string GetBdKey(DateTime perDate, decimal prodSkuId, decimal consSkuId) {
            return prodSkuId + " - " + consSkuId + " - " + perDate.ToShortDateString();
        }

        #endregion

        #region Metrics Calculation

        /** Method:  Calculate metrics (metric properties)  */
        public void CalculateMetrics() {
            if(sku.PlanningRule == PlanningRuleTypes.DeletePlanning) { return; }
            if(status < StatusType.calculated) { return; }
            sp.CalculateMetrics();
        }

        /** Method:  Dictionary of Allocs of a particular type by SkuId  
        AllocType -  Alloctype (from stock, from firm or from planned order) */
        internal Dictionary<decimal, List<Alloc>> GetAllocs(LocalizedAllocType AllocType) {
            if (sp == null)
                return new Dictionary<decimal, List<Alloc>>();

            var allocType = AllocType.FromStock;
            switch (AllocType) {
                case LocalizedAllocType.FromStock:
                    allocType = AllocType.FromStock;
                    break;
                case LocalizedAllocType.FromFirm:
                    allocType = AllocType.FromFirm;
                    break;
                case LocalizedAllocType.FromRec:
                    allocType = AllocType.FromRec;
                    break;
                default:
                    throw new ArgumentException("Valid values are FromStock, FromFirm, FromRec", "AllocType");
            }
            return sp.GetAllocs(allocType);
        }

        /** Method: Quantities of a particular Alloctype indexed by SkuId. 
        AllocType -  Alloctype (from stock, from firm or from planned order) */
        public Dictionary<decimal, List<Alloc>> GetAllocsQty(LocalizedAllocType AllocType) {
            Dictionary<decimal, List<Alloc>> allocs = this.GetAllocs(AllocType);
            Dictionary<decimal, List<Alloc>> allocsQty = new Dictionary<decimal, List<Alloc>>();
            Alloc alloc;
            foreach (decimal skuId in allocs.Keys) {
                if (!allocsQty.ContainsKey(skuId)) { allocsQty.Add(skuId, new List<Alloc>()); }
                foreach (Alloc a in allocs[skuId]) {
                    alloc = new Alloc(skuId, a); 
                    allocsQty[skuId].Add(alloc);
                }
            }
            return allocsQty;
        }

        /** Method: 
        Holds Allocs info.
         */
        [Serializable]
        public class Alloc : IEqualityComparer<Alloc> {
            /** Method: 
            ID.
             */
            public decimal ID { get; set; }
            /** Method: 
            Sku ID.
             */
            public decimal SkuID { get; set; }
            /** Method: 
            Type Demand.
             */
            public TypesDmd TypeDmd { get; set; }
            /** Method: 
            Quantity.
             */
            public double Qty { get; set; }

            /** Method: 
            Constructor
             */
            internal Alloc(decimal skuId, Alloc a) {
                this.SkuID = skuId;
                this.ID = a.RqstSupply.Id;
                this.Qty = a.Qty;
                if (this.ID == -2) { TypeDmd = TypesDmd.PlanFcst; } 
                else if (skuId == a.SkuId) { TypeDmd = TypesDmd.Demand; } 
                else if (a.SupplyTypeFor == UPlanning.TypeSupplyFor.PlOrderForBom) { TypeDmd = TypesDmd.BomDemand; } 
                else { this.TypeDmd = TypesDmd.PlOrder; }
            }

            public Alloc() {
            }

            /** Method: Types of demand.*/
            public enum TypesDmd { Demand, PlanFcst, PlOrder, BomDemand  };

            public bool Equals(Alloc x, Alloc y) {
                // descartamos el ID para la igualdad.
                var eq = ( // x.ID == y.ID &&
                    x.Qty == y.Qty && x.SkuID == y.SkuID && x.TypeDmd == y.TypeDmd);
                return eq;
            }

            public int GetHashCode(Alloc obj) {
                // var hcID = obj.ID.GetHashCode();
                var hcQty = obj.Qty.GetHashCode();
                var hcSkuID = obj.SkuID.GetHashCode();
                var hcTD = obj.TypeDmd.GetHashCode();
                return (hcQty ^ hcSkuID ^ hcTD);
            }
        }

        #endregion

        #region Generate Firm and PlOrders

        private void GenerateFirmAndPlOrders() {

            PlOrder po = new PlOrder();
            PlOrderLine pol;
            Supply fo;
            SupplyLine fol;
            double polQty = 0;
            Period consPer;
            PlOrders.Clear();

            foreach(Period per in sp.Periods.Values) {
                foreach(Supply su in per.GetSupplies()) {
                    //firm orders
                    if(su.Type == UPlanning.TypeSupply.FirmOrder) {
                        fo = firmOrders[su.Id];
                        fo.InitialPenDate = initialPenDates[su.Id];
                        fo.ActualPenDate = sp.GetPeriod(su.ReceptPN).Date;
                        fo.ConsPenDate = sp.GetPeriod(su.ConsumedPN).Date;
                        fo.SuppliedQty = su.Qty;
                        fo.SupplyLines.Clear();
                        fo.BackOrderQty = su.BackorderQty;
                        fo.BomQty = su.BomQty;
                        fo.OutQty = su.OutQty;
                        fo.LTFcstQty = su.LtFcstQty;
                        fo.OrderQty = su.StockQty;
                        foreach(Alloc a in su.Allocs) {
                            fol = new SupplyLine();
                            fol.Demand = new Demand();
                            db.Load(fol.Demand, a.RqstSupply.Id);
                            fol.Supply = fo;
                            fol.SupplyQty = a.Qty;
                            fol.AllocType = TranslateAllocType(a.SupplyTypeFor);
                            
                            consPer = sp.GetPeriod(a.RqstSupply.ReceptPN);
                            if (consPer != null) { 
                                fol.ConsumptionDate = Trunk(consPer.Date);
                                if (fol.ConsumptionDate < fo.ConsPenDate) { 
                                    fo.ConsPenDate = fol.ConsumptionDate;
                                    su.ConsumedPN = consPer.PeriodNumber;
                                }
                            }
                            fo.SupplyLines.Add(fol);

                            if (a.RqstSupply.Id != -1 && a.RqstSupply.Type == UPlanning.TypeSupply.Demand && per.PeriodNumber <= sp.EndFirstLTimePN) {
                                if (!qtysOnBackorder.ContainsKey(a.RqstSupply.Id)) { qtysOnBackorder.Add(a.RqstSupply.Id, a.Qty); } 
                                else { qtysOnBackorder[a.RqstSupply.Id] = qtysOnBackorder[a.RqstSupply.Id] + a.Qty; }
                            }
            
                        }
                    }
                    //planned orders
                    else { 
                        po = new PlOrder();
                        po.Sku = this.sku;
                        if(su.MaxOrderPN < 1) { su.MaxOrderPN = 1; }
                        consPer = sp.GetPeriod(su.ConsumedPN);
                        po.ReleaseDate = Trunk(sp.GetPeriod(su.OrderPN).Date);
                        po.ReceptionDate = Trunk(sp.GetPeriod(su.ReceptPN).Date);
                        po.PlOrderLines.Clear();
                        po.BackOrderQty = su.BackorderQty;
                        po.BomQty = su.BomQty;
                        po.OutQty = su.OutQty;
                        po.LtFcstQty = su.LtFcstQty;
                        po.StockQty = su.StockQty;

                        polQty = 0;
                        foreach(Alloc a in su.Allocs) {
                            pol = new PlOrderLine();
                            pol.PlOrder = po;
                            pol.PlannedQty = a.Qty;
                            polQty += pol.PlannedQty;
                            pol.AllocType = TranslateAllocType(a.SupplyTypeFor);
                            if (boms != null && boms.Count > 0) { pol.AllocType = AllocTypes.OrderForBom; }
                            pol.Demand = new Demand();
                            db.Load(pol.Demand, a.RqstSupply.Id);
                            if(sp.IsRoot) { pol.OrderType = SupplyTypes.OrderFabricacion; } //TODO: orden de compra pendiente
                            else { pol.OrderType = SupplyTypes.TransferenciasEntreAlmacenes; }
                            if(a.RqstSupply.MaxOrderPN == 0) { a.RqstSupply.MaxOrderPN = su.ReceptPN; } //TODO: revisar checkWarnings, el seteo
                            pol.LastReleaseDate = Trunk(sp.GetPeriod(a.RqstSupply.MaxOrderPN).Date);
                            if(su.ConsumedPN <= 0) { su.ConsumedPN = a.RqstSupply.ConsumedPN; }
                            else { if(a.RqstSupply.ConsumedPN > 0 && a.RqstSupply.ConsumedPN < su.ConsumedPN) { su.ConsumedPN = a.RqstSupply.ConsumedPN; } }
                            consPer = sp.GetPeriod(su.ConsumedPN);
                            if(consPer != null) { 
                                pol.ConsumptionDate = Trunk(consPer.Date); 
                                po.EarlierConsumptionDate = Trunk(consPer.Date);
                            }
                            po.PlOrderLines.Add(pol);

                            if (a.RqstSupply.Id != -1 && a.RqstSupply.Type == UPlanning.TypeSupply.Demand && per.PeriodNumber <= sp.EndFirstLTimePN) {
                                if (!qtysOnBackorder.ContainsKey(a.RqstSupply.Id)) { qtysOnBackorder.Add(a.RqstSupply.Id, a.Qty); } 
                                else { qtysOnBackorder[a.RqstSupply.Id] = qtysOnBackorder[a.RqstSupply.Id] + a.Qty; }
                            }
                   
                        }
                        if (boms != null && boms.Count > 0) {
                            po.BackOrderQty = 0;
                            po.BomQty = polQty;
                            po.OutQty = 0;
                            po.LtFcstQty = 0;
                            po.StockQty = 0;
                        }
                        po.TotalPlannedQty = polQty;
                        po.ModifiedQty = po.TotalPlannedQty;

                        if(po.TotalPlannedQty > 0) { PlOrders.Add(po); }
                    }
                } 
            }
        }

        private void GeneratePlanFcst() {
            if (planFcst == null || planFcst.Count == 0) { CreatePlanFcst(); }
            int firstP = planFcst[0].PeriodNumber;
            int lastP = planFcst[planFcst.Count - 1].PeriodNumber;
            int pNum;
            Period consPer;
            foreach (Period per in sp.Periods.Values) {
                pNum = GetPeriodNumber(per.Date);
                foreach (Supply su in per.GetSupplies()) {
                    if (su.Type == UPlanning.TypeSupply.PlanFcst) {
                        planFcst[pNum - firstP].Value = su.Qty;
                    }
                }
            }
        }

        private void CreatePlanFcst() {
            planFcst = LoadPlanFcstForSku(sku);
            int firstPN = GetPeriodNumber(sp.FirstDate);
            int lastPN = GetPeriodNumber(sp.Periods[sp.Periods.Count - 1].Date);
            if (lastPN < firstPN) { throw new Exception("Error. Bad last period number"); }
            for (int p = firstPN; p <= lastPN; p++) { planFcst.Add(new TSValue(p, 0d));  }
        }

  
        // lectura
        private PlanFcstTSValueCollection LoadPlanFcstForSku(Sku sku) {
            var serie = Serie.GetSerieObservable(TipoCalendar.Daily);
            var pf = new PlanFcstTSValueCollection();
            var dfu = new Dfu();

            db.Load(dfu, sku.Material, sku.IPALNode, ECC.Lib.TipoCalendar.Daily);
            if (dfu.ID == -1) {
                dfu.Material = sku.Material;
                dfu.IPALNode = sku.IPALNode;
                dfu.TipoCalendar = ECC.Lib.TipoCalendar.Daily;
                dfu.LastHistoricalPN = sku.IPALNode.GetMostProbableLastHistPN(ECC.Lib.TipoCalendar.Daily);
            }
            // fuerzo Calendar al cache.
            ECC.Lib.Cache.Manager.Put((ECC.Data.Calendar)sku.IPALNode.CalDaily.Periodos.Header);
            // leo la coleccion.
            db.Load(pf, new DfuSerie(dfu, serie));
            pf.Header = new DfuSerie(dfu, serie);

            return pf;
        }

        private void AdjustPeriodsPlanFcst(PlanFcstTSValueCollection pf) {
            int firstP = GetPeriodNumber(sp.FirstDate);
            int lastP = GetPeriodNumber(sp.Periods[sp.Periods.Count - 1].Date);

            pf.ExpandToPeriodRange(firstP, lastP);
        }

        private AllocTypes TranslateAllocType(UPlanning.TypeSupplyFor allocType) {
            switch(allocType) {
                case UPlanning.TypeSupplyFor.FirmOrderForBackorder:
                    return AllocTypes.OrderForBackorder;
                case UPlanning.TypeSupplyFor.FirmOrderForBom:
                    return AllocTypes.OrderForBom;
                case UPlanning.TypeSupplyFor.FirmOrderForExt:
                    return AllocTypes.OrderForExt;
                case UPlanning.TypeSupplyFor.FirmOrderForLtFcst:
                    return AllocTypes.OrderForLtFcst;
                case UPlanning.TypeSupplyFor.FirmOrderForStock:
                    return AllocTypes.OrderForStock;
                case UPlanning.TypeSupplyFor.PlOrderForBackorder:
                    return AllocTypes.OrderForBackorder;
                case UPlanning.TypeSupplyFor.PlOrderForBom:
                    return AllocTypes.OrderForBom;
                case UPlanning.TypeSupplyFor.PlOrderForExt:
                    return AllocTypes.OrderForExt;
                case UPlanning.TypeSupplyFor.PlOrderForLtFcst:
                    return AllocTypes.OrderForLtFcst;
                case UPlanning.TypeSupplyFor.PlOrderForStock:
                    return AllocTypes.OrderForStock;
            }
            return AllocTypes.OrderForStock;
        }

        #endregion

        #region PlanTables Generation

        /** Method:  Get a list of Periods containing totals
        plOrDate -  position of planned orders (order/rcpt/consume)
        fiOrDate -  position of firm orders (original/modified/consume)
        dayPeriods -  number of periods day by day 
        weekPeriods -  number of periods grouped in weeks */
        public List<PlanTable.Period> GetPlanList(FirmOrderDateType fiOrDate, PlOrderDateType plOrDate, int dayPeriods, int weekPeriods) {
            List<PlanTable.Period> periods = pt.GetPeriods(sp, dayPeriods, weekPeriods);
            pt.LoadPlan(periods, sku, sp, fiOrDate, plOrDate, unitConverter);
            return periods;
        }

        /** Method:  Get a list of Periods containing list of demands  
        dayPeriods -  number of periods day by day 
        weekPeriods -  number of periods grouped in weeks */
        public List<PlanTable.Period> GetDemandsList(int dayPeriods, int weekPeriods) {
            List<PlanTable.Period> periods = pt.GetPeriods(sp, dayPeriods, weekPeriods);
            pt.LoadDemands(periods, sku, sp, unitConverter);
            return periods;
        }

        /** Method:  Get a list of Periods containing list of non extraordinary demands 
        dayPeriods -  number of periods day by day 
        weekPeriods -  number of periods grouped in weeks */
        public List<PlanTable.Period> GetNonExtDemandsList(int dayPeriods, int weekPeriods) {
            List<PlanTable.Period> periods = pt.GetPeriods(sp, dayPeriods, weekPeriods);
            pt.LoadNonExtDemands(periods, sku, sp, unitConverter);
            return periods;
        }

        /** Method:  Get a list of Periods containing list of planned orders  
        dayPeriods -  number of periods day by day 
        weekPeriods -  number of periods grouped in weeks 
        plOrDate - position of planned orders (order/rcpt/consume) */
        public List<PlanTable.Period> GetPlOrdersList(PlOrderDateType plOrDate, int dayPeriods, int weekPeriods) {
            List<PlanTable.Period> periods = pt.GetPeriods(sp, dayPeriods, weekPeriods);
            pt.LoadPlOrders(plOrDate, periods, sp, PlOrders, unitConverter);
            return periods;
        }

        /** Method:  Get a list of Periods containing list of firm orders  
        fiOrDate - position of planned orders (order/rcpt/consume)
        dayPeriods -  number of periods day by day 
        weekPeriods -  number of periods grouped in weeks */
        public List<PlanTable.Period> GetFirmOrdersList(FirmOrderDateType fiOrDate, int dayPeriods, int weekPeriods) {
            List<PlanTable.Period> periods = pt.GetPeriods(sp, dayPeriods, weekPeriods);
            pt.LoadFirmOrders(fiOrDate, periods, sku, firmOrders, sp, unitConverter);
            return periods;
        }

        /** Method:  Get a list of Periods containing totals, list of demands and list of planned orders  
        plOrDate -  position of planned orders (order/rcpt/consume)
        fiOrDate -  position of firm orders (order/rcpt/consume)
        dayPeriods -  number of periods day by day 
        weekPeriods -  number of periods grouped in weeks */
        public List<PlanTable.Period> GetCompleteList(FirmOrderDateType fiOrDate, PlOrderDateType plOrDate, int dayPeriods, int weekPeriods) {
            List<PlanTable.Period> periods = pt.GetPeriods(sp, dayPeriods, weekPeriods);
            pt.LoadPlan(periods, sku, sp, fiOrDate, plOrDate, unitConverter);
            pt.LoadDemands(periods, sku, sp, unitConverter);
            pt.LoadFirmOrders(fiOrDate, periods, sku, firmOrders, sp, unitConverter);
            if (sku.PlanningRule == PlanningRuleTypes.ShiftFcstToRequirements && planFcst != null && planFcst.Count != 0) {
                int firstP = planFcst[0].PeriodNumber;
                int lastP = planFcst[planFcst.Count - 1].PeriodNumber;
                int pNum;
                Period consPer;
                List<PlOrder> shiftPlOrders = new List<PlOrder>();
                foreach (Period per in sp.Periods.Values) {
                    pNum = GetPeriodNumber(per.Date);
                    foreach (Supply su in per.GetSupplies()) {
                        if (su.Type == UPlanning.TypeSupply.PlanFcst) {
                            planFcst[pNum - firstP].Value = su.Qty;

                            PlOrder po = new PlOrder();
                            po.Sku = this.sku;
                            if (su.MaxOrderPN < 1) { su.MaxOrderPN = 1; }
                            consPer = sp.GetPeriod(su.ConsumedPN);
                            po.ReleaseDate = Trunk(sp.GetPeriod(su.OrderPN).Date);
                            po.ReceptionDate = Trunk(sp.GetPeriod(su.ReceptPN).Date);
                            po.TotalPlannedQty = su.Qty;
                            po.ModifiedQty = po.TotalPlannedQty;
                            po.PlOrderLines.Clear();
                            po.BackOrderQty = su.BackorderQty;
                            po.BomQty = su.BomQty;
                            po.OutQty = su.OutQty;
                            po.LtFcstQty = su.LtFcstQty;
                            po.StockQty = su.StockQty;
                            PlOrderLine pol = new PlOrderLine();
                            pol.PlOrder = po;
                            pol.PlannedQty = po.LtFcstQty;
                            pol.AllocType = AllocTypes.OrderForLtFcst;
                            pol.LastReleaseDate = Trunk(per.Date); //TODO: revisar cálculo
                            if (consPer != null) {
                                pol.ConsumptionDate = Trunk(consPer.Date);
                                po.EarlierConsumptionDate = Trunk(per.Date); //TODO: revisar cálculo
                            }
                            po.PlOrderLines.Add(pol);
                            shiftPlOrders.Add(po);
                        }
                    }
                }
                pt.LoadPlOrders(plOrDate, periods, sp, shiftPlOrders, unitConverter);
            } 
            else {
                pt.LoadPlOrders(plOrDate, periods, sp, PlOrders, unitConverter);
            }
            pt.LoadNonExtDemands(periods, sku, sp, unitConverter);
            pt.LoadQuotes(periods, sku, unitConverter);
            pt.LoadLog(periods, sp, dayPeriods, this.log, unitConverter);
            return periods;
        }

        private bool IsHoliday(Period per) {
            if(sku == null) { return false; }
            if(sku.DmdDaysPerWeek < 7 && per.Date.DayOfWeek == DayOfWeek.Sunday) { return true; }
            if(sku.DmdDaysPerWeek < 6 && per.Date.DayOfWeek == DayOfWeek.Saturday) { return true; }
            if(sku.DmdDaysPerWeek < 5 && per.Date.DayOfWeek == DayOfWeek.Friday) { return true; }
            return false;
        }

        private bool IsHoliday(Periodo per) {
            if(sku == null) { return false; }
            if(sku.DmdDaysPerWeek < 7 && per.FechaInicial.DayOfWeek == DayOfWeek.Sunday) { return true; }
            if(sku.DmdDaysPerWeek < 6 && per.FechaInicial.DayOfWeek == DayOfWeek.Saturday) { return true; }
            if(sku.DmdDaysPerWeek < 5 && per.FechaInicial.DayOfWeek == DayOfWeek.Friday) { return true; }
            return false;
        }

        /** Method:  Convert to Unit a value using unitConverter  
        pNum -  número de período (aplicable a conversiones por serie) 
        val -  valor a convertir */
        public double ConvertToUnit(int pNum, double val) {
            if(unitConverter == null) { return val; }
            TSValue tsv = new TSValue(pNum, val);
            unitConverter.ConvertToUnit(tsv);
            return tsv.Value;
        }

        /** Method:  Convert from unit a value using unitConverter  
        pNum -  número de período (aplicable a conversiones por serie)  
        val -  valor a convertir */
        public double ConvertFromUnit(int pNum, double val) {
            if(unitConverter == null) { return val; }
            TSValue tsv = new TSValue(pNum, val);
            unitConverter.ConvertFromUnit(tsv);
            return tsv.Value;
        }


        /** Method:  Mostrar asignaciones  */
        public DataTable GetAllocs() {
            DataTable dt = new DataTable();
            DataColumn dCol;
            dCol = new DataColumn("NodeOri");
            dCol.Caption = Strings.Origin;
            dt.Columns.Add(dCol);
            dCol = new DataColumn("Type");
            dCol.Caption = Strings.Tipo;
            dt.Columns.Add(dCol);
            dCol = new DataColumn("NodeDest");
            dCol.Caption = Strings.Destination;
            dt.Columns.Add(dCol);
            dCol = new DataColumn("DmdDate");
            dCol.Caption = Strings.Demanda;
            dt.Columns.Add(dCol);
            dCol = new DataColumn("ConfDate");
            dCol.Caption = Strings.Confirmación;
            dt.Columns.Add(dCol);
            dCol = new DataColumn("DmdQty");
            dCol.DataType = typeof(double);
            dCol.Caption = Strings.Cant_Demanda;
            dt.Columns.Add(dCol);
            dCol = new DataColumn("AllocQty");
            dCol.DataType = typeof(double);
            dCol.Caption = Strings.Cant_Asignada;
            dt.Columns.Add(dCol);
            dCol = new DataColumn("AllocType");
            dCol.Caption = Strings.Tipo_Asignación;
            dt.Columns.Add(dCol);
            dCol = new DataColumn("p");
            dCol.Caption = Strings.Periodo;
            dt.Columns.Add(dCol);
            if(sp == null) { return dt; }

            DataRow dRow;
            Sku skuNode;
            Sku skuDest;
            Period per;
            for(int p=sp.FirstPN;p<=sp.EndAllocHorizonPN;p++) {
                per = sp.GetPeriod(p);
                List<List<Alloc>> listAllocs = new List<List<Alloc>>();
                listAllocs.Add(per.BackOrderDemands);
                listAllocs.Add(per.LtFcstDemands);
                listAllocs.Add(per.OrderDemands);
                listAllocs.Add(per.BomDemands);
                listAllocs.Add(per.ExtDemands);
                foreach(List<Alloc> listAlloc in listAllocs) {
                    foreach(Alloc a in listAlloc) {
                        dRow = dt.NewRow();
                        skuNode = new Sku();
                        skuDest = new Sku();
                        db.Load(skuNode, a.RqstSupply.SkuId);
                        db.Load(skuDest, a.RqstSupply.ProdSkuId);
                        dRow["NodeOri"] = skuNode.IPALNode.Code;
                        dRow["Type"] = localize(a.RqstSupply.TypeFor);
                        dRow["NodeDest"] = skuDest.IPALNode.Code;
                        dRow["DmdDate"] = sp.GetPeriod(a.RqstSupply.ReceptPN).Date.ToShortDateString();
                        if(a.RqstSupply.ConfirmPN > 0) { dRow["ConfDate"] = sp.GetPeriod(a.RqstSupply.ConfirmPN).Date.ToShortDateString(); }
                        else { dRow["ConfDate"] = ""; }
                        dRow["DmdQty"] = ConvertToUnit(a.RqstSupply.ReceptPN, a.RqstSupply.Qty);
                        dRow["AllocQty"] = ConvertToUnit(a.RqstSupply.ReceptPN, a.Qty);
                        dRow["AllocType"] = localize(a.AllocType);
                        if(a.Qty > 0) { dt.Rows.Add(dRow); }
                    }
                }
            }
            return dt;
        }

        private string localize(AllocType AllocType) {
            ECC.Data.LocalizedAllocType lat = (ECC.Data.LocalizedAllocType)
                Enum.ToObject(typeof(ECC.Data.LocalizedAllocType), AllocType);
            System.ComponentModel.TypeConverter typeConv =
                System.ComponentModel.TypeDescriptor.GetConverter(typeof(ECC.Data.LocalizedAllocType));
            return typeConv.ConvertToString(lat);
        }

        private string localize(UPlanning.TypeSupplyFor supplyType) {
            ECC.Data.LocalizedSupplyType lst;
            switch(supplyType) {
                case UPlanning.TypeSupplyFor.PlOrderForBackorder:
                    lst = LocalizedSupplyType.BackorderDemand;
                    break;
                case UPlanning.TypeSupplyFor.PlOrderForLtFcst:
                    lst = LocalizedSupplyType.LTForecastDemand;
                    break;
                case UPlanning.TypeSupplyFor.PlOrderForStock:
                    lst = LocalizedSupplyType.OrderDemand;
                    break;
                case UPlanning.TypeSupplyFor.PlOrderForBom:
                    lst = LocalizedSupplyType.BomDemand;
                    break;
                case UPlanning.TypeSupplyFor.PlOrderForExt:
                    lst = LocalizedSupplyType.ExtDemand;
                    break;
                default:
                    return string.Empty;
            }

            System.ComponentModel.TypeConverter typeConv = System.ComponentModel.TypeDescriptor.GetConverter(typeof(ECC.Data.LocalizedSupplyType));
            return typeConv.ConvertToString(lst);
        }

        #endregion

        #region Save

        /** Method:  Save Sku Planning in a blob and Planned Orders  */
        public void Save() {
            Debug.WriteLine("DBP being Save() at " + DateTime.Now.ToLongTimeString());

            // si no hay plan borra el planfcst.
            if (sp == null || sku.PlanningRule != PlanningRuleTypes.ShiftFcstToRequirements) {
                planFcst = LoadPlanFcstForSku(sku);
                if (planFcst != null && planFcst.Count > 0) {
                    db.Drop(planFcst);
                }
                if (sp == null) { return; } 
            }

            //save skuPlan
            SkuPlanningBO sPlan = new SkuPlanningBO();
            db.Load(sPlan, this.sku);
            sPlan.Sku = sku;
            sPlan.CurrentPlan = this.sp;
            db.Save(sPlan);
            
            //delete old planned orders
            // el delete de las anteriores lo hacemos con un Drop personalizado.
            PlOrderDb podb = new PlOrderDb();
            podb.Drop("SKUID = " + sku.ID, db.CurrentTransaction);

            //read old firm orders
            List<Supply> oldSupplies = new List<Supply>();
            db.LoadMany(oldSupplies, "ACTUALSKUID = " + sku.ID + " AND PENDINGQTY > 0");
            //delete firm orders
            foreach (Supply sup in oldSupplies) {
                foreach (SupplyLine supLine in sup.SupplyLines) {
                    db.Drop(supLine);
                }
            }

            // limpia los BomDemands.
            if (sku.BomLevel != -1) {
                List<BomDemand> oldBomDemands = new List<BomDemand>();
                db.LoadMany(oldBomDemands, "PRODUCEDSKUID = " + sku.ID);
                foreach (BomDemand bd in oldBomDemands) {
                    db.Drop(bd);
                }
            }

            // save plan forecast. hay que guardarlo antes del DeletePlanning
            // porque hace un return.
            SavePlanFcstForSku(planFcst, sku);

            // si hay que borrar el planning, return.
            if (sku.PlanningRule == PlanningRuleTypes.DeletePlanning) {
                // si se borra el plan se pone a cero las firm orders.
                foreach (Supply sup in oldSupplies) {
                    sup.BackOrderQty = 0;
                    sup.LTFcstQty = 0;
                    sup.OrderQty = 0;
                    db.Save(sup);
                }
                db.Drop(sPlan);
                if (planFcst.Count > 0) { db.Drop(planFcst); }
                return;
            }

            // leo las demandas para volver a guardarlas luego de
            // setear la cantidad en Backorder.
            foreach (decimal iddem in this.qtysOnBackorder.Keys) {
                Demand dem = new Demand();
                if (db.Load(dem, iddem)) {
                    dem.QtyOnBackorder = this.qtysOnBackorder[iddem];
                    db.Save(dem);
                }
            }

            //save firm orders
            if (sp.EndAllocHorizonPN <= 0) { return; }
            DateTime endAllocHorizonDate = sp.GetPeriod(sp.EndAllocHorizonPN).Date;
            foreach (Supply fo in firmOrders.Values) {
                fo.ActualSku = sku;
                db.Save(fo);
                if (fo.ActualPenDate > endAllocHorizonDate) { continue; }
                foreach (SupplyLine fol in fo.SupplyLines) {
                    fol.Supply = fo;
                    db.Save(fol);
                }
            }

            //save planned orders
            foreach (PlOrder pO in PlOrders) {
                pO.Sku = sku;
                pO.SkuPlanning = sPlan;
                db.Save(pO);
                if (pO.ReleaseDate > endAllocHorizonDate) { continue; }
                foreach (PlOrderLine poL in pO.PlOrderLines) {
                    poL.PlOrder = pO;
                    db.Save(poL);
                }
 
            //save bom Demands
            BomDemandDb bdDb = new BomDemandDb();
            foreach (BomDemand bd in bomDemands.Values) {
                db.Save(bd);
            }
          }
        }
        // grabacion
        private void SavePlanFcstForSku(PlanFcstTSValueCollection pf, Sku sku) {
            if (pf == null) { return; }
            // la sku podría ser necesaria, pero parece que no, ya que la dfu
            // viene en el PlanFcst como header.
            Dfu dfu = ((DfuSerie)pf.Header).Dfu;

            // si en la colección no hay nada, no grabamos.
            if (pf.Count > 0) {
                // hay que grabar la DFU porque puede que no
                // haya sido dada de alta antes.
                db.Save(dfu);
                db.Save(pf);
            }
        }
        
        private DateTime Trunk(DateTime date) {
            return new DateTime(date.Year, date.Month, date.Day);
        }
        #endregion

        #endregion

        #region Private Methods

        #region Load Properties

        private void LoadProperties() {
            orderingCalendar = sku.OrderingCal;

            if(IPALStrDGGraphs.SDggs.GetIncidents(sku.IPALNode.Structure, sku.IPALNode.ID, Mode.rqmt).Count > 0) {
                sp.IsRoot = true;
            }
            else {
                sp.IsRoot = false;
            }

            sp.SkuId = sku.ID;
            sp.LeadTime = sku.PlanLeadTime;
            if (sp.LeadTime <= 0) {
                results.Add(DBPlanningResultType.LeadTimeZero); return;
            }
            sp.LoadRule((UPlanning.CalcType)Enum.ToObject(typeof(UPlanning.CalcType), sku.PlanningRule));

            if(sku.PlanLeadTime > sku.RsmpLeadTime && !sp.HasMinStock) {
                sp.HasMinStock = true;
                sp.Warnings.Add(UPlanning.Warning.PlanLeadTimeGreaterThanLeadTime);
            }
            else {
                sp.LeadTime = sku.PlanLeadTime;
            }
            sp.LeadTimeFcst = sku.LeadTimeFcstHist;
            sp.ReplenishmentFcst = sku.ReplenishmentFcstHist;
            sp.FirmPeriod = sku.FirmPeriod;
            sp.FixLot = (int)Math.Round(sku.LotSize);
            sp.MinUnitOrder = Convert.ToInt32(sku.RoundingQty);
            if (sp.FixLot < sp.MinUnitOrder) { sp.FixLot = sp.MinUnitOrder; }
            sp.StockIni = sku.OnHandQty;
            sp.PlanningHorizon = sku.PlanningHorizon;
            sp.AllocHorizon = sku.AllocHorizon;
            sp.Priority[0] = sku.AllocPriority;
            replenishmentPeriod = sku.PlanLeadTime + sku.DaysBtwnRpls;
            sp.FixPeriod = replenishmentPeriod;
            sp.RestToFP = sku.RestrictedToFp;
            if(sku.FirstSellingDate != StaticResources.FechaNula) { sp.FirstSellingDate = sku.FirstSellingDate; }
            if(sku.LastSupplyDate != StaticResources.FechaNula) { sp.LastSupplyDate = sku.LastSupplyDate; }
            this.onDmd = sku.PlanJustCustOrders;

        }

        #endregion

        #region Create Periods

        private void CreatePeriods(List<Period> periods) {
            foreach(Period per in periods) {
                sp.AddPeriod(per);
            }
        }

        private void CreatePeriods() {
            sp.FirstPN = 1;
            Period per;
            Periodo orderPer;
            skuSim = SkuSimilarity.GetSimilarityForSku(sku);
            for(int p=sp.FirstPN;p<=sp.FirstPN + sku.PlanningHorizon;p++) {
                per = new Period(sku.ID, p);
                if(per.PeriodNumber == 1) { per.IsFirstPeriod = true; }
                per.Date = iniDate.AddDays(p);
                switch (sku.LeadTimeFcstOrigin) {
                    case LeadTimeFcstOriginTypes.FromHistory:
                        per.LeadTimeFcst = sku.LeadTimeFcstHist;
                        break;
                    case LeadTimeFcstOriginTypes.Manual:
                        per.LeadTimeFcst = sku.LeadTimeFcstManual;
                        break;
                    case LeadTimeFcstOriginTypes.FromSimilarity:
                        if (skuSim == null || per.Date < skuSim.Since) { per.LeadTimeFcst = sku.LeadTimeFcstHist; } 
                        else { per.LeadTimeFcst = sku.LeadTimeFcstSimil; }
                        break;
                }
            
                per.Working = true;
                per.PlaceOrder = true;
                if(orderingCalendar != null) {
                    orderPer = orderingCalendar.Periodos.GetPeriodoByDate(per.Date);
                    per.PlaceOrder = (orderPer.Peso == 1);
                }
                sp.AddPeriod(per);
            }

            //working periods
            foreach(Holiday h in holidays) {
                per = sp.GetPeriod(h.HolidayDate);
                if(per != null) {
                    per.Working = false;
                    per.PlaceOrder = false;
                }
            }

            this.dateMgr = new UPlanning.DateMgr(sp.Periods, sp.FirstPN, sp.LastPN, minDaysForCalDays);
    
        }

        #endregion

        #region Load Demands

        /** Method:  Load Demands of this sku classifying as Forecast or Outlier  */
        private void LoadDemands() {

            Period per;
            this.extDemand = new Dictionary<int, List<Supply>>();
            
            List<Supply> supplies = new List<Supply>();
            supplies.AddRange(LoadPendingDemands());
            supplies.AddRange(LoadBomDemands());
            foreach(Supply s in supplies) {
                per = sp.GetPeriod(s.ReceptPN);
                if(!IsValidPlanningDate(per.Date)) { continue; }
                //assure that the recept period is a working period
                while (!per.Working) { 
                    s.ReceptPN = s.ReceptPN - 1;
                    per = sp.GetPeriod(s.ReceptPN);
                }
                
                //if it is extraordinary, then sum to period fcst
                if (s.TypeFor != UPlanning.TypeSupplyFor.PlOrderForBom && onDmd && per.PeriodNumber > sp.FirstPN) { s.TypeFor = UPlanning.TypeSupplyFor.PlOrderForExt; }
                //if (s.TypeFor == UPlanning.TypeSupplyFor.PlOrderForExt && s.ReceptPN > sp.FirstPN) { 
                if ((s.TypeFor == UPlanning.TypeSupplyFor.PlOrderForExt || s.TypeFor == UPlanning.TypeSupplyFor.PlOrderForBom) && s.ReceptPN > sp.FirstPN) { 
                    if(s.Qty >= sku.UnsualDmdUnitsThreshold || onDmd) {
                        if(!extDemand.ContainsKey(per.PeriodNumber)) { extDemand.Add(per.PeriodNumber, new List<Supply>()); }
                        extDemand[per.PeriodNumber].Add(s);
                    }
                    else {
                        if (s.TypeFor != UPlanning.TypeSupplyFor.PlOrderForBom) {
                            s.TypeFor = UPlanning.TypeSupplyFor.PlOrderForLtFcst;
                        }
                        per.AddDemandSupply(s);
                    }

                }
                //else add demand to the corresponding period
                else {
                    if (s.TypeFor == UPlanning.TypeSupplyFor.PlOrderForBackorder && s.ReceptPN > sp.FirstPN) { continue; }
                    if (s.ReceptPN > sp.FirstPN && s.TypeFor != UPlanning.TypeSupplyFor.PlOrderForBom && s.TypeFor != UPlanning.TypeSupplyFor.FirmOrderForBom) { 
                        s.TypeFor = UPlanning.TypeSupplyFor.PlOrderForStock; 
                    }
                    per.AddDemandSupply(s);
                }
            }
            
            //if(onDmd) {
                Supply suP;
                Period perd;
                foreach (int pNum in extDemand.Keys) {
                    foreach (Supply extDem in extDemand[pNum]) {
                        suP = new Supply(UPlanning.TypeSupply.Demand, extDem.Id, sku.ID, pNum, pNum, 0, extDem.Qty);
                        suP.ReceptPN = pNum;
                        if (extDem.TypeFor == UPlanning.TypeSupplyFor.PlOrderForBom) { suP.TypeFor = UPlanning.TypeSupplyFor.PlOrderForBom;  }
                        else { 
                            suP.TypeFor = UPlanning.TypeSupplyFor.PlOrderForExt; 
                        }
                        perd = sp.GetPeriod(pNum);
                        perd.AddDemandSupply(suP);
                    }
                }
                //return; 
            //}

            //add forecast demands
            if (sp.IsRoot || onDmd) { return; }
            Period periodo;
            foreach(Supply s in GetFcstDemands()) {
                //if(s.Type != UPlanning.TypeSupply.OrderForExt) { s.Type = UPlanning.TypeSupply.OrderForLtFcst; }
                periodo = sp.GetPeriod(s.ReceptPN);
                if(!IsValidPlanningDate(periodo.Date)) { continue; }
                periodo.AddDemandSupply(s);
            }
        }

        private void LoadPlOrders() {
            int i;
            Supply sup;
            Period per,consPer,maxOrderPer;

            //Debug.WriteLine("Sku\tPo\t:Pol\tDate\tQty");  
            branchesPlOrders.Clear();
            foreach(PlanCalculator spBranch in sp.GetSkuPlannings()) {
                List<PlOrder> pos = new List<PlOrder>();
                Sku sku = new Sku();
                db.Load(sku, spBranch.SkuId);
                if (HasSupplier(sku) || sku.BomLevel > 0) { continue; }
                db.LoadMany(pos, "SKUID = " + spBranch.SkuId);
                branchesPlOrders.Add(spBranch.SkuId, pos);
                foreach(PlOrder po in pos) {
                    i=0;
                    foreach(PlOrderLine pol in po.PlOrderLines) {
                        if(pol.PlOrder.ReleaseDate > sp.GetPeriod(sp.LastPN).Date) { continue; }
                        else if(pol.PlOrder.ReleaseDate < sp.FirstDate) { per = sp.GetPeriod(sp.FirstPN); }
                        else { per = sp.GetPeriod(pol.PlOrder.ReleaseDate); }

                        //assure that target period is working period
                        while(!per.Working && per.PeriodNumber > sp.FirstPN) {
                            per = sp.GetPeriod(per.PeriodNumber - 1);
                        }

                        //sup = new Supply(-1, sp.SkuId, per.PeriodNumber, per.PeriodNumber, i, pol.PlannedQty);
                        sup = new Supply(UPlanning.TypeSupply.PlOrder, po.ID, sp.SkuId, per.PeriodNumber, per.PeriodNumber, i, pol.PlannedQty);
                        sup.LineId = pol.ID;
                        switch (pol.AllocType) {
                            case AllocTypes.OrderForBackorder:
                                sup.TypeFor = UPlanning.TypeSupplyFor.PlOrderForBackorder;
                                break;
                            case AllocTypes.OrderForBom:
                                sup.TypeFor = UPlanning.TypeSupplyFor.PlOrderForBom;
                                break;
                            case AllocTypes.OrderForExt:
                                sup.TypeFor = UPlanning.TypeSupplyFor.PlOrderForExt;
                                break;
                            case AllocTypes.OrderForLtFcst:
                                sup.TypeFor = UPlanning.TypeSupplyFor.PlOrderForLtFcst;
                                break;
                            case AllocTypes.OrderForStock:
                                sup.TypeFor = UPlanning.TypeSupplyFor.PlOrderForStock;
                                break;
                        }
                        sup.ProdSkuId = spBranch.SkuId;
                        consPer = sp.GetPeriod(pol.ConsumptionDate);
                        if(consPer != null) { sup.ConsumedPN = consPer.PeriodNumber; }
                        sup.ReceptPN = per.PeriodNumber;
                        if (pol.LastReleaseDate < sp.FirstDate) { 
                            sup.MaxOrderPN = sp.FirstPN; 
                        } 
                        else {
                            maxOrderPer = sp.GetPeriod(pol.LastReleaseDate);
                            if (maxOrderPer != null) { sup.MaxOrderPN = sp.GetPeriod(pol.LastReleaseDate).PeriodNumber; } 
                            else { sup.MaxOrderPN = sp.LastPN; }
                        }
                        if(pol.PlOrder.ReleaseDate < sp.FirstDate) { sup.OrderPN = sp.FirstPN; }
                        else { sup.OrderPN = sp.GetPeriod(pol.PlOrder.ReleaseDate).PeriodNumber; }
                        switch(pol.Restriction) {
                            case RestrictionTypes.Ninguna:
                                sup.Result = UPlanning.ResultType.Ok;
                                break;
                            case RestrictionTypes.PeriodoFirme:
                                sup.Result = UPlanning.ResultType.OnFirmPeriod;
                                break;
                            case RestrictionTypes.PlazoEntrega:
                                sup.Result = UPlanning.ResultType.OnFirstLeadTime;
                                break;
                            case RestrictionTypes.Error:
                                sup.Result = UPlanning.ResultType.Error;
                                break;
                        }
                        if(sup.Qty != 0) { 
                            per.AddDemandSupply(sup); /*Debug.WriteLine(sup.SkuId + "\t" + po.ID +  "\t" + pol.ID + "\t" + sp.GetPeriod(sup.OrderPN).Date + "\t" + sup.Qty);*/
                        }
                        i++;
                    }
                }
            }
        }

        private void LoadPlanFcst() {
            PlanFcstTSValueCollection pfBranch;
            int branchNumber = -2;
            foreach(PlanCalculator branch in sp.GetSkuPlannings()) {
                Sku skuBranch = new Sku();
                db.Load(skuBranch, branch.SkuId);
                if (skuBranch.ID != -1) {
                    if (skuBranch.BomLevel > 0) { continue; }  //TODO: add a property in planFcst to avoid this asynchronic check
                    pfBranch = LoadPlanFcstForSku(skuBranch);
                    AdjustPeriodsPlanFcst(pfBranch);
                    foreach (TSValue tsv in pfBranch) {
                        if (tsv.Value > 0) {
                            Period per = sp.GetPeriod(tsv.IniDate);
                            
                            if (per != null) {
                                //assure that target period is working period
                                while (!per.Working && per.PeriodNumber > sp.FirstPN) {
                                    per = sp.GetPeriod(per.PeriodNumber - 1);
                                }
                                Supply s = new Supply(UPlanning.TypeSupply.Demand, branchNumber, skuBranch.ID, per.PeriodNumber, per.PeriodNumber, 1, tsv.Value);
                                s.TypeFor = UPlanning.TypeSupplyFor.PlOrderForLtFcst;
                                per.AddDemandSupply(s);
                            } else {
                                // este caso se da cuando el periodo inicial de planificacion es anterior al día de hoy.
                                Debug.WriteLine("PERIODO INEXISTENTE: " +  tsv.PeriodNumber + " - " + tsv.IniDate);
                            }
                        }
                    }
                }
                branchNumber++;
            }
        }

        private bool HasSupplier(Sku sku) {
            if (sku.Supplier != null && sku.Supplier.ID != -1) { 
                results.Add(DBPlanningResultType.BranchHasSupplier);
                return true;
            }
            return false;
        }

        /** Method: Loads Historical Demands already satisfied between two dates */
        [Obsolete("NO ES PORTABLE EN OTROS SGBD")]
        private UPlanning.DemandDist LoadHistDemand(DateTime start, DateTime end) {
            List<Demand> demands = new List<Demand>();
            string startDate = start.ToShortDateString();
            string endDate = end.ToShortDateString();
            db.LoadMany(demands,
                "PLANSKUID = " + sku.ID + " AND " +
                "DEMANDDESIREDDATE >= to_date('" + startDate + "' ,'DD/MM/YYYY') AND " +
                "DEMANDDESIREDDATE <= to_date('" + endDate   + "' ,'DD/MM/YYYY') AND " +
                "QTYREMAINING = 0");

            UPlanning.DemandDist dd = new UPlanning.DemandDist();
            foreach(Demand d in demands) { dd.Add(d.QtyOrdered); }
            return dd;
        }

        /** Method:  Load Demands still unsatisfied for planning  */
        private List<Supply> LoadPendingDemands() {
            List<Demand> demands = new List<Demand>();
            db.LoadMany(demands, "PLANSKUID = " + sku.ID + " AND QTYREMAINING > 0");
           
            Supply s;
            List<Supply> supplies = new List<Supply>();
            Period per;
            Period firstPer = sp.GetPeriod(sp.FirstPN);
            Period lastPer = sp.GetPeriod(sp.LastPN);
            foreach(Demand d in demands) {
                if(d.QtyRemaining <= 0) { continue; }
             
                if(d.DemandDesiredDate < firstPer.Date) {
                    per = firstPer;
                    s = new Supply(UPlanning.TypeSupply.Demand, d.ID, sku.ID, per.PeriodNumber, per.PeriodNumber, 0, d.QtyRemaining);
                    s.Type = UPlanning.TypeSupply.Demand;
                    s.TypeFor = UPlanning.TypeSupplyFor.PlOrderForBackorder;
                    per.AddNonExtDemandSupply(s);
                    supplies.Add(s);
                }
                else if(d.DemandDesiredDate > lastPer.Date) {
                    if(!planExcludeOutOfHorDmds) {
                        per = lastPer;
                        s = new Supply(UPlanning.TypeSupply.Demand, d.ID, sku.ID, per.PeriodNumber, per.PeriodNumber, 0, d.QtyRemaining);
                        s.Type = UPlanning.TypeSupply.Demand;
                        if(onDmd || sku.FcstPeriodicity == TipoCalendar.Monthly && d.MFiltered || sku.FcstPeriodicity == TipoCalendar.Weekly && d.WFiltered || sku.FcstPeriodicity == TipoCalendar.Daily && d.DFiltered) {
                            s.TypeFor = UPlanning.TypeSupplyFor.PlOrderForExt;
                        }
                        else {
                            s.TypeFor = UPlanning.TypeSupplyFor.PlOrderForBackorder;
                            per.AddNonExtDemandSupply(s);
                        }
                        supplies.Add(s);
                    }
                }
                else {
                    per = sp.GetPeriod(d.DemandDesiredDate);
                    s = new Supply(UPlanning.TypeSupply.Demand, d.ID, sku.ID, per.PeriodNumber, per.PeriodNumber, 0, d.QtyRemaining);
                    if(onDmd || sku.FcstProrateMethod == ProrateMethods.NonProrate || sku.FcstPeriodicity == TipoCalendar.Monthly && d.MFiltered || sku.FcstPeriodicity == TipoCalendar.Weekly && d.WFiltered || sku.FcstPeriodicity == TipoCalendar.Daily && d.DFiltered) {
                        s.TypeFor = UPlanning.TypeSupplyFor.PlOrderForExt;
                    }
                    else {
                        s.TypeFor = UPlanning.TypeSupplyFor.PlOrderForBackorder;
                        per.AddNonExtDemandSupply(s);
                    }
                    supplies.Add(s);
                }
            }
            return supplies;
        }

        private double CalcTotalLastFixPeriodDmds() {
            DateTime startDate = this.iniDate.AddDays(-replenishmentPeriod);
            return GetIntervalDemand(startDate, this.iniDate, false);
      
        }
     

        #region Load Forecast Demands

        private void LoadFcst() {

            Dfu dfu = new Dfu();
            if (!db.Load(dfu, sku.Material, sku.IPALNode, sku.FcstPeriodicity)) {
                var msg = string.Format(Strings.DBPlan_MissingDfu4Periodicity, sku.FcstPeriodicity);
                throw new ApplicationException(msg);
            }

            //fots
            Serie serie = Serie.GetSerieObservable(dfu.TipoCalendar);
            DfuSerie ds = new DfuSerie(dfu, serie);
            fots = new FcstTSValueCollection();
            db.Load(fots, ds);

            //Statoverrides
            sovs = new ArrayList();
            List<StatOverride> sos = new List<StatOverride>();
            db.LoadMany(sos, "DFUID = " + dfu.ID);
            foreach (StatOverride so in sos) {
                Cache.Manager.Put(so); // OJO! es necesario XXX.
                StatOverrideTSValueCollection sotsvCol = new StatOverrideTSValueCollection();
                db.Load(sotsvCol, so);
                sovs.Add(sotsvCol);
            }

            //validacion del firstDate
            if(fots != null && fots.Count > 0 && this.iniDate < fots[0].IniDate) {
                this.iniDate = fots[0].IniDate;
            }
        }

        private List<Supply> GetFcstDemands() {

            if(sku.FcstProrateMethod == ProrateMethods.NonProrate) {
                for(int p=sp.FirstPN;p<=sp.LastPN;p++) { sp.GetPeriod(p).BayesFcst = 0; }
                return new List<Supply>();
            }


            Period per = null;
            double fcst;
            double roundedFcst;

            double rest = 0.0;
            DateTime iniRest = StaticResources.FechaNula;
            Period perRest;
            Dictionary<int, double> fcsts = new Dictionary<int, double>();

            //Get prorate fcst
            sp.HasBayesFcst = false;
            int totDays;
            for(int p=sp.FirstPN;p<=sp.LastPN;p++) {
                per = sp.GetPeriod(p);
                fcst = 0.0;
                iniRest = StaticResources.FechaNula;
                if (fots != null) {
                    foreach (TSValue tsv in fots) {
                        if (per.Date >= tsv.IniDate && per.Date <= tsv.EndDate) {
                            if (per.Date.Date.Equals(tsv.EndDate.Date)) { iniRest = tsv.IniDate; }
                            totDays = (tsv.EndDate.Subtract(tsv.IniDate)).Days+1;
                            //fcst += Math.Round(tsv.Value) / (double)tsv.PeriodWeight; 
                            fcst += Math.Round(tsv.Value) / (double)totDays; 
                            break;
                        }
                    }
                }

                if(sovs != null) {
                    foreach(StatOverrideTSValueCollection soTsvCol in sovs) {
                        foreach(TSValue sotsv in soTsvCol) {
                            if(per.Date >= sotsv.IniDate && per.Date <= sotsv.EndDate) {
                                totDays = (sotsv.EndDate.Subtract(sotsv.IniDate)).Days+1;
                                //fcst += Math.Round(sotsv.Value) / (double)sotsv.PeriodWeight; 
                                fcst += Math.Round(sotsv.Value) / (double)totDays; 
                                break;
                            }
                        }
                    }
                }
                if(IsValidPlanningDate(per.Date)) {
                    fcsts.Add(p, fcst);
                    if(fcst > 0) { sp.HasBayesFcst = true; }
                }
            }


            //Calculo de nWorkingPeriods y nReplenishmentWorkingPeriods
            if(!sp.HasBayesFcst) {
                DateTime iniDate = sp.FirstDate;
                int iniPN = sku.IPALNode.GetCalendar(TipoCalendar.Daily).Periodos.GetPeriodoByDate(iniDate).NumeroPeriodo;
                DateTime lastDate = iniDate.AddYears(1);
                int lastPN = sku.IPALNode.GetCalendar(TipoCalendar.Daily).Periodos.GetPeriodoByDate(lastDate).NumeroPeriodo;

                int nHolidayPeriods = 0;
                //other holidays
                if(holidays != null) {
                    foreach(Holiday h in holidays) {
                        if (h.HolidayDate >= iniDate && h.HolidayDate <= endDate) { nHolidayPeriods++; }
                    }
                }
                nWorkingPeriods = (int)((double)((sku.PlanningHorizon - nHolidayPeriods) / (double)sku.PlanningHorizon)* 365);

                nReplensihmentWorkingPeriods = 0;
                int lastPNum = Math.Min(sp.LastPN, sp.FirstPN + replenishmentPeriod);
                for (int p = sp.FirstPN; p <= lastPNum; p++) { 
                    if(sp.GetPeriod(p).Working) { nReplensihmentWorkingPeriods++; }    
                }
            }

            perRest = null;
            for(int p=sp.FirstPN;p<=sp.LastPN;p++) {
                per = sp.GetPeriod(p);
                if(per == null ||  !IsValidPlanningDate(per.Date)) { continue; 
                }

                fcst = fcsts[p];
                
                //If it has not forecast, use Rsmp Rolling Forecast
                if(!sp.HasBayesFcst) {
                    switch (sku.LeadTimeFcstOrigin) {
                        case LeadTimeFcstOriginTypes.FromHistory:
                            if (p < replenishmentPeriod) { fcst = sku.ReplenishmentFcstHist / nReplensihmentWorkingPeriods; } 
                            else { fcst = sku.RsmpRollingFcstHist / nWorkingPeriods; }
                            break;
                        case LeadTimeFcstOriginTypes.Manual:
                            if (p < replenishmentPeriod) { fcst = sku.ReplenishmentFcstManual / nReplensihmentWorkingPeriods; } 
                            else { fcst = sku.RsmpRollingFcstManual / nWorkingPeriods; }
                            break;
                        case LeadTimeFcstOriginTypes.FromSimilarity:
                            if (skuSim == null || per.Date < skuSim.Since) {
                                if (p < replenishmentPeriod) { fcst = sku.ReplenishmentFcstHist / nReplensihmentWorkingPeriods; } 
                                else { fcst = sku.RsmpRollingFcstHist / nWorkingPeriods; }
                            } 
                            else {
                                if (p < replenishmentPeriod) { fcst = sku.ReplenishmentFcstSimil / nReplensihmentWorkingPeriods; } 
                                else { fcst = sku.RsmpRollingFcstSimil / nWorkingPeriods; }
                            }
                            break;
                    }
                }

                //rests assign
                roundedFcst = Math.Floor(fcst);
                rest = fcst + rest - roundedFcst;
                if(rest > 1) { 
                    roundedFcst += Math.Floor(rest);
                    rest -= Math.Floor(rest);
                }

                per.BayesFcst = roundedFcst;
                if(iniRest != StaticResources.FechaNula && rest > 0) {
                    perRest = sp.GetPeriod(iniRest);
                    if(perRest != null) {
                        perRest.BayesFcst = perRest.BayesFcst + rest;
                        //rest = 0;
                    }
                }
            }
            if(perRest != null) { perRest.BayesFcst = (int)Math.Round(perRest.BayesFcst); }
            
            //Proportional calculation
            if(sp.HasBayesFcst && sku.FcstProrateMethod == ProrateMethods.Proporcional) {
                TSValue firstTSV = null;
                foreach(TSValue tsv in fots) {
                    if(tsv.IniDate >= sp.FirstDate) {
                        firstTSV = tsv;
                        break;
                    }
                }

                if(firstTSV != null) {
                    double pendDem = GetIntervalDemand(firstTSV.IniDate, sp.FirstDate, true);
                    //obtain pending demand of the same (month, week) from database
                    if(pendDem > 0) {
                        double firstFcst = firstTSV.Value - pendDem;
                        //calculate total number of prorate periods
                        int nPeriods = 0;
                        for(int p=sp.FirstPN;p<=sp.LastPN;p++) {
                            per = sp.GetPeriod(p);
                            if(per == null) { continue; }
                            if(per.Date >= firstTSV.IniDate && per.Date <= firstTSV.EndDate) {
                                nPeriods++;
                            }
                        }
                        //overwrite prorated forecast
                        for(int p=sp.FirstPN;p<=sp.LastPN;p++) {
                            per = sp.GetPeriod(p);
                            if(per == null) { continue; }
                            if(per.Date >= firstTSV.IniDate && per.Date <= firstTSV.EndDate) {
                                per.BayesFcst = firstFcst/ (double)nPeriods;
                            }
                        }
                    }
                }
            }

            
            Period period;
            //re-assignment of forecast due to holidays
            Period lastWorkingPer = null;
            for (int p = 1; p < sp.Periods.Count; p++) {
                period = sp.Periods[p];
                if(period == null || !IsValidPlanningDate(period.Date)) { continue; }
                if (period.BayesFcst <= 0) {
                    if (period.Working) { lastWorkingPer = period; }
                    continue;
                }

                if (!period.Working && lastWorkingPer != null) {
                    lastWorkingPer.BayesFcst = lastWorkingPer.BayesFcst + period.BayesFcst;
                    period.BayesFcst = 0;
                } 
                else {
                    lastWorkingPer = period;
                }
            }
            
            //fcst supplies generation, bayesFcst reset (temporarily used for fcst demand sum)
            Supply s;
            List<Supply> supplies = new List<Supply>();
            for(int p = 1;p < sp.Periods.Count;p++) {
                period = sp.Periods[p];
                if(period == null || !period.Working || !IsValidPlanningDate(period.Date) || period.BayesFcst <= 0) { continue; }
                s = new Supply(UPlanning.TypeSupply.Demand , - 1, sku.ID, period.PeriodNumber, period.PeriodNumber, 0, period.BayesFcst);
                s.TypeFor = UPlanning.TypeSupplyFor.PlOrderForLtFcst;
                supplies.Add(s);
            }

            #region obsolete
            //extraordinary demand
            /*
            foreach(int pNum in extDemand.Keys) {
                foreach(Supply extDem in extDemand[pNum]) {
                    s = new Supply(UPlanning.TypeSupply.Demand, extDem.Id, sku.ID, pNum, pNum, 0, extDem.Qty);
                    s.ReceptPN = pNum;
                    s.TypeFor = UPlanning.TypeSupplyFor.PlOrderForExt;
                    supplies.Add(s);
                }
            }
            */
            #endregion

            return supplies;
        }

        private double GetIntervalDemand(DateTime iniDate, DateTime endDate, bool pending) {
            if(iniDate == endDate) { return 0.0; }
            List<Demand> demands = new List<Demand>();

            var periodo = string.Format("{0} BETWEEN {1} AND {2}",
                "DEMANDDESIREDDATE",
                db.ConvertToSqlDate(iniDate.Date),
                db.ConvertToSqlDate(endDate.Date.AddDays(1).AddSeconds(-1)));
            string condition = "PLANSKUID = " + sku.ID + " AND " + periodo;
            db.LoadMany(demands, condition);
            double pd = 0.0;
            foreach(Demand d in demands) {
                if (pending) { pd += d.QtyRemaining; } 
                else { pd += d.QtyOrdered; }
            }
            return pd;
        }

        #endregion


        #endregion

        #region Load Supplies

        private void LoadSupplies() {
            firmOrders.Clear();
            initialPenDates.Clear();
            List<Supply> supplies = new List<Supply>();
            if(sp == null) { return; }
            db.LoadMany(supplies, "ACTUALSKUID = " + sp.SkuId + " AND PENDINGQTY > 0 ORDER BY ACTUALPENDATE");
            Supply sup;
            Period per;
            foreach(Period prd in sp.Periods.Values) {
                if(prd != null) {
                    prd.GetSupplies().Clear();
                    prd.TotalFirmSupply = 0;
                }
            }
            foreach(Supply s in supplies) {
                s.BackOrderQty = 0.0;
                s.BomQty = 0.0;
                s.OutQty = 0.0;
                s.LTFcstQty = 0.0;
                s.OrderQty = 0.0;
                s.SupplyLines.Clear();
                firmOrders.Add(s.ID, s);
                per = sp.GetPeriod(s.InitialPenDate);
                if(per == null) {
                    if(s.InitialPenDate < sp.FirstDate) { per = sp.GetPeriod(sp.FirstPN); }
                    else if(s.InitialPenDate > sp.GetPeriod(sp.LastPN).Date) { AddResult(DBPlanningResultType.SuppliesBeyondHorizon); continue; } else {
                        throw new ApplicationException(string.Format(Strings.Error_Date_0_could_not_be_found, s.ActualPenDate));
                    }
                }
                if(!per.Working) {
                    per = dateMgr.GetNextWorking(per);
                    if(per == null) { continue; }
                }
                sup = new Supply(UPlanning.TypeSupply.FirmOrder, s.ID, s.ActualSku.ID, per.PeriodNumber, per.PeriodNumber, 0, s.PendingQty);
                sup.TypeFor = UPlanning.TypeSupplyFor.FirmOrderForStock;
                if(s.InitialPenDate <= sp.FirstDate) { sup.OrigReceptPN = sp.FirstPN; }
                else { sup.OrigReceptPN = sp.GetPeriod(s.InitialPenDate).PeriodNumber; }
                sup.ReceptPN = per.PeriodNumber;
                sup.OrigReceptPN = per.PeriodNumber;
                if(s.InitialPenDate > sp.GetPeriod(sp.LastPN).Date) { continue; }

                sup.OrderPN = sup.ReceptPN;
                sup.ConsumedPN = sp.LastPN;
                per.TotalOrderedFirmSupply = per.TotalOrderedFirmSupply + sup.Qty;
                initialPenDates.Add(sup.Id, s.InitialPenDate);
                per.AddSupply(sup);
            }
        }

        #endregion

        #region Load Branches

        #region From DGA

        private void LoadBranches() {
            IPALStructure str = sku.IPALNode.Structure;
            Sku skuBranch;
            PlanCalculator spBranch;
            DGVertex v = IPALStrDGGraphs.SDggs.GetVertex(str, sku.IPALNode.ID);
            sp.IsRoot = false;
            foreach(DGEdge e in IPALStrDGGraphs.SDggs.GetIncidents(str, v.Index, Mode.rqmt)) {
                skuBranch = new Sku();
                db.Load(skuBranch, sku.Material, e.Origin.IPALNode);
                if (skuBranch.ID != -1) {
                    spBranch = new PlanCalculator(skuBranch.ID, skuBranch.OnHandQty, minDaysForCalDays, Plan_LTToRPLimit);
                    spBranch.SkuId = skuBranch.ID;
                    sp.AddSkuPlanning(spBranch);
                    sp.IsRoot = true;
                }
            }
        }

        #endregion

        #endregion

        #region Load Policies

        private void LoadPolicies() {

            switch(sku.BckServPolicy) {
                case BackOrAllocPolicy.AllShareBack:
                    sp.BackOrAllocPol = UPlanning.BackOrAllocPolicy.AllShareBack;
                    break;
                case BackOrAllocPolicy.AllShareLtFcst:
                    sp.BackOrAllocPol = UPlanning.BackOrAllocPolicy.AllShareLtFcst;
                    break;
                case BackOrAllocPolicy.AllShareUniform:
                    sp.BackOrAllocPol = UPlanning.BackOrAllocPolicy.AllShareUniform;
                    break;
                case BackOrAllocPolicy.SomeFillMaxFirst:
                    sp.BackOrAllocPol = UPlanning.BackOrAllocPolicy.SomeFillMaxFirst;
                    break;
                case BackOrAllocPolicy.SomeFillMinFirst:
                    sp.BackOrAllocPol = UPlanning.BackOrAllocPolicy.SomeFillMinFirst;
                    break;
                case BackOrAllocPolicy.SomeFillPriority:
                    sp.BackOrAllocPol = UPlanning.BackOrAllocPolicy.SomeFillPriority;
                    break;
                case BackOrAllocPolicy.SomeSharePriority:
                    sp.BackOrAllocPol = UPlanning.BackOrAllocPolicy.SomeSharePriority;
                    break;
            }

            switch(sku.LtServPolicy) {
                case LtFcstAllocPolicy.AllShareBayFcst:
                    sp.LtFcstAllocPol = UPlanning.LtFcstAllocPolicy.AllShareBayFcst;
                    break;
                case LtFcstAllocPolicy.AllShareBayFcstTrunc:
                    sp.LtFcstAllocPol = UPlanning.LtFcstAllocPolicy.AllShareBayFcstTrunc;
                    break;
                case LtFcstAllocPolicy.AllShareCons:
                    sp.LtFcstAllocPol = UPlanning.LtFcstAllocPolicy.AllShareCons;
                    break;
                case LtFcstAllocPolicy.AllShareConsTrunc:
                    sp.LtFcstAllocPol = UPlanning.LtFcstAllocPolicy.AllShareConsTrunc;
                    break;
                case LtFcstAllocPolicy.AllShareLtFcst:
                    sp.LtFcstAllocPol = UPlanning.LtFcstAllocPolicy.AllShareLtFcst;
                    break;
                case LtFcstAllocPolicy.AllShareLtFcstTrunc:
                    sp.LtFcstAllocPol = UPlanning.LtFcstAllocPolicy.AllShareLtFcstTrunc;
                    break;
                case LtFcstAllocPolicy.AllShareUniform:
                    sp.LtFcstAllocPol = UPlanning.LtFcstAllocPolicy.AllShareUniform;
                    break;
                case LtFcstAllocPolicy.SomeFillMaxFirts:
                    sp.LtFcstAllocPol = UPlanning.LtFcstAllocPolicy.SomeFillMaxFirst;
                    break;
                case LtFcstAllocPolicy.SomeFillMinFirst:
                    sp.LtFcstAllocPol = UPlanning.LtFcstAllocPolicy.SomeFillMinFirst;
                    break;
                case LtFcstAllocPolicy.SomeFillPriority:
                    sp.LtFcstAllocPol = UPlanning.LtFcstAllocPolicy.SomeFillPriority;
                    break;
                case LtFcstAllocPolicy.SomeSharePriority:
                    sp.LtFcstAllocPol = UPlanning.LtFcstAllocPolicy.SomeSharePriority;
                    break;
                case LtFcstAllocPolicy.SomeFillPropLtFcst:
                    sp.LtFcstAllocPol = UPlanning.LtFcstAllocPolicy.SomeFillPropLtFcst;
                    break;
            }

            switch(sku.ObservPolicy) {
                case OrderAllocPolicy.AllShareTotal:
                    sp.OrderAllocPol = UPlanning.OrderAllocPolicy.AllShareTotal;
                    break;
                case OrderAllocPolicy.AllShareUniform:
                    sp.OrderAllocPol = UPlanning.OrderAllocPolicy.AllShareUniform;
                    break;
                case OrderAllocPolicy.SomeFillMinFirst:
                    sp.OrderAllocPol = UPlanning.OrderAllocPolicy.SomeFillMinFirst;
                    break;
                case OrderAllocPolicy.SomeFillPriority:
                    sp.OrderAllocPol = UPlanning.OrderAllocPolicy.SomeFillPriority;
                    break;
                case OrderAllocPolicy.SomeFilMaxFirst:
                    sp.OrderAllocPol = UPlanning.OrderAllocPolicy.SomeFillMaxFirst;
                    break;
                case OrderAllocPolicy.SomeSharePriority:
                    sp.OrderAllocPol = UPlanning.OrderAllocPolicy.SomeSharePriority;
                    break;
            }
        }

        #endregion

        #region Outlier Demand Methods


        private List<double> LoadHots(Sku sku, int nPeriods) {
            List<double> hotsSerie =  new List<double>();
            try {
                this.Sku = sku;
                Dfu dfu = new Dfu();
                db.Load(dfu, sku.Material, sku.IPALNode, TipoCalendar.Daily);
                Serie serie = Serie.GetSerieObservable(TipoCalendar.Daily);
                DfuSerie ds = new DfuSerie(dfu, serie);
                HistTSValueCollection hots = new HistTSValueCollection();
                db.Load(hots, ds);
                if(hots.Count < 2) {
                    System.Diagnostics.Debug.WriteLine("DBResampling.LoadHots: menos de 2 periodos.");
                }
                for(int i=hots.Count;i>=hots.Count-nPeriods;i--) {
                    hotsSerie.Add(hots[i].Value);
                }
            }
            catch(Exception e) {
                System.Diagnostics.Trace.WriteLine("DBResampling.LoadHots: " + e.Message);
            }
            return hotsSerie;
        }

        #endregion

        #region Results

        private void AddResult(DBPlanningResultType result) {
            if(!results.Contains(result)) { results.Add(result); }
        }

        private void LoadWarnings() {
            if(sp == null || sp.Warnings == null)
                return;

            foreach(UPlanning.Warning warn in sp.Warnings) {
                switch(warn) {
                    case UPlanning.Warning.FixLotTooSmall:
                        AddResult(DBPlanningResultType.FixLotTooSmall);
                        break;
                    case UPlanning.Warning.ProjStockNegativeOnFirm:
                        AddResult(DBPlanningResultType.ProjStockNegativeOnFirm);
                        break;
                    case UPlanning.Warning.ProjStockNegativeOutOfFirm:
                        if (sp.FirmPeriod == 0) { AddResult(DBPlanningResultType.ProjStockNegative); } 
                        else { AddResult(DBPlanningResultType.ProjStockNegativeOutOfFirm); }
                        break;
                    case UPlanning.Warning.BackOrderOnFirm:
                        AddResult(DBPlanningResultType.BackOrderOnFirm);
                        break;
                    case UPlanning.Warning.BackOrderOutOfFirm:
                        if (sp.FirmPeriod == 0) { AddResult(DBPlanningResultType.BackOrder); } 
                        else { AddResult(DBPlanningResultType.BackOrderOutOfFirm); }
                        break;
                    case UPlanning.Warning.ConsBeforeRec:
                        AddResult(DBPlanningResultType.ConsBeforeRec);
                        break;
                    case UPlanning.Warning.NoPlaceOrder:
                        AddResult(DBPlanningResultType.NoPlaceOrder);
                        break;
                    case UPlanning.Warning.AllFirmPeriod:
                        AddResult(DBPlanningResultType.AllFirmPeriod);
                        break;
                    case UPlanning.Warning.FixPeriodTooShort:
                        AddResult(DBPlanningResultType.FixPeriodTooShort);
                        break;
                    case UPlanning.Warning.LeadTimeFcstTooLow:
                        AddResult(DBPlanningResultType.LeadTimeFcstTooLow);
                        break;
                    case UPlanning.Warning.BackOrderOnFirstLeadTime:
                        AddResult(DBPlanningResultType.BackOrderOnFirstLeadTime);
                        break;
                    case UPlanning.Warning.LimitedByLeadTimeFcst:
                        if (sku.LeadTimeFcstOrigin == LeadTimeFcstOriginTypes.Manual) { AddResult(DBPlanningResultType.LimitedByLeadTimeFcst); }
                        break;
                    case UPlanning.Warning.PlanLeadTimeGreaterThanLeadTime:
                        AddResult(DBPlanningResultType.PlanLeadTimeGreaterThanLeadTime);
                        break;
                    case UPlanning.Warning.FirmOrdersMoved:
                        AddResult(DBPlanningResultType.FirmOrdersMoved);
                        break;
                    case UPlanning.Warning.BackOrderCoveredWithFirmOrderOnFirm:
                        AddResult(DBPlanningResultType.BackOrderCoveredWithFirmOrderOnFirm);
                        break;
                    case UPlanning.Warning.BackOrderCoveredWithFirmOrderOutOfFirm:
                        if (sp.FirmPeriod == 0) { AddResult(DBPlanningResultType.BackOrderCoveredWithFirmOrder); } 
                        else { AddResult(DBPlanningResultType.BackOrderCoveredWithFirmOrderOutOfFirm); }
                        break;
                    case UPlanning.Warning.BackOrderCoveredWithFirmAndPlOrderOnFirm:
                        AddResult(DBPlanningResultType.BackOrderCoveredWithFirmAndPlOrderOnFirm);
                        break;
                    case UPlanning.Warning.BackOrderCoveredWithFirmAndPlOrderOutOfFirm:
                        if (sp.FirmPeriod == 0) { AddResult(DBPlanningResultType.BackOrderCoveredWithFirmAndPlanned); } 
                        else { AddResult(DBPlanningResultType.BackOrderCoveredWithFirmAndPlOrderOutOfFirm); }
                        break;
                }
            }
        }

        #endregion

        #region Auxiliar Methods

        private int GetPeriodNumber(DateTime date) {
            if(skuCalendar != null) {
                Periodo per = skuCalendar.Periodos.GetPeriodoByDate(date);
                return per.NumeroPeriodo;
            }
            else {
                return -1;
            }
        }

        private DateTime GetDate(int periodNumber) {
            if(skuCalendar != null) {
                Periodo per = skuCalendar.Periodos.GetPeriodoByPeriodNumber(periodNumber);
                return per.FechaInicial;
            }
            else {
                return Periodo.NullValue.FechaInicial;
            }
        }

        private double GetDenom() {
            double denom = 0.0;
            switch(sku.FcstPeriodicity) {
                case TipoCalendar.Monthly:
                    denom = 22.0; //TODO: Consider other prorate methods
                    break;
                case TipoCalendar.Weekly:
                    denom = sku.DmdDaysPerWeek;
                    break;
                case TipoCalendar.Daily:
                    denom = 1.0;
                    break;
            }
            return denom;
        }

        private bool IsValidPlanningDate(DateTime date) {
            if((sp.FirstSellingDate != StaticResources.FechaNula && date < sp.FirstSellingDate) || (sp.LastSupplyDate != StaticResources.FechaNula && date > sp.LastSupplyDate)) {
                return false;
            }
            return true;
        }

        #endregion

        #region Clone skuPlanning

        private PlanCalculator Clone(PlanCalculator sp) {
            PlanCalculator clone = new PlanCalculator(sp.SkuId, sp.StockIni, minDaysForCalDays, Plan_LTToRPLimit);
            clone.Periods = new Dictionary<int, Period>();
            clone.DmdsAreSales = sp.DmdsAreSales;
            Period pCopy;
            AllocaCopy;
            Supply sCopy;

            foreach(int p in sp.PlOrders.Keys) {
                Supply s = sp.PlOrders[p];
                sCopy = new Supply(s);
                clone.PlOrders.Add(p, s);
            }

            foreach(Period p in sp.Periods.Values) {
                pCopy = new Period(p.SkuId, p.PeriodNumber);
                clone.AddPeriod(pCopy);
                    foreach(Alloc a in p.BackOrderDemands) {
                        aCopy = a.Clone();
                        pCopy.AddDemand(aCopy);
                    }
                    foreach(Alloc a in p.BomDemands) {
                        aCopy = a.Clone();
                        pCopy.AddDemand(aCopy);
                    }
                    foreach(Alloc a in p.ExtDemands) {
                        aCopy = a.Clone();
                        pCopy.AddDemand(aCopy);
                    }
                    foreach(Alloc a in p.LtFcstDemands) {
                        aCopy = a.Clone();
                        pCopy.AddDemand(aCopy);
                    }
                    foreach(Alloc a in p.OrderDemands) {
                        aCopy = a.Clone();
                        pCopy.AddDemand(aCopy);
                    }
                
                foreach(Supply s in p.GetSupplies()) {
                    sCopy = new Supply(s);
                    pCopy.AddSupply(s);
                }
            }
            return clone;
        }

        #endregion

        #region Log for Debugging

        private void WriteLog() {
            Period per;
            for (int p = sp.FirstPN; p <= sp.PlanningHorizon; p++) {
                per = sp.GetPeriod(p);
                if (p > 150 - sp.FirstPN) { return; } 
                Debug.WriteLine(string.Format("PERIODO {0} ({1}): {2}", p, per.PeriodNumber, sp.GetPeriod(per.Date).Date));
                if (!log.ContainsKey(p)) { Debug.WriteLine("\tNot valid"); continue; }

                Debug.WriteLine("\t Fin del plazo de entrega:                      \t" + sp.GetPeriod(log[p].endLeadTime).Date.ToString());
                if (log[p].endFixPeriod > 0) {
                    Debug.WriteLine("\t Fin del plazo de reposición:                   \t" + sp.GetPeriod(log[p].endFixPeriod).Date.ToString("dd-MM-yyyy"));
                }
                Debug.WriteLine("\t Stock proyectado final:                        \t" + per.StockEnd);
                Debug.WriteLine("\t Pronóstico plazo de entrega:                   \t" + per.LeadTimeFcst);
                Debug.WriteLine("\t Demanda en el plazo de entrega:                \t" + log[p].dmdOnLeadTime);
                Debug.WriteLine("\t Ordenes en firme en plazo de entrega:          \t" + log[p].firmOnLeadTime);
                if (log[p].PlOrderQty == 0) {
                    Debug.WriteLine("\t No planned orders");
                } else {
                    Debug.WriteLine("\t Demanda después del plazo de entrega:          \t" + log[p].dmdAfterLeadTime);
                    Debug.WriteLine("\t Ordenes en firme después del plazo de entrega: \t" + log[p].firmAfterLeadTime);
                    Debug.WriteLine("\t Orden planificada:                             \t" + log[p].PlOrderQty);
                }
                Debug.WriteLine("");
            }
        }
        
        #endregion

        #endregion

    }

    #region Enums

    /** Method:  Planification result  */
    [System.ComponentModel.TypeConverter(typeof(LocalizedEnumConverter))]
    public enum DBPlanningResultType {
        /** Method:  Planification with no errors nor warnings  */
        Ok,

        /** Method:  Firm stock not enough to cover demand in some period(s). Backorder generated  */
        FillNotRate,

        /** Method:  Some supplies are beyond planning horizon  */
        SuppliesBeyondHorizon,

        /** Method:  Stock loss as a result of fix lot quantity being too small  */
        FixLotTooSmall,

        /** Method:  Projected stock is negative (no firm period)  */
        ProjStockNegative,
        
        /** Method:  Projected stock is negative due to firm period constraints  */
        ProjStockNegativeOnFirm,

        /** Method:  Projected stock is negative out of firm period  */
        ProjStockNegativeOutOfFirm,

        /** Method:  Backorder generated (firm period)  */
        BackOrder,
        
        /** Method:  Backorder generated on firm period  */
        BackOrderOnFirm,

        /** Method:  Backorder generated out of firm period   */
        BackOrderOutOfFirm,

        /** Method:  Order consumed before receipt  */
        ConsBeforeRec,

        /** Method:  There are no place order periods  */
        NoPlaceOrder,

        /** Method:  All periods are into firm period  */
        AllFirmPeriod,

        /** Method:  Fix period has been fixed too short, so a stock loss takes place  */
        FixPeriodTooShort,

        /** Method:  Lead time forecast is to low, so a stock loss takes place  */
        LeadTimeFcstTooLow,

        /** Method:  There is backorder in first lead time  */
        BackOrderOnFirstLeadTime,

        /** Method:  Limited by Lead time Forecast, as lead time forecast is greater than whole demand in fix period  */
        LimitedByLeadTimeFcst,

        /** Method:  Plan lead time greater than leadtime (restricted to leadtime)  */
        PlanLeadTimeGreaterThanLeadTime,

        /** Method:  Backorder generated on firm period covered with firm order  */
        BackOrderCoveredWithFirmOrderOnFirm,

        /** Method:  Backorder generated covered with firm order (no firm period)  */
        BackOrderCoveredWithFirmOrder,
        
        /** Method:  Backorder generated out of firm period covered with firm order  */
        BackOrderCoveredWithFirmOrderOutOfFirm,

        /** Method:  Backorder generated on firm period covered with firm and planned order  */
        BackOrderCoveredWithFirmAndPlOrderOnFirm,

        /** Method:  Backorder generated covered with firm  and planned order (no firm period)  */
        BackOrderCoveredWithFirmAndPlanned,
        
        /** Method:  Backorder generated out of firm period covered with firm  and planned order  */
        BackOrderCoveredWithFirmAndPlOrderOutOfFirm,

        /** Method:  If firm orders have been moved  */
        FirmOrdersMoved,

        /** Method:  If initial date of planning was changed by the system itself  */
        InitialDateChanged, 
        /** Method:  No lead time  */
        LeadTimeZero,
        /** Method:  Error in planned orders  */
        ErrorPlOrders,
        /** Method:  If any branch has Supplier  */
        BranchHasSupplier
    }

    /** Method: 
    Tipos de asignaciones
     */
    [System.ComponentModel.TypeConverter(typeof(LocalizedEnumConverter))]
    public enum LocalizedAllocType {
        /** Method: 
        Pendiente
         */
        Pending=0,
        /** Method: 
        No puede asignar
         */
        CantAlloc=1,
        /** Method: 
        Desde O.Planificadas
         */
        FromRec=2,
        /** Method: 
        Desde O.Firme
         */
        FromFirm=3,
        /** Method: 
        Desde Stock
         */
        FromStock=4,
        /** Method: 
        Desde Stock y Desde O.Firme
         */
        FromStockAndFirm=5,
        /** Method: 
        Desde O.Firme y Desde O.Planificadas
         */
        FromFirmAndRec=6,
        /** Method: 
        Desde Stock, Desde O.Firme y Desde O.Planificadas
         */
        FromStockAndFirmAndRec=7,
        /** Method: 
        Desde Stock y Desde O.Planificadas
         */
        FromStockAndRec=8,
    }


    /** Method: Tipos de suminstro traducido.(deberia ser planningdemandtype  */
    public enum LocalizedSupplyType {
        /** Method:  Demand from backorder  */
        BackorderDemand,
        /** Method:  Demand from lead time forecast  */
        LTForecastDemand,
        /** Method:  Demand beyond lead time forecast for coverage  */
        OrderDemand,
        /** Method:  Demand from Bom relations  */
        BomDemand,
        /** Method:  Extraordinary demand  */
        ExtDemand
    }

    /** Method: 
    Control de resultado.
     */
    public enum CheckResultType {
        ///<summary>More Planned orders than previous planning */
        MorePlOrders,
        ///<summary>Less Planned orders than previous planning */
        LessPlOrders,
        ///<summary>More Planned order lines than previous planning */
        MorePlOrderLines,
        ///<summary>Less Planned order lines than previous planning */
        LessPlOrderLines,
        ///<summary>Different planned order quantity than previous planning */
        DifferentPlOrderQuantity,
        ///<summary>Different planned order release date */
        DifferentPlOrderReleaseDate,
        ///<summary>Different planned order reception date than previous planning */
        DifferentPlOrderReceptionDate,
        ///<summary>Different planned order consumption date than previous planning */
        DifferentPlOrderConsumptionDate,
        ///<summary>Different planned order last release date than previous planning */
        DifferentPlOrderLastReleaseDate,
        ///<summary>Equal planned orders but different SkuPlanning than previous one */
        DifferentSkuPlanning,
        ///<summary>Equal planned orders but different planning horizon */
        DifferentPlanningHorizon,
        ///<summary>Equal planned orders but different number of requests */
        DifferentNumberOfRequests,
        ///<summary>Equal planned orders but different supply type */
        DifferentSupplyType,
        ///<summary>Equal planned orders but different  */
        DifferentAllocQty,
        ///<summary>Equal planned orders but different  */
        DifferentAllocSupply,
        ///<summary>Equal planned orders but different  */
        DifferentRqstSupply,
        ///<summary>Equal planned orders but different  */
        DifferentConfirmPrd,
        ///<summary>Equal planned orders but different number of backorders */
        DifferentNumberOfBackorderDmds,
        ///<summary>Equal planned orders but different supply type in backorders  */
        DifferentBackSupplyType,
        ///<summary>Equal planned orders but different quantity  in backorders */
        DifferentBackAllocQty,
        ///<summary>Equal planned orders but different Allocsupply in backorders */
        DifferentBackAllocSupply,
        ///<summary>Equal planned orders but different request supply in backorders */
        DifferentBackRqstSupply,
        ///<summary>Equal planned orders but different confirm period in backorders */
        DifferentBackConfirmPrd,
        ///<summary>Equal planned orders but different number of bom demands */
        DifferentNumberOfBomDmds,
        ///<summary>Equal planned orders but different supply type in boms */
        DifferentBomSupplyType,
        ///<summary>Equal planned orders but different quantity in boms */
        DifferentBomAllocQty,
        ///<summary>Equal planned orders but different Allocsupply in boms */
        DifferentBomAllocSupply,
        ///<summary>Equal planned orders but different request supply in boms */
        DifferentBomRqstSupply,
        ///<summary>Equal planned orders but different confirm period in boms */
        DifferentBomConfirmPrd,
        ///<summary>Equal planned orders but different number of outlier demands */
        DifferentNumberOfExtDmds,
        ///<summary>Equal planned orders but different supply type in outlier demands */
        DifferentExtSupplyType,
        ///<summary>Equal planned orders but different quantity in outlier demands */
        DifferentExtAllocQty,
        ///<summary>Equal planned orders but different Allocsupply in outlier demands */
        DifferentExtAllocSupply,
        ///<summary>Equal planned orders but different request supply in outlier demands */
        DifferentExtRqstSupply,
        ///<summary>Equal planned orders but different confirm period in outlier demands */
        DifferentExtConfirmPrd,
        ///<summary>Equal planned orders but different number of ltForecast demands */
        DifferentNumberOfLtFcstDmds,
        ///<summary>Equal planned orders but different supply type in ltForecast demands */
        DifferentLtFcstSupplyType,
        ///<summary>Equal planned orders but different quantity in ltForecast demands */
        DifferentLtFcstAllocQty,
        ///<summary>Equal planned orders but different Allocsupply in ltForecast demands */
        DifferentLtFcstAllocSupply,
        ///<summary>Equal planned orders but different request supply in ltForecast demands */
        DifferentLtFcstRqstSupply,
        ///<summary>Equal planned orders but different confirm period in ltForecast demands */
        DifferentLtFcstConfirmPrd,
        ///<summary>Equal planned orders but different number of order demands */
        DifferentNumberOfOrderDmds,
        ///<summary>Equal planned orders but different supply type in order demands */
        DifferentOrderSupplyType,
        ///<summary>Equal planned orders but different quantity in order demands */
        DifferentOrderAllocQty,
        ///<summary>Equal planned orders but different Allocsupply in order demands */
        DifferentOrderAllocSupply,
        ///<summary>Equal planned orders but different request supply in order demands */
        DifferentOrderRqstSupply,
        ///<summary>Equal planned orders but different confirm period in order demands */
        DifferentOrderConfirmPrd,
        ///<summary>Equal planned orders but different number of supplies */
        DifferentNumberOfSupplies,
        ///<summary>Equal planned orders but different reception period in supplies */
        DifferentSupplyFcstRcvPrd,
        ///<summary>Equal planned orders but different consume period in supplies */
        DifferentSupplyFcstCsmPrd,
        ///<summary>Equal planned orders but different order period in supplies */
        DifferentSupplyOrderPrd,
        ///<summary>Equal planned orders but different quantity in supplies */
        DifferentSupplyQty,
        ///<summary>No changes. Identical to previous planning */
        NoChanges
    }

    /** Method:  Status of DBPlanning calculation  */
    public enum StatusType {
        /** Method:  object created  */
        created,
        /** Method:  data loaded  */
        loaded,
        /** Method:  data loaded for calculation  */
        loadedForCalc,
        /** Method:  planning calculated  */
        calculated
    }

    #endregion
}

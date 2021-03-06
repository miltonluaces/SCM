#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

#endregion

namespace AibuSet {

    internal class Sku : BusObject, INotifyPropertyChanged {

        #region Fields

        private Product product;
	    private Node node;
	    private Supplier supplier;
	    private DateTime simFirstDate;
	    private double serviceLevel;
	    private int leadTime;
        private double simFcst;
	    private int replenishmentTime;
	    private int lotSize;
	    private int roundingQty;
	    private bool isPeriodFixed;
	    private bool planJustCustOrders;
        private bool hasSubSkus;
	    private double obsRisk;
	    private double obsExpValue;
	    private string planningRule; //TODO: change type
	    private Calendar supplierCal;
	    private Policy policy;

	    private double rsmpFilteringProb;
	    private double rsmpNoise;
	    private double rsmpClusterThreshold;
	    private double stock;
	    private double bckSafetyStock;
	    private double price;
	    private double ltFcstMinPercOver;
	    private double orderMinPercOver;
	    private int lastNPeriods;
	    private int bomLevel;
	    private DateTime firstSellingDate;
	    private DateTime lastSupplyDate;
	    private double rsmpRollingFcstHist;
	    private double leadTimeFcstManual;
	    private double rsmpRollingFcstManual;
	    private double replenishmentFcstManual;
	    private string leadTimeFcstOrigin; //TODO: change type
	    private bool verySMP;
	    private double minServiceLevel; 
	    private double maxServiceLevel;
        private double cost;
        private double volume;
        private int planHorizon;
        private int validHorizon;
        private StateType state;

        private bool predictable = true;
        private bool checkPredict;

        private int fcstHorizon;
        private DateTime firstHistDate;
        private ProviderType provider;
        private DateTime nullDate;

        public event PropertyChangedEventHandler PropertyChanged;
        
        #endregion

        #region Constructor

        internal Sku() : base() {
            this.state = StateType.inactive;
            this.provider = ProviderType.none;
            this.supplierCal = new Calendar(1);
            this.nullDate = SysEnvironment.GetInstance().NullDate();
            this.firstSellingDate = nullDate;
            this.lastSupplyDate = nullDate;
            this.firstHistDate = nullDate;
            this.lastSupplyDate = nullDate;
            this.RsmpFirstDate = nullDate;
        }

        internal Sku(ulong id) : base(id) {
            this.state = StateType.inactive;
            this.provider = ProviderType.none;
            this.supplierCal = new Calendar(1);
        }

        #endregion

        #region Properties

        public Product Product {
		    get { return product; }
		    set { 
                product = value;
            
            }
	    }

        public Node Node
        {
		    get { return node; }
		    set { node = value; }
	    }

        public Supplier Supplier
        {
		    get { return supplier; }
		    set { supplier = value; }
	    }

        public DateTime RsmpFirstDate
        {
		    get { return simFirstDate; }
		    set { simFirstDate = value; }
	    }

        public ProviderType Provider
        {
            get { return provider; }
            set { provider = value; }
        }

        public StateType State
        {
            get { return state; }
            set { state = value; }
        }

        public double ServiceLevel
        {
		    get { return serviceLevel; }
		    set { serviceLevel = value; }
	    }

        public int LeadTime
        {
		    get { return leadTime; }
		    set { 
                leadTime = value;
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) { handler(this, new PropertyChangedEventArgs("LeadTime")); }
                //OnPropertyChanged("LeadTime");
            }
	    }

        public double SimFcst
        {
            get { return simFcst; }
            set { simFcst = value; } 
        }

        public int ReplenishmentTime
        {
		    get { return replenishmentTime; }
		    set { replenishmentTime = value; }
	    }

        internal int LotSize {
		    get { return lotSize; }
		    set { lotSize = value; }
	    }

        public int RoundingQty  {
		    get { return roundingQty; }
            set { roundingQty = value; }
	    }

        internal bool IsPeriodFixed  {
		    get { return isPeriodFixed; }
            set { isPeriodFixed = value; }
	    }
       
        public bool PlanJustCustOrders
        {
		    get { return planJustCustOrders; }
            set { planJustCustOrders = value; }
	    }

        public bool HasSubSkus
        {
            get { return hasSubSkus; }
            set { hasSubSkus = value; }
        }
        
        public double ObsRisk
        {
		    get { return obsRisk; }
            set { obsRisk = value; }
	    }

        public double ObsExpValue
        {
		    get { return obsExpValue; }
            set { obsExpValue = value; }
	    }

        public string PlanningRule
        {
		    get { return planningRule; }
		    set { planningRule = value; }
	    }

        internal Calendar SupplierCal
        {
		    get { return supplierCal; }
		    set { supplierCal = value; }
	    }

        public Policy Policy
        {
		    get { return policy; }
		    set { policy = value; }
	    }

        public double RsmpFilteringProb
        {
		    get { return rsmpFilteringProb; }
		    set { rsmpFilteringProb = value; }
	    }

        internal double RsmpNoise
        {
		    get { return rsmpNoise; }
		    set { rsmpNoise = value; }
	    }

        internal double RsmpClusterThreshold
        {
		    get { return rsmpClusterThreshold; }
		    set { rsmpClusterThreshold = value; }
	    }

        public double Stock
        {
		    get { return stock; }
		    set { stock = value; }
	    }

        internal double BckSafetyStock  {
		    get { return bckSafetyStock; }
		    set { bckSafetyStock = value; }
	    }

        internal double Price  {
            get { return price; }
            set { price = value; }
        }

        internal double LtFcstMinPercOver {
		    get { return ltFcstMinPercOver; }
		    set { ltFcstMinPercOver = value; }
	    }

        internal double OrderMinPercOver {
		    get { return orderMinPercOver; }
		    set { orderMinPercOver= value; }
	    }

        internal int LastNPeriods {
		    get { return lastNPeriods; }
		    set { lastNPeriods = value; }
	    }

        public int BomLevel
        {
		    get { return bomLevel; }
		    set { bomLevel = value; }
	    }

        public DateTime FirstSellingDate
        {
		    get { return firstSellingDate ; }
		    set { firstSellingDate = value; }
	    }
        public DateTime LastSupplyDate
        {
		    get { return lastSupplyDate; }
		    set { lastSupplyDate = value; }
	    }

        public double RsmpRollingFcstHist
        {
		    get { return rsmpRollingFcstHist; }
		    set { rsmpRollingFcstHist = value; }
	    }

        public double LeadTimeFcstManual
        {
		    get { return leadTimeFcstManual; }
		    set { leadTimeFcstManual = value; }
	    }

        public double RsmpRollingFcstManual
        {
		    get { return rsmpRollingFcstManual; }
		    set { rsmpRollingFcstManual = value; }
	    }

        internal double ReplenishmentFcstManual {
		    get { return replenishmentFcstManual; }
		    set { replenishmentFcstManual = value; }
	    }

        internal string LeadTimeFcstOrigin {
		    get { return leadTimeFcstOrigin; }
		    set { leadTimeFcstOrigin = value; }
	    }

        internal bool VerySMP {
		    get { return verySMP; }
		    set { verySMP = value; }
	    }

        public double MinServiceLevel
        {
		    get { return minServiceLevel; }
		    set { minServiceLevel = value; }
	    }

        public double MaxServiceLevel
        {
		    get { return maxServiceLevel; }
		    set { maxServiceLevel = value; }
        }

        public double Cost
        {
            get { return cost; }
            set { cost = value; }
        }

        public double Volume
        {
            get { return volume; }
            set { volume = value; }
        }

        public bool Predictable
        {
            get { return predictable; }
            set { predictable = value; }
        }

        public bool CheckPredict
        {
            get { return checkPredict; }
            set { checkPredict = value; }
        }

        public int FcstHorizon
        {
            get { return fcstHorizon; }
            set { fcstHorizon = value; }
        }

        public DateTime FirstHistDate
        {
            get { return firstHistDate; }
            set { firstHistDate = value; }
        }

        public int PlanHorizon
        {
            get { return planHorizon; }
            set { planHorizon = value; }
        }

        public int ValidHorizon
        {
            get { return validHorizon; }
            set { validHorizon = value; }
        }

        #endregion

        #region Persistence

        internal override Broker GetBroker() { return BrkrMgr.GetInstance().GetBroker((Sku)this); }

        #endregion

        #region ToString override

        public override string ToString()  {
            //return base.ToString() + " " + product.Code + " " + node.Code; 
            return this.id.ToString(); 
        }
        
        #endregion

        #region Enums

        internal enum StateType { inactive, desactived = 1, active = 2 };
        internal enum ProviderType { none = 0, rootNode = 1, supplier = 2 };

        #endregion

        private void OnPropertyChanged(string propName) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) { handler(this, new PropertyChangedEventArgs(propName)); }
        }
    }

    internal class SkuChangeArgs : EventArgs {
        internal Sku sku { get; set; }
    }

}


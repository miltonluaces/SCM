#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AibuSet {

    internal class Calendar : DbObject {

        #region Fields

        private DateTime firstDate;
        private DateTime lastDate;
        private bool[] weekHols;
        private Dictionary<DateTime, DateTime> yearHols;

        #endregion

        #region Constructors

        internal Calendar()
            : base() {
            this.weekHols = new bool[7];
            for (int i = 0; i < 5; i++) { weekHols[i] = false; }
            this.weekHols[5] = true;
            this.weekHols[6] = true;
            this.yearHols = new Dictionary<DateTime, DateTime>();
        }

        internal Calendar(ulong id)
            : base(id) {
            this.weekHols = new bool[7];
            for (int i = 0; i < 5; i++) { weekHols[i] = false; }
            this.weekHols[5] = true;
            this.weekHols[6] = true;
            this.yearHols = new Dictionary<DateTime, DateTime>();
        }

        internal Calendar(int firstPN, DateTime firstDate, DateTime lastDate, bool[] weekHols, List<DateTime> yearHols)
            : base() {
            this.id = id;
            this.firstDate = firstDate.Date;
            this.lastDate = lastDate.Date;
            this.weekHols = weekHols;
            this.yearHols = new Dictionary<DateTime, DateTime>();
            if (yearHols != null && yearHols.Count > 0) {
                foreach (DateTime yearHol in yearHols) { this.yearHols.Add(yearHol.Date, yearHol.Date); }
            }
            
        }

        #endregion

        #region Properties

        internal DateTime FirstDate {
            get { return firstDate; }
            set { firstDate = value; }
        }

        internal DateTime LastDate {
            get { return lastDate; }
            set { lastDate = value; }
        }

        internal bool[] WeekHols {
            get { return weekHols; }
            set { weekHols = value; }
        }

        internal Dictionary<DateTime, DateTime> YearHols {
            get { return yearHols; }
            set { yearHols = value; }
        }
        
        #endregion

        #region internal Methods

        internal bool IsHoliday(DateTime date) {
            bool isWeekHol = weekHols[((int)date.DayOfWeek + 6) % 7];
            bool isYearHol = yearHols.ContainsKey(date);
            return (isWeekHol || isYearHol);
        }

        #endregion

        #region Persistence

        internal override Broker GetBroker() { return BrkrMgr.GetInstance().GetBroker((Calendar)this); }

        #endregion

    }
}

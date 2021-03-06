#region Imports

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AibuSet {

    internal abstract class DbObject {

        #region Fields

        protected ulong id;
        protected string code;
        protected DateTime updated;
        protected DateTime created;

        #endregion
        
        #region Constructor

        protected DbObject() {
            this.created =DateTime.Now;
        }

        protected DbObject(ulong id) {
            this.id = id;
            this.created = DateTime.Now;
        } 

        #endregion

        #region Properties

        public ulong Id {
            get { return id; }
            set { id = value; }
        }

        public string Code {
            get { return code; }
            set { code = value; }
        }

        internal DateTime Updated {
            get { return updated; }
            set { updated = value; }
        }

        internal DateTime Created {
            get { return created; }
            set { created = value; }
        }

        #endregion

        #region internal Methods

        internal virtual Broker GetBroker() { return null; }

        internal void SaveUpdate() { GetBroker().SaveUpdate(this); }
        internal void Read() { GetBroker().Read(this); }
        internal void Read(string fieldName, ulong id) { GetBroker().Read(this, fieldName, id); }
        internal void Read(string condition) { GetBroker().Read(this, condition); }
        internal void Delete() { GetBroker().Delete(this); }
        internal void ReadMany(List<DbObject> objs, string condition) { GetBroker().ReadMany(objs, condition); }
        internal void DeleteMany(string condition) { GetBroker().DeleteMany(this, condition); }
        internal void UpdateMany(string updQuery) { GetBroker().UpdateMany(this, updQuery); }
        

        #endregion

        #region ToString override

        public override string ToString()  {
            return id + " " + code + " " + created;
        }

        #endregion

    }
}

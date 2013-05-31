using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FNWifiLocatorLibrary
{
    class FNDB
    {
        private datapds1Entities2 db = null;
        public FNDB()
        {
            this.db = new datapds1Entities2();
        }

        public datapds1Entities2 getDBInstance()
        {
            return this.db;
        }

        public void SaveChanges(){
            this.db.SaveChanges();
        }
    }
}

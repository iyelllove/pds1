using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pds1
{
    static class Helper
    {
        static public void printAllNetworks() {
            var db = new Model1Container1();

            if (db.Networks.ToList() != null && db.Networks.Any())
            {
                Log.trace("All Networks in the database:");
                foreach (var item in db.Networks.ToList())
                {
                    Log.trace("Networks: \t" + item.SSID + ",\t" + item.MAC);
                }
            }
        
        }
        static public void updateMeasures()
        {
            var db = new Model1Container1();
            if (db.Networks.ToList() != null && db.Networks.Any())
            {
                Log.trace("All Networks in the database:");
                foreach (var item in db.Networks.ToList())
                {
                    Log.trace("Networks: \t" + item.SSID + ",\t" + item.MAC);
                }
            }

        }
    }
}

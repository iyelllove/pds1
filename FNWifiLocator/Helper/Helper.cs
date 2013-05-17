using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using FNWifiLocatorLibrary;



namespace FNWifiLocator
{

    class Helper : FNWifiLocatorLibrary.Helper
    {
        static public List<FNWifiLocatorLibrary.Place> getAllRootPlaces()
        {
            datapds1Entities2 db = getDB();
            return db.Places.Where(c => c.Parent.Equals(null)).ToList();
        }
    }
}

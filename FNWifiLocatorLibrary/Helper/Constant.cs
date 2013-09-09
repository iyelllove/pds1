using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FNWifiLocatorLibrary
{
    public static class Constant
    {
        public static String DbConnectionString = "data source=C:\\Users\\Nico\\Documents\\Visual Studio 2012\\Projects\\pds1\\datapds1.sdf";

        public const String ServicePipeName = "FNPipeService";
        public const String LocatorPipeName = "FNPipeLocator";

        
        public const int DefaultRilevance = 10;
        public const int SearchPlaceTimeout = 30000;


        public static string getConnectionString() {
            /*
            if (DbConnectionString == null) {
                var db = Helper.getDB();
                DbConnectionString = db.Database.Connection.ConnectionString;
            }*/
            return DbConnectionString;
        }

    }
}
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
        public static string iconPath = "C:\\Users\\Nico\\Documents\\Visual Studio 2012\\Projects\\pds1\\FNWifiLocator\\icon.ico";


        public const String ServicePipeName = "FNPipeService1";
        public const String LocatorPipeName = "FNPipeLocator";
        public const String ApplicationName = "FNWifiLocator";

        public const String ServiceName = "FNWifiLocator";

        public const int DefaultTryToConnect = 10;

        public const bool startService = true;
        
        public const int DefaultRilevance = 10;
        public const int SearchPlaceTimeout = 10000;

        public const double FatDS = 3;
        public const int DVSlimit = 3;

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
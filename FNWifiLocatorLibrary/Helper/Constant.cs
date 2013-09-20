using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FNWifiLocatorLibrary
{
    public static class Constant
    {
        
            //"data source=C:\\Users\\Nico\\Documents\\Visual Studio 2012\\Projects\\pds1\\datapds1.sdf";
      /*  public static string iconPathYellow = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\icon_yellow.ico");
        public static string iconPathRed = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\icon_red.ico"); 
        */
        
        public static String DbConnectionString = "data source=" + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database\\datapds1.sdf");
        public static string iconPathRed = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\icon_red.ico");
        public static string iconPathYellow = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\icon_yellow.ico");
        public static string iconPathGreen = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\icon_green.ico");
        public static string serverIconUp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\connected.png");
        public static string serverIconDown = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images\\disconnected.png");
        /*/

        
        public static String DbConnectionString = "data source=C:\\Users\\Nico\\Documents\\Visual Studio 2012\\Projects\\pds1\\datapds1.sdf";
        public static string iconPathRed = "C:\\Users\\Nico\\Documents\\Visual Studio 2012\\Projects\\pds1\\FNWifiLocator\\images\\icon_red.ico";
        public static string iconPathYellow = "C:\\Users\\Nico\\Documents\\Visual Studio 2012\\Projects\\pds1\\FNWifiLocator\\images\\icon_yellow.ico";
        public static string iconPathGreen = "C:\\Users\\Nico\\Documents\\Visual Studio 2012\\Projects\\pds1\\FNWifiLocator\\images\\icon_green.ico";
        public static string serverIconUp = "C:\\Users\\Nico\\Documents\\Visual Studio 2012\\Projects\\pds1\\FNWifiLocator\\images\\connected.png";
        public static string serverIconDown = "C:\\Users\\Nico\\Documents\\Visual Studio 2012\\Projects\\pds1\\FNWifiLocator\\images\\disconnected.png";
        */

        //

        public const String ServicePipeName = "FNPipeService1";
        public const String LocatorPipeName = "FNPipeLocator";
        public const String ApplicationName = "FNWifiLocator";

        public const String ServiceName = "FNWifiLocator";

        public const int DefaultTryToConnect = 10;
        public const int UpdateRepeat = 3;
        public const int UpdateRepeatNew = 5;
        public const int CurrentPlaceCounter = 5;
        public const int tryForCheckin = 1;

        public const bool startService = true;
        
        public const int DefaultRilevance = 10;
        public const int SearchPlaceTimeout = 2*1000;
        public const int SearchPlaceWrongTimeout = 60*1000;



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
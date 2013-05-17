using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using NativeWifi;



namespace FNWifiLocatorLibrary
{
    
    public class Helper
    {
        static DateTime timestamp;
        static datapds1Entities2 dbistance = null;
        
        static List<Wlan.WlanBssEntry> networks = new List<Wlan.WlanBssEntry>();

        private static AutoResetEvent waitHandle = new AutoResetEvent(false);

        static void wlanIfacenNotification(Wlan.WlanNotificationData notifyData)
        {
            
            Log.trace(notifyData.NotificationCode.ToString());

            if (notifyData.NotificationCode.Equals(Wlan.WlanNotificationCodeAcm.ScanComplete))
            {
                Log.trace("Sblocco Scan Completed");
                
                waitHandle.Set();
            }
            //Console.WriteLine("{0} to {1} with quality level {2}",connNotifyData.wlanConnectionMode, connNotifyData.profileName, "-");
        }

        static public  List<Wlan.WlanBssEntry> getCurrentNetworks() {

            
            Log.trace((DateTime.Now - timestamp).TotalSeconds.ToString());
          //  if ((DateTime.Now - timestamp).TotalSeconds > 5)
          //  {
                Log.trace("waiting for networks....");
                networks.Clear();
                WlanClient client = new WlanClient();
                try
                {
                    foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
                    {
                        Log.trace(wlanIface.InterfaceState.ToString());
                        wlanIface.Scan();

                        waitHandle.Reset();
                        wlanIface.WlanNotification += new WlanClient.WlanInterface.WlanNotificationEventHandler(wlanIfacenNotification);
                        waitHandle.WaitOne();
                        Log.trace("Sbloccata: Scan Completed");
                        foreach (Wlan.WlanBssEntry network in wlanIface.GetNetworkBssList())
                        {
                            networks.Add(network);
                        }

                    }
                    timestamp = DateTime.Now;
                }
                catch (Exception ex)
                {
                    Log.error(ex.Message);
                }
     //       }
            return networks;
        }
        
        

        static public List<Place> getAllPlaces() {
            var db = getDB();
            return db.Places.ToList();
        }

        static public void printAllNetworks() {
            var db = getDB();

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
            var db = getDB();
            if (db.Networks.ToList() != null && db.Networks.Any())
            {
                Log.trace("All Networks in the database:");
                foreach (var item in db.Networks.ToList())
                {
                    Log.trace("Networks: \t" + item.SSID + ",\t" + item.MAC);
                }
            }

        }

        static public string getMacAddress(Wlan.WlanBssEntry network) {
            byte[] macAddr = network.dot11Bssid;
            string tMac = "";
            for (int i = 0; i < macAddr.Length; i++)
            {
                tMac += macAddr[i].ToString("x2").PadLeft(2, '0').ToUpper();
            }
            return tMac;
        }

        static public string getSSIDName(Wlan.WlanBssEntry network)
        {

            return System.Text.ASCIIEncoding.ASCII.GetString(network.dot11Ssid.SSID).ToString();
        }
        
        static public datapds1Entities2 getDB(){

           

            if (dbistance == null) {

                dbistance = new datapds1Entities2();
            }

            return dbistance;
        }

    }
}

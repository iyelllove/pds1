using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NativeWifi;

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

    }
}

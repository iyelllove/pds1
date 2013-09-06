using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using NativeWifi;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


namespace FNWifiLocatorLibrary
{

    public class Helper
    {
        static DateTime timestamp;
        static datapds1Entities2 dbistance = null;
        static FNDB fbndbistance = null;
        private static object xmppLock = new object();

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

        static public List<Wlan.WlanBssEntry> getCurrentNetworks()
        {
            if (Monitor.TryEnter(xmppLock, 15000))
            {
                //Monitor.Enter(xmppLock);
                try
                {
                    //Log.trace((DateTime.Now - timestamp).TotalSeconds.ToString());
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
                        Log.error(ex);
                    }
                }
                finally
                {

                    Monitor.Exit(xmppLock);

                }
                return networks;
            }
            return networks;
        }
        static public void saveAllCurrentNetworkInPlace(Place p)
        {
            if (p == null || p.ID == 0) return;
            using (var db = Helper.getDB())
            {
                lock (networks)
                {
                    Place dbplace = db.Places.Where(c => c.ID == p.ID).FirstOrDefault();
                    if (dbplace != null && dbplace.ID > 0)
                    {
                        List<Wlan.WlanBssEntry> currentnetworks = getCurrentNetworks();
                        foreach (Wlan.WlanBssEntry network in currentnetworks)
                        {
                            if (network.linkQuality >= 0/*Properties.Settings.Default.delta_signal_value*/)
                            {
                                string thename = Helper.getSSIDName(network);
                                string themac = Helper.getMacAddress(network);

                                //var m = db.Networks.Where(c => c.SSID == thename && c.MAC == themac).FirstOrDefault();
                                var m = (from a in db.Networks where a.SSID == thename && a.MAC == themac select a).FirstOrDefault();



                                if (m == null)
                                {
                                    m = new Network { SSID = thename, MAC = themac };
                                    db.Networks.Add(m);
                                }

                                var already_exist = db.PlacesNetworsValues.Where(c => c.Place.ID == dbplace.ID).Where(c => c.Network.ID == m.ID).FirstOrDefault();
                                if (already_exist == null)
                                {
                                    Log.trace("Aggiungo nuova rete ( ID:" + m.ID + " SSID:" + m.SSID + " MAC:" + m.MAC + ") al posto " + p.name);
                                    PlacesNetworsValue pnv = new PlacesNetworsValue { Network = m, Place = dbplace, media = Convert.ToInt16(network.rssi.ToString()), variance = (short)1, rilevance = 10 };
                                    db.PlacesNetworsValues.Add(pnv);
                                    //p.PlacesNetworsValues.Add(pnv);
                                }
                            }
                        }
                    }
                }
                db.SaveChanges();
            }


        }


        static public List<Place> getAllPlaces()
        {
            List<Place> allplaces = null;
            using (var db = getDB())
            {
                allplaces = db.Places.ToList();
            }
            return allplaces;
        }

        static public void printAllNetworks()
        {

            using (var db = getNewDB())
            {
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
        static public void updateMeasures()
        {
            using (var db = getDB())
            {
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

        static public string getMacAddress(Wlan.WlanBssEntry network)
        {
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



        static public datapds1Entities2 getNewDB()
        {
            return new datapds1Entities2();
        }

        static public datapds1Entities2 getDB()
        {
            return new datapds1Entities2();
            //fbndbistance = new datapds1Entities2();
            //return fbndbistance;
            //return fbndbistance.getDBInstance();
        }

        static public void saveChanges()
        {
            if (fbndbistance != null)
            {

            }
        }


        public static PipeMessage DeserializeFromString<PipeMessage>(string str)
        {
            byte[] b = Convert.FromBase64String(str);
            using (var stream = new MemoryStream(b))
            {
                var formatter = new BinaryFormatter();
                stream.Seek(0, SeekOrigin.Begin);
                return (PipeMessage)formatter.Deserialize(stream);
            }
        }

        public static string SerializeToString<PipeMessage>(PipeMessage message)
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, message);
                stream.Flush();
                stream.Position = 0;
                return Convert.ToBase64String(stream.ToArray());
            }
        }

    }
}

﻿using System;
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
        private static object xmppLock = new object();

        static List<Wlan.WlanBssEntry> networks = new List<Wlan.WlanBssEntry>();

        private static AutoResetEvent waitHandle = new AutoResetEvent(false);

        static void wlanIfacenNotification(Wlan.WlanNotificationData notifyData)
        {

            Log.trace("Helper:"+notifyData.NotificationCode.ToString());

            if (notifyData.NotificationCode.Equals(Wlan.WlanNotificationCodeAcm.ScanComplete))
            {
                Log.trace("Sblocco Scan Completed");

                waitHandle.Set();
            }
            //Console.WriteLine("{0} to {1} with quality level {2}",connNotifyData.wlanConnectionMode, connNotifyData.profileName, "-");
        }

        static public List<Wlan.WlanBssEntry> getCurrentNetworks()
        {
            Log.trace("getCurrentNetworks");
            List<Wlan.WlanBssEntry> newnetworks = new List<Wlan.WlanBssEntry>();
            if (Monitor.TryEnter(xmppLock, Constant.SearchPlaceTimeout))
            {

                //Monitor.Enter(xmppLock);
                try
                {

                    //Log.trace((DateTime.Now - timestamp).TotalSeconds.ToString());
                    //  if ((DateTime.Now - timestamp).TotalSeconds > 5)
                    //  {
                    Log.trace("waiting for networks....");
                    
                    WlanClient client = new WlanClient();
                    try
                    {
                        foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
                        {
                            newnetworks.Clear();
                            while (newnetworks.Count == 0)
                            {
                                Log.trace(wlanIface.InterfaceState.ToString());
                                wlanIface.Scan();

                                waitHandle.Reset();
                                wlanIface.WlanNotification += new WlanClient.WlanInterface.WlanNotificationEventHandler(wlanIfacenNotification);
                                waitHandle.WaitOne();
                                Log.trace("Sbloccata: Scan Completed");
                                foreach (Wlan.WlanBssEntry network in wlanIface.GetNetworkBssList())
                                {
                                    if (network.rssi < -10)
                                    {
                                        newnetworks.Add(network);
                                    }
                                }
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
                networks.Clear();
                networks.AddRange(newnetworks);
                return newnetworks;
            }
            else {
                newnetworks.AddRange(networks);
            }
            return newnetworks;
        }
        static public void saveAllCurrentNetworkInPlace(Place p, bool force)
        {
            try
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
                                        PlacesNetworsValue pnv = new PlacesNetworsValue { Network = m, Place = dbplace, media = Convert.ToInt16(network.rssi.ToString()), variance = 1, rilevance = Constant.DefaultRilevance };
                                        db.PlacesNetworsValues.Add(pnv);
                                        //p.PlacesNetworsValues.Add(pnv);
                                    }
                                    else if (force == true)
                                    {
                                        already_exist.media = Convert.ToInt16(network.rssi.ToString());
                                        already_exist.variance = 1;
                                        already_exist.measures = 1;
                                        Log.trace("Sovrascrivo media a causa del comando force");
                                    }
                                }
                            }
                        }
                    }
                    db.SaveChanges();
                }
            }
            catch(Exception exp) {
                Log.error(exp);
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
            try
            {
                var db = new datapds1Entities2();
                db.Database.Connection.ConnectionString = Constant.getConnectionString();
                //Log.trace(db.Database.Connection.ConnectionString);
                return db;
            }
            catch (Exception e)
            {
                Log.trace(e.ToString());
                return null;
            }
           
        }

        static public datapds1Entities2 getDB()
        {
            try
            {
                var db = new datapds1Entities2();
                db.Database.Connection.ConnectionString = Constant.getConnectionString();
                //Log.trace(db.Database.Connection.ConnectionString);
                return db;
            }
            catch (Exception e)
            {
                Log.trace(e.ToString());
                return null;
            }
           
            //fbndbistance = new datapds1Entities2();
            //return fbndbistance;
            //return fbndbistance.getDBInstance();
        }

        static public void saveChanges()
        {
           
        }


        public static PipeMessage DeserializeFromString<PipeMessage>(string str)
        {
            try
            {
                byte[] b = Convert.FromBase64String(str);
                using (var stream = new MemoryStream(b))
                {
                    var formatter = new BinaryFormatter();
                    stream.Seek(0, SeekOrigin.Begin);
                    return (PipeMessage)formatter.Deserialize(stream);
                }
            }
            catch {
                return default(PipeMessage);
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

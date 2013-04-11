using NativeWifi;
using System;
using System.Collections.Generic;
using System.Text;

namespace pds1
{
    class CurrentState
    {
        private DateTime timestamp;
        private Wlan.WlanBssEntry[] nets;
        Dictionary<string, Wlan.WlanBssEntry> network_list = new Dictionary<string, Wlan.WlanBssEntry>();
        private Wlan.WlanConnectionAttributes current_connections;

       
        public CurrentState()
        {
            WlanClient client = new WlanClient();
            try
            {
                foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
                {
                    foreach (Wlan.WlanBssEntry network in wlanIface.GetNetworkBssList()) {
                        if (network.linkQuality > 15)
                        {
                            byte[] macAddr = network.dot11Bssid;
                            string tMac = "";
                            for (int i = 0; i < macAddr.Length; i++) { tMac += macAddr[i].ToString("x2").PadLeft(2, '0').ToUpper(); }

                            if(network_list.ContainsKey(tMac)){
                                int i=1;
                                string n_tmac = tMac;
                                do{
                                    n_tmac = tMac + i.ToString();
                                    i++;
                                } while (!network_list.ContainsKey(n_tmac));
                                tMac = n_tmac;
                            }
                            network_list.Add(tMac, network);
                        }
                    }
                    current_connections = wlanIface.CurrentConnection;
                    Log.trace("You are connected at: "+new String(Encoding.ASCII.GetChars(current_connections.wlanAssociationAttributes.dot11Ssid.SSID, 0, (int)current_connections.wlanAssociationAttributes.dot11Ssid.SSIDLength)));
                } 
            }catch (Exception ex){
                Log.error(ex.Message);
            }
        }

        public override bool Equals(System.Object obj)
        {
            CurrentState p = obj as CurrentState;
            if ((System.Object)p == null)
            {
                return false;
            }
            else {
                int e = 0;
                int d = 0;
                List<string> used = new List<string>();
                foreach (var pair in p.network_list){
                    if (this.network_list.ContainsKey(pair.Key))
                    {
                        used.Add(pair.Key);
                        e++;
                    }
                    else {
                        d++;
                        Log.trace("La situa è cambiata");
                    }
                }

                foreach (var pair in this.network_list)
                {
                    if (!used.Contains(pair.Key)) {
                        used.Add(pair.Key);
                        d++;
                    }
                    
                }

                Log.trace("Uguali:\t"+e.ToString()+"\tdiverse:"+d.ToString());
            }
            return true;
        }
    }
}
    

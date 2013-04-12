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
                 //   wlanIface.Scan();
                  //  System.Threading.Thread.Sleep(5000);
                    foreach (Wlan.WlanBssEntry network in wlanIface.GetNetworkBssList()) {
                        if (network.linkQuality > Properties.Settings.Default.min_signal_value)
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



                    if (this.network_list.ContainsKey(pair.Key)
     //                 &&  (Encoding.ASCII.GetChars(pair.Value.dot11Ssid.SSID, 0, (int)pair.Value.dot11Ssid.SSID.Length)).Equals(Encoding.ASCII.GetChars(this.network_list[pair.Key].dot11Ssid.SSID, 0, (int)this.network_list[pair.Key].dot11Ssid.SSID.Length))
                        && (pair.Value.linkQuality <= this.network_list[pair.Key].linkQuality + Properties.Settings.Default.delta_signal_value) &&
                        (pair.Value.linkQuality >= this.network_list[pair.Key].linkQuality - Properties.Settings.Default.delta_signal_value)
                       ){
                            used.Add(pair.Key);
                            e++;
                        }
                    else {
                        d++;
                     }
                }

                foreach (var pair in this.network_list)
                {
                    if (!used.Contains(pair.Key)) {
                        used.Add(pair.Key);
                        d++;
                    }
                    
                }
                Log.trace("Uguali:\t" + e.ToString() + "\tdiverse:" + d.ToString() + "\tpercentuale:" + (100 * e / (d + e)).ToString() + " VS " + Properties.Settings.Default.min_perc_same_place.ToString());
                if((100*e/(d+e)) > Properties.Settings.Default.min_perc_same_place){
                    return true;
                }else{
                    Log.trace("Necessario aggiornamento valori");
                    return false;
                }
                
            }
            return true;
        }
    }
}
    

using NativeWifi;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceProcess;
using System.Configuration;
using System.Data.Entity;




namespace pds1
{
    class CurrentState
    {
        Dictionary<string, Wlan.WlanBssEntry> network_list = new Dictionary<string, Wlan.WlanBssEntry>();



        private List<PlacesNetworsValue> backuppnv = new List<PlacesNetworsValue>();
        private List<Place> possible_place = new List<Place>();
        private Wlan.WlanConnectionAttributes current_connections;
        private Place current_place;
        private float current_place_value;
        private Checkin checkin;
        private Int16 backupc;
        private Place forcePlace = null;

        private datapds1Entities2 db;



        //PRIVATE
       


        private void doCheckin()
        {

                datapds1Entities2 db = Helper.getDB();
                Log.trace("CheckIn at " + this.current_place.name);

                Checkin c = new Checkin() { @in = DateTime.Now};
                if (this.forcePlace == null)
                {
                    c.Place = this.current_place;
                }
                else {
                    c.Place = this.forcePlace;
                }
                this.db.Checkins.Add(c);
                this.db.SaveChanges();
                this.checkin = c;

        }

        private void doCheckout()
        {
                datapds1Entities2 db = Helper.getDB();
                Log.trace("CheckOut from " + this.checkin.Place.name);
                this.db.Checkins.Find(this.checkin.ID).@out = DateTime.Now;
                this.checkin = null;
                this.db.SaveChanges();
            
        }


        //PUBLIC

        public void forceCurrentPlace(Place p)
        {
            this.forcePlace = p;
            
        }

        public Place getCurrentPlace() {
            return this.current_place;
        }

        public List<Place> getPossiblePlaces()
        {
            return this.possible_place;
        }


        public CurrentState()
        {


            this.db = Helper.getDB();

            try
            {
                    foreach (Wlan.WlanBssEntry network in Helper.getCurrentNetworks()) {
                        if (network.linkQuality > Properties.Settings.Default.min_signal_value)
                        {
                            byte[] macAddr = network.dot11Bssid;
                            string tMac = Helper.getMacAddress(network);
                            network_list.Add(tMac, network);
                        }
                 } 
            }catch (Exception ex){
                Log.error(ex.Message);
            }
        }

       
        public override bool Equals(System.Object obj)
        {
            return false;
            CurrentState p = obj as CurrentState;
            if ((System.Object)p == null)
            {
                return false;
            }
            else {
                int e = 0;
                int d = 0;
                List<string> used = new List<string>();
                int w = 0;
                foreach (var pair in p.network_list){


                    String ssid_1 = new String(Encoding.ASCII.GetChars(pair.Value.dot11Ssid.SSID, 0, (int)pair.Value.dot11Ssid.SSID.Length));
                    String ssid_2 = new String(Encoding.ASCII.GetChars(pair.Value.dot11Ssid.SSID, 0, (int)pair.Value.dot11Ssid.SSID.Length));


                    w += (int)pair.Value.linkQuality;
                    if (this.network_list.ContainsKey(pair.Key)
                      && ssid_1.Equals(ssid_2) //  (Encoding.ASCII.GetChars(pair.Value.dot11Ssid.SSID, 0, (int)pair.Value.dot11Ssid.SSID.Length)).Equals(Encoding.ASCII.GetChars(this.network_list[pair.Key].dot11Ssid.SSID, 0, (int)this.network_list[pair.Key].dot11Ssid.SSID.Length))
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
        }

        public float searchPlace() {
            this.db = Helper.getDB();
            for (int k = 0; k < 1; k++)
            {
                //System.Threading.Thread.Sleep(1500);
                //datapds1Entities2 db = new datapds1Entities2();
                

                    //List<Place> places = new List<Place>();
                    List<Wlan.WlanBssEntry> networks = Helper.getCurrentNetworks();
                    Dictionary<string, PlacesNetworsValue> used = new Dictionary<string, PlacesNetworsValue>();
                    Dictionary<string, PlacesNetworsValue> used2remove = new Dictionary<string, PlacesNetworsValue>();

                    Dictionary<string, RightPlace> rightplaces = new Dictionary<string, RightPlace>();
                    this.possible_place.Clear();

                    if (this.forcePlace == null)
                    {
                        List<int> ns = new List<int>();
                        foreach (var network in networks)
                        {
                            string ssid = Helper.getSSIDName(network);
                            string mac = Helper.getMacAddress(network);
                            Network nn = this.db.Networks.Where(n => n.SSID == ssid).Where(n => n.MAC == mac).FirstOrDefault();
                            if (nn != null) {
                                ns.Add(nn.ID);
                            }
                        }

                        var ps= db.PlacesNetworsValues.Where(c => ns.Contains(c.Network.ID)).Where(c=>c.Place != null).GroupBy(c => c.Place).ToList();
                        foreach (var ppps in ps) {

                            Log.trace("sds" + ppps.Key.name);
                        }
                        

                        foreach (var network in networks)
                        {
                            string ssid = Helper.getSSIDName(network);
                            string mac = Helper.getMacAddress(network);

                            
                            var m2 = this.db.Networks.Where(n => n.SSID == ssid).Where(n => n.MAC == mac).FirstOrDefault();
                            if (m2 != null)
                            {
                                foreach (var pnv in m2.PlacesNetworsValues)
                                {
                                    if (pnv.Place != null && !rightplaces.ContainsKey(pnv.Place.name))
                                    {
                                        Log.trace("Possibile Posto:" + pnv.Place.name);
                                        //places.Add(pnv.Place);
                                        //if (!rightplaces.ContainsKey(pnv.Place.name))
                                        //{
                                        rightplaces.Add(pnv.Place.name, new RightPlace() { place = pnv.Place });
                                        //}
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        Log.trace("Force place " + this.forcePlace.name);
                        rightplaces.Add(this.forcePlace.name, new RightPlace() { place = this.forcePlace });
                        //places.Add(this.forcePlace);
                        this.forcePlace = null;
                    }


                    this.current_place = null;
                    float max = 0;
                    if (rightplaces.Count() > 0)
                    {
                        foreach (RightPlace rp in rightplaces.Values)
                        {
                            Place p = rp.place;
                            //foreach (Place p in places)
                            //{
                            int e = 0;
                            float w = 0;
                            int d = 0;
                            int imp;
                            used.Clear();
                            used2remove.Clear();
                            foreach (PlacesNetworsValue pnv in p.PlacesNetworsValues)
                            {

                                imp = Convert.ToInt16((pnv.rilevance * 100) / pnv.Place.m_num) + 1;
                                if (imp <= 0) imp = 1;
                                Log.trace("Importanza(" + pnv.Network.SSID + "):" + imp + "-->" + pnv.rilevance + "_" + pnv.Place.m_num);
                                foreach (var network in networks)
                                {

                                    String ssid_1 = Helper.getSSIDName(network);
                                    String mac_1 = Helper.getMacAddress(network);

                                    if (!used.ContainsKey(pnv.Network.MAC) && mac_1.Equals(pnv.Network.MAC))
                                    {
                                        int diff = Math.Abs(Math.Abs(pnv.media) - Math.Abs(network.rssi));

                                        used.Add(pnv.Network.MAC, pnv);
                                        Log.trace("Trovata");
                                        if (diff > 0)
                                        {
                                            Log.trace("Incentivo x la vicinanza {" + imp + "_" + diff + "}(" + imp / diff + ")");
                                            e += imp + imp * (1 / diff);
                                        }
                                        else
                                        {
                                            e += imp;
                                        }
                                    }


                                    /*
                                    if (!used.ContainsKey(pnv.Network.MAC) && mac_1.Equals(pnv.Network.MAC)
                                        && ((Math.Abs(Math.Abs(pnv.media) - Math.Abs(network.rssi)) <= (pnv.variance * step)) || (delta == Properties.Settings.Default.numer_of_try))
                                        //&& (ssid_1.Equals(pnv.Network.SSID))   && (pnv.media - pnv.variance > network.linkQuality)
                                        )
                                    {
                                        Log.trace("E->" + ((Math.Abs(Math.Abs(pnv.media) - Math.Abs(network.rssi)))));
                                        int diff = Math.Abs(Math.Abs(pnv.media) - Math.Abs(network.rssi));
                                        used.Add(pnv.Network.MAC, pnv);

                                   
                                        e += pnv.rilevance;
                                        //d += diff;
                                        break;
                                    }*/
                                }

                                //Questa rete presente nel DB non è presente ora
                                if (!used.ContainsKey(pnv.Network.MAC))
                                {

                                    used.Add(pnv.Network.MAC, pnv);
                                    used2remove.Add(pnv.Network.MAC, pnv);
                                    Log.trace("Rete non trovata al momento (" + pnv.Network.SSID + " -> " + imp + ")");
                                    if (imp > 1)
                                        d += imp;
                                }

                            }


                            foreach (var network in networks)
                            {

                                //Queste reti sono presenti in questo momento ma no nel DB

                                String mac_1 = Helper.getMacAddress(network);
                                if (!used.ContainsKey(mac_1))
                                {
                                    d++;
                                    Log.trace("Rete non trovata nel DB (" + Helper.getSSIDName(network) + ")");
                                    //d += Convert.ToInt16(network.linkQuality * step);
                                }

                            }

                            w = rightplaces[p.name].addStep(e, d, 1);
                            Log.trace("Valore x " + p.name + ": (d: " + d + " e: " + e + ") -->" + w + "%");

                        }
                    }


                   
                    Log.trace("################################ SCELGO IL MIGLIORE ################################ ");
                    foreach (RightPlace rp in rightplaces.Values)
                    {
                        this.possible_place.Add(rp.place);
                        Log.trace(rp.place + ": " + rp.avg);
                        max = Math.Max(max, rp.avg);
                        if (max == rp.avg && max > Properties.Settings.Default.min_perc_same_place)
                        {
                            this.current_place = rp.place;
                            this.current_place_value = max;
                            Log.trace("Nuovo Massimo-->" + rp.place.name + ": " + rp.avg);
                        }

                    }

                    if ((this.current_place != null && this.checkin == null) || (this.checkin != null && this.current_place.ID != this.checkin.Place.ID))
                    {

                        //Copia di backup dei luoghi per un eventuale errore
                        backuppnv.Clear();
                        if (this.current_place != null)
                        {
                            backupc = (short)this.current_place.m_num;
                            backuppnv = this.db.PlacesNetworsValues.Where(c => c.Place.ID == this.current_place.ID).ToList();
                        }

                        if (this.checkin == null)
                        {
                            this.doCheckin();
                        }
                        else if (this.current_place == null)
                        {

                            this.doCheckout();
                        }
                        else
                        {
                            this.doCheckout();
                            this.doCheckin();
                        }
                    }

                    if (this.current_place != null)
                    {
                        //ABBIAMO UN POSTO DEFINITO
                        //UPDATE STORED VALUES
                        foreach (Wlan.WlanBssEntry network in networks)
                        {
                            string ssid = Helper.getSSIDName(network);
                            string mac = Helper.getMacAddress(network);
                            PlacesNetworsValue pnv2u = db.PlacesNetworsValues.Where(c => c.Network.SSID == ssid).Where(c => c.Network.MAC == mac).Where(c => c.Place.ID == this.current_place.ID).FirstOrDefault();
                            if (pnv2u != null)
                            {
                                pnv2u.variance = Math.Max(pnv2u.variance, Math.Abs(Convert.ToInt16((Math.Abs(network.rssi) - Math.Abs(pnv2u.media)))));
                                //if (delta < Properties.Settings.Default.min_update_value || pnv2u.Place.m_num < Properties.Settings.Default.min_update_value)
                                //{
                                //    pnv2u.variance = Convert.ToInt16(((pnv2u.variance * Convert.ToInt16(pnv2u.Place.m_num)) + Math.Abs(Convert.ToInt16((Math.Abs(network.rssi) - Math.Abs(pnv2u.media))))) / (Convert.ToInt16(pnv2u.Place.m_num) + 1));
                                pnv2u.media = Convert.ToInt16(((pnv2u.media * Convert.ToInt16(pnv2u.Place.m_num)) + network.rssi) / (Convert.ToInt16(pnv2u.Place.m_num) + 1));
                                //}
                                //L'importanza della rete cresce sempre in base alla qualità del segnale. Una rete che prende tanto è più importante di una che prende poco 
                                pnv2u.rilevance = Convert.ToInt16(pnv2u.rilevance + (network.linkQuality / 10) + 1);

                            }
                            else
                            {
                                var m = db.Networks.Where(c => c.SSID == ssid).Where(c => c.MAC == mac).FirstOrDefault();
                                if (m == null)
                                {
                                    m = new Network { SSID = ssid, MAC = mac };
                                    db.Networks.Add(m);
                                    db.SaveChanges();
                                }
                                PlacesNetworsValue pnv = new PlacesNetworsValue();
                                pnv.Network = m;
                                pnv.Place = this.current_place;
                                pnv.media = Convert.ToInt16(network.rssi.ToString());
                                pnv.variance = (short)Properties.Settings.Default.delta_signal_value;
                                //pnv.rilevance = Convert.ToInt16((network.linkQuality/10)+1);
                                pnv.rilevance = 1;
                                Log.trace("Aggiungo nuova rete al posto ( ID:" + m.ID + " SSID:" + m.SSID + " MAC:" + m.MAC + ") a " + this.current_place.name);

                                db.PlacesNetworsValues.Add(pnv);
                                this.current_place.PlacesNetworsValues.Add(pnv);


                            }
                        }

                        //Quelle decremento quelle presenti nel DB ma non ora
                        foreach (PlacesNetworsValue pnv in used2remove.Values)
                        {
                            // pnv.rilevance = Convert.ToInt16(pnv.rilevance - ((pnv.rilevance / 10) + 1));
                            pnv.rilevance--;
                        }

                        /*
                        //Elimino tutte le reti che hanno come rilevanza 0 e meno
                        foreach (PlacesNetworsValue pnv in db.PlacesNetworsValues.Where(c => c.rilevance <= 0).ToList())
                        {
                            db.PlacesNetworsValues.Remove(pnv);
                        }
                        */


                        Place thisplace = db.Places.Where(c => c.ID == this.current_place.ID).FirstOrDefault();
                        if (thisplace != null)
                        {
                            
                            thisplace.m_num++;
                        }

                        db.SaveChanges();

                    }
                    else
                    {
                        Log.trace("Non sei in nessun posto conosciuto");
                    }
                

            }
           
             return this.current_place_value;
        }



        public void wrongPlace()
        {
            Log.trace("WRONG PLACE");
            //using  (var db = new datapds1Entities2()){
                if(this.backuppnv.Count > 0 && this.current_place != null){
                    if (this.forcePlace != null)
                    {
                        if (this.checkin != null)
                        {
                            db.Checkins.Find(this.checkin.ID).Place = this.forcePlace;
                        }
                        else {
                            this.doCheckin();
                        }
                        //this.checkin.Place = this.forcePlace;
                        
                    }
                    else {
                        db.Checkins.Remove(db.Checkins.Find(this.checkin.ID));
                        this.checkin = null;
                    }

                    foreach (PlacesNetworsValue pnv in this.backuppnv)
                    {
                        PlacesNetworsValue pnv2 = db.PlacesNetworsValues.Where(c => c.Network.ID == pnv.Network.ID).Where(c => c.Place.ID == pnv.Place.ID).First();
                        if (pnv2 != null) {
                            pnv2.rilevance = pnv.rilevance;
                            pnv2.media = pnv.media;
                            pnv2.variance = pnv.variance;
                        }
                    }
                    this.db.Places.Find(this.current_place.ID).m_num = this.backupc;
                    this.db.SaveChanges();
                }
            }
       // }

    }
}
    

using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Data.Entity;
using FNWifiLocatorLibrary;
using NativeWifi;



namespace ConsoleService
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
        double precision = 0;



        //PRIVATE


        /*
        private void doCheckin()
        {

            datapds1Entities2 db = Helper.getDB();
            Log.trace(".......................................");
            Log.trace("CheckIn at " + this.current_place.name);

            Checkin c = new Checkin() { @in = DateTime.Now };
            if (this.forcePlace == null)
            {
                c.Place = this.current_place;
            }
            else
            {
                c.Place = this.forcePlace;
            }
            using (var db = Helper.getDB())
            {
                db.Checkins.Add(c);
                db.SaveChanges();
            }
            this.checkin = c;

        }
         * */

        /*
        private void doCheckout()
        {
            datapds1Entities2 db = Helper.getDB();
            Log.trace("CheckOut from " + this.checkin.Place.name);
            using (var db = Helper.getDB())
            {
                this.db.Checkins.Find(this.checkin.ID).@out = DateTime.Now;
                this.checkin = null;
                this.db.SaveChanges();
            }

        }
         */


        //PUBLIC

        public void forceCurrentPlace(Place p)
        {
            this.forcePlace = p;

        }

        public Place getCurrentPlace()
        {
            return this.current_place;
        }

        public List<Place> getPossiblePlaces()
        {
            return this.possible_place;
        }


        public CurrentState()
        {


            //thisdb = Helper.getDB();

            try
            {
                foreach (Wlan.WlanBssEntry network in Helper.getCurrentNetworks())
                {
                    if (network.linkQuality > 15/*Properties.Settings.Default.min_signal_value*/)
                    {
                        byte[] macAddr = network.dot11Bssid;
                        string tMac = Helper.getMacAddress(network);
                        network_list.Add(tMac, network);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.error(ex.Message);
            }
        }

        /*
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
         */




        public Place searchPlace()
        {
            Log.trace("-----------------------inizio ricerca");
            Place place_found = null;
            //place_found.name = "non settato";
            double place_found_lp = double.MaxValue;
            using (var db = Helper.getDB())
            {

                //System.Threading.Thread.Sleep(1500);
                //datapds1Entities2 db = new datapds1Entities2();


                //List<Place> places = new List<Place>();
                List<Wlan.WlanBssEntry> networks = Helper.getCurrentNetworks();
                lock (networks)
                {
                    //Dictionary<string, PlacesNetworsValue> used = new Dictionary<string, PlacesNetworsValue>();
                    //Dictionary<string, PlacesNetworsValue> used2remove = new Dictionary<string, PlacesNetworsValue>();



                    //Dictionary<string, RightPlace> rightplaces = new Dictionary<string, RightPlace>();
                        List<Network> network_sniffed = new List<Network>();
                    List<Network> networks_candidate = new List<Network>();
                    this.possible_place.Clear();






                        List<int> ns = new List<int>();
                    Dictionary<Network, Int16> current_strength_network = new Dictionary<Network, Int16>();

                    foreach (var network in networks)
                    {

                        string ssid = Helper.getSSIDName(network);
                        string mac = Helper.getMacAddress(network);
                        // try
                        // {
                        Network nn = db.Networks.Where(n => n.SSID == ssid && n.MAC == mac).FirstOrDefault();
                        if (nn != null)
                        {
                            network_sniffed.Add(nn);
                            ns.Add(nn.ID);
                            current_strength_network.Add(nn, Convert.ToInt16(network.rssi.ToString()));
                        }
                        // }
                        // catch
                        // {

                        // }
                    }
                    //CERCO TUTTI I POSTI CHE HANNO ALMENO UNA RETE DI QUELLE CHE STO SENTENDO
                    var place_candidate = db.PlacesNetworsValues.Where(c => ns.Contains(c.Network.ID)).Where(c => c.Place.ID != null).GroupBy(c => c.Place).ToList();
                    int n_p = place_candidate.Count();
                    if (n_p > 0)
                    {
                        //OTTENGO SOLO I  NETWORK  CHE SENTO e che conosco
                        var network_candidate = db.PlacesNetworsValues.Where(c => c.Place.ID != null).Where(c => ns.Contains(c.Network.ID)).GroupBy(c => c.Network).ToList(); //Where(c => c.Count() < n_p).ToList();                       
                        foreach (var ppps in network_candidate)
                        {
                            networks_candidate.Add(ppps.Key);
                        }
                        foreach (var pc in place_candidate)
                        {
                            Place place = pc.Key;
                            Boolean inif = false;
                            double lp = 0;
                            int i = 0;
                            foreach (PlacesNetworsValue pnv in place.PlacesNetworsValues)
                            {
                                //per ogni posto candidato considero le reti che costituiscono il posto
                                //e che fanno parte delle reti candidate(quindi quelle di cui sono in ascolto)
                                if (networks_candidate.Contains(pnv.Network))
                                {
                                    Log.trace("**RETE** Val.Corr[" + current_strength_network[pnv.Network] + "]media:[" + pnv.media + "]");
                                    Log.trace("         dev.stndrd:[" + Math.Sqrt(pnv.variance) + "]");

                                    if ((current_strength_network[pnv.Network] >= (pnv.media - Constant.FatDS * (Math.Sqrt(pnv.variance)))) && (current_strength_network[pnv.Network] <= (pnv.media + Constant.FatDS * (Math.Sqrt(pnv.variance)))))
                                    {
                                        Log.trace("         Passata");
                                        inif = true;
                                        i++;
                                        int impronta = current_strength_network[pnv.Network];
                                        double media = pnv.media;
                                        lp = lp + Math.Pow(Math.Abs(impronta - media), 2);
                                    }
                                    else { Log.trace("----------NONpassata"); }
                                }

                            }
                            if (inif == true)
                            {
                                lp = Math.Sqrt(lp) / i;
                                if (lp < place_found_lp)
                                {
                                    place_found_lp = lp;
                                    place_found = place;
                                }
                            }
                        }

                    }

                }

            }
            if (place_found != null && place_found.ID > 0)
            {
                update_values(place_found);
            }
            //Log.trace("-------------------------------"+place_found.name);
            current_place = place_found;
            if (place_found == null)
            { precision = 0; }
            else
            {
                precision = place_found_lp;
                Log.trace("TROVATO POSTO:" + place_found.name + "-precisione" + precision);
            }
            return (place_found);

        }




        public void update_values(Place place_found)
        {
            try
            {
                Helper.saveAllCurrentNetworkInPlace(place_found, false);
                List<Wlan.WlanBssEntry> networks = Helper.getCurrentNetworks();
                lock (networks)
                {
                    using (var db = Helper.getDB())
                    {
                        foreach (var network in networks)
                        {
                            string ssid = Helper.getSSIDName(network);
                            string mac = Helper.getMacAddress(network);
                            PlacesNetworsValue pnv_up = db.PlacesNetworsValues.Where(c => c.Network.SSID == ssid).Where(c => c.Network.MAC == mac).Where(c => c.Place.ID == place_found.ID).FirstOrDefault();
                            if (pnv_up != null)
                            {
                                /*prendo tutte le reti che sto ascoltando e che quindi fanno parte del posto,
                                 per ogni rete vado a prendere il suo placeNetworkValue (rimangono fuori i 
                                 pnv delle reti non presenti)*/
                                if (pnv_up.rilevance < Constant.DefaultRilevance)
                                {
                                    Log.trace("updateRilev=10.pnvID:" + pnv_up.ID + "networkID:" + pnv_up.Network.ID + "-postoID:" + pnv_up.Place.ID);
                                }
                                pnv_up.rilevance = Constant.DefaultRilevance;
                                //Int16 app=pnv_up.media;
                                //pnv_up.media = Convert.ToInt16((pnv_up.media + Convert.ToInt16(network.rssi.ToString())) / 2);

                                pnv_up.media = (pnv_up.media * pnv_up.measures + Convert.ToInt16(network.rssi.ToString())) / (pnv_up.measures + 1);
                                pnv_up.variance = ((pnv_up.variance * pnv_up.measures + Math.Pow((Convert.ToInt16(network.rssi.ToString()) - pnv_up.media), 2)) / (pnv_up.measures + 1));
                                Log.trace("******UV***media:[" + pnv_up.media + "] varianza:[" + pnv_up.variance + "] N[" + pnv_up.measures + "]");
                                pnv_up.measures++;
                                //Log.trace("updateMEDIA.pnvID:" + pnv_up.ID + "MEDIA-B:" + app + "-MEDIA-A:" + pnv_up.media);
                                db.SaveChanges();
                            }
                            else
                            {
                                /*ERRORE avendo fatto saveAllCurrentNetworkInPlace il place net.value deve essere presente nel DB*/
                            }
                        }
                        if (place_found != null && place_found.ID > 0)
                        {
                            var pf = db.Places.Where(c => c.ID == place_found.ID).FirstOrDefault();
                            if (pf != null)
                            {
                                pf.m_num++;
                                db.SaveChanges();
                            }
                        }
                    }
                }
            }
            catch(Exception exp) {
                Log.error(exp);
            }
        }



        public void update_values_checkin(Place place_found)
        {
            try
            {

                if (place_found == null) return;
                List<Network> founded = new List<Network>();
                //Helper.saveAllCurrentNetworkInPlace(place_found);
                List<Wlan.WlanBssEntry> networks = Helper.getCurrentNetworks();
                using (var db = Helper.getDB())
                {

                    /*
                    lock (networks)
                    {
                        foreach (var network in networks)
                        {
                            string ssid = Helper.getSSIDName(network);
                            string mac = Helper.getMacAddress(network);
                            PlacesNetworsValue pnv_up = db.PlacesNetworsValues.Where(c => c.Network.SSID == ssid).Where(c => c.Network.MAC == mac).Where(c => c.Place.ID == place_found.ID).FirstOrDefault();
                            if (pnv_up != null)
                            {
                                /*prendo tutte le reti che sto ascoltando e che quindi fanno parte del posto,
                                 per ogni rete vado a prendere il suo placeNetworkValue (rimangono fuori i 
                                 pnv delle reti non presenti) salvo i pnv nella lista founded * /
                                Log.trace("rete presente.pnvID:" + pnv_up.ID + "networkID:" + pnv_up.Network.ID + "-postoID:" + pnv_up.Place.ID);
                                founded.Add(pnv_up.ID);
                                pnv_up.rilevance = 10;
                            }
                            else
                            {
                                /*ERRORE avendo fatto saveAllCurrentNetworkInPlace il place net.value deve essere presente nel DB* /
                            }
                        }
                    }
        */

                    lock (networks)
                    {
                        founded.Clear();
                        foreach (var network in networks)
                        {
                            string ssid = Helper.getSSIDName(network);
                            string mac = Helper.getMacAddress(network);
                            var n = db.Networks.Where(c => c.SSID == ssid && c.MAC == mac).FirstOrDefault();
                            if (n != null)
                            {
                                founded.Add(n);
                            }
                        }
                    }

                    var pnvs = db.PlacesNetworsValues.Where(c => c.Place.ID == place_found.ID).ToList();
                    foreach (PlacesNetworsValue pnv in pnvs)
                    {
                        if (pnv.ID != 0 && !founded.Contains(pnv.Network))
                        {
                            pnv.rilevance--;
                            if (pnv.rilevance <= 0)
                            {
                                db.PlacesNetworsValues.Remove(pnv);
                                Log.trace("rilevanza=0: rete eliminata dal DB");
                            }
                            /*
                             if (pnv.ID != 0)
                             {
                                 PlacesNetworsValue p = db.PlacesNetworsValues.Where(c => c.ID == pnv.ID).FirstOrDefault();
                                 if (p != null)
                                 {
                                     p.rilevance--;
                                     Log.trace("reteNONpresente.pnvID:" + pnv.ID + "networkID:" + pnv.Network.ID + "-postoID:" + pnv.Place.ID);
                                     if (p.rilevance <= 0)
                                     {
                                         db.PlacesNetworsValues.Remove(p);
                                         Log.trace("rilevanza=0: rete eliminata dal DB");
                                     }
                                     db.SaveChanges();
                                 }
                             }*/
                        }
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception exp) {
                Log.error(exp);
            }
        }


















        /*
        //Elimino tutte le reti che hanno come rilevanza 0 e meno
        foreach (PlacesNetworsValue pnv in db.PlacesNetworsValues.Where(c => c.rilevance <= 0).ToList())
        {
            db.PlacesNetworsValues.Remove(pnv);
        }
        //* /


        Place thisplace = db.Places.Where(c => c.ID == this.current_place.ID).FirstOrDefault();
        if (thisplace != null)
        {
                            
            thisplace.m_num++;
        }
                        
        db.SaveChanges();

}
                     

                        
        /*
        foreach (var ppps in place_candidate)
        {
            if (ppps.Key != null)
            {
                if (true) return ppps.Key;
                Log.trace("Posto Candidato: " + ppps.Key.name);
                RightPlace rp = new RightPlace() { place = ppps.Key };


                if(this.forcePlace != null && rp.place != this.forcePlace){
                    Log.trace("Disabilito");
                    rp.enable = false;
                }

                rightplaces.Add(ppps.Key.name, rp);
                                
                                
            }
        }
        Log.trace("****MI CONCENTRO SULLE RETI DIVERSE****");
        for (int i = 0; i < 5; i++)
        {
            foreach (RightPlace rp in rightplaces.Values)
            {
                if (rp.enable)
                {
                    int equal = 0;
                    int diff = 0;
                    Place p = rp.place;
                    Log.trace("Posto Candidato" + p.name);
                    used.Clear();
                    used2remove.Clear();
                    foreach (PlacesNetworsValue pnv in p.PlacesNetworsValues)
                    {
                        if (network_sniffed.Contains(pnv.Network))
                        {
                            if (networks_candidate.Contains(pnv.Network))
                            {
                                equal++;
                            }
                        }
                        else
                        {
                            diff++;
                        }

                    }
                    rp.addStep(equal, 0, 1);
                    Log.trace("-->" + equal + "  " + diff + "  ((" + p.name);

                }
            }
            networks = Helper.getCurrentNetworks();
            ns.Clear();
            network_sniffed.Clear();
            foreach (var network in networks)
            {

                                
                string ssid = Helper.getSSIDName(network);
                string mac = Helper.getMacAddress(network);
                Network nn = db.Networks.First(n => n.SSID == ssid && n.MAC == mac);
                                
                if (nn != null)
                {
                    network_sniffed.Add(nn);
                    ns.Add(nn.ID);
                }
            }
        }


                        

        /*
                    
    if (this.forcePlace != null)
    {
        //Forzo il posto.. deve essere spostato perchè voglio che la lista dei posti possibili sia sempre aggiornata
       foreach (RightPlace rp  in rightplaces.Values){

           if (rightplaces[rp.place.name].place != this.forcePlace)
           {
               rightplaces[rp.place.name].enable = false;
           }
           this.forcePlace = null;
    }
    */





        //Creo una lista di interi in cui metto tutti gli ID delle reti che sto sentendo e che conosco


        //place_candidate contiene tutti i possibili posti in cui posso essere.
        //Vuol dire che esiste almeno una rete in quel posto che sto sentendo anche ora.
        //foreach (var pc in place_candidate) {} pc. key restituisce un oggetto di tipo Place




        /*
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
        }*/






        //this.current_place = null;
        /*
        float max = 0;
        if (rightplaces.Count() > 0)
        {
            foreach (RightPlace rp in rightplaces.Values)
            {
                if (rp.enable)
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
                        // Log.trace("Importanza(" + pnv.Network.SSID + "):" + imp + "-->" + pnv.rilevance + "_" + pnv.Place.m_num);
                        foreach (var network in networks)
                        {

                            String ssid_1 = Helper.getSSIDName(network);
                            String mac_1 = Helper.getMacAddress(network);

                            if (!used.ContainsKey(pnv.Network.MAC) && mac_1.Equals(pnv.Network.MAC))
                            {
                                int diff = Math.Abs(Math.Abs(pnv.media) - Math.Abs(network.rssi));

                                used.Add(pnv.Network.MAC, pnv);
                                //Log.trace("Trovata");
                                if (diff > 0)
                                {
                                    //Log.trace("Incentivo x la vicinanza {" + imp + "_" + diff + "}(" + imp / diff + ")");
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
                            }* /
                        }

                        //Questa rete presente nel DB non è presente ora
                        if (!used.ContainsKey(pnv.Network.MAC))
                        {

                            used.Add(pnv.Network.MAC, pnv);
                            used2remove.Add(pnv.Network.MAC, pnv);
                            //Log.trace("Rete non trovata al momento (" + pnv.Network.SSID + " -> " + imp + ")");
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
                    // Log.trace("Valore x " + p.name + ": (d: " + d + " e: " + e + ") -->" + w + "%");

                }
            }
        }
        */

        /*
                float max = 0;
                Log.trace("########## SCELGO IL MIGLIORE ####### ");
                foreach (RightPlace rp in rightplaces.Values)
                {
                    this.possible_place.Add(rp.place);
                    Log.trace(rp.place + ": " + rp.avg);
                    max = Math.Max(max, rp.avg);
                    if (max == rp.avg && max > /*Properties.Settings.Default.min_perc_same_place*/
        /* 70)
{
this.current_place = rp.place;
this.current_place_value = max;
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
/*
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
pnv.variance = (short)//*Properties.Settings.Default.delta_signal_value* / 1;
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
//* /


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
*/
        /*
          }
   return this.current_place;
}*/



        static string GetStringForSSID(Wlan.Dot11Ssid ssid)
        {
            return Encoding.ASCII.GetString(ssid.SSID, 0, (int)ssid.SSIDLength);
        }
        /*
        public void wrongPlace()
        {
            Log.trace("WRONG PLACE");
            //using  (var db = new datapds1Entities2()){
            if (this.backuppnv.Count > 0 && this.current_place != null)
            {
               
                if (this.forcePlace != null)
                {
                    if (this.checkin != null)
                    {
                        using (var db = Helper.getDB())
                        {
                            db.Checkins.Find(this.checkin.ID).Place = this.forcePlace;
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        this.doCheckin();
                    }
                    //this.checkin.Place = this.forcePlace;

                }
                else
                {
                    using (var db = Helper.getDB())
                    {
                        db.Checkins.Remove(db.Checkins.Find(this.checkin.ID));
                        this.checkin = null;
                    }
                }
                using (var db = Helper.getDB())
                {

                    foreach (PlacesNetworsValue pnv in this.backuppnv)
                    {
                        PlacesNetworsValue pnv2 = db.PlacesNetworsValues.Where(c => c.Network.ID == pnv.Network.ID).Where(c => c.Place.ID == pnv.Place.ID).First();
                        if (pnv2 != null)
                        {
                            pnv2.rilevance = pnv.rilevance;
                            pnv2.media = pnv.media;
                            pnv2.variance = pnv.variance;
                        }
                    }
                    db.Places.Find(this.current_place.ID).m_num = this.backupc;
                    db.SaveChanges();
                }
            }
        }
        // }*/

    }
}


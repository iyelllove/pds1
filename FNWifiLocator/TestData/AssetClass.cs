using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using FNWifiLocatorLibrary;

namespace FNWifiLocator
{
    public class AssetClass : INotifyPropertyChanged
    {
        private String placeName;

        public String PlaceName
        {
            get { return placeName; }
            set {
                placeName = value;
                RaisePropertyChangeEvent("Placename");
            }
        }

        private double media;

        public double Media
        {
            get { return media; }
            set {
                media = value;
                RaisePropertyChangeEvent("Media");
            }
        }

        private String total;

        public String Total
        {
            get { return total; }
            set {
                total = value;
                RaisePropertyChangeEvent("Total");
            }
        }

        private double times;

        public double Times
        {
            get { return times; }
            set {
                times = value;
                RaisePropertyChangeEvent("Times");
            }
        }



        public static List<AssetClass> ConstructTestData()
        {
            List<AssetClass> assetClasses = new List<AssetClass>();


            using (var dddb = Helper.getDB())
            {

                var places = dddb.Places.ToList();
                foreach (Place place in places) {
                    int i = dddb.Checkins.Where(c => c.Place.ID == place.ID).Count();

                    var cks = dddb.Checkins.Where(c => c.Place.ID == place.ID).ToList();
                    var totaltime = 0;
                    
                    foreach (Checkin ck in cks) {
                        var temp = ((DateTime)(ck.@out)).Subtract(ck.@in);
                        totaltime += ((temp.Days*24 +((temp.Hours) * 60) + temp.Minutes)*60)+temp.Seconds;
                         //mediatime = mediatime+totaltime; 
                    }
                    var qurey = from ad in dddb.Checkins
                                where (ad.Place.ID == place.ID)
                                select ad;



                    assetClasses.Add(new AssetClass() { PlaceName = place.name, Media = totaltime / Math.Max(i, 1), Total = totaltime.ToString(), Times = i });
                }
               
            }



         

            return assetClasses;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangeEvent(String propertyName)
        {
            if (PropertyChanged!=null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            
        }

        #endregion
    }
}

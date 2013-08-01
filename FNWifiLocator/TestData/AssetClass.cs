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
        private String myClass;

        public String Class
        {
            get { return myClass; }
            set {
                myClass = value;
                RaisePropertyChangeEvent("Class");
            }
        }

        private double fund;

        public double Fund
        {
            get { return fund; }
            set {
                fund = value;
                RaisePropertyChangeEvent("Fund");
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

        private double benchmark;

        public double Benchmark
        {
            get { return benchmark; }
            set {
                benchmark = value;
                RaisePropertyChangeEvent("Benchmark");
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



                    var qurey = from ad in dddb.Checkins
                                where (ad.Place.ID == place.ID)
                                select ad;

                    assetClasses.Add(new AssetClass() { Class = place.name, Fund = 1+i, Total = place.name, Benchmark = i });
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

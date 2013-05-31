using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using FNWifiLocatorLibrary;



namespace FNWifiLocator
{

    class Helper : FNWifiLocatorLibrary.Helper
    {




        static public List<FNWifiLocatorLibrary.Place> getAllRootPlaces()
        {

            try
            {
                IQueryable<Place> places;

                using (var  db = getDB())
                {
                    places = db.Places.Where(c => c.Parent.Equals(null));

                    if(places.Any() != false)
                    {
                        return places.ToList(); // this line is the problem
                    }
                }


            }
            catch (Exception ex)
            {
                Log.error(ex.ToString());
            }
            return null;

           // datapds1Entities2
           // return db.Places.Where(c => c.Parent.Equals(null)).ToList();
        }
    }
}
